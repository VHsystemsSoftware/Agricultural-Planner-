using System.Net.Http.Json;
using VHS.Services.Auth.DTO;

namespace VHS.Client.Services.Auth;

public interface IAuthorizationClientService
{
    Task<bool> IsGlobalAdminAsync();
    Task<bool> CanViewUsersAsync();
    Task<bool> CanCreateUsersAsync();
    Task<bool> CanEditUsersAsync();
    Task<bool> CanDeleteUsersAsync();
    Task<bool> CanChangeUserRoleAsync();
    Task<string> GetCurrentUserRoleAsync();
    Task<Guid?> GetCurrentUserIdAsync();
    Task<bool> CanDeleteSpecificUserAsync(Guid targetUserId);
    Task<bool> CanManageFarmsAsync();
    Task<bool> CanDefineRacksAndLayersAsync();
    Task<bool> CanAccessPlanningOperationsAsync();
    Task<bool> CanAccessOverviewOperationsAsync();
    Task<bool> CanAccessSystemSettingsAsync();
    Task ClearCacheAsync();
}

public class AuthorizationClientService : IAuthorizationClientService
{
    private readonly HttpClient _httpClient;
    private UserAuthorizationDTO? _cachedPermissions;
    private DateTime? _cacheExpiry;
    private readonly TimeSpan _cacheTimeout = TimeSpan.FromMinutes(5);

    public AuthorizationClientService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private async Task<UserAuthorizationDTO> GetPermissionsAsync()
    {
        if (_cachedPermissions != null && _cacheExpiry.HasValue && DateTime.UtcNow < _cacheExpiry.Value)
        {
            return _cachedPermissions;
        }

        try
        {            
            var response = await _httpClient.GetFromJsonAsync<UserAuthorizationDTO>("api/auth/permissions");
            if (response != null)
            {
                _cachedPermissions = response;
                _cacheExpiry = DateTime.UtcNow.Add(_cacheTimeout);
                return response;
            }
        }
        catch (Exception ex)
        {
            
        }

        var defaultPermissions = new UserAuthorizationDTO
        {
            IsGlobalAdmin = false,
            CanViewUsers = false,
            CanCreateUsers = false,
            CanEditUsers = false,
            CanDeleteUsers = false,
            CanChangeUserRole = false,
            CurrentUserRole = "operator",
            CurrentUserId = null
        };

        return defaultPermissions;
    }

    public async Task<bool> IsGlobalAdminAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.IsGlobalAdmin;
    }

    public async Task<bool> CanViewUsersAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CanViewUsers;
    }

    public async Task<bool> CanCreateUsersAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CanCreateUsers;
    }

    public async Task<bool> CanEditUsersAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CanEditUsers;
    }

    public async Task<bool> CanDeleteUsersAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CanDeleteUsers;
    }

    public async Task<bool> CanChangeUserRoleAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CanChangeUserRole;
    }

    public async Task<string> GetCurrentUserRoleAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CurrentUserRole;
    }

    public async Task<Guid?> GetCurrentUserIdAsync()
    {
        var permissions = await GetPermissionsAsync();
        return permissions.CurrentUserId;
    }

    public async Task<bool> CanDeleteSpecificUserAsync(Guid targetUserId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<CanDeleteResponse>($"api/auth/can-delete-user/{targetUserId}");
            return response?.CanDelete ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CanManageFarmsAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "global_admin" || role == "admin";
    }

    public async Task<bool> CanDefineRacksAndLayersAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "global_admin" || role == "admin";
    }

    public async Task<bool> CanAccessPlanningOperationsAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "global_admin" || role == "admin" || role == "farm_manager";
    }

    public async Task<bool> CanAccessOverviewOperationsAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "global_admin" || role == "admin" || role == "farm_manager";
    }

    public async Task<bool> CanAccessSystemSettingsAsync()
    {
        var role = await GetCurrentUserRoleAsync();
        return role == "global_admin" || role == "admin";
    }

    public Task ClearCacheAsync()
    {
        _cachedPermissions = null;
        _cacheExpiry = null;
        return Task.CompletedTask;
    }
}

public class CanDeleteResponse
{
    public bool CanDelete { get; set; }
}
