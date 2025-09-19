using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.Login
{
    public class CustomAuthStateProvider: AuthenticationStateProvider
    {
        private readonly IJSRuntime _js;
        public CustomAuthStateProvider(IJSRuntime js)
        {
            _js = js;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                ClaimsPrincipal user;
                string token = await LocalstorageGet("authToken");
                if (string.IsNullOrWhiteSpace(token))
                {
                    // User is not logged
                    user = new ClaimsPrincipal(new ClaimsIdentity());
                }
                else
                {
                    var identity = GetIdentity(token);
                    NotifyUserAuthentication(token);

                    user = new ClaimsPrincipal(identity);
                }

                return new AuthenticationState(user);
            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        public void NotifyUserAuthentication(string token)
        {
            // Save token to localStorage
            LocalstorageSet("authToken", token);
            var identity = GetIdentity(token);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        }
        public async Task NotifyUserLogout()
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }
        private void LocalstorageSet(string key, string value)
        {
            _ = _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        private async Task<string> LocalstorageGet(string key)
        {
            string value = await _js.InvokeAsync<string>("localStorage.getItem", key);
            return value;
        }
        private ClaimsIdentity GetIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var claims = jwtToken.Claims
        .Select(c => new Claim(c.Type, c.Value))
        .ToList();

            var identity = new ClaimsIdentity(claims, "claim");

            return identity;
        }
    }
}
