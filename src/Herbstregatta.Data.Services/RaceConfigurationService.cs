using Herbstregatta.Data.Registration;
using Herbstregatta.Data.Services.Data;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Herbstregatta.Data.Services
{
    public class RaceConfigurationService : IRaceConfigurationService
    {
        private readonly HerbstregattaDbContext _Context;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<RaceConfigurationService> _Logger;

        private readonly object _ContextLock = new object();

        public RaceConfigurationService(HerbstregattaDbContext context, IConfiguration configuration, ILogger<RaceConfigurationService> logger)
        {
            _Context = context;
            _Configuration = configuration;
            _Logger = logger;
        }

        public async Task<IReadOnlyCollection<AnnouncedRace>> GetConfiguredRacesAsync()
        {
            return await _Context.RaceConfiguration.ToArrayAsync();
        }

        public async Task<AnnouncedRace?> CreateOrUpdateRaceConfigurationAsync(int raceNumber, string name, string description, string category, int countOfAthletes, bool hasCox, string token)
        {
            if (raceNumber == 00 || string.IsNullOrEmpty(name) || string.IsNullOrEmpty(description) || countOfAthletes == 0)
            {
                return default;
            }

            if (!TokenIsValid(token))
            {
                return default;
            }

            AnnouncedRace? raceConfiguration = null;
            lock (_ContextLock)
            {
                if (_Context.RaceConfiguration.Any())
                {
                    raceConfiguration = _Context.RaceConfiguration.FirstOrDefault(m => m.Number == raceNumber);
                }

                if (raceConfiguration is null)
                {
                    _Logger.LogInformation("No race configuration with race number '{raceNumber}' exists, create a new one.", raceNumber);

                    raceConfiguration = new AnnouncedRace
                    {
                        CountOfAthletes = countOfAthletes,
                        Description = description,
                        Category = category,
                        HasCox = hasCox,
                        Name = name,
                        Number = raceNumber
                    };
                    _Context.Add(raceConfiguration);
                }
                else
                {
                    _Logger.LogInformation("Update race configuration '{raceNumber}'. Name={name}, Description={description}, CountOfAthlete={countOfAthlete}, HasCox={hasCox}",
                        raceNumber, name, description, countOfAthletes, hasCox);

                    raceConfiguration.Name = name;
                    raceConfiguration.Description = description;
                    raceConfiguration.CountOfAthletes = countOfAthletes;
                    raceConfiguration.HasCox = hasCox;
                    _Context.Update(raceConfiguration);
                }
            }

            try
            {
                await _Context.SaveChangesAsync();
                _Logger.LogInformation("Successfull saved race configuration to database. Id={id}", raceConfiguration.Id);
                return raceConfiguration;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to update the race configuration in the database. Id={id}", raceConfiguration.Id);
                return null;
            }
        }

        public async Task<bool> RemoveRaceConfigurationAsync(int raceConfigurationId, string token)
        {
            if (raceConfigurationId == 00)
            {
                _Logger.LogError("Id of RaceConfiguration is empty!");
                return false;
            }

            if (!TokenIsValid(token))
            {
                _Logger.LogError("An invalid token was used!");
                return false;
            }

            lock (_ContextLock)
            {
                var raceConfiguration = _Context.RaceConfiguration.Find(raceConfigurationId);

                if (raceConfiguration == null)
                {
                    _Logger.LogError("Cannot find race configuration. Id={id}", raceConfigurationId);
                    return false;
                }

                _Context.RaceConfiguration.Remove(raceConfiguration);
            }
            try
            {
                await _Context.SaveChangesAsync();
                _Logger.LogInformation("Successfull removed race configuration from database. Id={id}", raceConfigurationId);
                return true;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to remove race configuration from database. Id={id}", raceConfigurationId);
                return false;
            }
        }

        private bool TokenIsValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!_Configuration["ManagementToken"].Equals(token, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }
}
