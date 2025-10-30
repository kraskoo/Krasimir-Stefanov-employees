namespace Employees.Application.Services;

using Employees.Application.Comparers;
using Employees.Application.Models;

public class TimePeriodService : ITimePeriodService
{
    public ResultPair GetLongestTimePeriod(IEnumerable<ProjectBindingModel> projects)
    {
        Dictionary<(int, int), SortedSet<ResultPair>> commonProjects = [];
        Dictionary<(int, int), int> workedDaysTogether = [];
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

                    var key = (firstEmployee.EmpID, secondEmployee.EmpID);
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

        var topWorkedDaysTogether = workedDaysTogether.OrderByDescending(x => x.Value);
        if (!topWorkedDaysTogether.Any())
        {
            return new();
        }

        var winnerKey = topWorkedDaysTogether.FirstOrDefault().Key;
        var winnerPair = commonProjects[winnerKey];
        var firstProject = winnerPair.FirstOrDefault()!;

        var bestResultPair = new ResultPair
        {
            CommonProjects = winnerPair,
            FirstEmployeeId = firstProject.FirstEmployeeId,
            SecondEmployeeId = firstProject.SecondEmployeeId
        };
        bestResultPair.TotalDays = bestResultPair.CommonProjects.Sum(x => x.TotalDays);

        return bestResultPair;
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
