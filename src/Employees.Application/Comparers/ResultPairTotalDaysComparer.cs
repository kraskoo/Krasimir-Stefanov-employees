namespace Employees.Application.Comparers;

using Employees.Application.Models;

public class ResultPairTotalDaysComparer : IComparer<ResultPair>
{
    public int Compare(ResultPair? x, ResultPair? y) =>
        (y ?? new()).TotalDays.CompareTo((x ?? new()).TotalDays);
}