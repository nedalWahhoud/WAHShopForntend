using WAHShopForntend.Components.Cart;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.Pages;

namespace WAHShopForntend.Components.ProductsF
{
    public class ProductService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        private static List<Product> DownloadedProduct { get; set; } = [];

        public async Task<List<Product>> GetProductByIdsServer(List<int> productIds)
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
        public async Task<Product> GetProductByIdsServer(int productId)
        {
            try
            {
                List<int> productIds = [productId];
                var response = await _http.PostAsJsonAsync("api/Products/getProductByIds", productIds);

                if (!response.IsSuccessStatusCode)
                    return null!;
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null && products.Count > 0)
                {
                    Product? product = products.FirstOrDefault();
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
        public async Task<List<Product>> SearchProductsAsync(string searchText, List<int> excludeIds)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return [];
            try
            {
                var DownloadedProductExistingIds = new HashSet<int>(DownloadedProduct.Select(p => p.Id));
                if (excludeIds != null && excludeIds.Count != 0)
                {
                    DownloadedProductExistingIds.UnionWith(excludeIds);
                }

                String query = $"?query={Uri.EscapeDataString(searchText.Trim())}";
                if (DownloadedProductExistingIds != null && DownloadedProductExistingIds.Count != 0)
                {
                    // تحويل القائمة إلى سلسلة مثل excludeCategoryIds=1&excludeCategoryIds=2
                    var excludedQueryPart = string.Join("&", DownloadedProductExistingIds.Select(id => $"excludeIds={id}"));
                    query += "&" + excludedQueryPart;
                }

                var response = await _http.GetAsync($"api/products/searchproducts{query}");
                if (!response.IsSuccessStatusCode)
                    return [];
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null)
                {
                    // add the product to the local list
                    AddProductToLocal(products);
                    return products;
                }
                return [];
            }
            catch
            {
                return [];
            }
        }
        // local
        public void AddProductToLocal(List<Product> products)
        {
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
        public Task<List<Product>> GetProductByCategoryIdLocal(int categoryId, List<int>? excludeProductsIds = null, int? excludeProductsId = null)
        {
            try
            {
                // initialize the excludeProductsIds and excludeProductsId if they are null
                excludeProductsIds ??= [];
                excludeProductsId ??= 0;

                return Task.FromResult(DownloadedProduct
                    .Where(p => p.CategoryId == categoryId
                    && !excludeProductsIds.Contains(p.Id)
                    && p.Id != excludeProductsId)
                    .ToList());
            }
            catch
            {
                return null!;
            }
        }
        public List<Product> SearchProductsLocal(string searchText, List<int> excludeIds)
        {
            if (string.IsNullOrEmpty(searchText))
                return [];
            try
            {
                return DownloadedProduct
               .Where(p =>
              !excludeIds.Contains(p.Id) && (
              (p.Name_de != null && p.Name_de.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
              (p.Description_de != null && p.Description_de.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
               (p.Name_ar != null && p.Name_ar.Contains(searchText.Trim(), StringComparison.OrdinalIgnoreCase)) ||
              (p.Description_ar != null && p.Description_ar.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
              (p.Category != null && p.Category.Name_de != null && p.Category.Name_de.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
              (p.Category != null && p.Category.Name_ar != null && p.Category.Name_ar.Contains(searchText, StringComparison.OrdinalIgnoreCase))
          )
      )
      .ToList();

            }
            catch
            {
                return [];
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
