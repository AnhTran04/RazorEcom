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
using RazorEcom.Services;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly RazorEcom.Data.ApplicationDbContext _context;
        private readonly ImageService _imageService;

        public EditModel(RazorEcom.Data.ApplicationDbContext context, ImageService imageService)
        {
            _context = context;
            _imageService = imageService;
        }

        [BindProperty]
        public Models.Products Product { get; set; } = default!;

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

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

            // Xử lý upload ảnh mới
            if (ImageFile != null)
            {
                // 1. Xóa ảnh cũ nếu có (và không phải là placeholder)
                if (!string.IsNullOrEmpty(Product.ImageUrl) && !Product.ImageUrl.StartsWith("http"))
                {
                    _imageService.DeleteImage(Product.ImageUrl);
                }

                // 2. Upload ảnh mới
                Product.ImageUrl = await _imageService.UploadImageAsync(ImageFile);
            }
            else
            {
                // Nếu không upload ảnh mới, giữ nguyên ảnh cũ
                // Cần reload lại từ DB để lấy ImageUrl cũ nếu form không gửi lên
                // Tuy nhiên, vì Product.ImageUrl được bind từ form (hidden input hoặc text input), 
                // nên nếu ta giữ input hidden cho ImageUrl trong view, nó sẽ được bind lại.
                // Nhưng an toàn hơn là không làm gì cả nếu ImageUrl đã được bind đúng.
                // Nếu ImageUrl bị null do không bind, ta cần lấy lại từ DB.
                
                // Ở đây ta giả định View sẽ có input hidden cho ImageUrl hoặc ta không thay đổi nó.
                // Nhưng EF Core Attach(Product).State = Modified sẽ update TẤT CẢ các trường.
                // Nếu ImageUrl trong Product là null (do không bind), nó sẽ ghi đè null vào DB.
                
                // Để an toàn, ta nên check:
                if (Product.ImageUrl == null)
                {
                     var oldProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == Product.Id);
                     if (oldProduct != null)
                     {
                         Product.ImageUrl = oldProduct.ImageUrl;
                     }
                }
            }

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
