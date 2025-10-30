namespace Employees.Application.Tests;

using Employees.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text;

public class CsvParserTests
{
    private static CsvParser CreateParser() => new(
        LoggerFactory.Create(builder => builder.AddConsole())
                     .CreateLogger<CsvParser>());

    [Theory]
    [InlineData("2020-01-01")]
    [InlineData("2020-01-01 12:34:56")]
    [InlineData("2020-01-01 12:34:56.789")]
    [InlineData("01/01/2020")]
    [InlineData("01-01-2020")]
    public void ParseDate_WithValidFormats_ShouldReturnDate(string input)
    {
        var result = CsvParser.ParseDate(input);
        Assert.NotNull(result);
        Assert.Equal(2020, result!.Value.Year);
    }

    [Theory]
    [InlineData("")]
    [InlineData("NULL")]
    [InlineData("invalid-date")]
    public void ParseDate_WithInvalidOrNull_ShouldReturnNull(string input)
    {
        var result = CsvParser.ParseDate(input);
        Assert.Null(result);
    }

    [Fact]
    public async Task Parse_WithValidCsv_ShouldReturnModels()
    {
        var csv = new StringBuilder()
            .AppendLine("EmpID, ProjectID, DateFrom, DateTo")
            .AppendLine("1, 100, 2020-01-01, 2020-01-10")
            .AppendLine("2, 100, 2020-01-05, NULL")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.True(result.IsSuccess);
        var list = result.Data!.ToList();
        Assert.Equal(2, list.Count);
        var first = list.Single(p => p.EmpID == 1);
        Assert.Equal(100, first.ProjectID);
        Assert.Equal(new DateTime(2020, 1, 1), first.DateFrom);
        Assert.Equal(new DateTime(2020, 1, 10), first.DateTo);
        var second = list.Single(p => p.EmpID == 2);
        Assert.Equal(100, second.ProjectID);
        Assert.Equal(new DateTime(2020, 1, 5), second.DateFrom);
        Assert.Null(second.DateTo);
    }

    [Fact]
    public async Task Parse_WithInvalidHeader_ShouldReturnError()
    {
        var csv = new StringBuilder()
            .AppendLine("Employee, Project, From, To")
            .AppendLine("1, 100, 2020-01-01, 2020-01-10")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Invalid CSV headers", result.ErrorMessage);
    }

    [Fact]
    public async Task Parse_WithInvalidColumnCount_ShouldReturnError()
    {
        var csv = new StringBuilder()
            .AppendLine("EmpID, ProjectID, DateFrom, DateTo")
            .AppendLine("1, 100, 2020-01-01")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Skipping invalid CSV line", result.ErrorMessage);
    }

    [Fact]
    public async Task Parse_WithInvalidEmpId_ShouldReturnError()
    {
        var csv = new StringBuilder()
            .AppendLine("EmpID, ProjectID, DateFrom, DateTo")
            .AppendLine("abc, 100, 2020-01-01, 2020-01-10")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Expected: Int32", result.ErrorMessage);
    }

    [Fact]
    public async Task Parse_WithInvalidProjectId_ShouldReturnError()
    {
        var csv = new StringBuilder()
            .AppendLine("EmpID, ProjectID, DateFrom, DateTo")
            .AppendLine("1, xyz, 2020-01-01, 2020-01-10")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Expected: Int32", result.ErrorMessage);
    }

    [Fact]
    public async Task Parse_WithInvalidDateFrom_ShouldReturnError()
    {
        var csv = new StringBuilder()
            .AppendLine("EmpID, ProjectID, DateFrom, DateTo")
            .AppendLine("1, 100, bad-date, 2020-01-10")
            .ToString();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
        var parser = CreateParser();
        var result = await parser.Parse(stream, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
        Assert.Contains("Expected: DateTime?", result.ErrorMessage);
    }
}