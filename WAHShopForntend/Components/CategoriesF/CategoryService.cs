using Microsoft.Extensions.Caching.Memory;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;

namespace WAHShopForntend.Components.CategoriesF
{
    public class CategoryService(HttpClient http, ProductService productService, IMemoryCache cache)
    {
        private readonly HttpClient _http = http;
        private readonly ProductService _productService = productService;
        private readonly IMemoryCache _cache = cache;
        private const int CacheAbsoluteExpiration = 60;
        private const int CacheSlidingExpiration = 10;
        private const CacheItemPriority CachePriority = CacheItemPriority.High;
        private const string DACCachKey = "DownloadedAllCategories";

        // zentrale taskCateories, damit die Kategorien nur einmal heruntergeladen werden, wenn mehrere Komponenten gleichzeitig auf die Kategorien zugreifen
        public Task taskCatyrories = Task.CompletedTask;
        public List<Categories> DownloadedCategories { get; private set; } = [];
        // Async
        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            DownloadedCategories = GetCategoriesFromCache(DACCachKey);
            if (DownloadedCategories.Count > 0)
                return DownloadedCategories;
            try
            {

                GetItems<Categories> getItems = new() { IsAdmin = false };
                var response = await _http.PostAsJsonAsync($"api/Categories/getCategories",getItems);
                if (!response.IsSuccessStatusCode)
                    return [];

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>() ?? new();
                // get now only the active categories
                var categories = FilterIsAktiv(getItems?.Items ?? []);

                // add the categories to the local list
                AddCategoriesToLocal(categories, DACCachKey);

                return categories;
            }
            catch
            {
                return [];
            }
        }
        public async Task<Categories> GetCategoryByIdAsync(int categoryId)
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
        public async Task<GetItems<Product>> GetProductsByCategoryIdAsync(int categoryId, int UserId,GetItems<Product> getItem, List<int>? excludeProductsIds = null)
        {
            // -1 is for OnOffer

            if (getItem.AllItemsLoaded)
                return getItem!;

            getItem.UserId = UserId;

            if(excludeProductsIds != null && excludeProductsIds.Count > 0)
            {
                getItem.ExcludeProductsIds = excludeProductsIds;
            }

            try
            {
                getItem.Filter = new() { Type = GetItemFilterType.Category, Id = categoryId };

                var response = await _http.PostAsJsonAsync($"api/Categories/getProductsByCategoryId",getItem);

                if (!response.IsSuccessStatusCode)
                    return getItem;

                getItem = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? new GetItems<Product>();

                // add the product to the local list
                await _productService.AddProductToLocal(getItem.Items);

                if (getItem.AllItemsLoaded == true)
                {
                    return getItem;
                }
                else
                {
                    getItem.CurrentPage++;
                    return getItem;
                }
            }
            catch
            {
                return getItem;
            }
        }
        // loacl
        public List<Categories> GetAllCategoriesLocal()
        {
            // check in cache first
            DownloadedCategories = GetCategoriesFromCache(DACCachKey);

            if(DownloadedCategories == null || DownloadedCategories.Count == 0)
                return null!;

            return DownloadedCategories;
        }
        public Categories GetCategoryByIdLocal(int categoryId)
        {
            // check in cache first
            DownloadedCategories = GetCategoriesFromCache(DACCachKey);

            var category = DownloadedCategories.Find(p => p.Id == categoryId);
            if (category != null)
                return category;
            else
            {
                return null!;
            }
        }
        public List<Product> GetProductByCategoryIdLocal(int categoryId, List<int>? excludeProductsIds = null, int? excludeProductsId = null)
        {
            // -1 is for OnOffer
            try
            {
                // initialize the excludeProductsIds and excludeProductsId if they are null
                excludeProductsIds ??= [];
                excludeProductsId ??= 0;
                //
                return _productService.DownloadedProduct
                    .Where(p => p.CategoryId != 0 
                    && (categoryId == -1 ?
                       (p.ProductDiscount != null &&
                        p.ProductDiscount.DiscountedPrice > 0 &&
                        DateTime.Today >= p.ProductDiscount.StartDate.Date &&
                        DateTime.Today <= p.ProductDiscount.EndDate.Date)
                        : p.CategoryId == categoryId
                        )
                    && !excludeProductsIds.Contains(p.Id)
                    && p.Id != excludeProductsId)
                    .ToList();
            }
            catch
            {
                return null!;
            }
        }
        public void AddCategoriesToLocal(List<Categories> categories, string? CacheKey = null)
        {
            if (categories.Count > 0 && DownloadedCategories.Count == 0)
            {
                DownloadedCategories.AddRange(categories);
            }
            else
            {
                foreach (var category in categories)
                {
                    if (!DownloadedCategories.Any(p => p.Id == category.Id))
                    {
                        DownloadedCategories.Add(category);
                    }
                }
            }
            // add zu cach, wenn CacheKey != null
            if (CacheKey != null)
            {
                AddToCategoriesToCache(CacheKey, DownloadedCategories);
            }
        }
        public void AddCategoriesToLocal(Categories category)
        {
            if (category != null && !DownloadedCategories.Any(p => p.Id == category.Id))
            {
                DownloadedCategories.Add(category);
            }
        }
        //
        public async Task<List<Categories>> GetRandomCategories(int count, List<int> excludeCategoriesIds)
        {
            var allCategories = GetAllCategoriesLocal() ?? await GetAllCategoriesAsync();

            if (allCategories == null || allCategories.Count == 0)
            {
                return [];
            }

            var filteredCategories = allCategories
                .Where(c => !excludeCategoriesIds.Contains(c.Id));

            List<Categories> categories = filteredCategories
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => Random.Shared.Next())
                .Take(count)
                .ToList();

           
            return categories;
        }
        // chache
        private void AddToCategoriesToCache(string CacheKey,List<Categories> CategoriesList)
        {
            _cache.Set(CacheKey, CategoriesList, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheAbsoluteExpiration),
                SlidingExpiration = TimeSpan.FromMinutes(CacheSlidingExpiration),
                Priority = CachePriority
            });
        }
        private List<Categories> GetCategoriesFromCache(string CacheKey)
        {
            if (_cache.TryGetValue(CacheKey, out List<Categories>? cachedCategories))
            {
                if (cachedCategories != null && cachedCategories.Count > 0)
                {
                    return cachedCategories;
                }
            }
            return [];
        }
        // filter
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
