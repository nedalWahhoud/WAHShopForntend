using WAHShopForntend.Components.Models;
namespace WAHShopForntend.Components.DiscountF
{
    public class DiscountService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<(ValidationResult validationResult, DiscountCodes discountCodes)> CheckDiscountCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCode/{code}");
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return (result1 ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." }, null!);
                }

                DiscountCodes discountCode = await response.Content.ReadFromJsonAsync<DiscountCodes>() ?? null!;

                return (new ValidationResult { Result = true, Message = "Code erfolgreich überprüft." }, discountCode);

            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" }, null!);
            }
        }
        public async Task<(ValidationResult validationResult, DiscountCategory discountCategory)> CheckDiscountCategory(string code)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCategory/{code}");
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>();
                    return (result1 ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." }, null!);
                }
                DiscountCategory discountCategory = await response.Content.ReadFromJsonAsync<DiscountCategory>() ?? null!;

                return (new ValidationResult { Result = true, Message = "Code erfolgreich überprüft." }, discountCategory);
            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" }, null!);
            }

        }
    }
}
