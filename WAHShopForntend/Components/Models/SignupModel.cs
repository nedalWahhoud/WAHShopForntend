using System.ComponentModel.DataAnnotations;

namespace WAHShopForntend.Components.Models
{
    public class SignupModel
    {
        [Required(ErrorMessage = "Benutzername ist erforderlich.")]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige Email-Adresse.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$",
        ErrorMessage = "Passwort muss mindestens einen Großbuchstaben, Kleinbuchstaben und ein Sonderzeichen enthalten.")]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwörter stimmen nicht überein.")]
        public string PasswordAgain { get; set; } = string.Empty;
        [Required(ErrorMessage = "Rolle ist erforderlich.")]
        public string BirthDate { get; set; } = DateTime.Now.ToString("yyyy.MM.dd");
        public bool IsGuest { get; set; } = false;
    }
}
