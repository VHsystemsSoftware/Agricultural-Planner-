using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.WebAPI.Authorization;

namespace VHS.WebAPI.Controllers.SystemManagement;

[ApiController]
[Route("api/system")]
public class SystemController : ControllerBase
{
    [HttpGet("version")]
    [AllowAnonymous]
    public IActionResult Version()
    {        
        return Ok(GetType().Assembly.GetName().Version?.ToString() ?? string.Empty);
    }
}
