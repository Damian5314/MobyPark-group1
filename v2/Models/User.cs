namespace v2.Models
{
    public class UserProfile
    {
        public string Id { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;

        public string Name { get; set; } = null!;
        public string? Email { get; set; }
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = "USER";
        public DateTime CreatedAt { get; set; }
        public int BirthYear { get; set; }
        public bool Active { get; set; }
    }
}