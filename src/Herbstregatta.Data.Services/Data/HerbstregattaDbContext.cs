using Herbstregatta.Data.Common;
using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Run;
using Herbstregatta.Data.Scheduling;
using Microsoft.EntityFrameworkCore;

namespace Herbstregatta.Data.Services.Data
{
    public class HerbstregattaDbContext : DbContext
    {
        public HerbstregattaDbContext(DbContextOptions<HerbstregattaDbContext> options)
            : base(options)
        {
        }

        public DbSet<AnnouncedRace> RaceConfiguration { get; set; } = default!;

        public DbSet<Team> Teams { get; set; } = default!;

        public DbSet<TeamAccount> TeamAccounts { get; set; } = default!;

        public DbSet<Person> Persons { get; set; } = default!;

        public DbSet<Athlete> Athletes { get; set; } = default!;

        public DbSet<RegisteredRace> RaceEntry { get; set; } = default!;

        public DbSet<AssignedRace> AssignedRaceEntries { get; set; } = default!;

        public DbSet<ScheduledRace> ScheduledRaces { get; set; } = default!;

        public DbSet<TimeMeasurement> TimeMeasurements { get; set; } = default!;

        public DbSet<TimeRecord> TimeRecords { get; set; } = default!;

        public DbSet<TimeSplit> TimeSplits { get; set; } = default!;
    }
}
