using Microsoft.Extensions.Options;
using System.Net.Http;
using System;
using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name_de { get; set; }
        public string? Description_de { get; set; }
        public int CategoryId { get; set; }
        public Categories? Category { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double PurchasePrice { get; set; }
        public double SalePrice { get; set; }
        public int MinimumStock { get; set; }
        public DateTime EXPDate { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacturers? Manufacturer { get; set; }
        public int UserId { get; set; }
        public ICollection<ProductImages> ProductImages { get; set; } = [];
        public string? Name_ar { get; set; }
        public string? Description_ar { get; set; }
        public int? ProductGroupID { get; set; }
        public GroupProducts? ProductGroup { get; set; }
        public bool IsShippable { get; set; }
        public double DiscountedPrice { get; set; }
        //
        public CartItem CartItem { get; set; } = null!;
        public void InitializeCartItem(int quantity)
        {
            CartItem = new CartItem
            {
                ProductId = this.Id,
                Quantity = quantity,
                Product = null! 
            };
        }
    }
}
