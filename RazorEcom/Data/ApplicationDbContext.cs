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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Rất quan trọng khi dùng Identity

            var seedTime = DateTime.SpecifyKind(new DateTime(2025, 1, 1), DateTimeKind.Utc);
            // Định nghĩa các mối quan hệ phức tạp ở đây (Fluent API)
            #region Fluent API Configurations
            modelBuilder.Entity<Category>()
                .HasMany(c => c.Children)
                .WithOne(c => c.Parent)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId);

            modelBuilder.Entity<Order>()
                .HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId);

            modelBuilder.Entity<ProductVariants>().Property(p => p.Price).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Order>().Property(o => o.Total).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<OrderItem>().Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Payment>().Property(p => p.Amount).HasColumnType("decimal(18,2)");
            #endregion

            // --- BẮT ĐẦU PHẦN DATA SEEDING ---
            #region Data Seeding
            // Thêm dữ liệu cho Bảng Categories
            modelBuilder.Entity<Category>().HasData(
                // Danh mục gốc
                new Category { Id = 1, Name = "Thời trang Nam", ParentId = null, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 2, Name = "Thời trang Nữ", ParentId = null, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 7, Name = "Phụ kiện", ParentId = null, CreatedAt = seedTime, UpdatedAt = seedTime },

                // Danh mục con Cấp 1
                new Category { Id = 3, Name = "Áo Nam", ParentId = 1, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 4, Name = "Quần Nam", ParentId = 1, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 8, Name = "Giày Nam", ParentId = 1, CreatedAt = seedTime, UpdatedAt = seedTime },

                new Category { Id = 5, Name = "Đầm Nữ", ParentId = 2, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 6, Name = "Áo Nữ", ParentId = 2, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 9, Name = "Giày Nữ", ParentId = 2, CreatedAt = seedTime, UpdatedAt = seedTime },

                new Category { Id = 10, Name = "Túi xách", ParentId = 7, CreatedAt = seedTime, UpdatedAt = seedTime },
                new Category { Id = 11, Name = "Cà vạt & Nơ", ParentId = 7, CreatedAt = seedTime, UpdatedAt = seedTime }
            );

            // Thêm dữ liệu cho Bảng Products
            modelBuilder.Entity<Products>().HasData(
                new Products
                {
                    Id = 1,
                    Name = "Áo Thun Cổ Tròn Trơn",
                    Description = "Áo thun nam cơ bản, chất liệu cotton 100% thoáng mát.",
                    Brand = "Coolmate",
                    CategoryId = 3, // Lấy ID từ "Áo Nam"
                    Status = "active",
                    ImageUrl = "/images/products/t-shirt/t-shirt-white.jpg",
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                new Products
                {
                    Id = 2,
                    Name = "Quần Jeans Nam Slim-fit",
                    Description = "Quần jeans nam phom slim-fit, co giãn nhẹ.",
                    Brand = "Levi's",
                    CategoryId = 4, // Lấy ID từ "Quần Nam"
                    Status = "active",
                    ImageUrl = "/images/products/Jeans/jeans-slim-fit-product.jpg",
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                new Products
                {
                    Id = 3,
                    Name = "Đầm Hoa Nhí Vintage",
                    Description = "Đầm voan hoa nhí phong cách vintage, phù hợp dạo phố.",
                    Brand = "Zara",
                    CategoryId = 5, // Lấy ID từ "Đầm Nữ"
                    Status = "active",
                    ImageUrl = "/images/products/dress/floral-dress.jpg",
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                // --- SẢN PHẨM MỚI ---
                new Products
                {
                    Id = 4,
                    Name = "Áo Sơ Mi Nam Dài Tay",
                    Description = "Áo sơ mi nam chất liệu kate cao cấp, ít nhăn.",
                    Brand = "An Phước",
                    CategoryId = 3, // Lấy ID từ "Áo Nam"
                    Status = "active",
                    ImageUrl = "/images/products/shirt/shirt-product.jpg",
                    CreatedAt = seedTime,
                    UpdatedAt = seedTime
                },
                 new Products
                 {
                     Id = 5,
                     Name = "Áo Blouse Nữ Tay Phồng",
                     Description = "Áo blouse nữ kiểu Hàn Quốc, chất liệu lụa cát.",
                     Brand = "Elise",
                     CategoryId = 6, // Lấy ID từ "Áo Nữ"
                     Status = "active",
                     ImageUrl = "/images/products/blouse/blouse.jpg",
                     CreatedAt = seedTime,
                     UpdatedAt = seedTime
                 },
                 new Products
                 {
                     Id = 6,
                     Name = "Giày Air Force 1",
                     Description = "Giày sneaker nam da PU, đế cao su, phong cách basic.",
                     Brand = "Nike",
                     CategoryId = 8, // Lấy ID từ "Giày Nam"
                     Status = "active",
                     ImageUrl = "/images/products/sneaker/nike-af1.jpg",
                     CreatedAt = seedTime,
                     UpdatedAt = seedTime
                 },
                 new Products
                 {
                     Id = 7,
                     Name = "Túi Xách Nữ Đeo Chéo",
                     Description = "Túi xách nữ da thật, thiết kế tối giản và sang trọng.",
                     Brand = "Vascara",
                     CategoryId = 10, // Lấy ID từ "Túi xách"
                     Status = "active",
                     ImageUrl = "/images/products/handbag/handbag-black.jpg",
                     CreatedAt = seedTime,
                     UpdatedAt = seedTime
                 }
            );
            // Thêm dữ liệu cho Bảng ProductVariants
            modelBuilder.Entity<ProductVariants>().HasData(
            // Các biến thể cho "Áo Thun Cổ Tròn Trơn" (ProductId = 1)
                new ProductVariants { Id = 1, ProductId = 1, Sku = "ATN-TR-M", Size = "M", Color = "Trắng", Price = 150000, Quantity = 50, ImageUrl = "/images/products/t-shirt/t-shirt-white.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 2, ProductId = 1, Sku = "ATN-TR-L", Size = "L", Color = "Trắng", Price = 150000, Quantity = 50, ImageUrl = "/images/products/t-shirt/t-shirt-white.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 3, ProductId = 1, Sku = "ATN-DE-M", Size = "M", Color = "Đen", Price = 150000, Quantity = 40, ImageUrl = "/images/products/t-shirt/t-shirt-black.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 4, ProductId = 1, Sku = "ATN-DE-L", Size = "L", Color = "Đen", Price = 150000, Quantity = 40, ImageUrl = "/images/products/t-shirt/t-shirt-black.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // Các biến thể cho "Quần Jeans Nam Slim-fit" (ProductId = 2)
                new ProductVariants { Id = 5, ProductId = 2, Sku = "QJN-XD-30", Size = "30", Color = "Xanh Đậm", Price = 790000, Quantity = 20, ImageUrl = "/images/products/Jeans/jeans-slim-fit-variant.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 6, ProductId = 2, Sku = "QJN-XD-32", Size = "32", Color = "Xanh Đậm", Price = 790000, Quantity = 20, ImageUrl = "/images/products/Jeans/jeans-slim-fit-variant.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // Các biến thể cho "Đầm Hoa Nhí Vintage" (ProductId = 3)
                new ProductVariants { Id = 7, ProductId = 3, Sku = "DHN-VA-S", Size = "S", Color = "Vàng", Price = 550000, Quantity = 15, ImageUrl = "/images/products/dress/floral-dress.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 8, ProductId = 3, Sku = "DHN-VA-M", Size = "M", Color = "Vàng", Price = 550000, Quantity = 15, ImageUrl = "/images/products/dress/floral-dress.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // --- BIẾN THỂ MỚI ---
                // Các biến thể cho "Áo Sơ Mi Nam Dài Tay" (ProductId = 4)
                new ProductVariants { Id = 9, ProductId = 4, Sku = "SMN-TR-S", Size = "S", Color = "Trắng", Price = 450000, Quantity = 30, ImageUrl = "/images/products/shirt/shirt-white.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 10, ProductId = 4, Sku = "SMN-TR-M", Size = "M", Color = "Trắng", Price = 450000, Quantity = 30, ImageUrl = "/images/products/shirt/shirt-white.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 11, ProductId = 4, Sku = "SMN-XA-M", Size = "M", Color = "Xanh Nhạt", Price = 450000, Quantity = 25, ImageUrl = "/images/products/shirt/shirt-blue.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // Các biến thể cho "Áo Blouse Nữ Tay Phồng" (ProductId = 5)
                new ProductVariants { Id = 12, ProductId = 5, Sku = "BLN-KE-S", Size = "S", Color = "Kẻ sọc", Price = 320000, Quantity = 20, ImageUrl = "/images/products/blouse/blouse.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 13, ProductId = 5, Sku = "BLN-KE-M", Size = "M", Color = "Kẻ sọc", Price = 320000, Quantity = 20, ImageUrl = "/images/products/blouse/blouse.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // Các biến thể cho "Giày Sneaker Nam Trắng" (ProductId = 6)
                new ProductVariants { Id = 14, ProductId = 6, Sku = "GSN-TR-40", Size = "40", Color = "Trắng", Price = 990000, Quantity = 10, ImageUrl = "/images/products/sneaker/nike-af1.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 15, ProductId = 6, Sku = "GSN-TR-41", Size = "41", Color = "Trắng", Price = 990000, Quantity = 10, ImageUrl = "/images/products/sneaker/nike-af1.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 16, ProductId = 6, Sku = "GSN-TR-42", Size = "42", Color = "Trắng", Price = 990000, Quantity = 10, ImageUrl = "/images/products/sneaker/nike-af1.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },

                // Các biến thể cho "Túi Xách Nữ Đeo Chéo" (ProductId = 7)
                // Lưu ý: Size = "N/A" (Không áp dụng)
                new ProductVariants { Id = 17, ProductId = 7, Sku = "TXN-DE-N", Size = "N/A", Color = "Đen", Price = 1200000, Quantity = 15, ImageUrl = "/images/products/handbag/handbag-black.jpg", CreatedAt = seedTime, UpdatedAt = seedTime },
                new ProductVariants { Id = 18, ProductId = 7, Sku = "TXN-BE-N", Size = "N/A", Color = "Đỏ", Price = 1200000, Quantity = 10, ImageUrl = "/images/products/handbag/handbag-red.jpg", CreatedAt = seedTime, UpdatedAt = seedTime }
            );
            #endregion
        }
    }
}
