using Herbstregatta.Data.Scheduling;

namespace Herbstregatta.Data.Run
{
    /// <summary>
    /// The <see cref="TimeMeasurement"/> that describes a recorded race
    /// </summary>
    public class TimeMeasurement
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Race for which the measurement is recorded
        /// </summary>
        public ScheduledRace Race { get; set; }

        /// <summary>
        /// Recorded start time
        /// </summary>
        public TimeRecord StartTime { get; set; }

        /// <summary>
        /// Recorded stop time
        /// </summary>
        public TimeRecord StopTime { get; set; }

        /// <summary>
        /// The responsible person that verified the measurement
        /// </summary>
        public string VerifiedByName { get; set; }
    }
}
