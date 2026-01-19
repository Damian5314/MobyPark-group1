using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using v2.Validation;
using Xunit;

namespace v2.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("AB-12-34", true)]  // XX-99-99
        [InlineData("12-34-AB", true)]  // 99-99-XX
        [InlineData("12-AB-34", true)]  // 99-XX-99
        [InlineData("AB-12-CD", true)]  // XX-99-XX
        [InlineData("AB-CD-12", true)]  // XX-XX-99
        [InlineData("12-AB-CD", true)]  // 99-XX-XX
        [InlineData("12-ABC-3", true)]  // 99-XXX-9
        [InlineData("1-ABC-12", true)]  // 9-XXX-99
        [InlineData("AB-123-C", true)]  // XX-999-X
        [InlineData("A-123-BC", true)]  // X-999-XX
        [InlineData("ABC-12-D", true)]  // XXX-99-X
        [InlineData("1-AB-123", true)]  // 9-XX-999
        [InlineData("123-AB-4", true)]  // 999-XX-9
        [InlineData("INVALID", false)]
        [InlineData("AB-12-3", false)]
        [InlineData("AB-1234", false)]
        [InlineData("123ABC", false)]
        [InlineData("AB12CD", false)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public void DutchLicensePlate_Should_Validate_Correctly(string? licensePlate, bool expectedValid)
        {
            var attribute = new DutchLicensePlateAttribute();
            var context = new ValidationContext(new object()) { MemberName = "LicensePlate" };

            var result = attribute.GetValidationResult(licensePlate, context);

            if (expectedValid)
            {
                result.Should().Be(ValidationResult.Success);
            }
            else
            {
                result.Should().NotBe(ValidationResult.Success);
                result?.ErrorMessage.Should().NotBeNullOrEmpty();
            }
        }

        [Theory]
        [InlineData("0612345678", true)]
        [InlineData("0698765432", true)]
        [InlineData("0600000000", true)]
        [InlineData("0699999999", true)]
        [InlineData("1234567890", false)]  // Doesn't start with 06
        [InlineData("06123456", false)]    // Too short
        [InlineData("061234567890", false)] // Too long
        [InlineData("06-12345678", false)]  // Contains special character
        [InlineData("06 12345678", false)]  // Contains space
        [InlineData("+31612345678", false)] // Contains + and country code
        [InlineData("06abcd1234", false)]   // Contains letters
        [InlineData("", false)]
        [InlineData(null, false)]
        public void DutchPhoneNumber_Should_Validate_Correctly(string? phoneNumber, bool expectedValid)
        {
            var attribute = new DutchPhoneNumberAttribute();
            var context = new ValidationContext(new object()) { MemberName = "Phone" };

            var result = attribute.GetValidationResult(phoneNumber, context);

            if (expectedValid)
            {
                result.Should().Be(ValidationResult.Success);
            }
            else
            {
                result.Should().NotBe(ValidationResult.Success);
                result?.ErrorMessage.Should().NotBeNullOrEmpty();
            }
        }

        [Fact]
        public void DutchLicensePlate_Should_Return_Correct_Error_Message()
        {
            var attribute = new DutchLicensePlateAttribute();
            var context = new ValidationContext(new object()) { MemberName = "LicensePlate" };

            var result = attribute.GetValidationResult("INVALID", context);

            result.Should().NotBe(ValidationResult.Success);
            result?.ErrorMessage.Should().Contain("Dutch format");
            result?.ErrorMessage.Should().Contain("XX-99-99");
        }

        [Fact]
        public void DutchPhoneNumber_Should_Return_Correct_Error_Message()
        {
            var attribute = new DutchPhoneNumberAttribute();
            var context = new ValidationContext(new object()) { MemberName = "Phone" };

            var result = attribute.GetValidationResult("1234567890", context);

            result.Should().NotBe(ValidationResult.Success);
            result?.ErrorMessage.Should().Contain("06");
            result?.ErrorMessage.Should().Contain("10 digits");
        }
    }
}
