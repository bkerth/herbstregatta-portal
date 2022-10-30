using Herbstregatta.Data.Registration;

namespace Herbstregatta.Data.Services.Interfaces
{
    public interface IRaceConfigurationService
    {
        public Task<IReadOnlyCollection<AnnouncedRace>> GetConfiguredRacesAsync();

        public Task<AnnouncedRace?> CreateOrUpdateRaceConfigurationAsync(int raceNumber, string name, string description, string category, int countOfAthletes, bool hasCox, string token);

        public Task<bool> RemoveRaceConfigurationAsync(int raceConfigurationId, string token);
    }
}
