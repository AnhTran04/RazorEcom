using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using RazorEcom.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

// Sử dụng alias để tránh xung đột tên với namespace 'Pages.Cart'
using CartModel = global::RazorEcom.Models.Cart;

namespace RazorEcom.Pages.Products
{
    // Đặt ValidateAntiForgeryToken ở đây để bảo vệ tất cả các POST handler
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryService _categoryService; // <-- TỐI ƯU HÓA

        // TỐI ƯU HÓA: Sử dụng Dependency Injection, không 'new'
        public IndexModel(ApplicationDbContext context, CategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService; // <-- Được inject
        }

        // ================================================
        // CÁC THUỘC TÍNH BINDING CHO BỘ LỌC (TỪ URL)
        // ================================================

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; }

        // (THÊM LẠI) Các bộ lọc bị thiếu
        [BindProperty(SupportsGet = true)]
        public decimal? MinPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public decimal? MaxPrice { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Brand { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Color { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Size { get; set; }


        // ================================================
        // CÁC THUỘC TÍNH ĐỂ HIỂN THỊ TRANG
        // ================================================

        public List<Category> Categories { get; set; } = new List<Category>();
        public List<ProductVariants> ProductVariants { get; set; } = new List<ProductVariants>();
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>();

        // (THÊM LẠI) Danh sách cho các dropdown của bộ lọc
        public List<string> Brands { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();


        // ================================================
        // HANDLER GET (TẢI TRANG)
        // ================================================

        public async Task OnGetAsync()
        {
            // 1. Khởi tạo các tùy chọn (options) cho bộ lọc
            InitializeSortOptions();
            
            // (THÊM LẠI) Tải dữ liệu cho các bộ lọc dropdown
            Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            Brands = await _context.Products.Where(p => p.Brand != null).Select(p => p.Brand).Distinct().OrderBy(b => b).ToListAsync();
            Colors = await _context.ProductVariants.Where(v => v.Color != null).Select(v => v.Color).Distinct().OrderBy(c => c).ToListAsync();
            Sizes = await _context.ProductVariants.Where(v => v.Size != null).Select(v => v.Size).Distinct().OrderBy(s => s).ToListAsync();

            // 2. Xây dựng truy vấn (query) sản phẩm
            var query = _context.ProductVariants.Include(v => v.Product).AsQueryable();

            // 3. Áp dụng các bộ lọc (filter)
            
            // (GIỮ LẠI) Logic lọc danh mục con thông minh của bạn
            if (CategoryId.HasValue)
            {
                var categoryIds = _categoryService.GetAllChildCategoryIds(CategoryId.Value);
                query = query.Where(v => categoryIds.Contains(v.Product.CategoryId));
            }

            // (THÊM LẠI) Các bộ lọc bị thiếu
            if (MinPrice.HasValue)
            {
                query = query.Where(v => v.Price >= MinPrice.Value);
            }
            if (MaxPrice.HasValue)
            {
                query = query.Where(v => v.Price <= MaxPrice.Value);
            }
            if (!string.IsNullOrEmpty(Brand))
            {
                query = query.Where(v => v.Product.Brand == Brand);
            }
            if (!string.IsNullOrEmpty(Color))
            {
                query = query.Where(v => v.Color == Color);
            }
            if (!string.IsNullOrEmpty(Size))
            {
                query = query.Where(v => v.Size == Size);
            }

            // 4. Áp dụng sắp xếp (sort)
            // (SỬA LẠI) Đồng bộ các giá trị (key) với file .cshtml và sửa logic "newest"
            switch (SortBy)
            {
                case "price-asc":
                    query = query.OrderBy(v => v.Price);
                    break;
                case "price-desc":
                    query = query.OrderByDescending(v => v.Price);
                    break;
                case "name-asc":
                    query = query.OrderBy(v => v.Product.Name);
                    break;
                default: // "newest" hoặc mặc định
                    // SỬA LẠI: Sắp xếp theo CreatedAt (mới nhất) thay vì Id
                    query = query.OrderByDescending(v => v.CreatedAt); 
                    break;
            }

            // 5. Thực thi truy vấn
            ProductVariants = await query.AsNoTracking().ToListAsync();
        }

        // ================================================
        // HANDLER POST (THÊM VÀO GIỎ)
        // ================================================
        
        public async Task<IActionResult> OnPostAddToCart(int variantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            CartModel cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    VariantId = variantId,
                    Quantity = 1,
                    CreatedAt = DateTime.Now
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            
            // (CẢI THIỆN UX) Quay lại trang Products với các filter được giữ nguyên
            return RedirectToPage(new 
            { 
                CategoryId = CategoryId,
                SortBy = SortBy,
                MinPrice = MinPrice,
                MaxPrice = MaxPrice,
                Brand = Brand,
                Color = Color,
                Size = Size
            });
        }

        // ================================================
        // PHƯƠNG THỨC TRỢ GIÚP
        // ================================================

        private async Task<CartModel> GetOrCreateCartAsync(string userId)
        {
            CartModel cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new CartModel
                {
                    CreatedAt = DateTime.Now,
                    UserId = userId
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            return cart;
        }

        // (SỬA LẠI) Dùng lại hàm InitializeSortOptions
        // để đồng bộ giá trị (key) với file .cshtml
        private void InitializeSortOptions()
        {
            SortOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "newest", Text = "Mới nhất" },
                new SelectListItem { Value = "price-asc", Text = "Giá: Thấp đến Cao" },
                new SelectListItem { Value = "price-desc", Text = "Giá: Cao đến Thấp" },
                new SelectListItem { Value = "name-asc", Text = "Tên: A-Z" },
            };
        }
    }
}