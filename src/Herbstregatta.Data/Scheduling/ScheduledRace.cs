using Herbstregatta.Data.Registration;
using System;
using System.Collections.Generic;

namespace Herbstregatta.Data.Scheduling
{
    /// <summary>
    /// The sheduled race
    /// </summary>
    public class ScheduledRace
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The race run number
        /// </summary>
        public string RunNumber { get; set; }

        /// <summary>
        /// The <see cref="Registration.AnnouncedRace"/> that describes the race
        /// </summary>
        public AnnouncedRace AnnouncedRace { get; set; }

        /// <summary>
        /// The race entries that are assigned to this race run and their track id
        /// </summary>
        public ICollection<AssignedRace> AssignedRaces { get; set; }

        /// <summary>
        /// The planned start time
        /// </summary>
        public DateTime PlannedStartTime { get; set; }

        /// <summary>
        /// The planned start time
        /// </summary>
        public bool IsFinished { get; set; }
    }
}
