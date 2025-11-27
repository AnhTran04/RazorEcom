using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace RazorEcom.Areas.Admin.Pages.Orders
{
    // CỰC KỲ QUAN TRỌNG:
    // Đảm bảo chỉ có Admin mới vào được trang này.
    // Bạn cần phải thiết lập Role "Admin" trong hệ thống Identity của mình.
    [Authorize(Roles = "Admin")]
    public class ManagementOrderModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ManagementOrderModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Order> AllOrders { get; private set; } = new();

        // Danh sách các trạng thái đơn hàng để hiển thị trong dropdown
        public List<string> OrderStatuses { get; } = new()
        {
            "Pending",    // Đang chờ xử lý
            "Processing", // Đang xử lý
            "Shipped",    // Đã giao hàng
            "Completed",  // Hoàn thành
            "Cancelled"   // Đã hủy
        };

        public async Task OnGetAsync()
        {
            // Tải tất cả đơn hàng, sắp xếp mới nhất lên đầu
            // Include() User để lấy email, Payment để lấy trạng thái thanh toán
            AllOrders = await _context.Orders
                .Include(o => o.User) 
                .Include(o => o.Payment)
                .OrderByDescending(o => o.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        // Xử lý khi Admin thay đổi trạng thái đơn hàng
        public async Task<IActionResult> OnPostUpdateStatusAsync(int orderId, string newStatus)
        {
            if (string.IsNullOrEmpty(newStatus))
            {
                return RedirectToPage();
            }

            var order = await _context.Orders.FindAsync(orderId);

            if (order != null)
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
            }

            // Tải lại trang
            return RedirectToPage();
        }
    }
}