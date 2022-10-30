namespace Herbstregatta.Data.Common
{
    public class Athlete
    {
        /// <summary>
        /// Internal Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The person behind an athlete
        /// </summary>
        public Person Person { get; set; }

        /// <summary>
        /// The role / position of the athlete
        /// </summary>
        public string Role { get; set; }
    }
}
