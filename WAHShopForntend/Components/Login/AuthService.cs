namespace WAHShopForntend.Components.Login
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;
    using Microsoft.JSInterop;
    using WAHShopForntend.Components.Models;


    public class AuthService
    {
        private readonly HttpClient? _http;
        private readonly AuthenticationStateProvider _authStateProvider;
        public AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _authStateProvider = authStateProvider;
        }

        public async Task<bool> Login(LoginModel loginModel, HttpResponseMessage signupResponse)
        {
            try
            {
                HttpResponseMessage response;

                if (signupResponse == null)
                    response = await _http!.PostAsJsonAsync("api/Users/login", loginModel);
                else
                    response = signupResponse;

                if (!response.IsSuccessStatusCode)
                    return false;
                // get result
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                    return false;

                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result.Token);

                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task Logout()
        {
            if (_authStateProvider is CustomAuthStateProvider customAuthStateProvider)
            {
                await customAuthStateProvider.NotifyUserLogout();
            }
            _http!.DefaultRequestHeaders.Authorization = null;
        }
        public async Task<ValidationResult> UpdateProfileAsync(UpdateProfile updateProfile)
        {
            try
            {
                var response = await _http!.PutAsJsonAsync("api/Users/update", updateProfile);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null || !result.Result)
                    return new ValidationResult { Result = false, Message = "Unknown error." };

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> AddGuestAsync(User user)
        {
            try
            {
                var response = await _http!.PostAsJsonAsync("api/Users/addGuest", user);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null)
                    return new ValidationResult { Result = false, Message = "Unknown error." };
                
                return result;
            }
            catch
            {
                return null!;
            }
        }
    }
}
