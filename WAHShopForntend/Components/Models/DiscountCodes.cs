using System.ComponentModel.DataAnnotations;

namespace WAHShopForntend.Components.Models
{
    public class DiscountCodes
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name ist erforderlich.")]
        [StringLength(8, MinimumLength = 8, ErrorMessage = "Code muss genau 8 Zeichen lang sein.")]
        public string Code { get; set; } = string.Empty;
        public int DiscountAmount { get; set; }
        public int UsageLimit { get; set; }
        public int TimesUsed { get; set; }
        public DateTime StartDate { get; set; } 
        public DateTime EndDate { get; set; } 
        public bool IsActive { get; set; }
        public DiscountType DiscountType { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
