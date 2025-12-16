using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;

namespace WAHShopForntend.Components.SearchF
{
    public class SearchService
    {
        private readonly HttpClient _http ;
        private readonly ProductService _productService;
        public SearchService(HttpClient http, ProductService productService)
        {
            _http = http;
            _productService = productService;
        }
        public async Task<List<Product>> SearchProductsAsync(string searchText, List<int> excludeIds)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return [];
            try
            {
                var DownloadedProductExistingIds = new HashSet<int>(_productService.DownloadedProduct.Select(p => p.Id));
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

                var response = await _http.GetAsync($"api/Search/searchproducts{query}");
                if (!response.IsSuccessStatusCode)
                    return [];
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null)
                {
                    // not active exclusion
                    products = products.Where(p => p.Category != null && p.Category.IsAktiv).ToList();
                    // add the product to the local list
                    _productService.AddProductToLocal(products);
                    return products;
                }
                return [];
            }
            catch
            {
                return [];
            }
        }
        public List<Product> SearchProductsLocal(string searchText, List<int> excludeIds)
        {
            if (string.IsNullOrEmpty(searchText))
                return [];
            try
            {
                return _productService.DownloadedProduct
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
    }
}
