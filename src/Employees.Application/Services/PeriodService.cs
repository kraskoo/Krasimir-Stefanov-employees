namespace Employees.Application.Services;

using Employees.Application.Comparers;
using Employees.Application.Models;

public class PeriodService : IPeriodService
{
    public ResultPair GetLongestTimePeriod(IEnumerable<ProjectBindingModel> projects)
    {
        SortedSet<ResultPair> commonProjects = new(new ResultPairTotalDaysComparer());
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
                    if (workedTogether > 0)
                    {
                        commonProjects.Add(new()
                        {
                            FirstEmployeeId = firstEmployee.EmpID,
                            SecondEmployeeId = secondEmployee.EmpID,
                            ProjectId = employeeProject.Key,
                            TotalDays = workedTogether
                        });
                    }
                }
            }
        }

        var winnerPair = commonProjects.FirstOrDefault();
        if (winnerPair is null)
        {
            return new();
        }

        bestResult.CommonProjects = commonProjects.Where(x =>
            x.FirstEmployeeId == winnerPair.FirstEmployeeId &&
            x.SecondEmployeeId == winnerPair.SecondEmployeeId);

        bestResult.ProjectId = winnerPair.ProjectId;
        bestResult.FirstEmployeeId = winnerPair.FirstEmployeeId;
        bestResult.SecondEmployeeId = winnerPair.SecondEmployeeId;
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
