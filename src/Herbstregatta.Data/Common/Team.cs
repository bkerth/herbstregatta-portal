using System.Collections.Generic;

namespace Herbstregatta.Data.Common
{
    /// <summary>
    /// Describes a Team
    /// </summary>
    public class Team
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the Team
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Default Phone number of the Team
        /// </summary>
        public string DefaultPhone { get; set; }

        /// <summary>
        /// Default Email of the Team
        /// </summary>
        public string DefaultEmail { get; set; }

        /// <summary>
        /// The Persons assigned to the Team
        /// </summary>
        public ICollection<Person> Persons { get; set; }
    }
}
