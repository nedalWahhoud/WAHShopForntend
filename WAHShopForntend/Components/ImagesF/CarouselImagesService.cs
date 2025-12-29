using Microsoft.Extensions.Options;
using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.ImagesF
{
    public class CarouselImagesService(HttpClient http, IOptions<AppConfig> appConfig, IWebHostEnvironment env)
    {
        public List<CarouselImage> DownloadedCarouselImage { get; private set; } = [];
        private readonly HttpClient _http = http;
        private readonly IOptions<AppConfig> _appConfig = appConfig;
        private readonly IWebHostEnvironment _env = env;

        public string GetImageUrl(CarouselImage carouselImage)
        {
            if (carouselImage == null)
                return "/images/sample.jpg";

            if (carouselImage.ImageUrl != null)
            {
                string dbImageUrl = carouselImage.ImageUrl.TrimStart('/');
                // ✅ Füge eine Zufallszahl hinzu, um Cash zu vermeiden.
                string unique = $"?v={carouselImage.LastModified}";
                //
                if (_env.IsDevelopment())
                {
                    string baseUri = _appConfig.Value.ApiUri.ToString().TrimEnd('/');
                    string path = _appConfig.Value.WebRequestProductImagePath.Trim('/');

                    string completteUrl = $"{baseUri}/{path}/{dbImageUrl}{unique}";
                    return completteUrl;
                }
                else
                {
                    if (dbImageUrl.StartsWith("CarouselImages/", StringComparison.OrdinalIgnoreCase))
                    {
                        dbImageUrl = dbImageUrl["CarouselImages/".Length..];
                    }
                    string domin = _appConfig.Value.Domin.TrimEnd('/');

                    string completteUrl = $"{domin}/{_appConfig.Value.CarouselImagesproxy}/{dbImageUrl}{unique}";
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
        // async
        public async Task<List<CarouselImage>> GetAllCarouselAsync()
        {
            if (DownloadedCarouselImage.Count > 0)
                return DownloadedCarouselImage;

            try
            {
                var response = await _http.GetAsync("api/Carousel/getAllCarouselImages");
                if (!response.IsSuccessStatusCode)
                {
                    return [];
                }
                var carouselImages = await response.Content.ReadFromJsonAsync<List<CarouselImage>>();
                if (carouselImages == null)
                {
                    return [];
                }

                // add to local list
                AddProductToLocal(carouselImages);

                return carouselImages;
            }
            catch
            {
                return [];
            }
        }

        // local
        public void AddProductToLocal(CarouselImage carouselImage)
        {
            if (!DownloadedCarouselImage.Any(p => p.Id == carouselImage.Id))
            {
                DownloadedCarouselImage.Add(carouselImage);
            }
        }
        public void AddProductToLocal(List<CarouselImage> carouselImage)
        {
            if (carouselImage.Count > 0 && DownloadedCarouselImage.Count == 0)
            {
                DownloadedCarouselImage.AddRange(carouselImage);
                return;
            }
            foreach (var product in carouselImage)
            {
                if (!DownloadedCarouselImage.Any(p => p.Id == product.Id))
                {
                    DownloadedCarouselImage.Add(product);
                }
            }
        }
    }
}
