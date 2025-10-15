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
        [JsonIgnore]
        public bool ShowStatusDropdown { get; set; } = false;
        [JsonIgnore]
        public bool IsUpdatingStatus { get; set; } = false;
        public string Notes { get; set; } = string.Empty;
        public int? UserId { get; set; }
        public List<OrderItems> OrderItems { get; set; } = [];
        public int? DiscountCodeId { get; set; }
        public DiscountCodes? DiscountCode { get; set; }
        public int? DiscountCategoryId { get; set; }
        public DiscountCategory? DiscountCategory { get; set; }
        public int? ShippingProviderId { get; set; }
        public ShippingProvider? ShippingProviders { get; set; }
        public string? TrackingNumber { get; set; }
        public bool IsUserCreated { get; set; }

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
