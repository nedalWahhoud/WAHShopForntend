using Microsoft.Extensions.Options;
using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.ImagesF
{
    public class ProductImagesService(IOptions<AppConfig> appConfig, IWebHostEnvironment env)
    {
        private readonly IOptions<AppConfig> _appConfig = appConfig;
        private readonly IWebHostEnvironment _env = env;

        public string GetProductImageUrl(ProductImages productImages)
        {
            string dbImageUrl = productImages.ImageUrl;
            if (dbImageUrl != null)
            {
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={productImages.LastModified}";

                dbImageUrl = dbImageUrl.TrimStart('/');
                if (_env.IsDevelopment())
                {
                    string baseUri = _appConfig.Value.ApiUri.ToString().TrimEnd('/');
                    string path = _appConfig.Value.WebRequestProductImagePath.Trim('/');

                    string completteUrl = $"{baseUri}/{path}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
                else
                {

                    if (dbImageUrl.StartsWith("ProductsImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["ProductsImages/".Length..];
                    }
                    string domin = _appConfig.Value.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{_appConfig.Value.ProductImagesproxy}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
            }
            else
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "DPImage.png");
                var relativePath = path.Split("wwwroot")[1].Replace("\\", "/");

                return relativePath;
            }
        }
    }
}
