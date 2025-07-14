using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace VHS.WebAPI.Controllers.Auth;

[ApiController]
[Route("api/user/settings")]
[AllowAnonymous] // Temp allow
public class UserSettingController : ControllerBase
{
    private readonly IUserSettingService _userSettingService;

    public UserSettingController(IUserSettingService userSettingService)
    {
        _userSettingService = userSettingService;
    }

    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserSettings(Guid userId)
    {
        var settings = await _userSettingService.GetUserSettingsByUserIdAsync(userId);
        if (settings == null)
        {
            return NotFound();
        }
        return Ok(settings);
    }

    [HttpPut("update")]
    public async Task<IActionResult> UpdateUserSettings([FromBody] UserSettingDTO settingsDto)
    {
        try
        {
            var updatedSettings = await _userSettingService.UpdateUserSettingsAsync(settingsDto);
            return Ok(updatedSettings);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
