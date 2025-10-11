using System;

namespace v2.Models
{
    public class Payment
    {
        public string Transaction { get; set; } = null!;
        public decimal Amount { get; set; }
        public string? CoupledTo { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;         // e.g., "Pending", "Completed", "Failed"
        public string PaymentMethod { get; set; } = null!; // e.g., "CreditCard", "PayPal"
    }

    public class RefundRequest
    {
        public string Transaction { get; set; } = null!;
        public decimal Amount { get; set; }
        public string CoupledTo { get; set; } = null!;
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = null!;
        public string PaymentMethod { get; set; } = null!;
    }
}
