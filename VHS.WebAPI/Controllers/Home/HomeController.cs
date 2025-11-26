using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VHS.Services.Home;
using VHS.WebAPI.Authorization;

namespace VHS.WebAPI.Controllers.Home;

[ApiController]
[Route("api/home")]
[Authorize(Policy = AuthorizationPolicies.CanViewDashboards)]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    [HttpGet("planning")]
    public async Task<IActionResult> GetPlanningStats()
    {
        return Ok(await _homeService.GetPlanningStatsAsync());
    }

    [HttpGet("operational")]
    public async Task<IActionResult> GetOperationalStats()
    {
        return Ok(await _homeService.GetOperationalStatsAsync());
    }

    [HttpGet("results")]
    public async Task<IActionResult> GetResultsStats()
    {
        return Ok(await _homeService.GetResultsStatsAsync());
    }

    [HttpGet("messages")]
    public async Task<IActionResult> GetSystemMessages()
    {
        return Ok(await _homeService.GetSystemMessagesAsync());
    }
}
