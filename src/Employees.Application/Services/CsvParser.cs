namespace Employees.Application.Services;

using Employees.Application.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Frozen;
using System.Globalization;
using System.Threading.Tasks;

public class CsvParser(ILogger<CsvParser> logger) : ICsvParser
{
    private const int ExpectedColumnCount = 4;
    private static readonly FrozenSet<string> Headers = new List<string>(
        ["EmpID", "ProjectID", "DateFrom", "DateTo"]).ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);
    private static readonly string[] DateFormats =
    [
        "yyyy-MM-dd",
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss.fff",
        "yyyy-MM-dd HH:mm:ss.fffffff",
        "MM/dd/yyyy",
        "dd-MM-yyyy"
    ];

    public async ValueTask<Result<IEnumerable<ProjectBindingModel>>> Parse(Stream stream, CancellationToken cancellationToken = default)
    {
        using var reader = new StreamReader(stream);
        var line = await reader.ReadLineAsync(cancellationToken);
        var headers = (line?.Split(',') ?? []).Select(x => x.Trim()).ToFrozenSet(StringComparer.InvariantCultureIgnoreCase);
        if (!Headers.SetEquals(headers))
        {
            logger.LogError(
                "Invalid CSV headers. Expected: {ExpectedHeaders}. Found: {FoundHeaders}.",
                string.Join(", ", Headers),
                string.Join(", ", headers));
            return $"Invalid CSV headers. Expected: {string.Join(", ", Headers)}. Found: {string.Join(", ", headers)}.";
        }

        var models = new List<ProjectBindingModel>();
        line = await reader.ReadLineAsync(cancellationToken);
        while (!string.IsNullOrEmpty(line))
        {
            var columns = (line?.Split(',') ?? []).Select(x => x.Trim()).ToList();
            if (columns.Count != ExpectedColumnCount)
            {
                logger.LogWarning("Skipping invalid CSV line: {Line}.", line);
                return $"Skipping invalid CSV line: {line}.";
            }

            var column0 = columns[0];
            if (!int.TryParse(column0, out var empId))
            {
                logger.LogWarning(
                    "Invalid column type. Expected: {Type}. Found: {Value} which cannot be parsed.",
                    nameof(Int32),
                    column0);
                return $"Invalid column type. Expected: {nameof(Int32)}. Found: {column0} which cannot be parsed.";
            }

            var column1 = columns[1];
            if (!int.TryParse(column1, out var projectId))
            {
                logger.LogWarning(
                    "Invalid column type. Expected: {Type}. Found: {Value} which cannot be parsed.",
                    nameof(Int32),
                    column1);
                return $"Invalid column type. Expected: {nameof(Int32)}. Found: {column1} which cannot be parsed.";
            }

            var column2 = columns[2];
            var dateFrom = ParseDate(column2);
            if (dateFrom == null)
            {
                logger.LogWarning(
                    "Invalid column type. Expected: {Type}. Found: {Value} which cannot be parsed.",
                    "DateTime?",
                    column2);
                return $"Invalid column type. Expected: {"DateTime?"}. Found: {column2} which cannot be parsed.";
            }

            var column3 = columns[3];
            var dateTo = ParseDate(column3);
            models.Add(new()
            {
                EmpID = empId,
                ProjectID = projectId,
                DateFrom = dateFrom.GetValueOrDefault(),
                DateTo = dateTo
            });
            line = await reader.ReadLineAsync(cancellationToken);
        }

        return models;
    }

    public static DateTime? ParseDate(string dateString)
    {
        if (string.IsNullOrEmpty(dateString) ||
            string.Equals(dateString, "NULL", StringComparison.InvariantCultureIgnoreCase) ||
            !DateTime.TryParseExact(
                dateString,
                DateFormats,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var date))
        {
            return null;
        }

        return date;
    }
}