using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Cần thiết cho IQueryable

namespace RazorEcom.Pages.Products
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ProductVariants> ProductVariants { get; set; } = new List<ProductVariants>();
        public List<Category> Categories { get; set; } = new List<Category>();

        // BindProperty hỗ trợ GET để nhận giá trị từ query string
        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        // == PHẦN BỊ THIẾU ĐÃ ĐƯỢC THÊM VÀO ==
        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } // Nhận giá trị "priceAsc" hoặc "priceDesc"

        public async Task OnGetAsync()
        {
            // 1. Tải Categories để hiển thị dropdown
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking() // Tối ưu, không cần theo dõi thay đổi
                .ToListAsync();

            // 2. Xây dựng câu truy vấn
            var query = _context.ProductVariants
                .Include(v => v.Product) // Nối bảng để lấy CategoryId
                .AsNoTracking() // Tối ưu, không cần theo dõi thay đổi
                .AsQueryable();

            // 3. Lọc (Filter)
            if (CategoryId.HasValue)
            {
                query = query.Where(v => v.Product.CategoryId == CategoryId.Value);
            }

            // 4. Sắp xếp (Sort) - == LOGIC BỊ THIẾU ĐÃ ĐƯỢC THÊM VÀO ==
            switch (SortBy)
            {
                case "priceAsc":
                    query = query.OrderBy(v => v.Price);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(v => v.Price);
                    break;
                default:
                    query = query.OrderByDescending(v => v.CreatedAt); // Mặc định
                    break;
            }

            // 5. Thực thi truy vấn
            ProductVariants = await query.ToListAsync();
        }
    }
}

