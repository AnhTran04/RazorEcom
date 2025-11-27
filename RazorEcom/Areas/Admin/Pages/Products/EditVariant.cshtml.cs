using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Threading.Tasks;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditVariantModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditVariantModel(ApplicationDbContext context)
        {
            _context = context;
        }

    [BindProperty]
    public ProductVariants? Variant { get; set; }
    public Models.Products Product { get; set; } = default!;


        public async Task<IActionResult> OnGetAsync(int id)
        {
            if (id <= 0) return NotFound();
            Variant = await _context.ProductVariants
                .Include(v => v.Product) // lấy tên product để hiển thị
                .FirstOrDefaultAsync(v => v.Id == id);
            if (Variant == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            Product = product;

            // Lấy danh sách Categories để tạo dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", Product.CategoryId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (Variant == null)
            {
                return Page();
            }

            Variant.UpdatedAt = System.DateTime.UtcNow;

            if (Variant.Id == 0)
            {
                _context.ProductVariants.Add(Variant);
            }
            else
            {
                _context.Attach(Variant).State = EntityState.Modified;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }
    }
}
