using System.ComponentModel.DataAnnotations;

namespace WAHShopForntend.Components.Models
{
    public class CustomerLoginDto
    {
        public int  Id { get; set; }
        [Required(ErrorMessage = "Bitte Telefonnummer eingeben.")]
        public string PhoneNumber { get; set; } = string.Empty;
        [Required(ErrorMessage = "Bitte PIN eingeben.")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "PIN muss genau 4 Zahlen lang sein.")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "PIN darf nur aus 4 Zahlen bestehen.")]
        public string PIN { get; set; } = string.Empty;

        public string LoginError { get; set; } = string.Empty;
    }
}
