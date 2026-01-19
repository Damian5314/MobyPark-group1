using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace v2.Validation
{
    public class DutchLicensePlateAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return new ValidationResult("License plate is required.");
            }

            var licensePlate = value.ToString()!;

            // Dutch license plate formats:
            // XX-99-99, 99-99-XX, 99-XX-99, XX-99-XX, XX-XX-99, 99-XX-XX
            // 99-XXX-9, 9-XXX-99, XX-999-X, X-999-XX, XXX-99-X, 9-XX-999, 999-XX-9
            var patterns = new[]
            {
                @"^[A-Z]{2}-\d{2}-\d{2}$",      // XX-99-99
                @"^\d{2}-\d{2}-[A-Z]{2}$",      // 99-99-XX
                @"^\d{2}-[A-Z]{2}-\d{2}$",      // 99-XX-99
                @"^[A-Z]{2}-\d{2}-[A-Z]{2}$",   // XX-99-XX
                @"^[A-Z]{2}-[A-Z]{2}-\d{2}$",   // XX-XX-99
                @"^\d{2}-[A-Z]{2}-[A-Z]{2}$",   // 99-XX-XX
                @"^\d{2}-[A-Z]{3}-\d{1}$",      // 99-XXX-9
                @"^\d{1}-[A-Z]{3}-\d{2}$",      // 9-XXX-99
                @"^[A-Z]{2}-\d{3}-[A-Z]{1}$",   // XX-999-X
                @"^[A-Z]{1}-\d{3}-[A-Z]{2}$",   // X-999-XX
                @"^[A-Z]{3}-\d{2}-[A-Z]{1}$",   // XXX-99-X
                @"^\d{1}-[A-Z]{2}-\d{3}$",      // 9-XX-999
                @"^\d{3}-[A-Z]{2}-\d{1}$"       // 999-XX-9
            };

            foreach (var pattern in patterns)
            {
                if (Regex.IsMatch(licensePlate, pattern))
                {
                    return ValidationResult.Success;
                }
            }

            return new ValidationResult(
                "License plate must match one of the Dutch formats: " +
                "XX-99-99, 99-99-XX, 99-XX-99, XX-99-XX, XX-XX-99, 99-XX-XX, " +
                "99-XXX-9, 9-XXX-99, XX-999-X, X-999-XX, XXX-99-X, 9-XX-999, 999-XX-9 " +
                "(X = letter, 9 = digit)"
            );
        }
    }
}
