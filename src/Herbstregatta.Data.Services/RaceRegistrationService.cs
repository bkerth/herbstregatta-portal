using Herbstregatta.Data.Common;
using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Services.Data;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Herbstregatta.Data.Services
{
    public class RaceRegistrationService : IRaceRegistrationService
    {
        private readonly HerbstregattaDbContext _Context;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<RaceRegistrationService> _Logger;

        private readonly object _TeamsAccountContextLock = new object();
        private readonly object _TeamsContextLock = new object();
        private readonly object _PersonsContextLock = new object();
        private readonly object _RaceEntryContextLock = new object();
        private readonly object _RaceConfigurationContextLock = new object();

        public RaceRegistrationService(HerbstregattaDbContext context, IConfiguration configuration, ILogger<RaceRegistrationService> logger)
        {
            _Context = context;
            _Configuration = configuration;
            _Logger = logger;
        }

        public bool IsTeamTokenValid(int teamId, string plainPassword)
        {
            if (teamId == 00)
            {
                throw new ArgumentNullException(nameof(teamId));
            }

            lock (_TeamsAccountContextLock)
            {
                var teamAccount = _Context.TeamAccounts.FirstOrDefault(t => t.Team.Id.Equals(teamId));

                if (teamAccount == null)
                {
                    _Logger.LogError("No team is created with this name!");
                    return false;
                }
                string hashedTeamPassword = PaswordHashingHelper.HashPassword(plainPassword, teamAccount.Salt);
                if (!teamAccount.HashedTeamToken.Equals(hashedTeamPassword, StringComparison.InvariantCulture))
                {
                    _Logger.LogError("Ivalid token!");
                    return false;
                }
            }

            return true;
        }

        private bool TokenIsValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!_Configuration["TeamToken"].Equals(token, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Teams without Persons!
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Team>> GetTeams()
        {
            return await _Context.Teams.ToArrayAsync(); ;
        }

        /// <summary>
        /// Teams without Persons!
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<Team>> GetTeamsWithTeamPersons()
        {
            return await _Context.Teams.Include(t => t.Persons).ToArrayAsync(); ;
        }

        private Team? GetTeamById(int clubId)
        {
            if (clubId == 00)
            {
                throw new ArgumentNullException(nameof(clubId));
            }

            lock (_TeamsContextLock)
            {
                var team = _Context.Teams.Find(clubId);

                if (team == null)
                {
                    _Logger.LogError("There is no team with the id '{name}'", clubId);
                }

                return team;
            }
        }

        public async Task<Team?> CreateTeam(string name, string phone, string email, string plainPassword, string token)
        {
            if (!TokenIsValid(token))
            {
                _Logger.LogError("Received token is not valid!");
                return default;
            }

            Team? team = null;
            lock (_TeamsContextLock)
            {
                if (_Context.Teams.Any())
                {
                    team = _Context.Teams.FirstOrDefault(t => t.Name.Equals(name));
                };

                if (team == null)
                {
                    team = new Team
                    {
                        Name = name,
                        DefaultPhone = phone,
                        DefaultEmail = email
                    };
                    _Context.Teams.Add(team);

                    var pwd_data = PaswordHashingHelper.CreatePasswordHash(plainPassword);
                    var teamAccount = new TeamAccount
                    {
                        HashedTeamToken = pwd_data.Hash,
                        Salt = pwd_data.Salt,
                        Team = team
                    };
                    _Context.TeamAccounts.Add(teamAccount);
                }
                else
                {
                    // Exists already
                    return team;
                }
            }

            await _Context.SaveChangesAsync();

            _Logger.LogInformation("Successfull saved team configuration in database. Id={id}", team.Id);

            return team;
        }

        public Task<ICollection<Person>> GetTeamPersons(int teamId, string plainPassword)
        {
            if (!IsTeamTokenValid(teamId, plainPassword))
            {
                _Logger.LogError("Ivalid token!");
                return Task.FromResult<ICollection<Person>>(Array.Empty<Person>());
            }

            Team? team;
            lock (_TeamsContextLock)
            {
                team = _Context.Teams
                    .Find(teamId);

                if (team is null)
                {
                    return Task.FromResult<ICollection<Person>>(Array.Empty<Person>());
                }

                _Context.Entry(team)
                    .Collection(t => t.Persons)
                    .Load();
            }

            lock (_PersonsContextLock)
            {
                if (team.Persons is null)
                {
                    return Task.FromResult<ICollection<Person>>(Array.Empty<Person>());
                }

                return Task.FromResult(team.Persons);
            }
        }

        public async Task<Person?> CreatePerson(int teamId, string plainPassword, string name, string firstName, int birthYear)
        {
            if (!IsTeamTokenValid(teamId, plainPassword))
            {
                _Logger.LogError("Ivalid token!");
                return default;
            }

            Person person = new Person
            {
                Name = name,
                FirstName = firstName,
                Year = birthYear
            };

            lock (_PersonsContextLock)
            {
                _Context.Persons.Add(person);
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Team team = GetTeamById(teamId);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            if (team is null)
            {
                return default;
            }

            lock (_TeamsContextLock)
            {
                if (team.Persons == null)
                {
                    team.Persons = new List<Person>();
                }
                if (!team.Persons.Contains(person))
                {
                    team.Persons.Add(person);
                }
                _Context.Teams.Update(team);
            }

            await _Context.SaveChangesAsync();

            _Logger.LogInformation("Successfull saved person in database. Id={id}", team.Id);

            return person;
        }

        private bool IsTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!_Configuration["ManagementToken"].Equals(token, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public Task<ICollection<RegisteredRace>> GetRaceEntries(string token)
        {
            if (!IsTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return Task.FromResult<ICollection<RegisteredRace>>(Array.Empty<RegisteredRace>());
            }

            lock (_RaceEntryContextLock)
            {
                List<RegisteredRace> raceEntries = new();

                if (_Context.RaceEntry.Any())
                {
                    raceEntries = _Context.RaceEntry
                        .Include(r => r.AnnouncedRace)
                        .Include(r => r.Athletes)
                        .Include(r => r.Trainer)
                        .Include(r => r.Team)
                        .Include(r => r.Team.Persons)
                        .ToList();
                }

                return Task.FromResult<ICollection<RegisteredRace>>(raceEntries);
            }
        }

        public Task<ICollection<RegisteredRace>> GetUnassignedRaceEntries(string token)
        {
            if (!IsTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return Task.FromResult<ICollection<RegisteredRace>>(Array.Empty<RegisteredRace>());
            }

            lock (_RaceEntryContextLock)
            {
                List<RegisteredRace> raceEntries = new();

                if (_Context.RaceEntry.Any())
                {
                    var scheduledRaces = _Context.ScheduledRaces.Include(s => s.AssignedRaces);

                    var unassignedRaceEntryQuery = _Context.RaceEntry
                        .Where(r => !scheduledRaces.Any(s => s.AssignedRaces.Select(a => a.RegisteredRace).Contains(r)));

                    unassignedRaceEntryQuery
                        .Include(r => r.AnnouncedRace)
                        .Load();

                    unassignedRaceEntryQuery
                        .Include(r => r.Athletes)
                        .Load();

                    raceEntries = unassignedRaceEntryQuery
                        .Include(r => r.Team)
                        .ToList();
                }

                return Task.FromResult<ICollection<RegisteredRace>>(raceEntries);
            }
        }

        public Task<ICollection<RegisteredRace>> GetTeamRaceEntries(int clubId, string plainPassword)
        {
            if (!IsTeamTokenValid(clubId, plainPassword))
            {
                _Logger.LogError("Ivalid token!");
                return Task.FromResult<ICollection<RegisteredRace>>(Array.Empty<RegisteredRace>());
            }

            lock (_RaceEntryContextLock)
            {
                List<RegisteredRace> raceEntries = new();

                if (_Context.RaceEntry.Any())
                {


                    raceEntries = _Context.RaceEntry
                        .Include(r => r.AnnouncedRace)
                        .Include(r => r.Athletes)
                        .Include(r => r.Team)
                        .Include(r => r.Team.Persons)
                        .Include(r => r.Trainer)
                        .Where(r => r.Team.Id.Equals(clubId)).Include(r => r.Athletes)
                        .ToList();
                }

                return Task.FromResult<ICollection<RegisteredRace>>(raceEntries);
            }
        }

        public async Task<RegisteredRace?> CreateRaceEntry(int clubId, string plainPassword, int raceNumber, Person chairPerson, string phoneContact, string emailContact, ICollection<Athlete> athletes, string? boatName = null)
        {
            if (!IsTeamTokenValid(clubId, plainPassword))
            {
                _Logger.LogError("Ivalid token!");
                return default;
            }

            RegisteredRace raceEntry;
            lock (_RaceConfigurationContextLock)
            {
                AnnouncedRace? raceConfiguration = _Context.RaceConfiguration.Find(raceNumber);

                if (raceConfiguration == null)
                {
                    _Logger.LogError("Cannot find RaceConfiguration!");
                    return default;
                }

                Team? team = GetTeamById(clubId);

                if (team == null)
                {
                    _Logger.LogError("Cannot find the club! Id={id}", clubId);
                    return default;
                }

                lock (_RaceEntryContextLock)
                {
                    raceEntry = new RegisteredRace
                    {
                        Team = team,
                        Athletes = athletes.ToList(),
                        AnnouncedRace = raceConfiguration,
                        Trainer = chairPerson,
                        EmailContact = emailContact,
                        PhoneContact = phoneContact,
                        BoatName = boatName
                    };

                    _Context.RaceEntry.Add(raceEntry);
                }
            }

            await _Context.SaveChangesAsync();

            _Logger.LogInformation("Successfull saved race entry in database. Id={id}", raceEntry.Id);

            return raceEntry;
        }

        public async Task<RegisteredRace?> GetRaceEntry(string token, int raceEntryId)
        {
            if (!IsTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return null;
            }

            _Context.RaceEntry.Include(e => e.Trainer).Load();
            _Context.RaceEntry.Include(e => e.Team).ThenInclude(t => t.Persons).Load();

            RegisteredRace? raceEntry = await _Context.RaceEntry
                .FindAsync(raceEntryId);

            if (raceEntry != null && raceEntry.BoatName == null)
            {
                raceEntry.BoatName = "-";
            }

            return raceEntry;
        }

        public async Task<RegisteredRace?> UpdateRaceEntry(RegisteredRace raceEntryToUpdate, string plainPasswordOrManagementToken)
        {
            if (!IsTeamTokenValid(raceEntryToUpdate.Team.Id, plainPasswordOrManagementToken)
                && !IsTokenValid(plainPasswordOrManagementToken))
            {
                _Logger.LogError("Ivalid token!");
                return null;
            }


            lock (_RaceEntryContextLock)
            {
                _Context.Update(raceEntryToUpdate);
            }

            await _Context.SaveChangesAsync();

            _Logger.LogInformation("Successfull saved race entry in database. Id={id}", raceEntryToUpdate.Id);

            return raceEntryToUpdate;
        }

        public async Task<bool> RemoveRaceEntry(RegisteredRace raceEntryToRemove, string plainPasswordOrManagementToken)
        {
            if (!IsTeamTokenValid(raceEntryToRemove.Team.Id, plainPasswordOrManagementToken)
                && !IsTokenValid(plainPasswordOrManagementToken))
            {
                _Logger.LogError("Ivalid token!");
                return false;
            }

            lock (_RaceEntryContextLock)
            {
                _Context.RaceEntry.Remove(raceEntryToRemove);
            }

            await _Context.SaveChangesAsync();

            _Logger.LogInformation("Successfull removed race entry from database. Id={id}", raceEntryToRemove.Id);
            return true;
        }
    }
}

