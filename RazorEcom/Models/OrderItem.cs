namespace RazorEcom.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public  Order Order { get; set; } = null!;
        public int VariantId { get; set; }
        public  ProductVariants Variant { get; set; } = null!;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; } // Giá tại thời điểm mua
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}