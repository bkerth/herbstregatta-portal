using Herbstregatta.Data.Common;

namespace Herbstregatta.Data.Services.Data
{
    public class SelectedTeamData
    {
        /// <summary>
        /// Creates a new <see cref="SelectedTeamData"/>
        /// </summary>
        public SelectedTeamData(Team selectedTeam, string plainPassword)
        {
            if (string.IsNullOrEmpty(plainPassword))
            {
                throw new ArgumentException($"'{nameof(plainPassword)}' cannot be null or empty.", nameof(plainPassword));
            }
            SelectedTeam = selectedTeam ?? throw new ArgumentNullException(nameof(selectedTeam));
            PlainPassword = plainPassword;
        }

        /// <summary>
        /// The selected Team
        /// </summary>
        public Team SelectedTeam { get; set; }

        /// <summary>
        /// The hashed password of the team
        /// </summary>
        public string PlainPassword { get; private set; }
    }
}
