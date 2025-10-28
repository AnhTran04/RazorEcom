namespace RazorEcom.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public  ApplicationUser User { get; set; } = null!;
        public string Status { get; set; } = "pending";
        public decimal Total { get; set; }
        public int ShippingAddressId { get; set; }
        public  AddressBook ShippingAddress { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public  ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public  Payment? Payment { get; set; }
    }
}