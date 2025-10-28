using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Mono.TextTemplating;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RazorEcom.Pages.Orders
{
    [Authorize] // Chỉ người đã đăng nhập mới xem được
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dùng Order (model) để chứa dữ liệu, vì nó đã chứa mọi thứ chúng ta cần
        public Order Order { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge(); // Yêu cầu đăng nhập
            }

            // Truy vấn đơn hàng
            Order = await _context.Orders
                // 1. Lấy thông tin địa chỉ giao hàng
                .Include(o => o.ShippingAddress)
                // 2. Lấy thông tin thanh toán (nếu có)
                .Include(o => o.Payment)
                // 3. Lấy danh sách các sản phẩm trong đơn hàng (OrderItem)
                .Include(o => o.OrderItems)
                    // 4. Với mỗi sản phẩm, lấy thông tin Biến thể (Variant)
                    .ThenInclude(oi => oi.Variant)
                        // 5. Với mỗi biến thể, lấy thông tin Sản phẩm gốc (Product)
                        .ThenInclude(v => v.Product)
                // Lọc theo ID đơn hàng VÀ ID người dùng (để bảo mật)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (Order == null)
            {
                return NotFound("Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này.");
            }

            return Page();
        }
    }
}

