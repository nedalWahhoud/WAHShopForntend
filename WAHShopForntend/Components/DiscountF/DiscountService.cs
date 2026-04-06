using WAHShopForntend.Components.Models;
namespace WAHShopForntend.Components.DiscountF
{
    public class DiscountService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<(ValidationResult validationResult, DiscountCodes discountCodes)> CheckDiscountCodeAsync(string code,int userId)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCode/{code}/{userId}");
                if (!response.IsSuccessStatusCode)
                {
                    var result1 = await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unbekannter Fehler." };
                    return (result1, null!);
                }

                DiscountCodes discountCode = await response.Content.ReadFromJsonAsync<DiscountCodes>() ?? null!;

                return (new ValidationResult { Result = true, Message = "Code erfolgreich überprüft." }, discountCode);

            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = $"Fehler: {ex.Message}" }, null!);
            }
        }
        public async Task<(ValidationResult validationResult, DiscountCategory discountCategory)> CheckDiscountCategoryAsync(string code, int userId)
        {
            if (string.IsNullOrWhiteSpace(code) || code.Length < 8)
            {
                return (new ValidationResult { Result = false, Message = "Der Code muss genau 8 Zeichen lang sein." }, null!);
            }
            try
            {
                var response = await _http.GetAsync($"api/Discounts/checkDiscountCategory/{code}/{userId}");
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
