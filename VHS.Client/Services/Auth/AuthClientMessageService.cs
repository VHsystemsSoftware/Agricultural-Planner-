using Blazored.LocalStorage;
using System.Net.Http.Headers;

namespace VHS.Client.Services.Auth
{
    public class AuthClientMessageService : DelegatingHandler
    {
        private readonly ILocalStorageService _localStorage;

        public AuthClientMessageService(ILocalStorageService localStorage)
        {
            _localStorage = localStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _localStorage.GetItemAsync<string>("VHS_AUTH_TOKEN");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
