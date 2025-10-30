namespace Employees.Application.Services;

using Employees.Application.Models;

public interface ITimePeriodService
{
    ResultPair GetLongestTimePeriod(IEnumerable<ProjectBindingModel> projects);
}