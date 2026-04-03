using System.Diagnostics;
using BookLoans.Abstractions.Interfaces;
using Microsoft.AspNetCore.Mvc;
using BookLoans.Web.Models;

namespace BookLoans.Web.Controllers;

public class HomeController(IPublicHomepageQueryService queryService) : Controller
{
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        HomepageViewModel viewModel = HomepageViewModel.FromDtos(await queryService.GetAsync(ct));
        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
