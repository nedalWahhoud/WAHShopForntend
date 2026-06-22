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
    }
}
