using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace WAHShopForntend.Components.Models
{
    public class UpdateProfile : IValidatableObject

    {

        public int UserId { get; set; }
        public string NewPassword { get; set; } = string.Empty;
        public string PasswordAgain { get; set; } = string.Empty;
        public string OldPassword { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public UpdateTypeEnum UpdateType { get; set; }
        [JsonIgnore]
        public bool IsProviderLogin { get; set; } = false; // Indicates if the user is logged in via a provider (e.g., Google, Facebook)
        public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext)
        {
            if (UpdateType == UpdateTypeEnum.Password)
            {
                if (!string.IsNullOrWhiteSpace(NewPassword) &&
                    !Regex.IsMatch(NewPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\W).{6,}$"))
                {
                    yield return new System.ComponentModel.DataAnnotations.ValidationResult(
                        "Passwort muss mindestens einen Großbuchstaben, Kleinbuchstaben und ein Sonderzeichen enthalten.",
                        [nameof(NewPassword)]);
                }

                if (string.IsNullOrWhiteSpace(OldPassword))
                    yield return new System.ComponentModel.DataAnnotations.ValidationResult("Altes Passwort ist erforderlich.", [nameof(OldPassword)]);



                if (NewPassword != PasswordAgain)
                    yield return new System.ComponentModel.DataAnnotations.ValidationResult("Passwörter stimmen nicht überein.", [nameof(PasswordAgain)]);
            }
            else if (UpdateType == UpdateTypeEnum.Birthday)
            {
                if (string.IsNullOrWhiteSpace(BirthDate))
                    yield return new System.ComponentModel.DataAnnotations.ValidationResult("Bitte Geburtsdatum eingeben", [nameof(BirthDate)]);
            }
            else if (UpdateType == UpdateTypeEnum.AccountDelete)
            {
                if (string.IsNullOrWhiteSpace(OldPassword))
                    yield return new System.ComponentModel.DataAnnotations.ValidationResult("Passwort ist erforderlich.", [nameof(OldPassword)]);
            }
        }
    }
    public enum UpdateTypeEnum : byte
    {
        Password,
        Birthday,
        AccountDelete
    }
}
