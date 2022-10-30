namespace Herbstregatta.Data.Registration
{
    /// <summary>
    /// Describes the configuration of a race
    /// </summary>
    public class AnnouncedRace
    {
        /// <summary>
        /// Internal race id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Nuber of the race
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Name of the race
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the race
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Category of the race
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Describes the count of athlethes that are in a boat
        /// </summary>
        public int CountOfAthletes { get; set; }

        /// <summary>
        /// Describes if the boat is controlled by a cox
        /// </summary>
        public bool HasCox { get; set; }
    }
}
