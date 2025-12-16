namespace WAHShopForntend.Components.Models
{
    public class ProductImages
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int ProductId { get; set; }
        public DateTime LastModified { get; set; }
        public Product? Product { get; set; }
    }
}
