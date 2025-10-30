namespace Employees.Application.Services;

using Employees.Application.Models;

public interface IPeriodService
{
    ResultPair GetLongestTimePeriod(IEnumerable<ProjectBindingModel> projects);
}