using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WAHShopForntend.Components.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Email ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige Email-Adresse.")]
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; }
    }
}
