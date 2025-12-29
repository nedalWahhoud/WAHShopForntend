using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.Login
{
    public class CustomAuthStateProvider(IJSRuntime js) : AuthenticationStateProvider
    {
        private readonly IJSRuntime _js = js;

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            try
            {
                string token = null!;
                ClaimsPrincipal user;
                string localToken = await LocalstorageGet("authToken");
                string sessionToken = await SessionStorageGet("authToken");

                if (localToken == null && sessionToken == null)
                {
                    // User is not logged
                    user = new ClaimsPrincipal(new ClaimsIdentity());
                    return new AuthenticationState(user);
                }
                else
                {
                    token = localToken ?? sessionToken;
                }

                var identity = GetIdentity(token);
                NotifyUserAuthentication(token);

                user = new ClaimsPrincipal(identity);
                return new AuthenticationState(user);

            }
            catch
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }
        public void NotifyUserAuthentication(string token)
        {
            var identity = GetIdentity(token);
       
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(identity))));
        }
        public async Task NotifyUserLogout()
        {
            // Remove token from local storage
            await LocalstorageRemove("authToken");
            // remove token from session storage
            await SessionStorageRemove("authToken");

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
        }
        public void LocalstorageSet(string key, string value)
        {
            _ = _js.InvokeVoidAsync("localStorage.setItem", key, value);
        }
        public async Task<string> LocalstorageGet(string key)
        {
            string value = await _js.InvokeAsync<string>("localStorage.getItem", key);
            return value;
        }
        private async Task LocalstorageRemove(string key)
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
        }
        public async Task SessionStorageSet(string key, string value)
        {
            await _js.InvokeVoidAsync("sessionStorage.setItem", key, value);
        }

        public async Task<string> SessionStorageGet(string key)
        {
            return await _js.InvokeAsync<string>("sessionStorage.getItem", key);
        }

        public async Task SessionStorageRemove(string key)
        {
            await _js.InvokeVoidAsync("sessionStorage.removeItem", key);
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
