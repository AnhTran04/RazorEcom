using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace RazorEcom.Pages.Products
{
    // BỎ [Authorize] ở đây để ai cũng xem được sản phẩm
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // === CÁC THUỘC TÍNH HIỂN THỊ ===
        public RazorEcom.Models.Products Product { get; set; } = null!;
        public ProductVariant DefaultVariant { get; set; } = null!; // Sửa ProductVariants thành ProductVariant (số ít) nếu model tên vậy
        public SelectList VariantOptions { get; set; } = null!;
        
        // === BINDING ===
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // Variant ID từ URL

        [BindProperty]
        public int Quantity { get; set; } = 1;

        // ================= GET =================
        public async Task<IActionResult> OnGetAsync()
        {
            // Tìm variant theo ID
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .ThenInclude(p => p.Variants) // Load danh sách variant anh em
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.Id == Id);

            // Nếu không tìm thấy variant theo ID (ví dụ user nhập ID bậy)
            if (variant == null)
            {
                // Thử tìm xem có phải user nhập ProductID không, nếu có thì redirect về Variant đầu tiên của Product đó
                var product = await _context.Products
                    .Include(p => p.Variants)
                    .FirstOrDefaultAsync(p => p.Id == Id);

                if (product != null && product.Variants.Any())
                {
                    return RedirectToPage("Detail", new { id = product.Variants.First().Id });
                }

                return NotFound("Không tìm thấy sản phẩm.");
            }

            Product = variant.Product;
            DefaultVariant = variant;

            LoadVariantOptions();
            return Page();
        }

        // ================= POST (THÊM GIỎ HÀNG) =================
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Kiểm tra đăng nhập tại đây
            if (!User.Identity.IsAuthenticated)
            {
                // Redirect sang trang login, sau khi login xong quay lại trang này
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl = Url.Page("/Products/Detail", new { id = Id }) });
            }

            if (!ModelState.IsValid)
            {
                await ReloadDataOnError();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 2. Logic thêm giỏ hàng
            var selectedVariant = await _context.ProductVariants.FindAsync(Id);
            if (selectedVariant == null) return NotFound();

            if (selectedVariant.Quantity < Quantity)
            {
                TempData["error"] = $"Kho chỉ còn {selectedVariant.Quantity} sản phẩm.";
                await ReloadDataOnError();
                return Page();
            }

            // Tìm hoặc tạo giỏ hàng
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new RazorEcom.Models.Cart { UserId = user.Id, CreatedAt = DateTime.Now };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Lưu để lấy CartId
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.VariantId == Id);
            if (cartItem != null)
            {
                cartItem.Quantity += Quantity;
                cartItem.UpdatedAt = DateTime.Now;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    VariantId = Id,
                    Quantity = Quantity,
                    CreatedAt = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã thêm vào giỏ hàng thành công!";
            
            // Redirect lại chính trang này (theo mẫu Post-Redirect-Get)
            return RedirectToPage(new { id = Id });
        }

        // === HELPERS ===
        private void LoadVariantOptions()
        {
            var allVariants = Product.Variants.OrderBy(v => v.Price).ToList();
            
            // Tạo Dropdown: Value = VariantId, Text = "Size - Màu (Giá)"
            VariantOptions = new SelectList(allVariants.Select(v => new
            {
                v.Id,
                DisplayText = $"{v.Size} - {v.Color} ({v.Price:N0}đ)"
            }), "Id", "DisplayText", DefaultVariant.Id);
        }

        private async Task ReloadDataOnError()
        {
            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .ThenInclude(p => p.Variants)
                .FirstOrDefaultAsync(v => v.Id == Id);
            
            if (variant != null)
            {
                Product = variant.Product;
                DefaultVariant = variant;
                LoadVariantOptions();
            }
        }
    }
}