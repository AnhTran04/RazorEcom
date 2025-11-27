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
    [ValidateAntiForgeryToken]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly CategoryService _categoryService;

        public IndexModel(ApplicationDbContext context, CategoryService categoryService)
        {
            _context = context;
            _categoryService = categoryService;
        }

        // ================================================
        // CÁC THUỘC TÍNH BINDING CHO BỘ LỌC
        // ================================================

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; }

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
        
        // THAY ĐỔI: Hiển thị danh sách Product thay vì Variant
        public List<RazorEcom.Models.Products> Products { get; set; } = new List<RazorEcom.Models.Products>();
        
        public List<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>();

        public List<ProductVariants> ProductVariants { get; set; } = new List<ProductVariants>();

        public List<string> Brands { get; set; } = new List<string>();
        public List<string> Colors { get; set; } = new List<string>();
        public List<string> Sizes { get; set; } = new List<string>();


        // ================================================
        // HANDLER GET (TẢI TRANG)
        // ================================================

        public async Task OnGetAsync()
        {
            InitializeSortOptions();
            
            // Tải dữ liệu cho các bộ lọc dropdown
            Categories = await _context.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
            Brands = await _context.Products.Where(p => p.Brand != null).Select(p => p.Brand!).Distinct().OrderBy(b => b).ToListAsync();
            Colors = await _context.ProductVariants.Where(v => v.Color != null).Select(v => v.Color!).Distinct().OrderBy(c => c).ToListAsync();
            Sizes = await _context.ProductVariants.Where(v => v.Size != null).Select(v => v.Size!).Distinct().OrderBy(s => s).ToListAsync();

            // 2. Xây dựng truy vấn (query) SẢN PHẨM (Products)
            // Include Variants để tính giá min/max
            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .AsQueryable();

            // 3. Áp dụng các bộ lọc (filter)
            
            // Lọc theo danh mục
            if (CategoryId.HasValue)
            {
                var categoryIds = _categoryService.GetAllChildCategoryIds(CategoryId.Value);
                query = query.Where(p => categoryIds.Contains(p.CategoryId));
            }

            // Lọc theo Brand (trên bảng Product)
            if (!string.IsNullOrEmpty(Brand))
            {
                query = query.Where(p => p.Brand == Brand);
            }

            // --- CÁC BỘ LỌC LIÊN QUAN ĐẾN VARIANT (Dùng .Any()) ---

            // Lọc theo Giá: Sản phẩm có ÍT NHẤT 1 biến thể nằm trong khoảng giá
            if (MinPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price >= MinPrice.Value));
            }
            if (MaxPrice.HasValue)
            {
                query = query.Where(p => p.Variants.Any(v => v.Price <= MaxPrice.Value));
            }

            // Lọc theo Màu: Sản phẩm có biến thể màu này
            if (!string.IsNullOrEmpty(Color))
            {
                query = query.Where(p => p.Variants.Any(v => v.Color == Color));
            }

            // Lọc theo Size: Sản phẩm có biến thể size này
            if (!string.IsNullOrEmpty(Size))
            {
                query = query.Where(p => p.Variants.Any(v => v.Size == Size));
            }

            // 4. Áp dụng sắp xếp (sort)
            switch (SortBy)
            {
                case "price-asc":
                    // Sắp xếp theo giá thấp nhất của biến thể trong sản phẩm đó
                    // query = query.OrderBy(p => p.Variants.Min(v => v.Price));
                    query = query.OrderBy(p => p.Id); // Temporary fallback
                    break;
                case "price-desc":
                    // Sắp xếp theo giá cao nhất của biến thể
                    // query = query.OrderByDescending(p => p.Variants.Max(v => v.Price));
                    query = query.OrderByDescending(p => p.Id); // Temporary fallback
                    break;
                case "name-asc":
                    query = query.OrderBy(p => p.Name);
                    break;
                default: // "newest"
                    query = query.OrderByDescending(p => p.CreatedAt); 
                    break;
            }

            // 5. Thực thi truy vấn
            Products = await query.AsNoTracking().ToListAsync();
        }

        // ================================================
        // HANDLER POST (THÊM VÀO GIỎ)
        // ================================================
        
        // Lưu ý: Ở trang danh sách sản phẩm (Product level), ta không thể thêm vào giỏ ngay
        // vì chưa biết user chọn Size/Màu nào.
        // Hàm này chỉ dùng nếu bạn implement Quick View hoặc chọn default variant.
        // Tạm thời giữ nguyên nhưng UI sẽ chuyển hướng sang Detail.
        
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