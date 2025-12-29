using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WAHShopForntend.Components.Models
{
    public class CarouselImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public DateTime StartDate { get; set; } = DateTime.Now.AddDays(0);
        public DateTime EndDate { get; set; } = DateTime.Now.AddDays(7);
        public int DisplayOrder { get; set; }
        public DateTime LastModified { get; set; }

    }
}
