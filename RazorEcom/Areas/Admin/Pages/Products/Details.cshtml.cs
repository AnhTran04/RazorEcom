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
    public class DetailsModel : PageModel
    {
        private readonly RazorEcom.Data.ApplicationDbContext _context;

        public DetailsModel(RazorEcom.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public Models.Products Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            Product = product;
            return Page();
        }
    }
}
