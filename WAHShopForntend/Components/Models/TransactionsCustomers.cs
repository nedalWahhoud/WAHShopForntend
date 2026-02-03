using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class TransactionsCustomers
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime TransactionDate { get; set; }
    }
    public enum TransactionType
    {
        Borrow,
        Repay
    }
}
