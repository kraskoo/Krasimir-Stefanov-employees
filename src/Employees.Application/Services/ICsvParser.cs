namespace Employees.Application.Services;

using Employees.Application.Models;

public interface ICsvParser
{
    ValueTask<Result<IEnumerable<ProjectBindingModel>>> Parse(Stream stream, CancellationToken cancellationToken = default);
}