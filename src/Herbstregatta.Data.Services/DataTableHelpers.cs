using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Services.Extensions;
using System.Data;

namespace Herbstregatta.Data.Services
{
    public static class DataTableHelpers
    {
        /// <summary>
        /// Converts a list of race entries to a data table
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static DataTable ToDataTable(this ICollection<RegisteredRace> raceEntries)
        {
            DataTable table = new DataTable();

            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("RaceNumber", typeof(int));
            table.Columns.Add("RaceName");
            table.Columns.Add("Club");
            table.Columns.Add("ListOfAthletes");
            table.Columns.Add("Trainer");
            table.Columns.Add("Phone");
            table.Columns.Add("Email");
            table.Columns.Add("BoatName");

            foreach (var raceEntry in raceEntries)
            {
                table.Rows.Add(raceEntry.Id,
                    raceEntry.AnnouncedRace.Number,
                    raceEntry.AnnouncedRace.Name,
                    raceEntry.Team.Name,
                    raceEntry.Athletes.ToAthletesString(),
                    raceEntry.Trainer,
                    raceEntry.PhoneContact,
                    raceEntry.EmailContact,
                    raceEntry.BoatName);
            }

            return table;
        }
    }
}
