using System.ComponentModel.DataAnnotations;

namespace WAHShopForntend.Components.Models
{
    public class ForgotPassword
    {
        [Required(ErrorMessage = "Email ist erforderlich.")]
        [EmailAddress(ErrorMessage = "Ungültige Email-Adresse.")]
        public string? Email { get; set; }
        public string EmailPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Passwort ist erforderlich.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$",
        ErrorMessage = "Passwort muss mindestens einen Großbuchstaben, Kleinbuchstaben und ein Sonderzeichen enthalten.")]
        public string NewPassword { get; set; } = string.Empty;
        [Compare("NewPassword", ErrorMessage = "Passwörter stimmen nicht überein.")]
        public string PasswordAgain { get; set; } = string.Empty;
    }
}
