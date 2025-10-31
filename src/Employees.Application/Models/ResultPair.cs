namespace Employees.Application.Models;

public class ResultPair
{
    public int ProjectId { get; set; }
    public int FirstEmployeeId { get; set; }
    public int SecondEmployeeId { get; set; }
    public int TotalDays { get; set; }
    public IEnumerable<ResultPair> CommonProjects { get; set; } = [];

    public override int GetHashCode() =>
        HashCode.Combine(ProjectId, (FirstEmployeeId, SecondEmployeeId));

    public override bool Equals(object? obj) =>
        obj is ResultPair other &&
        GetHashCode() == other.GetHashCode();
}