namespace Employees.Application.Tests;

using Employees.Application.Models;
using Employees.Application.Services;

public class TimePeriodServiceTests
{
    private readonly TimePeriodService _service = new();

    [Fact]
    public void GetLongestTimePeriod_WithSingleOverlap_ShouldReturnCorrectPair()
    {
        var projects = new List<ProjectBindingModel>
        {
            new() { EmpID = 1, ProjectID = 100, DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 10) },
            new() { EmpID = 2, ProjectID = 100, DateFrom = new DateTime(2020, 1, 5), DateTo = new DateTime(2020, 1, 15) }
        };
        var result = _service.GetLongestTimePeriod(projects);
        Assert.Equal(1, result.FirstEmployeeId);
        Assert.Equal(2, result.SecondEmployeeId);
        Assert.Equal(6, result.TotalDays);
        var common = result.CommonProjects?.ToList() ?? [];
        Assert.Single(common);
        Assert.Equal(100, common[0].ProjectId);
    }

    [Fact]
    public void GetLongestTimePeriod_WithMultipleProjects_ShouldSumDaysAndPicksMaxPair()
    {
        var projects = new List<ProjectBindingModel>
        {
            new() { EmpID = 1, ProjectID = 100, DateFrom = new DateTime(2020, 1, 1), DateTo = new DateTime(2020, 1, 10) },
            new() { EmpID = 2, ProjectID = 100, DateFrom = new DateTime(2020, 1, 5), DateTo = new DateTime(2020, 1, 15) },
            new() { EmpID = 1, ProjectID = 101, DateFrom = new DateTime(2020, 2, 1), DateTo = new DateTime(2020, 2, 10) },
            new() { EmpID = 2, ProjectID = 101, DateFrom = new DateTime(2020, 2, 5), DateTo = new DateTime(2020, 2, 8) },
            new() { EmpID = 3, ProjectID = 200, DateFrom = new DateTime(2020, 3, 1), DateTo = new DateTime(2020, 3, 2) },
            new() { EmpID = 1, ProjectID = 200, DateFrom = new DateTime(2020, 3, 2), DateTo = new DateTime(2020, 3, 3) },
        };
        var result = _service.GetLongestTimePeriod(projects);
        Assert.Equal(1, result.FirstEmployeeId);
        Assert.Equal(2, result.SecondEmployeeId);
        Assert.Equal(10, result.TotalDays);
        var common = result.CommonProjects?.ToList() ?? [];
        Assert.Equal(2, common.Count);
        Assert.Contains(common, p => p.ProjectId == 100);
        Assert.Contains(common, p => p.ProjectId == 101);
    }

    [Fact]
    public void GetLongestTimePeriod_WithNoOverlap_ShouldReturnEmptyResult()
    {
        var projects = new List<ProjectBindingModel>
        {
            new() { EmpID = 1, ProjectID = 100, DateFrom = new DateTime(2020,1,1), DateTo = new DateTime(2020,1,2) },
            new() { EmpID = 2, ProjectID = 101, DateFrom = new DateTime(2020,2,1), DateTo = new DateTime(2020,2,2) },
        };
        var result = _service.GetLongestTimePeriod(projects);
        Assert.Equal(0, result.FirstEmployeeId);
        Assert.Equal(0, result.SecondEmployeeId);
        Assert.Equal(0, result.TotalDays);
        Assert.Empty(result.CommonProjects);
    }
}