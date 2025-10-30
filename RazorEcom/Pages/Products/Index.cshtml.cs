using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Cần thiết cho SelectList
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        [BindProperty(SupportsGet = true)]
        public int? CategoryId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string SortBy { get; set; } = "newest"; // Đặt "newest" làm mặc định

        // Tạo SelectList cho các tùy chọn sắp xếp
        public SelectList SortOptions { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            var sortList = new List<SelectListItem>
            {
                new SelectListItem { Value = "newest", Text = "Mới nhất" },
                new SelectListItem { Value = "priceAsc", Text = "Giá: thấp → cao" },
                new SelectListItem { Value = "priceDesc", Text = "Giá: cao → thấp" }
            };
            SortOptions = new SelectList(sortList, "Value", "Text", SortBy);

            var query = _context.ProductVariants
                .Include(v => v.Product)
                .AsNoTracking();

            if (CategoryId.HasValue)
            {
                query = query.Where(v => v.Product.CategoryId == CategoryId.Value);
            }

            switch (SortBy)
            {
                case "priceAsc":
                    query = query.OrderBy(v => v.Price);
                    break;
                case "priceDesc":
                    query = query.OrderByDescending(v => v.Price);
                    break;
                case "newest":
                default:
                    // Sắp xếp theo ID biến thể (hoặc CreatedAt nếu bạn muốn)
                    query = query.OrderByDescending(v => v.Id);
                    break;
            }

            // 6. Thực thi truy vấn
            ProductVariants = await query.ToListAsync();
        }
    }
}
