using Newtonsoft.Json;

namespace v2.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = "USER";

        [JsonProperty("created_at")]
        public DateTime CreatedAt { get; set; }
        
        [JsonProperty("birth_year")]
        public int BirthYear { get; set; }
        public bool Active { get; set; }
    }
}