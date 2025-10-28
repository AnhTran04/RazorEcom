using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEcom.Pages.Api
{
    // Các model này có thể được chuyển ra tệp riêng nếu bạn muốn
    public class CartPopupViewModel
    {
        public List<CartPopupItem> Items { get; set; } = new List<CartPopupItem>();
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
    }

    public class CartPopupItem
    {
        public int Id { get; set; } // CartItem Id
        public string ProductName { get; set; }
        public string VariantInfo { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
    }

    public class CartModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CartModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Đây là API Handler (chỉ OnGet) trả về JSON
        public async Task<IActionResult> OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return new JsonResult(new CartPopupViewModel()); // Trả về giỏ hàng rỗng
            }

            var userId = _userManager.GetUserId(User);
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return new JsonResult(new CartPopupViewModel()); // Trả về giỏ hàng rỗng
            }

            var viewModel = new CartPopupViewModel
            {
                Items = cart.Items.Select(i => new CartPopupItem
                {
                    Id = i.Id,
                    ProductName = i.Variant.Product.Name,
                    VariantInfo = $"{i.Variant.Size} - {i.Variant.Color}", // (Bạn có thể thêm Material nếu muốn)
                    UnitPrice = i.Variant.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.Variant.ImageUrl ?? i.Variant.Product.ImageUrl
                }).ToList(),

                TotalPrice = cart.Items.Sum(i => i.Variant.Price * i.Quantity),
                TotalItems = cart.Items.Sum(i => i.Quantity)
            };

            return new JsonResult(viewModel);
        }
    }
}
