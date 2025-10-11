using System.Collections.Generic;
using System.Linq;

namespace v2.Models
{
    public class Billing
    {
        public string? User { get; set; }
        public List<Payment> Payments { get; set; } = new();
        public decimal TotalAmount => Payments.Sum(p => p.Amount);
    }
}
