using WAHShopForntend.Components.Models;

namespace WAHShopForntend.Components.CategoriesF
{
    public class CategoryService(HttpClient http)
    {
        private readonly HttpClient _http = http;
        public  List<Categories> DownloadedCategories { get; private set; } = [];

        public async Task<List<Categories>> GetCategories()
        {
            if(DownloadedCategories.Count > 0)
            {
                return DownloadedCategories;
            }
            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategories");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>();
                // get now only the active categories
                var categories = FilterIsAktiv(getItems?.Items ?? []);

                // add the categories to the local list
                AddCategoriesToLocal(categories);

                return categories;
            }
            catch
            {
                return [];
            }
        }
        public async Task<Categories> getCategoryById(int categoryId)
        {
            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategoryById/{categoryId}");
                if (!response.IsSuccessStatusCode)
                    return null!;
                var category = await response.Content.ReadFromJsonAsync<Categories>();
                // add the categories to the local list
                AddCategoriesToLocal(category!);
                return category ?? null!;
            }
            catch
            {
                return null!;
            }
        }
        public async Task<List<Categories>> getCategoriesByIds(List<int> categoryIds)
        {
            try
            {
                if (categoryIds == null || categoryIds.Count == 0)
                    return [];
                var queryString = string.Join("&", categoryIds.Select(id => $"categoryIds={id}"));
                var response = await _http.GetAsync($"api/Categories/getCategoriesByIds?{queryString}");
                if (!response.IsSuccessStatusCode)
                    return [];
                var Categories = await response.Content.ReadFromJsonAsync<List<Categories>>();
                // get now only the active categories
                var categories = FilterIsAktiv(Categories ?? []);
                // add the categories to the local list
                AddCategoriesToLocal(categories);

                return categories;
            }
            catch
            {
                return [];
            }
        }
        public GetItems<Product> _getItems = new() { PageSize = 10 };
        public async Task<GetItems<Product>> GetProductsByCategoryIdAsync(int categoryId, int? pageSize = null,bool? AllItemsLoaded = null, List<int>? excludeProductsIds = null)
        {
            /* die Ausschluss von category if inaktiv wird in der getCategories gemacht */

            // define the page size
            if (pageSize.HasValue && pageSize.Value > 0)
            {
                _getItems.PageSize = pageSize.Value;
            }
            // ignore the AllItemsLoaded get random
            if (AllItemsLoaded.HasValue)
            {
                _getItems.AllItemsLoaded = AllItemsLoaded.Value;
            }


            if (_getItems.AllItemsLoaded)
                return _getItems!;
            try
            {

                String queryString = string.Empty;
                if (excludeProductsIds != null && excludeProductsIds.Count != 0)
                {
                    queryString = "&";
                    queryString += string.Join("&", excludeProductsIds.Select(id => $"excludeProductsIds={id}"));

                }
                queryString = $"?PageSize={_getItems.PageSize}" + queryString;

                var response = await _http.GetAsync($"api/Categories/getProductsByCategoryId/{categoryId}{queryString}");


                if (!response.IsSuccessStatusCode)
                    return _getItems;

                _getItems = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? new GetItems<Product>();

                if (_getItems.AllItemsLoaded == true)
                {
                    return _getItems;
                }
                else
                {
                    _getItems.CurrentPage++;
                    return _getItems;
                }
            }
            catch
            {
                return _getItems;
            }
        }
        // loacl
        public List<Categories> GetAllCategoriesLocal()
        {
            return DownloadedCategories;
        }
        public Categories GetProductByIdLocal(int categoryId)
        {
            var category = DownloadedCategories.Find(p => p.Id == categoryId);
            if (category != null)
                return category;
            else
            {
                return null!;
            }
        }
        public Categories GetCategoryByIdLocal(int categoryId)
        {
            var category = DownloadedCategories.Find(p => p.Id == categoryId);
            if (category != null)
                return category;
            else
            {
                return null!;
            }
        }
        public void AddCategoriesToLocal(List<Categories> categories)
        {
            if (categories.Count > 0 && DownloadedCategories.Count == 0)
            {
                DownloadedCategories.AddRange(categories);
                return;
            }

            foreach (var category in categories)
            {
                if (!DownloadedCategories.Any(p => p.Id == category.Id))
                {
                    DownloadedCategories.Add(category);
                }
            }
        }
        public void AddCategoriesToLocal(Categories category)
        {
            if (category != null && !DownloadedCategories.Any(p => p.Id == category.Id))
            {
                DownloadedCategories.Add(category);
            }
        }
        public List<Categories> FilterIsAktiv(List<Categories> categories)
        {
            return categories.Where(c => c.IsAktiv == true).ToList();
        }
        public Categories FilterIsAktiv(Categories category)
        {
            if (category.IsAktiv == true)
                return category;
            else
                return null!;
        }
    }
}
