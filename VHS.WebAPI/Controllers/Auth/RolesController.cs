using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace VHS.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temp allow
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RolesController(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        return Ok(roles);
    }
}
