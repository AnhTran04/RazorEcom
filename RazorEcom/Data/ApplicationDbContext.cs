// ĐẢM BẢO BẠN CÓ ĐỦ CÁC DÒNG USING NÀY
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace RazorEcom.Data
{
    // Class của bạn PHẢI kế thừa từ IdentityDbContext<ApplicationUser>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        // Constructor này cần "using Microsoft.EntityFrameworkCore;" để hiểu DbContextOptions
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AddressBook> AddressBooks { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<ProductVariants> ProductVariants { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Rất quan trọng khi dùng Identity

            // Định nghĩa các mối quan hệ phức tạp ở đây (Fluent API)
            #region Fluent API Configurations
            builder.Entity<Category>()
                .HasOne(c => c.Parent)
                .WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId);

            builder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId);

            builder.Entity<ProductVariants>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            builder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");
            builder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            builder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            #endregion

            // --- BẮT ĐẦU PHẦN DATA SEEDING ---
            #region Data Seeding
            // Thêm dữ liệu cho Bảng Categories
            builder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Áo Thun", Description = "Các loại áo thun nam nữ" },
                new Category { Id = 2, Name = "Quần Jeans", Description = "Các loại quần jeans thời trang" }
            );
            var fixedTime = new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc);

            // Thêm dữ liệu cho Bảng Products
            builder.Entity<Products>().HasData(
                    new Products { Id = 1, Name = "Áo Thun Cổ Tròn Basic", Description = "Áo thun cotton 100%, thoáng mát.", Brand = "Coolmate", CategoryId = 1, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?basic-t-shirt" },
                new Products { Id = 2, Name = "Áo Thun Polo", Description = "Áo polo lịch lãm, phù hợp công sở.", Brand = "Uniqlo", CategoryId = 1, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?polo-shirt" },
                new Products { Id = 3, Name = "Áo Thun In Hình", Description = "Áo thun in hình độc đáo, cá tính.", Brand = "H&M", CategoryId = 1, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?graphic-t-shirt" },
                new Products { Id = 4, Name = "Áo Thun Ba Lỗ", Description = "Áo ba lỗ thể thao, năng động.", Brand = "Nike", CategoryId = 1, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?tank-top" },
                new Products { Id = 5, Name = "Áo Thun Dài Tay", Description = "Áo thun dài tay giữ ấm nhẹ.", Brand = "Zara", CategoryId = 1, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?long-sleeve-shirt" },
                new Products { Id = 6, Name = "Quần Jeans Slim-fit", Description = "Quần jeans ống côn, tôn dáng.", Brand = "Levi's", CategoryId = 2, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?slim-fit-jeans" },
                new Products { Id = 7, Name = "Quần Jeans Rách Gối", Description = "Quần jeans rách gối phong cách.", Brand = "Topman", CategoryId = 2, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?ripped-jeans" },
                new Products { Id = 8, Name = "Quần Jeans Ống Rộng", Description = "Quần jeans ống rộng thoải mái.", Brand = "Pull&Bear", CategoryId = 2, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?wide-leg-jeans" },
                new Products { Id = 9, Name = "Quần Jeans Đen Trơn", Description = "Quần jeans đen cơ bản, dễ phối đồ.", Brand = "Calvin Klein", CategoryId = 2, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?black-jeans" },
                new Products { Id = 10, Name = "Quần Short Jeans", Description = "Quần short jeans năng động cho mùa hè.", Brand = "ASOS", CategoryId = 2, Status = "active", ImageUrl = "https://source.unsplash.com/300x300/?jean-shorts" }
            );
            // Thêm dữ liệu cho Bảng ProductVariants
            builder.Entity<ProductVariants>().HasData(
           // Áo Thun Cổ Tròn Basic (Id = 1)
           new ProductVariants { Id = 1, ProductId = 1, Size = "M", Color = "Trắng", Price = 250000m, Quantity = 100, Sku = "AOT-CL-M-TR", ImageUrl = "https://source.unsplash.com/600x600?tshirt,white", CreatedAt = fixedTime },
           new ProductVariants { Id = 2, ProductId = 1, Size = "L", Color = "Đen", Price = 250000m, Quantity = 100, Sku = "AOT-CL-L-DE", ImageUrl = "https://source.unsplash.com/600x600?tshirt,black", CreatedAt = fixedTime },

           // Áo Thun Polo (Id = 2)
           new ProductVariants { Id = 3, ProductId = 2, Size = "S", Color = "Xanh Navy", Price = 450000m, Quantity = 80, Sku = "AOP-UQ-S-XA", ImageUrl = "https://source.unsplash.com/600x600?polo,navy", CreatedAt = fixedTime },
           new ProductVariants { Id = 4, ProductId = 2, Size = "M", Color = "Xám", Price = 450000m, Quantity = 80, Sku = "AOP-UQ-M-XA", ImageUrl = "https://source.unsplash.com/600x600?polo,grey", CreatedAt = fixedTime },

           // Áo Thun In Hình (Id = 3)
           new ProductVariants { Id = 5, ProductId = 3, Size = "M", Color = "Vàng", Price = 320000m, Quantity = 50, Sku = "AOI-HM-M-VA", ImageUrl = "https://source.unsplash.com/600x600?graphic,tshirt,yellow", CreatedAt = fixedTime },

           // Áo Thun Ba Lỗ (Id = 4)
           new ProductVariants { Id = 6, ProductId = 4, Size = "L", Color = "Đen", Price = 280000m, Quantity = 70, Sku = "AOB-NK-L-DE", ImageUrl = "https://source.unsplash.com/600x600?tanktop,black", CreatedAt = fixedTime },
           new ProductVariants { Id = 7, ProductId = 4, Size = "XL", Color = "Trắng", Price = 280000m, Quantity = 70, Sku = "AOB-NK-XL-TR", ImageUrl = "https://source.unsplash.com/600x600?tanktop,white", CreatedAt = fixedTime },

           // Áo Thun Dài Tay (Id = 5)
           new ProductVariants { Id = 8, ProductId = 5, Size = "M", Color = "Rêu", Price = 380000m, Quantity = 60, Sku = "AOD-ZR-M-RE", ImageUrl = "https://source.unsplash.com/600x600?longsleeve,shirt,olive", CreatedAt = fixedTime },
           new ProductVariants { Id = 9, ProductId = 5, Size = "L", Color = "Be", Price = 380000m, Quantity = 60, Sku = "AOD-ZR-L-BE", ImageUrl = "https://source.unsplash.com/600x600?longsleeve,shirt,beige", CreatedAt = fixedTime },

           // Quần Jeans Slim-fit (Id = 6)
           new ProductVariants { Id = 10, ProductId = 6, Size = "30", Color = "Xanh Đậm", Price = 750000m, Quantity = 100, Sku = "QJS-LV-30-XD", ImageUrl = "https://source.unsplash.com/600x600?slim,jeans,darkblue", CreatedAt = fixedTime },
           new ProductVariants { Id = 11, ProductId = 6, Size = "32", Color = "Xanh Đậm", Price = 750000m, Quantity = 100, Sku = "QJS-LV-32-XD", ImageUrl = "https://source.unsplash.com/600x600?slim,jeans,darkblue", CreatedAt = fixedTime },

           // Quần Jeans Rách Gối (Id = 7)
           new ProductVariants { Id = 12, ProductId = 7, Size = "31", Color = "Xanh Nhạt", Price = 800000m, Quantity = 50, Sku = "QJR-TP-31-XN", ImageUrl = "https://source.unsplash.com/600x600?ripped,jeans,lightblue", CreatedAt = fixedTime },
           new ProductVariants { Id = 13, ProductId = 7, Size = "32", Color = "Đen", Price = 820000m, Quantity = 40, Sku = "QJR-TP-32-DE", ImageUrl = "https://source.unsplash.com/600x600?ripped,jeans,black", CreatedAt = fixedTime },

           // Quần Jeans Ống Rộng (Id = 8)
           new ProductVariants { Id = 14, ProductId = 8, Size = "M", Color = "Trắng", Price = 650000m, Quantity = 60, Sku = "QJO-PB-M-TR", ImageUrl = "https://source.unsplash.com/600x600?wide,jeans,white", CreatedAt = fixedTime },
           new ProductVariants { Id = 15, ProductId = 8, Size = "L", Color = "Be", Price = 650000m, Quantity = 60, Sku = "QJO-PB-L-BE", ImageUrl = "https://source.unsplash.com/600x600?wide,jeans,beige", CreatedAt = fixedTime },

           // Quần Jeans Đen Trơn (Id = 9)
           new ProductVariants { Id = 16, ProductId = 9, Size = "30", Color = "Đen", Price = 700000m, Quantity = 90, Sku = "QJD-CK-30-DE", ImageUrl = "https://source.unsplash.com/600x600?black,jeans,plain", CreatedAt = fixedTime },
           new ProductVariants { Id = 17, ProductId = 9, Size = "32", Color = "Đen", Price = 700000m, Quantity = 90, Sku = "QJD-CK-32-DE", ImageUrl = "https://source.unsplash.com/600x600?black,jeans", CreatedAt = fixedTime },
           new ProductVariants { Id = 18, ProductId = 9, Size = "34", Color = "Đen", Price = 700000m, Quantity = 90, Sku = "QJD-CK-34-DE", ImageUrl = "https://source.unsplash.com/600x600?plain,black,jeans", CreatedAt = fixedTime },

           // Quần Short Jeans (Id = 10)
           new ProductVariants { Id = 19, ProductId = 10, Size = "M", Color = "Xanh Nhạt", Price = 380000m, Quantity = 100, Sku = "AQS-AS-M-XN", ImageUrl = "https://source.unsplash.com/600x600?denim,shorts,light", CreatedAt = fixedTime },
           new ProductVariants { Id = 20, ProductId = 10, Size = "L", Color = "Xanh Đậm", Price = 380000m, Quantity = 100, Sku = "AQS-AS-L-XD", ImageUrl = "https://source.unsplash.com/600x600?denim,shorts,dark", CreatedAt = fixedTime }
       );
            #endregion
        }
    }
}
