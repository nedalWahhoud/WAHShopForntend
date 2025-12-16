using Microsoft.Extensions.Caching.Memory;
using System;
using System.Globalization;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;

namespace WAHShopForntend.Components.CategoriesF
{
    public class CategoryService(HttpClient http, ProductService productService, IMemoryCache cache)
    {
        private readonly HttpClient _http = http;
        private readonly ProductService _productService = productService;
        private readonly IMemoryCache _cache = cache;
       
        public List<Categories> DownloadedCategories { get; private set; } = [];
        private const string DACCachKey = "DownloadedAllCategories";
        // Async
        public async Task<List<Categories>> GetAllCategoriesAsync()
        {
            DownloadedCategories = GetCategoriesFromCache(DACCachKey);
            if (DownloadedCategories.Count > 0)
                return DownloadedCategories;
            try
            {
                var response = await _http.GetAsync($"api/Categories/getCategories");
                if (!response.IsSuccessStatusCode)
                    return [];

                var getItems = await response.Content.ReadFromJsonAsync<GetItems<Categories>>();
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
        public GetItems<Product> _getItems = new() { PageSize = 10 };
        public async Task<GetItems<Product>> GetProductsByCategoryIdAsync(int categoryId, int? pageSize = null,bool? AllItemsLoaded = null, List<int>? excludeProductsIds = null)
        {
            // -1 is for OnOffer

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

                var response = await _http.GetAsync($"api/Categories/getProductsByCategoryId/{categoryId.ToString(CultureInfo.InvariantCulture)}{queryString}");


                if (!response.IsSuccessStatusCode)
                    return _getItems;

                _getItems = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? new GetItems<Product>();


                // add the product to the local list
                _productService.AddProductToLocal(_getItems.Items);

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
            catch(Exception ex)
            {
                return _getItems;
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
                    .Where(p => p.CategoryId != 0 && (categoryId == -1 ? p.DiscountedPrice > 0 : p.CategoryId == categoryId)
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
                AddToCategoriesToCache(CacheKey, 10, 2, CacheItemPriority.High, DownloadedCategories);
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
        public async Task<List<Categories>> GetRandomCategories(int count, List<int> excludeCategoriesIds, Random random)
        {
            List<Categories> categories = (GetAllCategoriesLocal() ?? await GetAllCategoriesAsync())
                .Where(c => !excludeCategoriesIds.Contains(c.Id))
                .GroupBy(c => c.Id)
                .Select(g => g.First())
                .OrderBy(c => random.Next())
                .Take(count)
                .ToList();
            
            return categories;
        }
        // chache
        private void AddToCategoriesToCache(string CacheKey,int Minutes,int Expiration, CacheItemPriority Priority,List<Categories> CategoriesList)
        {
            _cache.Set(CacheKey, CategoriesList, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Minutes),
                SlidingExpiration = TimeSpan.FromMinutes(Expiration),
                Priority = Priority
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
