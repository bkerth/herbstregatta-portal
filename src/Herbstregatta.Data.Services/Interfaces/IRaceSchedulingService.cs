using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Scheduling;

namespace Herbstregatta.Data.Services.Interfaces
{
    public interface IRaceSchedulingService
    {
        public Task<ICollection<RegisteredRace>> GetUnassignedRaceEntries(string token);

        public Task<ICollection<ScheduledRace>> GetScheduledRaces(string token);

        public Task<ScheduledRace?> CreateScheduledRace(string token, AnnouncedRace raceConfiguration, ICollection<AssignedRace> assignedRaceEntries, DateTime plannedStartTime);

        public Task<bool> DeleteScheduledRace(string token, int scheduledRaceId);
    }
}
