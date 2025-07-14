using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VHS.Data.Auth.Models.Auth;
using VHS.Services.Auth.DTO;

namespace VHS.Services;

public interface IUserService
{
    Task<IEnumerable<UserDTO>> GetAllUsersAsync();
    Task<UserDTO> CreateUserAsync(UserDTO userDto);
    Task<UserDTO> UpdateUserAsync(UserDTO userDto);
    Task<UserDTO?> GetUserByIdAsync(Guid id);
    Task DeleteUserAsync(Guid id);
}

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.Where(u => u.DeletedDateTime == null).ToListAsync();
        var userDtos = new List<UserDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault()
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

    public async Task<UserDTO> CreateUserAsync(UserDTO userDto)
    {
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

        var roleToAssign = !string.IsNullOrWhiteSpace(userDto.Role) ? userDto.Role : "user";
        await _userManager.AddToRoleAsync(newUser, roleToAssign);

        userDto.Id = newUser.Id;
        userDto.Password = null;
        return userDto;
    }

    public async Task<UserDTO> UpdateUserAsync(UserDTO userDto)
    {
        var existingUser = await _userManager.FindByIdAsync(userDto.Id.ToString());
        if (existingUser == null)
        {
            throw new Exception("User not found.");
        }

        existingUser.FirstName = userDto.FirstName;
        existingUser.LastName = userDto.LastName;
        existingUser.Email = userDto.Email;
        existingUser.UserName = userDto.Email;
        existingUser.ModifiedDateTime = DateTime.UtcNow;

        var updateResult = await _userManager.UpdateAsync(existingUser);
        if (!updateResult.Succeeded)
        {
            throw new InvalidOperationException("Failed to update user details.");
        }

        var currentRoles = await _userManager.GetRolesAsync(existingUser);
        var roleToAssign = !string.IsNullOrWhiteSpace(userDto.Role) ? userDto.Role : "user";

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

        return userDto;
    }

    public async Task DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user != null)
        {
            user.DeletedDateTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
        }
    }
}
