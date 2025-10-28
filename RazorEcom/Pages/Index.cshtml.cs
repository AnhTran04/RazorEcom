using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;

namespace RazorEcom.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<ProductVariants> FeaturedVariants { get; set; } = new List<ProductVariants>();
        public List<Category> Categories { get; set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            // Lấy 8 sản phẩm nổi bật mới nhất
            FeaturedVariants = await _context.ProductVariants
                .Include(v => v.Product)
                .OrderByDescending(v => v.CreatedAt)
                .Take(8)
                .ToListAsync();

            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }
    }
}
