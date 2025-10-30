namespace Employees.Application.Models;

public class ProjectBindingModel
{
    public int ProjectID { get; set; }
    public int EmpID { get; set; }
    public DateTime DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public override int GetHashCode() =>
        HashCode.Combine(ProjectID, EmpID);

    public override bool Equals(object? obj) =>
        obj is ProjectBindingModel other &&
        GetHashCode() == other.GetHashCode();
}