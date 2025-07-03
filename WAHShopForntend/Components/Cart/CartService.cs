using WAHShopForntend.Components.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.AspNetCore.Cors.Infrastructure;
using WAHShopForntend.Components.ProductsF;

namespace WAHShopForntend.Components.Cart
{
    public class CartService
    {
        public List<CartItem> CartItems { get; private set; } = [];
        public event Action? OnChange;
        private readonly IJSRuntime _js;
        private readonly ProductService _productService;
        public CartService(IJSRuntime js, ProductService productService)
        {
            _js = js;
            _productService = productService;
        }
        // event to change the cart state
        private void NotifyStateChanged() => OnChange?.Invoke();
        public async Task InitializeAsync()
        {
            try
            {
                var json = await _js.InvokeAsync<string>("localStorage.getItem", "cart");
                //
                var items = (json == null ? [] : JsonSerializer.Deserialize<List<CartItem>>(json));
                CartItems = items ?? [];
                NotifyStateChanged();
            }
            catch 
            {
                CartItems = [];
            }
        }
        public void AddToCart(CartItem cartItem)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == cartItem.ProductId);
            if (item != null)
            {
                item.Quantity = cartItem.Quantity;
            }
            else
            {
                CartItems.Add(new CartItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    Product = cartItem.Product
                });
            }
            SaveCart(); // Save the updated cart to local storage
            NotifyStateChanged();
        }
        public void RemoveFromCart(int ProductId)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == ProductId);
            if (item != null)
            {
                CartItems.Remove(item);
                SaveCart();
                NotifyStateChanged();
            }
        }
        public bool IsQuantityZero(CartItem cartItem)
        {
            if (cartItem.Quantity <= 0)
            {
                bool isProductAdded = IsProductAdded(cartItem.ProductId);
                if (isProductAdded)
                    RemoveFromCart(cartItem.ProductId);

                return true; // Do not add to cart if quantity is zero
            }
            return false; // Quantity is greater than zero, proceed with adding to cart
        }
        public void ClearCart()
        {
            CartItems.Clear();
            SaveCart(); 
            NotifyStateChanged();
        }
       
        private void SaveCart()
        {
            // save the cart items to local storage or a database
            _ = _js.InvokeVoidAsync("localStorage.setItem", "cart",JsonSerializer.Serialize(CartItems));
        }
        //
        public int GetTotalQuantity()
        {
            return CartItems.Sum(ci => ci.Quantity);
        }
        public int GetQuantityOfProduct(int productId)
        {
            return CartItems.FirstOrDefault(c => c.ProductId == productId)?.Quantity ?? 0;
        }
        public double GetTotalPrice()
        {
            return (double)(CartItems.Sum(ci => ci.Product?.SalePrice * ci.Quantity) ?? 0);
        }
        public bool IsProductAdded(int productId)
        {
            return CartItems.Any(ci => ci.ProductId == productId);
        }
        public async Task<ValidationResult> LoadCartProductsAsync()
        {
            List<int> idsUnlocalProducts = new List<int>();
            for (int i = 0; i < CartItems.Count; ++i)
            {
                if (CartItems[i].Product == null)
                {
                    var product = _productService.GetProductByIdLocal(CartItems[i].ProductId);
                    if (product != null)
                        CartItems[i].Product = product;
                    else
                        // if no local product found, add to idsUnlocalProducts to fetch from server
                        idsUnlocalProducts.Add(CartItems[i].ProductId);
                }
            }
            // Fetch products from server for those not found locally and update cart items
            if (idsUnlocalProducts.Count > 0)
            {
                var products = await _productService.GetProductByIdsServer(idsUnlocalProducts);
                if (products != null && products.Count > 0)
                {
                    foreach (var product in products)
                    {
                        var cartItem = CartItems.FirstOrDefault(ci => ci.ProductId == product.Id);
                        if (cartItem != null)
                        {
                            cartItem.Product = product;
                        }
                    }
                }
                else
                    return new ValidationResult() { Result = false , Message = "Failed to retrieve products from server."};
                
            }

            return new ValidationResult() { Result = true };
        }
    }
}
