using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VHS.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temp allow
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateUser([FromBody] UserDTO userDto)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(userDto);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUser([FromBody] UserDTO userDto)
    {
        try
        {
            var updatedUser = await _userService.UpdateUserAsync(userDto);
            return Ok(updatedUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            await _userService.DeleteUserAsync(id);
            return NoContent(); // Success, no content to return
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
