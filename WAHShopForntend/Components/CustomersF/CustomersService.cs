using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WAHShopForntend.Components.Models;
namespace WAHShopForntend.Components.CustomersF
{
    public class CustomersService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<ValidationResult> Login(CustomerLoginDto customerLoginDto)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Customers/customerLogin", customerLoginDto);
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return result1 ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                }
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                return result ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" };
            }
        }
        public async Task<Customers> GetCustomerByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/Customers/getCustomerById/{id}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var customer = await response.Content.ReadFromJsonAsync<Customers>();
                if (customer != null)
                {
                    return customer;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public int? GetIdentity(string token)
        {
            var handler = new JwtSecurityTokenHandler();

            try
            {
                var jwtToken = handler.ReadJwtToken(token);

                // Ablaufdatum prüfen
                var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp")?.Value;
                if (expClaim != null && long.TryParse(expClaim, out long exp))
                {
                    var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                    if (DateTime.UtcNow > expDate)
                    {
                        // Token abgelaufen
                        return null;
                    }
                }
                else
                {
                    // Wenn kein Ablaufdatum für den Anspruch existiert, gilt das Token als abgelaufen.
                    return null;
                }

                // Kunden-ID extrahieren
                var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out int customerId))
                {
                    return customerId;
                }

                return null;
            }
            catch
            {
                // Ungültiges Token
                return null;
            }
        }
    }
}
