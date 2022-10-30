using Herbstregatta.Data.Common;
using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Scheduling;

namespace Herbstregatta.Data.Services.Extensions
{
    public static class DataExtensions
    {
        public static string ToEntryinfo(this AssignedRace entry)
        {
            if (entry == null)
            {
                return string.Empty;
            }
            return $"{entry.Track} / {entry.RegisteredRace.ToScheduledTeamInfo()}";
        }

        public static string ToAthletesString(this ICollection<Athlete> athletes)
        {
            if (athletes == null || !athletes.Any())
            {
                return string.Empty;
            }

            var stb = new System.Text.StringBuilder();
            foreach (Athlete athlete in athletes)
            {
                if (athlete.Person != null)
                {
                    stb.Append($"{athlete.Person.FirstName} {athlete.Person.Name} ({athlete.Person.Year})");
                }

                stb.Append(", ");
            }
            // Remove last ", "
            stb.Remove(stb.Length - 2, 2);
            return stb.ToString();
        }

        public static string ToScheduledTeamInfo(this RegisteredRace raceEntry)
        {
            var stb = new System.Text.StringBuilder();
            foreach (Athlete athlete in raceEntry.Athletes)
            {
                if (athlete.Person != null)
                {
                    stb.Append($"{athlete.Person.FirstName.Substring(0, 1)}. {athlete.Person.Name}");
                }

                stb.Append(", ");
            }
            // Remove last ", "
            stb.Remove(stb.Length - 2, 2);

            return $"{raceEntry.Team.Name} ({stb})";
        }
    }
}
