using WAHShopForntend.Components.Cart;
using WAHShopForntend.Components.CategoriesF;
using WAHShopForntend.Components.Models;
using WAHShopForntend.Components.ProductsF;
using static System.Net.WebRequestMethods;
namespace WAHShopForntend.Components.OrderF
{
    public class OrderService(HttpClient http, ProductService productService, CategoryService categoryService)
    {
        private readonly HttpClient _http = http;
        private readonly ProductService _productService = productService;
        private readonly CategoryService _categoryService = categoryService;
        public List<PaymentMethod> DownloadedPaymentMethods { get; private set; } = [];
        public List<Order> DownloadedOrders { get; private set; } = [];
        public List<OrderStatus> DownloadedOrderStatus { get; private set; } = [];
        public List<ShippingProvider> DownloadedShippingProviders { get; private set; } = [];
        public async Task<ValidationResult> AddOrderAsync(Order order)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/Orders/addOrder", order);
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        private GetItems<Order> getItems = new() { PageSize = 5 };
        public async Task<ValidationResult> GetOrdersAsync(int? userId = null)
        {
            try
            {
                var response = await _http.GetAsync($"api/Orders/getOrders/{userId}?CurrentPage={getItems.CurrentPage}&PageSize={getItems.PageSize}&AllItemsLoaded={getItems.AllItemsLoaded}");

                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to retrieve Orders ." };
                }

                getItems = await response.Content.ReadFromJsonAsync<GetItems<Order>>() ?? new GetItems<Order>();
                if (getItems.AllItemsLoaded == true)
                {
                    getItems.AllItemsLoaded = true; // No more items to load
                    return new ValidationResult { Result = true, Message = "AllItemsLoaded" };
                }
                else
                {
                    getItems.CurrentPage++;

                    DownloadedOrders.AddRange(getItems.Items);

                    return new ValidationResult { Result = true, Message = "Orders retrieved successfully." };

                }
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetPaymentMethodsAsync()
        {
            if (DownloadedPaymentMethods.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Payment methods already loaded." };
            }
            try
            {
                var response = await _http.GetAsync("api/Orders/getPaymentMethods");
                if (!response.IsSuccessStatusCode)
                {
                    return new ValidationResult { Result = false, Message = "Failed to retrieve payment methods." };
                }
                DownloadedPaymentMethods = await response.Content.ReadFromJsonAsync<List<PaymentMethod>>() ?? [];
                return new ValidationResult { Result = true, Message = "Payment methods retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetOrderStatusAsync()
        {
            if (DownloadedOrderStatus.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Order status already loaded." };
            }

            try
            {
                var response = await _http.GetAsync("api/Orders/getOrderStatusList");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error." };
                }
                DownloadedOrderStatus = await response.Content.ReadFromJsonAsync<List<OrderStatus>>() ?? [];
                return new ValidationResult { Result = true, Message = "Order status retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> UpdateStatusOrderAsync(int orderId, int newStatusId)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Orders/updateStatusOrder/{orderId}", newStatusId);

                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown Error." };
                }
                return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown Error." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        public async Task<ValidationResult> GetShippingProvidersAsync()
        {
            if (DownloadedShippingProviders.Count > 0)
            {
                return new ValidationResult { Result = true, Message = "Shipping providers already loaded." };
            }
            try
            {
                var response = await _http.GetAsync("api/Orders/getShippingProvider");
                if (!response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown error" };
                }
                var shippingProvider = await response.Content.ReadFromJsonAsync<List<ShippingProvider>>();

                if (shippingProvider == null)
                {
                    return new ValidationResult { Result = false, Message = "Unknown error" };
                }
                DownloadedShippingProviders = shippingProvider;
                return new ValidationResult { Result = true, Message = "Shipping providers retrieved successfully." };
            }
            catch (Exception ex)
            {
                return new ValidationResult { Result = false, Message = ex.Message };
            }
        }
        // producte ab 18 jahre alt
        public async Task<ValidationResult> CheckAge(List<CartItem> cartItems, User user)
        {
            try
            {
                for (int i = 0; i < cartItems.Count; i++)
                {
                    var item = cartItems[i];
                    if (item == null)
                        continue;
                    var product = item.Product
                                ?? _productService.GetProductByIdLocal(item.ProductId)
                    ?? await _productService.GetProductByIdAsync(item.ProductId);

                    var category = product.Category
                                ?? _categoryService.GetCategoryByIdLocal(product.CategoryId)
                                ?? await _categoryService.GetCategoryByIdAsync(product.CategoryId);

                    if (category == null || user == null)
                        return new ValidationResult { Result = false, Message = "Unbekannte Fehler" };

                    if (category.Requires18Plus)
                    {
                        try
                        {
                            bool result = DateTime.TryParse(user.BirthDate, out _);
                            if (result)
                            {
                                int age = (int)((DateTime.Now - DateTime.Parse(user.BirthDate)).TotalDays / 365.25);
                                if (age >= 18)
                                    return new ValidationResult { Result = true, Message = null };
                                else
                                    return new ValidationResult { Result = false, Message = "AgeError" };
                            }
                            else
                            {
                                return new ValidationResult { Result = false, Message = "ValidBirthDate" };
                            }
                        }
                        catch (Exception ex)
                        {
                            return new ValidationResult { Result = false, Message = ex.Message };
                        }
                    }
                }
                return new ValidationResult { Result = true, Message = null! };
            }
            catch
            {
                return new ValidationResult { Result = false, Message = "UnknownError" };
            }
        }
        public async Task<(bool result, string errorMessage, string productsName)> CanShipByPostAsync(List<CartItem> cartItems, int selectedPaymentId, string languageCode)
        {
            bool if18Plus = false;
            (bool, string) IsShippable = (true, null!);
            for (int i = 0; i < cartItems.Count; i++)
            {
                var item = cartItems[i];
                if (item == null)
                    continue;

                var product = item.Product
                            ?? _productService.GetProductByIdLocal(item.ProductId)
                            ?? await _productService.GetProductByIdAsync(item.ProductId);

                var category = product.Category
                            ?? _categoryService.GetCategoryByIdLocal(product.CategoryId)
                            ?? await _categoryService.GetCategoryByIdAsync(product.CategoryId);
                // if 18 plus required
                if (category.Requires18Plus)
                {
                    if18Plus = true;
                    break;
                }
                // if not shippable
                if (!product.IsShippable)
                {
                    // Fix for CS8600: Ensure that nullable values are handled properly in the switch statement.  
                    string productName = languageCode switch
                    {
                        "de" => product.Name_de ?? string.Empty, // Use null-coalescing operator to provide a default value.  
                        "ar" => product.Name_ar ?? string.Empty,
                        _ => product.Name_de ?? string.Empty // Fallback to a non-nullable default value.  
                    };
                    IsShippable.Item1 = false;
                    IsShippable.Item2 = IsShippable.Item2 == null! ? productName! : IsShippable.Item2 + $", {productName}";
                }
            }
            if (IsShippable.Item1 == false)
            {
                if (selectedPaymentId == 5) // 5 id is pay by transfer and shipping by post
                {
                    return (false, $"ShippingProductNotPossible", IsShippable.Item2);
                }
                else
                {
                    return (true, null!, null!);
                }
            }
            else if (if18Plus)
            {
                if (selectedPaymentId == 5) // 5 id is pay by transfer and shipping by post
                {
                    return (false, "ShippingNotPossibleCannotVerify", null!);
                }
                else
                {
                    return (true, "VerifyIdentity", null!);
                }
            }
            return (true, null!, null!);
        }
        public async Task<(ValidationResult, OurDeliveryServiceArea)> CheckIfWithinDeliveryRangeAsync(string postalCode)
        {
            try
            {
                var response = await _http.GetAsync($"api/Orders/CheckIfWithinDeliveryRange/{postalCode}");
                if (response == null || !response.IsSuccessStatusCode)
                {
                    if (response == null)
                    {
                        return (new ValidationResult { Result = false, Message = "No response received from Server." }, null!);
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        return (await response.Content.ReadFromJsonAsync<ValidationResult>() ?? new ValidationResult { Result = false, Message = "Unknown Error." }, null!);
                    }
                    else
                    {
                        return (new ValidationResult { Result = false, Message = "Unknown Error." }, null!);
                    }
                }

                return (new ValidationResult { Result = true, Message = "Diese Gebiet ist in user Lieferbereich" }, await response.Content.ReadFromJsonAsync<OurDeliveryServiceArea>() ?? null!);
            }
            catch (Exception ex)
            {
                return (new ValidationResult { Result = false, Message = ex.Message }, null!);
            }
        }
    }
}
