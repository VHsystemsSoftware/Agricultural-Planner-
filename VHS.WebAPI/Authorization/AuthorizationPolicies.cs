using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace VHS.WebAPI.Authorization;

public static class AuthorizationPolicies
{
    // Policy Names
    public const string GlobalAdminOnly = "GlobalAdminOnly";
    public const string AdminAndAbove = "AdminAndAbove";
    public const string FarmManagerAndAbove = "FarmManagerAndAbove";
    public const string AllAuthenticated = "AllAuthenticated";
    
    // Specific Function Policies
    public const string CanManageUsers = "CanManageUsers";
    public const string CanManageFarms = "CanManageFarms";
    public const string CanDefineRacksAndLayers = "CanDefineRacksAndLayers";
    public const string CanAccessPlanningOperations = "CanAccessPlanningOperations";
    public const string CanAccessOverviewOperations = "CanAccessOverviewOperations";
    public const string CanAccessSystemSettings = "CanAccessSystemSettings";
    public const string CanViewDashboards = "CanViewDashboards";

    public static void AddPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // Role-based policies
            options.AddPolicy(GlobalAdminOnly, policy => 
                policy.RequireRole("global_admin"));
            
            options.AddPolicy(AdminAndAbove, policy => 
                policy.RequireRole("global_admin", "admin"));
            
            options.AddPolicy(FarmManagerAndAbove, policy => 
                policy.RequireRole("global_admin", "admin", "farm_manager"));
            
            options.AddPolicy(AllAuthenticated, policy => 
                policy.RequireAuthenticatedUser());

            // Function-specific policies
            options.AddPolicy(CanManageUsers, policy =>
                policy.RequireRole("global_admin", "admin"));

            options.AddPolicy(CanManageFarms, policy =>
                policy.RequireRole("global_admin", "admin"));

            options.AddPolicy(CanDefineRacksAndLayers, policy =>
                policy.RequireRole("global_admin", "admin"));

            options.AddPolicy(CanAccessPlanningOperations, policy =>
                policy.RequireRole("global_admin", "admin", "farm_manager"));

            options.AddPolicy(CanAccessOverviewOperations, policy =>
                policy.RequireRole("global_admin", "admin", "farm_manager"));

            options.AddPolicy(CanAccessSystemSettings, policy =>
                policy.RequireRole("global_admin", "admin"));

            options.AddPolicy(CanViewDashboards, policy =>
                policy.RequireRole("global_admin", "admin", "farm_manager", "operator"));
        });
    }
}

public static class RoleConstants
{
    public const string GlobalAdmin = "global_admin";
    public const string Admin = "admin";
    public const string FarmManager = "farm_manager";
    public const string Operator = "operator";
}
