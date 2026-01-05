namespace v2.Models
{
    public class ChangePasswordRequest
    {
        public string? CurrentPassword { get; set; } // required for normal user
        public string NewPassword { get; set; } = null!;
    }
}
