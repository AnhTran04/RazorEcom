using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // Thêm using System

namespace RazorEcom.Pages.Checkout
{
    [Authorize] // Bắt buộc đăng nhập để thanh toán
    public class CartModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dữ liệu để hiển thị
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public SelectList ShippingAddresses { get; set; } = null!;
        public decimal TotalPrice { get; set; } = 0;

        // Dữ liệu ràng buộc từ Form POST
        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public int SelectedAddressId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán.")]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; } = "COD"; // Mặc định là COD

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge(); // Sẽ chuyển đến trang đăng nhập
            }

            // 1. Tải giỏ hàng
            // Sửa lỗi: Cần tham chiếu đến Cart.UserId
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                // Xử lý trường hợp không có giỏ hàng
                CartItems = new List<CartItem>();
            }
            else
            {
                CartItems = await _context.CartItems
                    .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                    .Where(ci => ci.CartId == cart.Id) // Lọc theo CartId
                    .ToListAsync();
            }

            if (CartItems.Count == 0)
            {
                TempData["error"] = "Giỏ hàng của bạn đang trống. Không thể thanh toán.";
                return RedirectToPage("/Cart"); // Giả sử trang Cart là /Cart
            }

            // 2. Tải địa chỉ của người dùng
            var addresses = await _context.AddressBooks
                .Where(ab => ab.UserId == userId)
                .OrderByDescending(ab => ab.IsDefault) // Ưu tiên địa chỉ mặc định
                .ToListAsync();
            
            if (!addresses.Any())
            {
                TempData["error"] = "Bạn cần thêm địa chỉ giao hàng trước.";
                return RedirectToPage("/Account/AddressBook");
            }

            // Tạo SelectList cho dropdown
            ShippingAddresses = new SelectList(addresses.Select(a => new {
                Id = a.Id,
                DisplayText = $"{a.FullName} - {a.Phone} - {a.AddressLine}, {a.Ward}, {a.District}, {a.City}"
            }), "Id", "DisplayText");

            // 3. Tính tổng tiền
            TotalPrice = CartItems.Sum(item => (item.Variant?.Price ?? 0) * item.Quantity);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }

            // Tải lại dữ liệu giỏ hàng và địa chỉ (quan trọng cho việc xác thực)
            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                CartItems = new List<CartItem>();
            }
            else
            {
                CartItems = await _context.CartItems
                    .Include(ci => ci.Variant)
                    .ThenInclude(v => v.Product)
                    .Where(ci => ci.CartId == cart.Id) // Lọc theo CartId
                    .ToListAsync();
            }
            
            TotalPrice = CartItems.Sum(item => (item.Variant?.Price ?? 0) * item.Quantity);

            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ (ví dụ: chưa chọn địa chỉ), tải lại danh sách địa chỉ và hiển thị lại trang
                var addresses = await _context.AddressBooks
                    .Where(ab => ab.UserId == userId)
                    .OrderByDescending(ab => ab.IsDefault)
                    .ToListAsync();
                ShippingAddresses = new SelectList(addresses.Select(a => new {
                    Id = a.Id,
                    DisplayText = $"{a.FullName} - {a.Phone} - {a.AddressLine}, {a.Ward}, {a.District}, {a.City}"
                }), "Id", "DisplayText");

                return Page();
            }

            // Kiểm tra xem địa chỉ đã chọn có thực sự thuộc về người dùng này không
            var selectedAddress = await _context.AddressBooks
                .FirstOrDefaultAsync(ab => ab.Id == SelectedAddressId && ab.UserId == userId);

            if (selectedAddress == null)
            {
                ModelState.AddModelError(string.Empty, "Địa chỉ giao hàng không hợp lệ.");
                return Page(); // Tải lại trang với lỗi
            }

            if (CartItems.Count == 0)
            {
                TempData["error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToPage("/Cart"); // Sửa lại đường dẫn nếu cần
            }

            // Bắt đầu một giao dịch (Transaction) để đảm bảo tất cả các bước đều thành công
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra tồn kho (quan trọng)
                foreach (var item in CartItems)
                {
                    // Sửa lỗi: Cần tham chiếu đến ProductVariantId
                    var variantInDb = await _context.ProductVariants.FindAsync(item.VariantId); 
                    if (variantInDb == null || variantInDb.Quantity < item.Quantity)
                    {
                        await transaction.RollbackAsync(); // Hủy giao dịch
                        TempData["error"] = $"Sản phẩm '{item.Variant.Product.Name}' (Size: {item.Variant.Size}, Màu: {item.Variant.Color}) không đủ số lượng tồn kho (chỉ còn {variantInDb?.Quantity ?? 0}).";
                        return RedirectToPage("/Cart"); // Sửa lại đường dẫn nếu cần
                    }
                }

                // 2. Tạo Đơn hàng (Order) mới
                var order = new Order
                {
                    UserId = userId,
                    Status = "Pending", // Trạng thái chờ xử lý
                    Total = TotalPrice,
                    ShippingAddressId = SelectedAddressId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Lưu để lấy OrderId

                // 3. Sao chép CartItems sang OrderItems
                foreach (var item in CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        VariantId = item.VariantId, // Sửa lỗi
                        Quantity = item.Quantity,
                        UnitPrice = item.Variant.Price, // Lưu giá tại thời điểm mua
                        CreatedAt = DateTime.UtcNow,
                	UpdatedAt = DateTime.UtcNow
                    };
                    _context.OrderItems.Add(orderItem);

                    // 4. Trừ số lượng tồn kho
                    var variantInDb = await _context.ProductVariants.FindAsync(item.VariantId); // Sửa lỗi
                    if (variantInDb != null)
                    {
                        variantInDb.Quantity -= item.Quantity;
                    }
                }

                // 5. Tạo thông tin thanh toán (Payment)
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Amount = TotalPrice,
                    Method = PaymentMethod,
                    Status = (PaymentMethod == "COD") ? "Pending" : "AwaitingPayment", // Nếu là COD thì chờ, nếu online thì đợi thanh toán
                    CreatedAt = DateTime.UtcNow,
                  	UpdatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(payment);

                // 6. Xóa các mục khỏi giỏ hàng
                _context.CartItems.RemoveRange(CartItems);

                // 7. Lưu tất cả thay đổi vào CSDL
                await _context.SaveChangesAsync();

                // 8. Hoàn tất giao dịch
                await transaction.CommitAsync();

                TempData["success"] = $"Đặt hàng thành công! Mã đơn hàng của bạn là #{order.Id}.";
                return RedirectToPage("/Orders/OrderDetail", new { id = order.Id }); // Chuyển đến trang chi tiết đơn hàng vừa tạo
            }
            catch (Exception ex)
{
                // Nếu có bất kỳ lỗi nào xảy ra, hủy bỏ tất cả thay đổi
                await transaction.RollbackAsync();

                // Ghi log lỗi (trong môi trường thực tế)
                // _logger.LogError(ex, "Lỗi nghiêm trọng khi tạo đơn hàng cho User {UserId}", userId);
                System.Diagnostics.Debug.WriteLine($"[TRANSACTION ERROR] {ex.Message}"); // In lỗi ra debug

                TempData["error"] = "Đã xảy ra lỗi nghiêm trọng khi đặt hàng. Vui lòng thử lại.";

                // Tải lại dữ liệu cho trang
                var addresses = await _context.AddressBooks
                    .Where(ab => ab.UserId == userId)
	             .OrderByDescending(ab => ab.IsDefault)
                    .ToListAsync();
                ShippingAddresses = new SelectList(addresses.Select(a => new {
                    Id = a.Id,
                    DisplayText = $"{a.FullName} - {a.Phone} - {a.AddressLine}, {a.Ward}, {a.District}, {a.City}"
                }), "Id", "DisplayText");

                return Page();
            }
        }
    }
}