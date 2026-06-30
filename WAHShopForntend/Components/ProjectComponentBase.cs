using Microsoft.AspNetCore.Components;
using System.Globalization;

namespace WAHShopForntend.Components
{
    public class ProjectComponentBase : ComponentBase
    {
        protected (bool Initialized, bool ParametersSet, bool AfterRender) IsRendered;
        protected bool IsArabic =>
         CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar";
        protected override void OnInitialized()
        {
            base.OnInitialized();
        }
        public static string GenerateSlug(string phrase)
        {
            if (string.IsNullOrEmpty(phrase)) return "";

            string str = phrase.ToLowerInvariant();
            str = str.Replace("ä", "a")
             .Replace("ö", "o")
             .Replace("ü", "u")
             .Replace("ß", "ss");

            // Ersetzen Sie Leerzeichen und Symbole durch einen Bindestrich -
            str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "-");
            // Unerwünschte Symbole entfernen (unter Beibehaltung arabischer und lateinischer Zeichen)
            str = System.Text.RegularExpressions.Regex.Replace(str, @"[^a-z0-9\u0600-\u06FF\-]", "");
            return str.Trim('-');
        }
    }
}
