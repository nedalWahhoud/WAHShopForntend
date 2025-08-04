using System.ComponentModel.DataAnnotations;
namespace WAHShopForntend.Components.Models
{
    public class LoginModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Benutzername oder die Email ist erforderlich.")]
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; }
        public string SignupProvider { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
