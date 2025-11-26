using Microsoft.AspNetCore.Mvc;
using VHS.Services.Results.DTO;
using VHS.Services.Results;
using Microsoft.AspNetCore.Authorization;

namespace VHS.WebAPI.Controllers.Results;

[ApiController]
[Route("api/[controller]")]
public class ResultController : ControllerBase
{
    private readonly IResultService _resultService;

    public ResultController(IResultService resultService)
    {
        _resultService = resultService;
    }

    [HttpPost("filter")]
    [Authorize(Policy = "CanAccessOverviewOperations")]
    public async Task<ActionResult<List<ResultItemDTO>>> GetResults([FromBody] ResultFilterDTO filter)
    {
        var results = await _resultService.GetResultsAsync(filter);
        return Ok(results);
    }
}
