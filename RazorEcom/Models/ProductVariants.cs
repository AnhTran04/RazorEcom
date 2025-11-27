    using System.Text.Json.Serialization;

    namespace RazorEcom.Models
    {
        public class ProductVariants
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string? ImageUrl { get; set; }
            public string Sku { get; set; } = string.Empty;
            public string? Size { get; set; }
            public string? Color { get; set; }
            public string? Material { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
            public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
            [JsonIgnore]
            public  Products Product { get; set; } = null!;
        
        }
    }