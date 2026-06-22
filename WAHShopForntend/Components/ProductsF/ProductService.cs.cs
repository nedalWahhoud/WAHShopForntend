using WAHShopForntend.Components.Cart;
using WAHShopForntend.Components.FavoriteF;
using WAHShopForntend.Components.Login;
using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.ProductsF
{
    public class ProductService(HttpClient http,AuthService authService, FavoriteService favoriteService)
    {
        private readonly HttpClient _http = http;
        private readonly AuthService _authService = authService;
        private readonly FavoriteService _favoriteService = favoriteService;
        public  List<Product> DownloadedProduct { get;  set; } = [];
        public async Task<List<Product>> GetProductByIdsAsync(List<int> productIds)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Products/getProductByIds", productIds);

                if (!response.IsSuccessStatusCode)
                    return null!;
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null)
                {
                    // add the product to the local list
                    await AddProductToLocal(products);
                    return products;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<Product> GetProductByIdAsync(int productId, int userId = 0)
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getProductById/{productId}?onlyInStock={true}&userId={userId}");

                if (!response.IsSuccessStatusCode)
                    return null!;
                var product = await response.Content.ReadFromJsonAsync<Product>();
                if (product != null)
                {
                    // add the product to the local list
                    await AddProductToLocal(product!);

                    return product!;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        // local
        public async Task AddProductToLocal(List<Product> products)
        {
            // check if eingeloggen
            int UserId = (await _authService.GetUser()).Id;

            foreach (var product in products)
            {
                if (!DownloadedProduct.Any(p => p.Id == product.Id))
                {
                    // wenn nicht eingeloggen dann, gucken wir ob in LocalStorage Favorite für diese Produkt gibt es
                    if(UserId == 0)
                    {
                        product.IsFavorite = await _favoriteService.IsFavoriteInLocalStorage(product.Id);
                    }
                    DownloadedProduct.Add(product);
                }
            }
        }
        public async Task AddProductToLocal(Product product)
        {
            // check if eingeloggen
            int UserId = (await _authService.GetUser()).Id;

            if (!DownloadedProduct.Any(p => p.Id == product.Id))
            {
                if (UserId == 0)
                {
                    product.IsFavorite = await _favoriteService.IsFavoriteInLocalStorage(product.Id);
                }
                DownloadedProduct.Add(product);
            }
        }
        public Product GetProductByIdLocal(int productId)
        {
            var product = DownloadedProduct.Find(p => p.Id == productId);
            if (product != null)
                return product;
            else
            {
                return null!;
            }
        }
        public List<Product> GetProductByGroupIdLocal(int groubProductId, List<int>? excludeProductsIds = null)
        {
            excludeProductsIds ??= [];
            try
            {
                return DownloadedProduct
                    .Where(p => p.ProductGroupID == groubProductId
                    && !excludeProductsIds.Contains(p.Id))
                    .ToList();
            }
            catch
            {
                return [];
            }

        }
    }
}
