namespace RazorEcom.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public  Order Order { get; set; } = null!;
        public string Method { get; set; } = string.Empty;
        public string Status { get; set; } = "pending";
        public string? TransactionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}