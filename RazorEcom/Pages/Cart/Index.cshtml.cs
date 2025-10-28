using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEcom.Pages.Cart
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ViewModel để hiển thị thông tin giỏ hàng
        public CartViewModel Cart { get; set; } = new CartViewModel();

        public class CartViewModel
        {
            public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();
            public decimal TotalPrice { get; set; }
            public int TotalItems { get; set; }
        }

        public class CartItemViewModel
        {
            public int CartItemId { get; set; }
            public int VariantId { get; set; }
            public string ProductName { get; set; }
            public string VariantInfo { get; set; } // (Size, Color, Material)
            public string ImageUrl { get; set; }
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal TotalItemPrice => UnitPrice * Quantity;
        }

        // Tải giỏ hàng khi vào trang
        public async Task<IActionResult> OnGetAsync()
        {
            await LoadCartAsync();
            if (Cart == null)
            {
                // Mặc dù đã Authorize, vẫn nên kiểm tra
                return Challenge();
            }
            return Page();
        }

        // Xử lý Cập nhật số lượng (từ form)
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                // Nếu số lượng là 0 hoặc âm, coi như là xóa
                return await OnPostRemoveItemAsync(cartItemId);
            }

            var cartItem = await GetCartItemAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            // Kiểm tra tồn kho (nếu cần)
            // if (newQuantity > cartItem.Variant.Quantity)
            // {
            //     TempData["ErrorMessage"] = "Số lượng vượt quá tồn kho.";
            //     return RedirectToPage();
            // }

            cartItem.Quantity = newQuantity;
            _context.CartItems.Update(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật số lượng sản phẩm!";
            return RedirectToPage();
        }

        // Xử lý Xóa sản phẩm
        public async Task<IActionResult> OnPostRemoveItemAsync(int cartItemId)
        {
            var cartItem = await GetCartItemAsync(cartItemId);
            if (cartItem == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng!";
            return RedirectToPage();
        }


        // == HÀM HỖ TRỢ ==

        // Hàm tải giỏ hàng (dùng chung cho OnGet và các hàm Post)
        private async Task LoadCartAsync()
        {
            var userId = _userManager.GetUserId(User);
            var userCart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null || !userCart.Items.Any())
            {
                Cart = new CartViewModel(); // Giỏ hàng rỗng
                return;
            }

            Cart.Items = userCart.Items.Select(i => new CartItemViewModel
            {
                CartItemId = i.Id,
                VariantId = i.VariantId,
                ProductName = i.Variant.Product.Name,
                VariantInfo = $"{i.Variant.Size} - {i.Variant.Color} - {i.Variant.Material}",
                ImageUrl = i.Variant.ImageUrl ?? i.Variant.Product.ImageUrl, // Lấy ảnh variant, nếu không có thì lấy ảnh gốc
                UnitPrice = i.Variant.Price, // Lấy giá hiện tại của variant
                Quantity = i.Quantity
            }).ToList();

            Cart.TotalPrice = Cart.Items.Sum(i => i.TotalItemPrice);
            Cart.TotalItems = Cart.Items.Sum(i => i.Quantity);
        }

        // Hàm lấy CartItem và đảm bảo nó thuộc về đúng người dùng
        private async Task<CartItem> GetCartItemAsync(int cartItemId)
        {
            var userId = _userManager.GetUserId(User);

            // Truy vấn CartItem, bao gồm cả Cart để kiểm tra UserId
            var cartItem = await _context.CartItems
                .Include(i => i.Cart)
                .FirstOrDefaultAsync(i => i.Id == cartItemId && i.Cart.UserId == userId);

            return cartItem;
        }
    }
}

