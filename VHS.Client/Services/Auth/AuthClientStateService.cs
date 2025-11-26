using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace VHS.Client.Services.Auth
{
    public class AuthClientStateService : AuthenticationStateProvider
    {
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;
        private IAuthorizationClientService? _authorizationClientService;

        public AuthClientStateService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
        
        public void SetAuthorizationService(IAuthorizationClientService authorizationClientService)
        {
            _authorizationClientService = authorizationClientService;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("VHS_AUTH_TOKEN");
            if (string.IsNullOrWhiteSpace(token))
                return _anonymous;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo < DateTime.UtcNow)
                {
                    await MarkUserAsLoggedOutAsync();
                    return _anonymous;
                }

                var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
                var user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);
            }
            catch
            {
                await MarkUserAsLoggedOutAsync();
                return _anonymous;
            }
        }

        public async Task MarkUserAsAuthenticatedAsync(string token)
        {
            await _localStorage.SetItemAsync("VHS_AUTH_TOKEN", token);

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task MarkUserAsLoggedOutAsync()
        {
            await _localStorage.RemoveItemAsync("VHS_AUTH_TOKEN");
            await _localStorage.RemoveItemAsync("VHS_USER");

            if (_authorizationClientService != null)
            {
                await _authorizationClientService.ClearCacheAsync();
            }

            NotifyAuthenticationStateChanged(Task.FromResult(_anonymous));
        }
    }
}
