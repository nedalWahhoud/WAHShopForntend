using WAHShopForntend.Components.Cart;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.Pages;
using static System.Net.WebRequestMethods;

namespace WAHShopForntend.Components.ProductsF
{
    public class ProductService(HttpClient http)
    {
        private readonly HttpClient _http = http;
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
                    AddProductToLocal(products);
                    return products;
                }
                return null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<Product> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Products/getProductById/{productId}");

                if (!response.IsSuccessStatusCode)
                    return null!;
                var product = await response.Content.ReadFromJsonAsync<Product>();
                if (product != null)
                {
                    // add the product to the local list
                    AddProductToLocal(product!);
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
        public void AddProductToLocal(List<Product> products)
        {
            if (products.Count > 0 && DownloadedProduct.Count == 0)
            {
                DownloadedProduct.AddRange(products);
                return;
            }

            foreach (var product in products)
            {
                if (!DownloadedProduct.Any(p => p.Id == product.Id))
                {
                    DownloadedProduct.Add(product);
                }
            }
        }
        public void AddProductToLocal(Product product)
        {
            if (!DownloadedProduct.Any(p => p.Id == product.Id))
            {
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
