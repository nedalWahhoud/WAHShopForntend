using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;
using static System.Net.WebRequestMethods;

namespace WAHShopForntend.Components.ProductGroupF
{
    public class ProductGroupService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public async Task<List<Product>> GetProductsByGroupProductsIdAsynce(int groupProductsId, List<int>? excludeProductsIds = null)
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

                GetItems<Product> getItems = new ();

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? new GetItems<Product>();
                return getItems.Items;
            }
            catch
            {
                return [];
            }
        }
    }
}
