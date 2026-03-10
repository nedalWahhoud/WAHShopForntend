using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class ProductDiscounts
    {
        public int Id { get; set; }
        public int ProductsId { get; set; }
        public double DiscountedPrice { get; set; }
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(2);
        [JsonIgnore]
        public Product? Product { get; set; }
    }
}
