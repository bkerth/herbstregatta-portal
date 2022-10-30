namespace Herbstregatta.Data.Common
{
    public class TeamAccount
    {
        /// <summary>
        /// The internal team account
        /// </summary>
        public int Id { get; set; }

        public Team Team { get; set; }

        public string Salt { get; set; }

        public string HashedTeamToken { get; set; }
    }
}
