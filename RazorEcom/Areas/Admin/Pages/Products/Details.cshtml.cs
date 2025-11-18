using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class DetailsModel(RazorEcom.Data.ApplicationDbContext context) : PageModel
    {
        private readonly RazorEcom.Data.ApplicationDbContext _context = context;

        public required ProductVariants ProductVariants { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var ProductVariants = await _context.ProductVariants
                .Include(p => p.Product)
                    .ThenInclude(v => v.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

             if (ProductVariants == null || ProductVariants.Product == null || ProductVariants.Product.Category == null)
            {
                // Nếu một trong các liên kết bắt buộc bị thiếu, trả về Not Found
                return NotFound();
            }
            return Page();
        }
    }
}
