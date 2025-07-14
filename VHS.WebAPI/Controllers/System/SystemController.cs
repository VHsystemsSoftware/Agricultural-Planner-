using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VHS.WebAPI.Controllers.Produce;

[ApiController]
[Route("api/system")]
[AllowAnonymous]
public class SystemController : ControllerBase
{
    private readonly IProductService _productService;

    public SystemController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("version")]
    public async Task<IActionResult> Version()
    {        
        return Ok(GetType().Assembly.GetName().Version.ToString());
    }

    
}
