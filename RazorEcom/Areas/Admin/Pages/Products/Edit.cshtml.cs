using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly RazorEcom.Data.ApplicationDbContext _context;

        public EditModel(RazorEcom.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Models.Products Product { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
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
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", Product.CategoryId);
                return Page();
            }

            // Đánh dấu thời gian cập nhật
            Product.UpdatedAt = DateTime.UtcNow;

            _context.Attach(Product).State = EntityState.Modified;

            // Đảm bảo CreatedAt không bị ghi đè nếu nó không được gửi lên
            _context.Entry(Product).Property(x => x.CreatedAt).IsModified = false;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(Product.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["success"] = "Đã cập nhật sản phẩm thành công.";
            return RedirectToPage("./Index");
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
