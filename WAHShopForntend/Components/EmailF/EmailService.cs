using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.EmailF
{
    public class EmailService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<ValidationResult> ForgotpasswordSendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Emails/sendMail", emailRequest);
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
        public async Task<ValidationResult> EmailPasswordRestAsync(ForgotPassword forgotPassword)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Emails/resetPassword", forgotPassword);
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
    }
}
