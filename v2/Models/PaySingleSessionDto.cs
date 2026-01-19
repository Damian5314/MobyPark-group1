public class PaySingleSessionDto
{
    public string LicensePlate { get; set; } = null!;
    public int SessionId { get; set; }
    public string Method { get; set; } = "card";
    public string Initiator { get; set; } = null!;
    public string Bank { get; set; } = "internal";
}

public class UserPaySingleSessionDto
{
    public string LicensePlate { get; set; } = null!;
    public int SessionId { get; set; }
    public string Method { get; set; } = "card";
    public string Bank { get; set; } = "internal";
}