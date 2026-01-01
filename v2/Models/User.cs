using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace v2.Models
{
    public class UserProfile
    {
        public int? company_id { get; set; }
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        public string Password { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;


        [Required]
        [RegularExpression(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!;

        [Required]
        public string Phone { get; set; } = null!;

        public string Role { get; set; }


        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }


        [JsonProperty("birth_year")]
        [Required]
        [Range(1900, 2025, ErrorMessage = "BirthYear must be between 1900 and 2025.")]
        public int BirthYear { get; set; }

        public bool Active { get; set; }
    }
}