using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Scheduling;
using Herbstregatta.Data.Services.Data;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Herbstregatta.Data.Services
{
    public class RaceSchedulingService : IRaceSchedulingService
    {
        private readonly HerbstregattaDbContext _Context;
        private readonly IRaceRegistrationService _RaceEntryService;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<RaceSchedulingService> _Logger;

        public RaceSchedulingService(HerbstregattaDbContext context, IRaceRegistrationService raceEntryService, IConfiguration configuration, ILogger<RaceSchedulingService> logger)
        {
            _Context = context;
            _RaceEntryService = raceEntryService;
            _Configuration = configuration;
            _Logger = logger;
        }

        private bool IsManagementTokenValid(string token)
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

        private bool IsStopwatchTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!_Configuration["StopwatchToken"].Equals(token, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        public bool IsTeamTokenValid(string token)
        {
            var teamAccounts = _Context.TeamAccounts.ToList();

            foreach (var teamAccount in teamAccounts)
            {
                string hashedTeamPassword = PaswordHashingHelper.HashPassword(token, teamAccount.Salt);
                if (teamAccount.HashedTeamToken.Equals(hashedTeamPassword, StringComparison.InvariantCulture))
                {
                    return true;
                }
            }

            _Logger.LogError("Ivalid token!");
            return false;
        }


        public async Task<ICollection<RegisteredRace>> GetUnassignedRaceEntries(string token)
        {
            if (!IsManagementTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return new List<RegisteredRace>();
            }

            return await _RaceEntryService.GetUnassignedRaceEntries(token);
        }

        public async Task<bool> DeleteScheduledRace(string token, int scheduledRaceId)
        {
            if (!IsManagementTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return false;
            }

            lock (_lock)
            {
                var scheduledRaceToRemove = _Context.ScheduledRaces.Find(scheduledRaceId);

                if (scheduledRaceToRemove == null)
                {
                    return false;
                }
                _Context.ScheduledRaces.Remove(scheduledRaceToRemove);
            }

            await _Context.SaveChangesAsync();
            return true;
        }


        public async Task<ScheduledRace?> CreateScheduledRace(string token, AnnouncedRace raceConfiguration, ICollection<AssignedRace> assignedRaceEntries, DateTime plannedStartTime)
        {
            if (!IsManagementTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return null;
            }

            var scheduledRaces = await _Context.ScheduledRaces
                .Where(s => s.AnnouncedRace.Equals(raceConfiguration))
                .OrderBy(s => s.RunNumber)
                .ToArrayAsync();

            var newRunNumber = scheduledRaces.Max(s => s.RunNumber) + 1;

            var scheduledRace = new ScheduledRace
            {
                RunNumber = newRunNumber,
                AnnouncedRace = raceConfiguration,
                AssignedRaces = assignedRaceEntries,
                PlannedStartTime = plannedStartTime,
            };

            _Context.ScheduledRaces.Add(scheduledRace);
            await _Context.SaveChangesAsync();

            return scheduledRace;
        }

        private readonly object _lock = new();

        public Task<ICollection<ScheduledRace>> GetScheduledRaces(string token)
        {
            ICollection<ScheduledRace> scheduledRaces = new List<ScheduledRace>();

            if (!IsManagementTokenValid(token) && !IsStopwatchTokenValid(token) && !IsTeamTokenValid(token))
            {
                _Logger.LogError("Ivalid token!");
                return Task.FromResult(scheduledRaces);
            }

            lock (_lock)
            {
                _Context.RaceEntry
                    .Include(r => r.Team)
                    .Include(r => r.Athletes)
                    .ThenInclude(a => a.Person)
                    .Load();

                _Context.ScheduledRaces
                    .Include(s => s.AssignedRaces)
                    .Load();

                scheduledRaces = _Context.ScheduledRaces
                    .Include(s => s.AnnouncedRace)
                    .ToList();
            }

            return Task.FromResult(scheduledRaces);
        }
    }
}
