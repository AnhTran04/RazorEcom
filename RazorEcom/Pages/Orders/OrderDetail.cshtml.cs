using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEcom.Pages.Orders
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Order? Order { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            // Tải đơn hàng VÀ các dữ liệu liên quan
            Order = await _context.Orders
                .Include(o => o.ShippingAddress) // Tải địa chỉ giao hàng
                .Include(o => o.Payment)         // Tải thông tin thanh toán
                .Include(o => o.OrderItems)      // Tải các mục trong đơn hàng
                    .ThenInclude(oi => oi.Variant)   // ... Tải biến thể của mục đó
                        .ThenInclude(v => v.Product) // ... Tải sản phẩm của biến thể đó
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId); // Chỉ tìm đơn hàng của user này

            if (Order == null)
            {
                TempData["error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem.";
                return RedirectToPage("/Index"); // Hoặc trang danh sách đơn hàng
            }

            return Page();
        }
    }
}