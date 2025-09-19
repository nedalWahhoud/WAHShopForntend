namespace WAHShopForntend.Components.Login
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;
    using WAHShopForntend.Components.Models;

    public class AuthService(HttpClient http, AuthenticationStateProvider authStateProvider)
    {
        private readonly HttpClient _http = http;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;

        public async Task<ValidationResult> Signup(SignupModel signupModel)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Users/signup", signupModel);

                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Registrierung fehlgeschlagen" };
                }

                return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Registrierung fehlgeschlagen" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }

        public async Task<ValidationResult> Login(LoginModel loginModel, HttpResponseMessage signupResponse)
        {
            try
            {
                HttpResponseMessage response;

                if (signupResponse == null)
                    response = await _http!.PostAsJsonAsync("api/Users/login", loginModel);
                else
                    response = signupResponse;

                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Einloggen fehlgeschlagen" };
                // get result token
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                    return new ValidationResult { Result = false, Message = "Einloggen fehlgeschlagen" };

                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result.Token);

                return new ValidationResult { Result = true, Message = "erfolgreich eingeloggt" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
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

                // get result token
                var result1 = await response.Content.ReadFromJsonAsync<LoginResponse>();

                if (result1 == null || string.IsNullOrEmpty(result1.Token))
                    return new ValidationResult { Result = false, Message = "Token Error" };

                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result1.Token);

                return new ValidationResult { Result = true, Message = "erfolgreich Userdata geupdatet" };
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
        public async Task<ValidationResult> UserActivate(ActivateRequest activateRequest)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Users/userActivate", activateRequest);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannt fehler." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (result == null)
                    return new ValidationResult { Result = false, Message = "Unbekannt fehler." };
                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> AccountDeleteAsync(UpdateProfile updateProfile)
        {
            try
            {
                var checkPassword = await _http!.GetAsync($"api/Users/checkPassword?userId={updateProfile.UserId}&password={updateProfile.OldPassword}");
                if (!checkPassword.IsSuccessStatusCode)
                {
                    return await checkPassword.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannt fehler." };
                }
                var checkPasswordResult = await checkPassword.Content.ReadFromJsonAsync<ValidationResult>();
                if (checkPasswordResult != null && checkPasswordResult.Result)
                {
                    var response = await _http!.DeleteAsync($"api/Users/accountDelete/{updateProfile.UserId}");
                    if (!response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannt fehler." };
                    }
                    var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    if (result == null)
                        return new ValidationResult { Result = false, Message = "Unbekannt fehler." };
                    return result;
                }
                else
                    return new ValidationResult { Result = false, Message = checkPasswordResult?.Message ?? "Unbekannt fehler." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
