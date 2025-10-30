namespace Employees.Web.Controllers;

using Employees.Application.Models;
using Employees.Application.Services;
using Employees.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

public class HomeController(ICsvParser csvParser, ITimePeriodService periodService) : Controller
{
    public IActionResult Index() => View(new ResultPair());

    public async Task<IActionResult> Upload(IFormFile csvFile, CancellationToken cancellationToken)
    {
        if (csvFile == null || csvFile.Length == 0)
        {
            ModelState.AddModelError(string.Empty, "Please upload a CSV file.");
            return View("Index", new ResultPair());
        }


        using var stream = csvFile.OpenReadStream();
        var projects = await csvParser.Parse(stream, cancellationToken);
        if (!projects.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, projects.ErrorMessage!);
            return View("Index", new ResultPair());
        }

        var result = periodService.GetLongestTimePeriod(projects.Data!);
        return View("Index", result);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
}
