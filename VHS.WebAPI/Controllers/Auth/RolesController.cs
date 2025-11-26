using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VHS.Services.Auth;

namespace VHS.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IUserAuthorizationService _authorizationService;

    public RolesController(RoleManager<IdentityRole<Guid>> roleManager, IUserAuthorizationService authorizationService)
    {
        _roleManager = roleManager;
        _authorizationService = authorizationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRoles()
    {
        var allRoles = await _roleManager.Roles.Select(r => r.Name).Where(r => r != null).ToListAsync();
        
        var isGlobalAdmin = _authorizationService.IsGlobalAdmin(User);
        if (!isGlobalAdmin)
        {
            allRoles = allRoles.Where(r => r != "global_admin").ToList();
        }

        allRoles = allRoles
            .OrderBy(r => r == "admin" ? 0 : r == "farm_manager" ? 1 : r == "operator" ? 2 : 3)
            .ThenBy(r => r)
            .ToList();
        
        return Ok(allRoles);
    }
}
