public class PaymentCreateDto
{
    public string Initiator { get; set; } = null!;
    public string LicensePlate { get; set; } = null!; // used to select sessions
    public string Method { get; set; } = "method";
    public string Bank { get; set; } = "bank";
}