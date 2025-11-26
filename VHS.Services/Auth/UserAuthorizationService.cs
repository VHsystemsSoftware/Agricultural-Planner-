using System.Security.Claims;

namespace VHS.Services.Auth;

public interface IUserAuthorizationService
{
    bool IsGlobalAdmin(ClaimsPrincipal user);
    bool CanViewUsers(ClaimsPrincipal user);
    bool CanCreateUsers(ClaimsPrincipal user);
    bool CanEditUser(ClaimsPrincipal currentUser, Guid targetUserId);
    bool CanDeleteUser(ClaimsPrincipal currentUser, Guid targetUserId);
    bool CanChangeUserRole(ClaimsPrincipal currentUser);
    bool CanAssignRole(ClaimsPrincipal currentUser, string roleToAssign);
    string GetCurrentUserRole(ClaimsPrincipal user);
    Guid? GetCurrentUserId(ClaimsPrincipal user);
    bool CanManageFarms(ClaimsPrincipal user);
    bool CanDefineRacksAndLayers(ClaimsPrincipal user);
    bool CanAccessPlanningOperations(ClaimsPrincipal user);
    bool CanAccessOverviewOperations(ClaimsPrincipal user);
    bool CanAccessSystemSettings(ClaimsPrincipal user);
}

public class UserAuthorizationService : IUserAuthorizationService
{
    private const string GlobalAdminRole = "global_admin";
    private const string AdminRole = "admin";
    private const string FarmManagerRole = "farm_manager";
    private const string OperatorRole = "operator";

    public bool IsGlobalAdmin(ClaimsPrincipal user)
    {
        return user.IsInRole(GlobalAdminRole);
    }

    public bool CanViewUsers(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole);
    }

    public bool CanCreateUsers(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user);
    }

    public bool CanEditUser(ClaimsPrincipal currentUser, Guid targetUserId)
    {
        return IsGlobalAdmin(currentUser) || currentUser.IsInRole(AdminRole);
    }

    public bool CanDeleteUser(ClaimsPrincipal currentUser, Guid targetUserId)
    {
        if (!IsGlobalAdmin(currentUser))
        {
            return false;
        }

        var currentUserId = GetCurrentUserId(currentUser);
        return currentUserId.HasValue && currentUserId.Value != targetUserId;
    }

    public bool CanChangeUserRole(ClaimsPrincipal currentUser)
    {
        return IsGlobalAdmin(currentUser) || currentUser.IsInRole(AdminRole);
    }

    public bool CanAssignRole(ClaimsPrincipal currentUser, string roleToAssign)
    {
        if (IsGlobalAdmin(currentUser))
        {
            return true;
        }
        
        if (currentUser.IsInRole(AdminRole))
        {
            return roleToAssign == AdminRole || roleToAssign == FarmManagerRole || 
                   roleToAssign == OperatorRole;
        }
        
        return false;
    }

    public string GetCurrentUserRole(ClaimsPrincipal user)
    {
        if (user.IsInRole(GlobalAdminRole))
            return GlobalAdminRole;
        if (user.IsInRole(AdminRole))
            return AdminRole;
        if (user.IsInRole(FarmManagerRole))
            return FarmManagerRole;
        if (user.IsInRole(OperatorRole))
            return OperatorRole;

        return OperatorRole;
    }

    public Guid? GetCurrentUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub") ?? user.FindFirst("id");
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    public bool CanManageFarms(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole);
    }

    public bool CanDefineRacksAndLayers(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole);
    }

    public bool CanAccessPlanningOperations(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole) || user.IsInRole(FarmManagerRole);
    }

    public bool CanAccessOverviewOperations(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole) || user.IsInRole(FarmManagerRole);
    }

    public bool CanAccessSystemSettings(ClaimsPrincipal user)
    {
        return IsGlobalAdmin(user) || user.IsInRole(AdminRole);
    }
}
