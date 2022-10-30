using Herbstregatta.Data.Common;
using System.Collections.Generic;

namespace Herbstregatta.Data.Registration
{
    /// <summary>
    /// Describes the registered entry for a race
    /// </summary>
    public class RegisteredRace
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The assigned RaceConfiguration
        /// </summary>
        public AnnouncedRace AnnouncedRace { get; set; }

        /// <summary>
        /// Information about the Team
        /// </summary>
        public Team Team { get; set; }

        /// <summary>
        /// Responsible person of the Team
        /// </summary>
        public Person Trainer { get; set; }

        /// <summary>
        /// A phone contact where the Trainer is reachable
        /// </summary>
        public string PhoneContact { get; set; }

        /// <summary>
        /// A email contact where the Trainer is reachable
        /// </summary>
        public string EmailContact { get; set; }

        /// <summary>
        /// The Athletes with their position
        /// </summary>
        public ICollection<Athlete> Athletes { get; set; }

        /// <summary>
        /// Optional parameter for the boat name
        /// </summary>
        public string BoatName { get; set; }
    }
}
