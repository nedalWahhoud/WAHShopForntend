using WAHShopForntend.Components.Models;
using Microsoft.JSInterop;
using System.Text.Json;
using Microsoft.AspNetCore.Cors.Infrastructure;
using WAHShopForntend.Components.ProductsF;
using WAHShopForntend.Components.CategoriesF;

namespace WAHShopForntend.Components.Cart
{
    public class CartService(IJSRuntime js, ProductService productService)
    {
        public List<CartItem> CartItems { get; private set; } = [];
        public event Action? OnChange;
        private readonly IJSRuntime _js = js;
        private readonly ProductService _productService = productService;
        
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
        public async Task<string> AddToCart(int productId)
        {
            Product product = _productService.GetProductByIdLocal(productId) ?? await _productService.GetProductByIdAsync(productId);
            
            //
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (item != null)
            {
                // Update the existing item quantity
                item.Quantity++;

                // check if has stock
                if (!HasStock(item.Quantity, product.Quantity))
                {
                    item.Quantity--; // revert the quantity increase
                    return "NoStock";
                }

                
                if (item.Quantity > 5)
                {
                    item.Quantity = 5;
                    return "MaxQuantity";
                }
            }
            // hier wird die quantität nicht überprüft, da es sich um ein neues produkt handelt und die quantität sollte mindestens 1 sein, um zu benutzer zu zeigen
            else
            {
                // add the new item to the genrall cart
                CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1, // add first item with quantity 1
                    Product = _productService.GetProductByIdLocal(productId) ?? await _productService.GetProductByIdAsync(productId),
                    TextMessage = null
                });
            }
            SaveCart(); // Save the updated cart to local storage
            NotifyStateChanged();
            return null!;
        }
        public string DecreaseFromCart(int productId)
        {
            if (IsQuantityZero(productId))
            {
                return null!; // Cannot decrease below zero
            }

            else
            {
                var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                if (item != null)
                {

                    item.Quantity--;

                    if (item.Quantity == 0)
                    {
                        RemoveFromCart(productId);
                    }

                    SaveCart();
                    NotifyStateChanged();
                    return null!;
                }
                return "ProductNotFound"; // Product not found in the cart
            }
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
        public bool IsQuantityZero(int productId)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if(item == null)
                return true; // Item not found, treat as zero quantity

            if (item.Quantity <= 0)
            {
                bool isProductAdded = IsProductAdded(item.ProductId);
                if (isProductAdded)
                    RemoveFromCart(item.ProductId);

                return true; // Do not add to cart if quantity is zero
            }
            return false; // Quantity is greater than zero, proceed with adding to cart
        }
        public bool HasStock(int requestedQuantity, int minimumStock)
        {
            return minimumStock >= requestedQuantity;
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
        public double GetPriceOfProduct(int id)
        {
            var item = CartItems.FirstOrDefault(ci => ci.ProductId == id && ci.Product != null);

            return (double)(item != null
                ? (item.Product.DiscountedPrice > 0 ? item.Product.DiscountedPrice : item.Product.SalePrice)
                : 0);

        }
        public double GetTotalPrice()
        {
            return (double)(CartItems
                .Sum(ci => (ci.Product?.DiscountedPrice > 0 ? ci.Product?.DiscountedPrice : ci.Product?.SalePrice) * ci.Quantity) ?? 0);
        }
        public double GetTotalPriceOfProduct(int id)
        {
            return (double)CartItems
           .Where(ci => ci.ProductId == id && ci.Product != null)
           .Sum(ci => ((ci.Product?.DiscountedPrice > 0 ? ci.Product?.DiscountedPrice : ci.Product?.SalePrice ) * ci.Quantity) ?? 0);

        }
        public bool IsProductAdded(int productId)
        {
            return CartItems.Any(ci => ci.ProductId == productId);
        }
        public CartItem GetCartItemByProductId(int productId)
        {
            return CartItems.Find(ci => ci.ProductId == productId) ?? null!;
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
                var products = await _productService.GetProductByIdsAsync(idsUnlocalProducts);
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
