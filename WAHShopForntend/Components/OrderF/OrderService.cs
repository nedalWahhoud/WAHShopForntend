using WAHShopForntend.Components.Models;
using static System.Net.WebRequestMethods;
namespace WAHShopForntend.Components.OrderF
{
    public class OrderService(HttpClient http)
    {
        private readonly HttpClient _http = http;
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
        public async Task<ValidationResult> UpdateStatusOrder(int orderId, int newStatusId)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/Orders/updateStatusOrder/{orderId}", newStatusId);

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
    }
}
