using Herbstregatta.Data.Scheduling;
using System;

namespace Herbstregatta.Data.Run
{
    /// <summary>
    /// Describes a split time result
    /// </summary>
    public class TimeSplit
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Placing of the time split
        /// </summary>
        public int Placing { get; set; }

        /// <summary>
        /// The scheduled race
        /// </summary>
        public TimeMeasurement TimeMeasurement { get; set; }

        /// <summary>
        /// The time that was recorded
        /// </summary>
        public TimeRecord Time { get; set; }

        /// <summary>
        /// The duration of the time split
        /// </summary>
        public TimeSpan Duration { get; set; }

        /// <summary>
        /// <see cref="Scheduling.AssignedRace"/> that is assigned to this <see cref="TimeSplit"/>
        /// </summary>
        public AssignedRace AssignedRaceEntry { get; set; }
    }
}
