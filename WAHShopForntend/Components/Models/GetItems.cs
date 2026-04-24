namespace WAHShopForntend.Components.Models
{
    public class GetItems<T>
    {
        public List<T> Items { get; set; } = [];
        public bool AllItemsLoaded { get; set; }
        public int CurrentPage { get; set; } = 0;
        public int PageSize { get; set; } = 11;
        public FilterOption? Filter { get; set; }
    }

    public enum GetItemFilterType
    {
        None,
        Category,
        Custom,
    }

    public class FilterOption
    {
        public int Id { get; set; } = 0;
        public GetItemFilterType Type { get; set; } = GetItemFilterType.None;
    }
}
