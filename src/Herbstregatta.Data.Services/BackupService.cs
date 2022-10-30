using ClosedXML.Excel;
using Herbstregatta.Data.Run;
using Herbstregatta.Data.Services.Extensions;
using Herbstregatta.Data.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Herbstregatta.Data.Services;

/// <summary>
/// Implements the <see cref="IBackupService"/>
/// </summary>
public class BackupService : IBackupService
{
    private readonly ILogger<BackupService> _logger;

    /// <summary>
    /// Creates an instance of the <see cref="BackgroundService"/>
    /// </summary>
    /// <param name="races"></param>
    /// <param name="logger"></param>
    public BackupService(ILogger<BackupService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts a list of race entries to a data table
    /// </summary>
    /// <param name="jsonString"></param>
    /// <returns></returns>
    public MemoryStream ExportTimeMeasurementToExcel(TimeMeasurement timeMeasurement, ICollection<TimeSplit> timeSplits)
    {
        using XLWorkbook wb = new XLWorkbook();

        string raceName = $"{timeMeasurement.Race.AnnouncedRace.Number}_{timeMeasurement.Race.RunNumber}";

        var ws = wb.Worksheets.Add(raceName);

        ws.Cell("A1").Value = "Rennergebnis";

        var rngTitle = ws.Range("A1:C1").Merge();

        rngTitle.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        rngTitle.Style.Font.FontSize = 15;
        rngTitle.Style.Font.Bold = true;

        ws.Cell("A3").Value = timeMeasurement.Race.AnnouncedRace.Number;
        ws.Cell("B3").Value = $"{timeMeasurement.Race.AnnouncedRace.Name} (LaufNr. {timeMeasurement.Race.RunNumber})";
        ws.Cell("C3").Value = timeMeasurement.Race.PlannedStartTime;

        var dataTable = GetResultTable(timeSplits);

        var table = ws.Cell(5, 1).InsertTable(dataTable.AsEnumerable());

        table.Theme = XLTableTheme.TableStyleMedium17;
        ws.Columns().AdjustToContents();

        //Save the document as a stream and return the stream.
        MemoryStream stream = new MemoryStream();

        //Save the created Excel document to MemoryStream.
        wb.SaveAs(stream);
        return stream;
    }

    private DataTable GetResultTable(ICollection<TimeSplit> timeSplits)
    {
        DataTable table = new DataTable();
        table.Columns.Add("Platz", typeof(int));
        table.Columns.Add("Team", typeof(string));
        table.Columns.Add("Zeit", typeof(TimeSpan));

        foreach (var timeSplit in timeSplits)
        {
            table.Rows.Add(timeSplit.Placing, timeSplit.AssignedRaceEntry.ToEntryinfo(), timeSplit.Duration);
        }

        return table;
    }


    /// <summary>
    /// Converts a <see cref="DataTable"/> to an excle MemoryStream
    /// </summary>
    /// <param name="dataTable"></param>
    /// <returns></returns>
    public MemoryStream ExportDataTableToExcel(DataTable dataTable, string worksheetName)
    {
        using XLWorkbook wb = new XLWorkbook();

        wb.Worksheets.Add(dataTable, worksheetName);

        var ws = wb.Worksheet(worksheetName);
        ws.Columns().AdjustToContents();

        var table = ws.Tables.Table(0);
        table.Theme = XLTableTheme.TableStyleMedium17;

        //Save the document as a stream and return the stream.
        MemoryStream stream = new MemoryStream();

        //Save the created Excel document to MemoryStream.
        wb.SaveAs(stream);
        return stream;
    }
}
