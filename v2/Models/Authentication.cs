namespace v2.Models
{
    using System.ComponentModel.DataAnnotations;
    using v2.Validation;

    public class RegisterRequest
    {
        [Required]
        public string Username { get; set; } = "";

        [Required, MinLength(6)]
        public string Password { get; set; } = "";

        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = "";

        [Required]
        [DutchPhoneNumber]
        public string Phone { get; set; } = "";

        [Required]
        [Range(1900, 2025)]
        public int BirthYear { get; set; }
    }


    public class LoginRequest
    {
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}