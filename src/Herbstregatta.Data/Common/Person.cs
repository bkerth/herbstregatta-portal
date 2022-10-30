using System.Collections.Generic;

namespace Herbstregatta.Data.Common
{
    /// <summary>
    /// Describes a person
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the person
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// First Name of the person
        /// </summary>

        public string FirstName { get; set; }

        /// <summary>
        /// Birth year of the person
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// The teams the person is associated
        /// </summary>
        public ICollection<Team> Teams { get; set; }

        public static Person NoName(Team team)
        {
            return new Person
            {
                Name = "N.",
                FirstName = "N.",
                Year = 2022,
                Teams = new List<Team>() { team }
            };
        }

        public override string ToString()
        {
            return $"{Name}, {FirstName} ({Year})";
        }
    }
}
