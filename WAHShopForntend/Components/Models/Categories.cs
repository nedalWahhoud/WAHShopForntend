namespace WAHShopForntend.Components.Models
{
    public class Categories
    {
        public int Id { get; set; }
        public string? Name_de { get; set; }
        public string? Name_ar { get; set; }
        public List<Product>? Products { get; set; } = [];
    }
}
