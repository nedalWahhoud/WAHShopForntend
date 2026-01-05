using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;

namespace WAHShopForntend.Components.CookieF
{
    public class CookieService(IJSRuntime js)
    {
        private readonly IJSRuntime _js = js;
        private const string ConsentCookieName = "cookieConsent";
        private const int ExpireDays = 365; // Gültigkeitsdauer des Cookies in Tagen  

        // Cookie zustimmung  
        public async Task SetConsentAsync(bool accepted)
        {
            try
            {
                var value = accepted ? "1" : "0";
                await _js.InvokeVoidAsync(
                    "cookieHelper.set",
                    ConsentCookieName,
                    value,
                    ExpireDays
                );

                await HideBannerForSessionAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error setting consent cookie: {ex.Message}");
            }
        }
        public async Task<bool?> GetConsentAsync()
        {
            try
            {
                var value = await _js.InvokeAsync<string>(
                    "cookieHelper.get",
                    ConsentCookieName
                );

                if (string.IsNullOrEmpty(value))
                    return null;

                return value == "1";
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error setting consent cookie: {ex.Message}");
                return null;
            }
        }
        public async Task DeleteConsentAsync()
        {
            try
            {
                await _js.InvokeVoidAsync("cookieHelper.delete", ConsentCookieName);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error deleting consent cookie: {ex.Message}");
            }
        }
        // schnelle Überprüfung, ob der Benutzer zugestimmt hat  
        public async Task<bool> HasAcceptedAsync()
        {
            try
            {
                var consent = await GetConsentAsync();
                return consent.HasValue && consent.Value;
            }
            catch
            {
                return false;
            }
        }
        public async Task HideBannerForSessionAsync()
        {
            try
            {
                // Wir haben einen speziellen Wert festgelegt, der angibt, dass das Band in der aktuellen Sitzung ausgeblendet werden soll.  
                await _js.InvokeVoidAsync("sessionStorage.setItem", "hideCookieBanner", "1");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting banner session: {ex.Message}");
            }
        }
        public async Task<bool> GetBannerSessionAsync()
        {
            try
            {
                var hide = await _js.InvokeAsync<string>("sessionStorage.getItem", "hideCookieBanner");
                bool shouldShow = hide != "1";
                return shouldShow;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting banner session: {ex.Message}");
                return true;
            }
        }
        // Sprache im Cookie speichern  
        public async Task<bool> SetLanguageAsync(string culture, bool skipAlreadySetCheck)
        {
            try
            {
                // Überprüfen, ob die Kultur bereits gesetzt wurde in dieser Sitzung  
                string cultureSet = await _js.InvokeAsync<string>("sessionStorage.getItem", "cultureSet");
                if (skipAlreadySetCheck == true || (cultureSet == null || cultureSet == "false"))
                {
                    if (!string.IsNullOrEmpty(culture))
                    {
                        // set culture in cookie  
                        await _js.InvokeVoidAsync("blazorCulture.set", culture);

                        var cultureInfo = new CultureInfo(culture);
                        CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
                        CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
                        CultureInfo.CurrentCulture = cultureInfo;
                        CultureInfo.CurrentUICulture = cultureInfo;
                        // setze session storage item um zu verhindern, dass die culture mehrmals gesetzt wird  
                        await _js.InvokeVoidAsync("sessionStorage.setItem", "cultureSet", "true");
                        // soll reloaden um die sprache zu ändern  
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting language cookie: {ex.Message}");
                return false;
            }
        }
        public async Task<string> GetLanguageCookiesync()
        {
            try
            {
                var culture = await _js.InvokeAsync<string>("blazorCulture.get");
                return culture;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting language cookie: {ex.Message}");
                return null!;
            }
        }
    }
}
