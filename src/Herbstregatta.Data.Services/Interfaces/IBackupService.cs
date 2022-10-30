using Herbstregatta.Data.Run;
using System.Data;

namespace Herbstregatta.Data.Services.Interfaces;

/// <summary>
/// Describes the Interface to backup and restore the race configuration
/// </summary>
public interface IBackupService
{
    /// <summary>
    /// Exports a time measurement to spreadsheet excel
    /// </summary>
    /// <param name="timeMeasurement"></param>
    /// <param name="timeSplits"></param>
    /// <returns></returns>
    public MemoryStream ExportTimeMeasurementToExcel(TimeMeasurement timeMeasurement, ICollection<TimeSplit> timeSplits);

    /// <summary>
    /// Converts a <see cref="DataTable"/> to an spreadsheet excel <see cref="MemoryStream"/>
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    public MemoryStream ExportDataTableToExcel(DataTable dataTable, string worksheetName);
}
