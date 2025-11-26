using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using VHS.Data.Auth.Models.Auth;
using VHS.Services.Auth;
using VHS.Services.Auth.DTO;

namespace VHS.Services;

public interface IUserService
{
    Task<IEnumerable<UserDTO>> GetAllUsersAsync(ClaimsPrincipal currentUser);
    Task<UserDTO> CreateUserAsync(UserDTO userDto, ClaimsPrincipal currentUser);
    Task<UserDTO> UpdateUserAsync(UserDTO userDto, ClaimsPrincipal currentUser);
    Task<UserDTO?> GetUserByIdAsync(Guid id);
    Task DeleteUserAsync(Guid id, ClaimsPrincipal currentUser);
}

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IUserAuthorizationService _authorizationService;

    public UserService(UserManager<User> userManager, IUserAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync(ClaimsPrincipal currentUser)
    {
        if (!_authorizationService.CanViewUsers(currentUser))
        {
            throw new UnauthorizedAccessException("You don't have permission to view users.");
        }

        var users = await _userManager.Users.Where(u => u.DeletedDateTime == null).ToListAsync();
        var userDtos = new List<UserDTO>();
        var isGlobalAdmin = _authorizationService.IsGlobalAdmin(currentUser);

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault();

            if (!isGlobalAdmin && userRole == "global_admin")
                continue;

            userDtos.Add(new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = userRole
            });
        }
        return userDtos;
    }

    public async Task<UserDTO?> GetUserByIdAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null || user.DeletedDateTime != null)
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDTO
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AddedDateTime = user.AddedDateTime,
            ModifiedDateTime = user.ModifiedDateTime,
            Role = roles.FirstOrDefault()
        };
    }

    public async Task<UserDTO> CreateUserAsync(UserDTO userDto, ClaimsPrincipal currentUser)
    {
        if (!_authorizationService.CanCreateUsers(currentUser))
        {
            throw new UnauthorizedAccessException("You don't have permission to create users.");
        }

        if (userDto == null || string.IsNullOrWhiteSpace(userDto.Email) || string.IsNullOrWhiteSpace(userDto.Password))
        {
            throw new ArgumentException("User data, email, and password are required for creation.");
        }

        var newUser = new User
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName
        };

        var result = await _userManager.CreateAsync(newUser, userDto.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        var roleToAssign = !string.IsNullOrWhiteSpace(userDto.Role) ? userDto.Role : "operator";
        await _userManager.AddToRoleAsync(newUser, roleToAssign);

        userDto.Id = newUser.Id;
        userDto.Password = null;
        return userDto;
    }

    public async Task<UserDTO> UpdateUserAsync(UserDTO userDto, ClaimsPrincipal currentUser)
    {
        if (!_authorizationService.CanEditUser(currentUser, userDto.Id))
        {
            throw new UnauthorizedAccessException("You don't have permission to edit this user.");
        }

        var existingUser = await _userManager.FindByIdAsync(userDto.Id.ToString());
        if (existingUser == null)
        {
            throw new Exception("User not found.");
        }

        existingUser.FirstName = userDto.FirstName;
        existingUser.LastName = userDto.LastName;

        if (_authorizationService.IsGlobalAdmin(currentUser))
        {
            existingUser.Email = userDto.Email;
            existingUser.UserName = userDto.Email;
        }
        
        existingUser.ModifiedDateTime = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(existingUser);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to update user details.");
        }

        if (_authorizationService.CanChangeUserRole(currentUser) && !string.IsNullOrWhiteSpace(userDto.Role))
        {
            var roleToAssign = userDto.Role;
            
            if (!_authorizationService.CanAssignRole(currentUser, roleToAssign))
            {
                throw new UnauthorizedAccessException($"You don't have permission to assign the '{roleToAssign}' role.");
            }
            
            var currentRoles = await _userManager.GetRolesAsync(existingUser);

            if (currentRoles.Contains("global_admin") && !_authorizationService.IsGlobalAdmin(currentUser))
            {
                throw new UnauthorizedAccessException("You don't have permission to modify global admin users.");
            }

            if (!currentRoles.Contains(roleToAssign))
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
                if (!removeResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to remove existing user roles.");
                }

                var addResult = await _userManager.AddToRoleAsync(existingUser, roleToAssign);
                if (!addResult.Succeeded)
                {
                    throw new InvalidOperationException("Failed to add new user role.");
                }
            }
        }

        return userDto;
    }

    public async Task DeleteUserAsync(Guid id, ClaimsPrincipal currentUser)
    {
        if (!_authorizationService.CanDeleteUser(currentUser, id))
        {
            throw new UnauthorizedAccessException("You don't have permission to delete this user.");
        }

        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            user.DeletedDateTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
}
