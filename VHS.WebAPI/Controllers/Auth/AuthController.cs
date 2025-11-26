using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using VHS.Data.Auth.Models.Auth;
using VHS.Services.Auth.DTO;
using Microsoft.AspNetCore.Authorization;
using VHS.Services.Auth;

namespace VHS.WebAPI.Controllers.Auth
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;
        private readonly IUserAuthorizationService _authorizationService;

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration config,
            IUserAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _authorizationService = authorizationService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null || user.DeletedDateTime != null)
            {
                return Unauthorized(LoginResultDTO.Failed());
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized(LoginResultDTO.Failed());
            }

            var userDto = new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };

            var token = await GenerateJwtToken(user);

            return Ok(LoginResultDTO.Success(token, userDto));
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] PasswordResetDTO dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || user.DeletedDateTime != null)
                return NotFound(new { message = "User not found." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!resetResult.Succeeded)
                return BadRequest(resetResult.Errors);

        return NoContent();
    }

    [HttpGet("permissions")]
    [Authorize]
    public IActionResult GetCurrentUserPermissions()
    {
        try
        {
            var currentUser = User;
            
            var permissions = new UserAuthorizationDTO
            {
                IsGlobalAdmin = _authorizationService.IsGlobalAdmin(currentUser),
                CanViewUsers = _authorizationService.CanViewUsers(currentUser),
                CanCreateUsers = _authorizationService.CanCreateUsers(currentUser),
                CanEditUsers = _authorizationService.CanEditUser(currentUser, Guid.Empty),
                CanDeleteUsers = _authorizationService.CanDeleteUser(currentUser, Guid.Empty),
                CanChangeUserRole = _authorizationService.CanChangeUserRole(currentUser),
                CurrentUserRole = _authorizationService.GetCurrentUserRole(currentUser),
                CurrentUserId = _authorizationService.GetCurrentUserId(currentUser)
            };

            return Ok(permissions);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("can-delete-user/{targetUserId}")]
    [Authorize]
    public IActionResult CanDeleteSpecificUser(Guid targetUserId)
    {
        try
        {
            var currentUser = User;
            var canDelete = _authorizationService.CanDeleteUser(currentUser, targetUserId);
            
            return Ok(new { CanDelete = canDelete });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private async Task<string> GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.Email!)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
