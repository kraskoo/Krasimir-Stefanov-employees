namespace Employees.Application.Services;

using Employees.Application.Comparers;
using Employees.Application.Models;

public class PeriodService : IPeriodService
{
    public ResultPair GetLongestTimePeriod(IEnumerable<ProjectBindingModel> projects)
    {
        SortedDictionary<int, SortedSet<ResultPair>> commonProjects = [];
        SortedDictionary<int, int> workedDaysTogether = [];
        var bestResult = new ResultPair { TotalDays = 0 };
        var employeeProjects = projects.GroupBy(p => p.ProjectID)
            .ToDictionary(g => g.Key, g => g.OrderBy(x => x.EmpID).ToList());
        foreach (var employeeProject in employeeProjects)
        {
            var employees = employeeProject.Value;
            for (int i = 0; i < employees.Count; i++)
            {
                for (int j = i + 1; j < employees.Count; j++)
                {
                    var firstEmployee = employees[i];
                    var secondEmployee = employees[j];
                    var workedTogether = GetOverlapDays(
                        firstEmployee.DateFrom, firstEmployee.DateTo,
                        secondEmployee.DateFrom, secondEmployee.DateTo);
                    if (workedTogether <= 0)
                    {
                        continue;
                    }

                    var key = firstEmployee.EmpID + secondEmployee.EmpID;
                    if (!commonProjects.ContainsKey(key))
                    {
                        commonProjects[key] = new SortedSet<ResultPair>(new ResultPairTotalDaysComparer());
                        workedDaysTogether[key] = 0;
                    }

                    commonProjects[key].Add(new()
                    {
                        FirstEmployeeId = firstEmployee.EmpID,
                        SecondEmployeeId = secondEmployee.EmpID,
                        ProjectId = employeeProject.Key,
                        TotalDays = workedTogether
                    });
                    workedDaysTogether[key] += workedTogether;
                }
            }
        }

        var keys = workedDaysTogether.OrderByDescending(x => x.Value);
        if (!keys.Any())
        {
            return new();
        }

        var winnerKey = keys.FirstOrDefault().Key;
        var winnerPair = commonProjects[winnerKey];
        var firstProject = winnerPair.FirstOrDefault()!;

        bestResult.CommonProjects = winnerPair;

        bestResult.FirstEmployeeId = firstProject.FirstEmployeeId;
        bestResult.SecondEmployeeId = firstProject.SecondEmployeeId;
        bestResult.TotalDays = bestResult.CommonProjects.Sum(x => x.TotalDays);
        return bestResult;
    }

    private static int GetOverlapDays(
        DateTime firstEmployeeStart,
        DateTime? firstEmployeeEnd,
        DateTime secondEmployeeStart,
        DateTime? secondEmployeeEnd)
    {
        if (firstEmployeeEnd == null)
        {
            firstEmployeeEnd = DateTime.Today;
        }

        if (secondEmployeeEnd == null)
        {
            secondEmployeeEnd = DateTime.Today;
        }

        var latestStart = new DateTime(Math.Max(firstEmployeeStart.Ticks, secondEmployeeStart.Ticks));
        var earliestEnd = new DateTime(Math.Min(
            firstEmployeeEnd.GetValueOrDefault().Ticks,
            secondEmployeeEnd.GetValueOrDefault().Ticks));
        return latestStart <= earliestEnd ? (earliestEnd - latestStart).Days + 1 : 0;
    }
}
