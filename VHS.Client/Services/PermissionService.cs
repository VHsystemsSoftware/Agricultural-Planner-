using System.Net.Http.Json;
using VHS.Client.Services.Auth;
using VHS.Data.Models.Audit;

namespace VHS.Client.Services
{
    public class PermissionService
		{
				private readonly IAuthorizationClientService _authorizationService;
		
				public bool CanAccessPlanningOperations { get; private set; }
				public bool CanAccessOverviewOperations { get; private set; }
				public bool CanAccessSystemSettings { get; private set; }
		
				public bool CanViewUsers { get; private set; }
				public bool CanCreateUsers { get; private set; }
				public bool CanEditUsers { get; private set; }
				public bool CanDeleteUsers { get; private set; }
				public bool CanChangeUserRole { get; private set; }
				public bool IsGlobalAdmin { get; private set; }
				public Guid? CurrentUserId { get; private set; }

				public PermissionService(IAuthorizationClientService authorizationService)
				{
						_authorizationService = authorizationService;
				}

				public async Task CheckPermissions()
				{
						try
						{
								CanAccessPlanningOperations = await _authorizationService.CanAccessPlanningOperationsAsync();
								CanAccessOverviewOperations = await _authorizationService.CanAccessOverviewOperationsAsync();
								CanAccessSystemSettings = await _authorizationService.CanAccessSystemSettingsAsync();
				
								CanViewUsers = await _authorizationService.CanViewUsersAsync();
								CanCreateUsers = await _authorizationService.CanCreateUsersAsync();
								CanEditUsers = await _authorizationService.CanEditUsersAsync();
								CanDeleteUsers = await _authorizationService.CanDeleteUsersAsync();
								CanChangeUserRole = await _authorizationService.CanChangeUserRoleAsync();
								IsGlobalAdmin = await _authorizationService.IsGlobalAdminAsync();
								CurrentUserId = await _authorizationService.GetCurrentUserIdAsync();
						}
						catch (Exception ex)
						{
								ResetPermissions();
						}
				}
		
				public async Task<bool> CanEditSpecificUser(Guid userId)
				{
					try
					{
							var canEdit = await _authorizationService.CanEditUsersAsync();
							return canEdit && userId != CurrentUserId;
					}
					catch
					{
							return false;
					}
				}
		
				public async Task<bool> CanDeleteSpecificUser(Guid userId)
				{
						try
						{
								return await _authorizationService.CanDeleteSpecificUserAsync(userId);
						}
						catch
						{
								return false;
						}
				}
		
				private void ResetPermissions()
				{
						CanAccessPlanningOperations = false;
						CanAccessOverviewOperations = false;
						CanAccessSystemSettings = false;
						CanViewUsers = false;
						CanCreateUsers = false;
						CanEditUsers = false;
						CanDeleteUsers = false;
						CanChangeUserRole = false;
						IsGlobalAdmin = false;
						CurrentUserId = null;
				}
    }
}
