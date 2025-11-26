using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VHS.Services;
using VHS.Services.Auth.DTO;
using VHS.WebAPI.Authorization;

namespace VHS.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.CanManageUsers)]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync(User);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = AuthorizationPolicies.CanManageUsers)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create")]
    [Authorize(Policy = AuthorizationPolicies.GlobalAdminOnly)]
    public async Task<IActionResult> CreateUser([FromBody] UserDTO userDto)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(userDto, User);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update")]
    [Authorize(Policy = AuthorizationPolicies.AdminAndAbove)]
    public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDto)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(userDto, User);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = AuthorizationPolicies.GlobalAdminOnly)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id, User);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
