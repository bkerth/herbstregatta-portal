using Herbstregatta.Data.Common;
using Herbstregatta.Data.Registration;

namespace Herbstregatta.Data.Services.Interfaces
{
    public interface IRaceRegistrationService
    {
        public bool IsTeamTokenValid(int teamId, string plainPassword);

        public Task<ICollection<Team>> GetTeams();
        public Task<ICollection<Team>> GetTeamsWithTeamPersons();

        public Task<Team?> CreateTeam(string name, string phone, string email, string plainPassword, string token);

        public Task<ICollection<Person>> GetTeamPersons(int teamId, string plainPassword);

        public Task<Person?> CreatePerson(int teamId, string plainPassword, string name, string firstName, int birthYear);

        public Task<ICollection<RegisteredRace>> GetTeamRaceEntries(int clubId, string plainPassword);

        public Task<ICollection<RegisteredRace>> GetRaceEntries(string token);

        public Task<ICollection<RegisteredRace>> GetUnassignedRaceEntries(string token);

        public Task<RegisteredRace?> CreateRaceEntry(int clubId, string plainPassword, int raceNumber, Person chairPerson, string phoneContact, string emailContact, ICollection<Athlete> athletes, string? boatName = null);

        public Task<RegisteredRace?> GetRaceEntry(string token, int raceEntryId);

        public Task<RegisteredRace?> UpdateRaceEntry(RegisteredRace raceEntryToUpdate, string plainPasswordOrManagementToken);

        public Task<bool> RemoveRaceEntry(RegisteredRace raceEntryToUpdate, string plainPasswordOrManagementToken);
    }
}
