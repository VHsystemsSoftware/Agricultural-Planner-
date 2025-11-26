using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.Net;
using System.Net.Http.Headers;

namespace VHS.Client.Services.Auth
{
    public class AuthClientMessageService : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;
        private readonly NavigationManager _navigationManager;

        public AuthClientMessageService(ILocalStorageService localStorage, NavigationManager navigationManager)
        {
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("VHS_AUTH_TOKEN");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                await _localStorage.RemoveItemAsync("VHS_AUTH_TOKEN");
                await _localStorage.RemoveItemAsync("VHS_USER");
                
                var currentUri = _navigationManager.Uri;
                var baseUri = _navigationManager.BaseUri;
                var relativePath = currentUri.Replace(baseUri, "").TrimStart('/');
                
                if (!relativePath.StartsWith("login", StringComparison.OrdinalIgnoreCase))
                {
                    _navigationManager.NavigateTo("/login", forceLoad: true);
                }
            }
            else if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                var currentUri = _navigationManager.Uri;
                var baseUri = _navigationManager.BaseUri;
                var relativePath = currentUri.Replace(baseUri, "").TrimStart('/');

                if (!relativePath.StartsWith("access-denied", StringComparison.OrdinalIgnoreCase) && 
                    !string.IsNullOrEmpty(relativePath) && relativePath != "/")
                {
                    _navigationManager.NavigateTo("/access-denied", forceLoad: true);
                }
            }

            return response;
        }
    }
}
