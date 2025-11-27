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

        public List<RazorEcom.Models.Products> FeaturedProducts { get; set; } = new List<RazorEcom.Models.Products>();
        public List<Category> Categories { get; set; } = new List<Category>();

        public async Task OnGetAsync()
        {
            // Lấy 8 SẢN PHẨM mới nhất (Include Variants để lấy ID cho link Detail)
            FeaturedProducts = await _context.Products
                .Include(p => p.Variants)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .AsNoTracking()
                .ToListAsync();

            Categories = await _context.Categories
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAddToCart(int variantId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            CartModel cart = await GetOrCreateCartAsync(userId);

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

        private async Task<CartModel> GetOrCreateCartAsync(string userId)
        {
            CartModel cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new CartModel
                {
                    CreatedAt = DateTime.Now,
                    UserId = userId 
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }
    }
}