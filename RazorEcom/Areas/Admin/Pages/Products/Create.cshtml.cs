using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RazorEcom.Data;
using RazorEcom.Models;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly RazorEcom.Data.ApplicationDbContext _context;

        public CreateModel(RazorEcom.Data.ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            // Lấy danh sách Categories để tạo dropdown
            ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name");
            return Page();
        }

        [BindProperty]
        public Models.Products Product { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ViewData["CategoryId"] = new SelectList(_context.Categories.OrderBy(c => c.Name), "Id", "Name", Product.CategoryId);
                return Page();
            }

            Product.CreatedAt = DateTime.UtcNow;
            Product.UpdatedAt = DateTime.UtcNow;
            Product.Status = "active"; // Mặc định là active

            _context.Products.Add(Product);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã tạo sản phẩm thành công.";
            return RedirectToPage("./Index");
        }
    }
}
