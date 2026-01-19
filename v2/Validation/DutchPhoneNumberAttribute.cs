using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace v2.Validation
{
    public class DutchPhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("Phone number is required.");
            }

            var phoneNumber = value.ToString()!;

            // Must start with 06 and have exactly 10 digits total
            // No characters or strings allowed, only numbers
            if (!Regex.IsMatch(phoneNumber, @"^06\d{8}$"))
            {
                return new ValidationResult(
                    "Phone number must start with 06 and contain exactly 10 digits (e.g., 0612345678)"
                );
            }

            return ValidationResult.Success;
        }
    }
}
