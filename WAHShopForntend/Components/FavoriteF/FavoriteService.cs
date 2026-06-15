using Microsoft.JSInterop;
using System.Net.Http.Json;
using System.Text.Json;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;

namespace WAHShopForntend.Components.FavoriteF
{
    public class FavoriteService(HttpClient http, IJSRuntime js)
    {
        private readonly HttpClient _http = http;
        private readonly IJSRuntime _js = js;
        public  GetItems<Product> DownloadedFavoriteProducts { get; private set; } = new () { PageSize = 7 };
        public async Task<ValidationResult> Add(UserFavorite userFavorite)
        {
            try
            {
                var response = await _http.PostAsync($"api/UserFavorite/add/{userFavorite.UserId}/{userFavorite.ProductId}", null);


                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();

                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Hinzufügen der Einmalzahlung." };
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<ValidationResult> Delete(int UserId, int ProductId)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/UserFavorite/delete/{UserId}/{ProductId}");
                var result = await response.Content.ReadFromJsonAsync<ValidationResult>();
                if (!response.IsSuccessStatusCode || result == null || !result.Result)
                {
                    return result ?? new ValidationResult { Result = false, Message = "Fehler beim Löschen der Einmalzahlung." };
                }

                if(result.Result)
                {
                    DownloadedFavoriteProducts.Items.RemoveAll(p => p.Id == ProductId);
                   
                }

                return result;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<ValidationResult> GetFavoritesProductsAsync(int userId)
        {
            try
            {
               if(DownloadedFavoriteProducts.AllItemsLoaded)
                    return new ValidationResult { Result = true, Message = "Alle Produkte sind bereits geladen." };

                var response = await _http.PostAsJsonAsync($"api/UserFavorite/getFavoritesProducts/{userId}", DownloadedFavoriteProducts);

                if (response.IsSuccessStatusCode)
                {
                    var getitem = await response.Content.ReadFromJsonAsync<GetItems<Product>>() ?? null!;
                    if (getitem != null)
                    {
                        if (getitem.AllItemsLoaded)
                        {
                            DownloadedFavoriteProducts.AllItemsLoaded = true;
                            DownloadedFavoriteProducts.Items.AddRange(getitem.Items);

                            return new ValidationResult { Result = true, Message = "Alle Produkte wurden erfolgreich geladen." };
                        }
                        else
                        {
                            DownloadedFavoriteProducts.CurrentPage++;
                            DownloadedFavoriteProducts.Items.AddRange(getitem.Items);

                            return new ValidationResult { Result = true, Message = "Weitere Produkte geladen." };
                        }
                    }
                }
                else
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    return new ValidationResult { Result = false, Message = $"Fehler beim Laden: {errorText}" };
                }

                return null!;
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
     

        public async Task<ValidationResult> SyncFavoritesAfterLogin(int UserId)
        {
            try
            {
                var localFavorites = await GetFavoritesFromLocalStorage();
                if (localFavorites != null && localFavorites.Count != 0)
                {
                    foreach (var productId in localFavorites)
                    {
                        var userFavorite = new UserFavorite
                        {
                            UserId = UserId,
                            ProductId = productId
                        };


                        await Add(userFavorite);
                    }
                    await ClearLocalStorage();
                }
                return new ValidationResult { Result = true, Message = "Favoriten erfolgreich synchronisiert." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        // local
        public void AddToLocalList(Product product)
        {
            if (!DownloadedFavoriteProducts.Items.Any(p => p.Id == product.Id))
            {
                DownloadedFavoriteProducts.Items.Add(product);
            }
        }
        public async Task<ValidationResult> AddLocalStorage(int ProductId)
        {
            try
            {
                var currentFavoritesJson = await _js.InvokeAsync<string>("localStorage.getItem", "favorite");
                List<int> favoritesList = [];

                if (!string.IsNullOrEmpty(currentFavoritesJson))
                {
                    favoritesList = JsonSerializer.Deserialize<List<int>>(currentFavoritesJson) ?? [];
                }

                if (!favoritesList.Contains(ProductId))
                {
                    favoritesList.Add(ProductId);
                }

                var updatedJson = JsonSerializer.Serialize(favoritesList);
                await _js.InvokeVoidAsync("localStorage.setItem", "favorite", updatedJson);

                return new ValidationResult { Result = true, Message = "Produkt zur Favoritenliste hinzugefügt." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<ValidationResult> RemoveLocalStorage(int ProductId)
        {
            try
            {
                var currentFavoritesJson = await _js.InvokeAsync<string>("localStorage.getItem", "favorite");

                if (!string.IsNullOrEmpty(currentFavoritesJson))
                {
                    var favoritesList = JsonSerializer.Deserialize<List<int>>(currentFavoritesJson) ?? [];

                    if (!favoritesList.Contains(ProductId))
                    {
                        return new ValidationResult { Result = true, Message = "Dieses Produkt ist nicht in der Favoritenliste." };
                    }
                    favoritesList.Remove(ProductId);

                    var updatedJson = JsonSerializer.Serialize(favoritesList);
                    await _js.InvokeVoidAsync("localStorage.setItem", "favorite", updatedJson);
                    // remove product from downloadFavorite
                    DownloadedFavoriteProducts.Items.RemoveAll(p => p.Id == ProductId);

                    return new ValidationResult { Result = true, Message = "Produkt von der Favoritenliste entfernt." };
                }
                return new ValidationResult { Result = true, Message = "Produkt von der Favoritenliste entfernt." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.InnerException?.Message ?? ex.Message };
            }
        }
        public async Task<bool> IsFavoriteInLocalStorage(int ProductId)
        {
            try
            {
                List<int> favoritesList = await GetFavoritesFromLocalStorage();

                return favoritesList.Contains(ProductId);
            }
            catch
            {
                return false;
            }
        }
        public async Task ClearLocalStorage()
        {
            try
            {
                await _js.InvokeVoidAsync("localStorage.removeItem", "favorite");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Löschen des localStorage: {ex.InnerException?.Message ?? ex.Message}");
            }
        }
        public async Task<List<int>> GetFavoritesFromLocalStorage()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "favorite");
                var favorites = (json == null ? [] : JsonSerializer.Deserialize<List<int>>(json));
                return favorites ?? [];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fehler beim Abrufen der Favoriten aus localStorage: {ex.InnerException?.Message ?? ex.Message}");
                return [];
            }
        }
    }
}
