using Herbstregatta.Data.Run;
using Herbstregatta.Data.Scheduling;
using Herbstregatta.Data.Services.Data;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Herbstregatta.Data.Services
{
    public class StopwatchService : IStopwatchService
    {
        private readonly HerbstregattaDbContext _Context;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<StopwatchService> _Logger;

        private readonly object _timeLock = new();

        public StopwatchService(HerbstregattaDbContext context, IConfiguration configuration, ILogger<StopwatchService> logger)
        {
            _Context = context;
            _Configuration = configuration;
            _Logger = logger;
        }

        private bool IsManagementTokenValid(string token)
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

        private bool IsStopwatchTokenValid(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            if (!_Configuration["StopwatchToken"].Equals(token, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns the <see cref="TimeMeasurement"/> for <paramref name="scheduledRaceId"/>
        /// </summary>
        /// <param name="scheduledRaceId"></param>
        /// <returns>Null if no <see cref="TimeMeasurement"/> was found!</returns>
        public async Task<TimeMeasurement?> GetTimeMeasurementByScheduledRaceId(int scheduledRaceId)
        {
            TimeMeasurement? timeMeasurement = null;

            try
            {
                await _Context.TimeMeasurements.Include(t => t.Race).LoadAsync();
                await _Context.TimeMeasurements.Include(t => t.StopTime).LoadAsync();
                await _Context.TimeMeasurements.Include(t => t.StartTime).LoadAsync();

                lock (_timeLock)
                {
                    timeMeasurement = _Context.TimeMeasurements
                        .FirstOrDefault(t => t.Race.Id.Equals(scheduledRaceId));
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to load time measurement! ScheduledRaceId={id}", scheduledRaceId);
            }

            return timeMeasurement;
        }

        /// <summary>
        /// Creates an empyty <see cref="TimeMeasurement"/> in the database
        /// </summary>
        /// <param name="scheduledRace"></param>
        /// <returns></returns>
        /// <exception cref="DbUpdateException">If database update fails</exception>
        public async Task<TimeMeasurement> CreateEmptyTimeMeasurement(ScheduledRace scheduledRace)
        {
            var existingTimeMeasurement = await _Context.TimeMeasurements.FirstOrDefaultAsync(t => t.Race.Equals(scheduledRace));

            if (existingTimeMeasurement != null)
            {
                return existingTimeMeasurement;
            }

            try
            {
                _Logger.LogInformation("Create new emtpy TimeMeasurement. RaceName={raceName}, RunNumber={runNumber}",
                scheduledRace.AnnouncedRace.Name, scheduledRace.RunNumber);

                var timeMeasurement = new TimeMeasurement
                {
                    Race = scheduledRace
                };

                _Context.TimeMeasurements.Add(timeMeasurement);

                await _Context.SaveChangesAsync();
                return timeMeasurement;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Could not create a new time measurement! ScheduledRace={id}", scheduledRace.Id);

                throw new DbUpdateException("Time measurement could not be created in Database!");
            }
        }

        /// <summary>
        /// Sets the start time in <see cref="TimeMeasurement"/> if not set!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deviceStartTime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<TimeMeasurement> StartTimeMeasurement(int id, DateTimeOffset deviceStartTime, string name)
        {
            var timeMeasurement = await _Context.TimeMeasurements.FindAsync(id);

            if (timeMeasurement == null)
            {
                _Logger.LogError("Could not find time measurement! Id={id}", id);
                throw new FileNotFoundException($"Could not find time measurement! Id={id}");
            }

            if (timeMeasurement.StartTime != null)
            {
                _Logger.LogWarning("Time measurement was already startet!");
                return timeMeasurement;
            }

            var timeRecord = new TimeRecord
            {
                Name = name,
                ReceivedServerTime = DateTimeOffset.Now,
                RecordedDeviceTime = deviceStartTime
            };

            try
            {
                lock (_timeLock)
                {
                    timeMeasurement.StartTime = timeRecord;

                    _Context.TimeRecords.Add(timeRecord);
                    _Context.TimeMeasurements.Update(timeMeasurement);
                }

                await _Context.SaveChangesAsync();

                return timeMeasurement;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to update start time in database! Id={id}, StartTime={startTime}", id, deviceStartTime);
                throw new DbUpdateException("Could not save start time in database!");
            }

        }

        /// <summary>
        /// Sets the stop time in <see cref="TimeMeasurement"/> if not set!
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deviceStopTime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<TimeMeasurement> StopTimeMeasurement(int id, DateTimeOffset deviceStopTime, string name)
        {
            var timeMeasurement = await _Context.TimeMeasurements.FindAsync(id);

            if (timeMeasurement == null)
            {
                _Logger.LogError("Could not find time measurement! Id={id}", id);
                throw new FileNotFoundException($"Could not find time measurement! Id={id}");
            }

            if (timeMeasurement.StopTime != null)
            {
                _Logger.LogWarning("Time measurement was already stopped!");
                return timeMeasurement;
            }

            var timeRecord = new TimeRecord
            {
                Name = name,
                ReceivedServerTime = DateTimeOffset.Now,
                RecordedDeviceTime = deviceStopTime
            };

            var timeSplit = new TimeSplit
            {
                Time = timeRecord,
                Duration = deviceStopTime.Subtract(timeMeasurement.StartTime.RecordedDeviceTime),
                TimeMeasurement = timeMeasurement,
            };

            timeMeasurement.StopTime = timeRecord;

            try
            {
                lock (_timeLock)
                {
                    _Context.TimeRecords.Add(timeRecord);
                    _Context.TimeSplits.Add(timeSplit);
                    _Context.TimeMeasurements.Update(timeMeasurement);
                }

                await _Context.SaveChangesAsync();
                return timeMeasurement;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to update stop time in database! Id={id}, StopTime={stopTime}", id, deviceStopTime);
                throw new DbUpdateException("Could not save stop time in database!");
            }
        }

        /// <summary>
        /// Saves the <see cref="TimeMeasurement"/> and finishes the race
        /// </summary>
        /// <param name="timeMeasurement"></param>
        /// <param name="timeSplits"></param>
        /// <param name="verifiedByName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<TimeMeasurement> SaveTimeMeasurement(TimeMeasurement timeMeasurement, ICollection<TimeSplit> timeSplits, string verifiedByName)
        {
            if (timeMeasurement == null)
            {
                throw new ArgumentNullException(nameof(timeMeasurement));
            }

            timeMeasurement.VerifiedByName = verifiedByName;
            timeMeasurement.Race.IsFinished = true;

            try
            {
                lock (_timeLock)
                {
                    _Context.TimeMeasurements.Update(timeMeasurement);
                    _Context.TimeSplits.UpdateRange(timeSplits);
                    _Context.ScheduledRaces.Update(timeMeasurement.Race);
                }

                await _Context.SaveChangesAsync();

                return timeMeasurement;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to update time measurement in database! Id={id}", timeMeasurement.Id);
                throw new DbUpdateException("Could not save time measurement in database!");
            }
        }

        /// <summary>
        /// Saves the <see cref="TimeMeasurement"/> in the database
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<TimeMeasurement> ResetTimeMeasurement(string token, int id)
        {
            if (!IsManagementTokenValid(token) && !IsStopwatchTokenValid(token))
            {
                _Logger.LogError("Received token is invalid!");
                throw new UnauthorizedAccessException("Received token is invalid!");
            }

            var timeMeasurement = await _Context.TimeMeasurements.FindAsync(id);

            if (timeMeasurement == null)
            {
                _Logger.LogError("Cannot find time measurement! Id={id}", id);
                throw new FileNotFoundException($"Cannot find time measurement! Id={id}");
            }

            try
            {
                lock (_timeLock)
                {
                    _Context.TimeRecords.Remove(timeMeasurement.StartTime);
                    _Context.TimeRecords.Remove(timeMeasurement.StopTime);

                    timeMeasurement.StopTime = null;
                    timeMeasurement.StartTime = null;

                    _Context.TimeMeasurements.Update(timeMeasurement);

                    var timeSplits = _Context.TimeSplits
                        .Where(t => t.TimeMeasurement.Equals(timeMeasurement))
                        .ToList();

                    foreach (var split in timeSplits)
                    {
                        _Context.TimeSplits.Remove(split);
                    }
                }

                await _Context.SaveChangesAsync();
                return timeMeasurement;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to reset time measurement!");
                throw new DbUpdateException("Failed to reset the time measurement!");
            }
        }


        /// <summary>
        /// Retruns the <see cref="TimeSplit"/>s for a <see cref="TimeMeasurement"/>
        /// </summary>
        /// <param name="timeMeasurement"></param>
        /// <returns></returns>
        public Task<ICollection<TimeSplit>> GetSplitTimesForTimeMeasurement(TimeMeasurement timeMeasurement)
        {
            ICollection<TimeSplit> timeSplits = new List<TimeSplit>();

            try
            {
                lock (_timeLock)
                {
                    _Context.RaceEntry.Include(r => r.Athletes).Load();
                    _Context.AssignedRaceEntries.Include(a => a.RegisteredRace).Load();
                    _Context.TimeSplits.Include(t => t.AssignedRaceEntry).Load();

                    timeSplits = _Context.TimeSplits
                        .Where(t => t.TimeMeasurement.Equals(timeMeasurement))
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to load the split times. TimeMeasurementId={id}", timeMeasurement.Id);
            }

            return Task.FromResult(timeSplits);
        }

        /// <summary>
        /// Sets a <see cref="TimeSplit"/> in the database
        /// </summary>
        /// <param name="id"></param>
        /// <param name="deviceSplitTime"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<ICollection<TimeSplit>> SplitTimeMeasurement(int id, DateTimeOffset deviceSplitTime, string name)
        {
            var timeMeasurement = await _Context.TimeMeasurements.FindAsync(id);

            if (timeMeasurement == null)
            {
                _Logger.LogError("Could not find time measurement! Id={id}", id);
                throw new FileNotFoundException($"Could not find time measurement! Id={id}");
            }

            try
            {
                var lastPlace = 0;
                lock (_timeLock)
                {
                    var splitTimes = _Context.TimeSplits
                        .Where(t => t.TimeMeasurement.Equals(timeMeasurement))
                        .OrderBy(t => t.Duration)
                        .ToArray();

                    var lastPlaceItem = splitTimes.LastOrDefault();
                    if (lastPlaceItem != null)
                    {
                        lastPlace = lastPlaceItem.Placing + 1;
                    }

                    var timeSplit = new TimeSplit
                    {
                        TimeMeasurement = timeMeasurement,
                        Time = new TimeRecord
                        {
                            Name = name,
                            ReceivedServerTime = DateTimeOffset.Now,
                            RecordedDeviceTime = deviceSplitTime,
                        },
                        Duration = deviceSplitTime.Subtract(timeMeasurement.StartTime.RecordedDeviceTime),
                        Placing = lastPlace + 1,
                    };

                    _Context.TimeSplits.Add(timeSplit);
                }

                await _Context.SaveChangesAsync();
                return await GetSplitTimesForTimeMeasurement(timeMeasurement);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to update split time in database! Id={id}, SplitTime={splitTime}", id, deviceSplitTime);
                throw new DbUpdateException("Could not save split time in database!");
            }
        }

        /// <summary>
        /// Deletes a <see cref="TimeSplit"/> from database
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="UnauthorizedAccessException"></exception>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="DbUpdateException"></exception>
        public async Task<ICollection<TimeSplit>> DeleteSplitTimeMeasurement(string token, int id)
        {
            if (!IsManagementTokenValid(token) && !IsStopwatchTokenValid(token))
            {
                _Logger.LogError("Received token is invalid!");
                throw new UnauthorizedAccessException("Received token is invalid!");
            }

            var splitTime = await _Context.TimeSplits.FindAsync(id);

            if (splitTime == null)
            {
                _Logger.LogError("Cannot find split time! Id={id}", id);
                throw new FileNotFoundException($"Cannot find time measurement! Id={id}");
            }

            try
            {
                var timeMeasurement = splitTime.TimeMeasurement;

                _Context.TimeSplits.Remove(splitTime);
                await _Context.SaveChangesAsync();

                lock (_timeLock)
                {
                    var splitTimesToUpdate = _Context.TimeSplits
                        .Where(t => t.TimeMeasurement.Equals(timeMeasurement))
                        .OrderBy(t => t.Duration)
                        .ToList();

                    for (var i = 0; i < splitTimesToUpdate.Count; i++)
                    {
                        splitTimesToUpdate[i].Placing = i + 1;
                    }
                    _Context.TimeSplits.UpdateRange(splitTimesToUpdate);
                }

                await _Context.SaveChangesAsync();

                return await GetSplitTimesForTimeMeasurement(timeMeasurement);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to remove split time from database! Id={id}", id);
                throw new DbUpdateException("Could not remove split time from database!");
            }
        }
    }
}
