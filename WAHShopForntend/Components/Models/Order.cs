using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public int? DeliveryAddressId { get; set; }
        [ForeignKey("DeliveryAddressId")]
        public Address? Address { get; set; }
        public int PaymentMethodId { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public double TotalPrice { get; set; }
        public int StatusId { get; set; }
        public OrderStatus? Status { get; set; }

        public string Notes { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public List<OrderItems> OrderItems { get; set; } = [];
        [Required(ErrorMessage = "Bitte geben Sie eine Lieferadresse an.")]
        public string DeliveryAddress1 { get; set; } = string.Empty;
    }
    public enum OrderStatusEnum : byte
    {
        Pending = 1,
        Processing = 2,
        Commpleted = 3,
    }
}
