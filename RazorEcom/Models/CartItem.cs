namespace RazorEcom.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public  Cart Cart { get; set; } = null!;
        public int VariantId { get; set; }
        public  ProductVariants Variant { get; set; } = null!;
        public int Quantity { get; set; } = 1;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}