namespace WAHShopForntend.Components.Login
{
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Components.Authorization;
    using WAHShopForntend.Components.FavoriteF;
    using WAHShopForntend.Components.Models;

    public class AuthService(HttpClient http, AuthenticationStateProvider authStateProvider, FavoriteService favoriteService)
    {
        private readonly HttpClient _http = http;
        private readonly AuthenticationStateProvider _authStateProvider = authStateProvider;
        private readonly FavoriteService _favoriteService = favoriteService;

        public async Task<ValidationResult> Signup(SignupModel signupModel)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Users/signup", signupModel);

                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Registrierung fehlgeschlagen" };
                }


                return result;
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
                var result = await response.Content.ReadFromJsonAsync<LoginModel>();

                if (result == null || string.IsNullOrEmpty(result.Token))
                    return new ValidationResult { Result = false, Message = "Einloggen fehlgeschlagen" };

                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result.Token);

                // Save token to localStorage
                if (loginModel.RememberMe)
                    (_authStateProvider as CustomAuthStateProvider)?.LocalstorageSet("authToken", result.Token);
                else
                    (_authStateProvider as CustomAuthStateProvider)?.SessionStorageSet("authToken", result.Token);

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
                // remove Favorite from localStorage
                await _favoriteService.ClearLocalStorage();
            }
            _http!.DefaultRequestHeaders.Authorization = null;
        }
        public async Task<LoginModel> GetItemsUsersAsync(int id)
        {
            try
            {
                HttpResponseMessage response = await _http!.GetAsync($"api/Users/getUserById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var result = await response.Content.ReadFromJsonAsync<LoginModel>();
                return result ?? null!;
            }
            catch
            {
                return null!;
            }
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
                var result1 = await response.Content.ReadFromJsonAsync<LoginModel>();

                if (result1 == null || string.IsNullOrEmpty(result1.Token))
                    return new ValidationResult { Result = false, Message = "Token Error" };

                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(result1.Token);

                string localToken = await (_authStateProvider as CustomAuthStateProvider)?.LocalstorageGet("authToken")!;

                // update die Token in localStorage or sessionStorage
                if (!string.IsNullOrEmpty(localToken))
                    (_authStateProvider as CustomAuthStateProvider)?.LocalstorageSet("authToken", result1.Token);
                else
                    (_authStateProvider as CustomAuthStateProvider)?.SessionStorageSet("authToken", result1.Token);



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
        public async Task<User> GetUser()
        {
            User userModel = new();

            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier) ?? user.FindFirst("sub");

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    userModel.Id = userId;
                }
                userModel.SignupProvider = user.FindFirst(c => c.Type == "SignupProvider")?.Value!;
            }

            return userModel; 
        }
        // google login
        public ValidationResult GoogleLogin(LoginModel loginModel)
        {
            try
            {
                (_authStateProvider as CustomAuthStateProvider)?.NotifyUserAuthentication(loginModel.Token);

                // Save token to localStorage
                if (loginModel.RememberMe)
                    (_authStateProvider as CustomAuthStateProvider)?.LocalstorageSet("authToken", loginModel.Token);
                else
                    (_authStateProvider as CustomAuthStateProvider)?.SessionStorageSet("authToken", loginModel.Token);

                return new ValidationResult { Result = true, Message = "erfolgreich eingeloggt" };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
    }
}
