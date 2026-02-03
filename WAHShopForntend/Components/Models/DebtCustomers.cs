using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class DebtCustomers
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Balance { get; set; }
    }
}
