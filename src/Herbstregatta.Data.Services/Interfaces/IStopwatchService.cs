using Herbstregatta.Data.Run;
using Herbstregatta.Data.Scheduling;

namespace Herbstregatta.Data.Services.Interfaces
{
    public interface IStopwatchService
    {
        public Task<TimeMeasurement?> GetTimeMeasurementByScheduledRaceId(int scheduledRaceId);

        public Task<TimeMeasurement> CreateEmptyTimeMeasurement(ScheduledRace scheduledRace);

        public Task<TimeMeasurement> StartTimeMeasurement(int id, DateTimeOffset deviceStartTime, string name);

        public Task<TimeMeasurement> StopTimeMeasurement(int id, DateTimeOffset deviceStopTime, string name);

        public Task<TimeMeasurement> SaveTimeMeasurement(TimeMeasurement timeMeasurement, ICollection<TimeSplit> timeSplits, string verifiedByName);

        public Task<TimeMeasurement> ResetTimeMeasurement(string token, int id);


        public Task<ICollection<TimeSplit>> GetSplitTimesForTimeMeasurement(TimeMeasurement timeMeasurement);

        public Task<ICollection<TimeSplit>> SplitTimeMeasurement(int id, DateTimeOffset deviceSplitTime, string name);

        public Task<ICollection<TimeSplit>> DeleteSplitTimeMeasurement(string token, int id);
    }
}
