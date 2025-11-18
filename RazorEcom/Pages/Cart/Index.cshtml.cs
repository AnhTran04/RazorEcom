using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models; // Đảm bảo bạn đã import namespace Models
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace RazorEcom.Pages.Cart
{
    [Authorize] // Người dùng phải đăng nhập để xem giỏ hàng
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ViewModel để hiển thị dữ liệu an toàn cho View
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
        public decimal TotalPrice { get; set; }

        // Lớp ViewModel lồng bên trong
        public class CartItemViewModel
        {
            public int Id { get; set; } // Id của CartItem
            public int VariantId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public string VariantInfo { get; set; } = string.Empty; // "M - Trắng - Cotton"
            public string ImageUrl { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; } // Giá của 1 sản phẩm
            public int Quantity { get; set; }
            public int MaxQuantity { get; set; } // Tồn kho
            public decimal TotalItemPrice => UnitPrice * Quantity; // Giá tổng của dòng này
        }

        // =================================================================
        // HÀM TẢI TRANG (GET)
        // =================================================================
        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge(); // Yêu cầu đăng nhập
            }

            // Tải giỏ hàng và các mục liên quan
            var cart = await _context.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Variant)
                        .ThenInclude(v => v.Product) // Cần Product.Name
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null || !cart.Items.Any())
            {
                TotalPrice = 0;
                return Page(); // Trả về trang trống
            }

            // Ánh xạ sang ViewModel
            CartItems = cart.Items.Select(i => new CartItemViewModel
            {
                Id = i.Id,
                VariantId = i.VariantId,
                ProductName = i.Variant.Product.Name,
                // Kết hợp Size, Color, Material
                VariantInfo = string.Join(" - ", new[] { i.Variant.Size, i.Variant.Color, i.Variant.Material }.Where(s => !string.IsNullOrEmpty(s))),
                ImageUrl = i.Variant.ImageUrl ?? i.Variant.Product.ImageUrl ?? "https://placehold.co/100x100?text=No+Image",
                UnitPrice = i.Variant.Price, // Lấy giá MỚI NHẤT từ biến thể
                Quantity = i.Quantity,
                MaxQuantity = i.Variant.Quantity // Số lượng tồn kho
            }).ToList();

            TotalPrice = CartItems.Sum(i => i.TotalItemPrice);

            return Page();
        }

        // =================================================================
        // HANDLER ĐỂ CẬP NHẬT SỐ LƯỢNG
        // =================================================================
        public async Task<IActionResult> OnPostUpdateQuantityAsync(int cartItemId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                // Nếu số lượng là 0 hoặc âm, hãy xóa nó
                return await OnPostRemoveItemAsync(cartItemId);
            }

            var cartItem = await FindUserCartItem(cartItemId);
            if (cartItem == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ.";
                return RedirectToPage();
            }

            // Kiểm tra tồn kho
            if (newQuantity > cartItem.Variant.Quantity)
            {
                TempData["ErrorMessage"] = $"Xin lỗi, chỉ còn {cartItem.Variant.Quantity} sản phẩm trong kho.";
                return RedirectToPage();
            }

            cartItem.Quantity = newQuantity;
            cartItem.UpdatedAt = System.DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã cập nhật số lượng.";
            return RedirectToPage();
        }

        // =================================================================
        // HANDLER ĐỂ XÓA SẢN PHẨM
        // =================================================================
        public async Task<IActionResult> OnPostRemoveItemAsync(int cartItemId)
        {
            var cartItem = await FindUserCartItem(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy sản phẩm trong giỏ.";
            }

            return RedirectToPage();
        }

        // =================================================================
        // HÀM TRỢ GIÚP
        // =================================================================
        // Hàm này tìm và xác thực cartItem thuộc về người dùng đang đăng nhập
        private async Task<RazorEcom.Models.CartItem?> FindUserCartItem(int cartItemId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var cartItem = await _context.CartItems
                .Include(i => i.Cart)  // Cần Cart để kiểm tra UserId
                .Include(i => i.Variant) // Cần Variant để kiểm tra tồn kho
                .FirstOrDefaultAsync(i => i.Id == cartItemId);

            // Đảm bảo cartItem này thuộc về người dùng đang đăng nhập
            if (cartItem == null || cartItem.Cart.UserId != user.Id)
            {
                return null;
            }

            return cartItem;
        }
    }
}

