using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.ProductGroupF
{
    public class ProductGroupService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<List<Product>> GetProductsByGroupProductsIdAsync(int groupProductsId, List<int>? excludeProductsIds = null)
        {
            try
            {
                String queryString = string.Empty;
                if (excludeProductsIds != null && excludeProductsIds.Count != 0)
                {
                    queryString += string.Join("&", excludeProductsIds.Select(id => $"excludeProductsIds={id}"));
                }
                queryString = $"?" + queryString;
                var response = await _http.GetAsync($"api/GroupProducts/getProductsByGroupProductsId/{groupProductsId}{queryString}");
                if (!response.IsSuccessStatusCode)
                    return [];

                GetItems<Product> getItems = new();

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? new GetItems<Product>();
                return getItems.Items;
            }
            catch
            {
                return [];
            }
        }
        public async Task<GroupProducts> GetGroupProductByIdAsync(int groupProductsId)
        {
            try
            {
                var response = await _http.GetAsync($"api/GroupProducts/getGroupProductById/{groupProductsId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var groupProduct = await response.Content.ReadFromJsonAsync<GroupProducts>();
                return groupProduct ?? null!;
            }
            catch
            {
                return null!;
            }
        }
    }
}
