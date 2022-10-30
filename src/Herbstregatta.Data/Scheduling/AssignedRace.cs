using Herbstregatta.Data.Registration;

namespace Herbstregatta.Data.Scheduling
{
    public class AssignedRace
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The RaceEntry
        /// </summary>
        public RegisteredRace RegisteredRace { get; set; }

        /// <summary>
        /// The assigned track for the race entry
        /// </summary>
        public string Track { get; set; }
    }
}
