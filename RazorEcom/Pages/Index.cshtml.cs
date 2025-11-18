using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using RazorEcom.Pages.Cart;
using CartModel = global::RazorEcom.Models.Cart;

namespace RazorEcom.Pages
{
    [ValidateAntiForgeryToken]
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
        public async Task<IActionResult> OnPostAddToCart(int variantId)
        {
            // BƯỚC 1: KIỂM TRA ĐĂNG NHẬP
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                // Nếu chưa đăng nhập, chuyển đến trang đăng nhập
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // BƯỚC 2: LẤY HOẶC TẠO GIỎ HÀNG (BÂY GIỜ DỰA TRÊN USERID)
            CartModel cart = await GetOrCreateCartAsync(userId);

            // BƯỚC 3: KIỂM TRA SẢN PHẨM TRONG GIỎ HÀNG
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    VariantId = variantId,
                    Quantity = 1,
                    CreatedAt = DateTime.Now
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("/Index");
        }

        /// <summary>
        /// Phương thức trợ giúp đã được sửa đổi để hoạt động với UserId
        /// </summary>
        private async Task<CartModel> GetOrCreateCartAsync(string userId)
        {
            // Tìm Cart dựa trên UserId
            CartModel cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                // Nếu không có Cart, tạo mới và gán UserId
                cart = new CartModel
                {
                    CreatedAt = DateTime.Now,
                    UserId = userId // Gán UserId (bắt buộc)
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}
