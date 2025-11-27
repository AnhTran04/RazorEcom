using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEcom.Areas.Admin.Pages
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DashboardModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // =============================================
        // Thuộc tính cho các Thẻ Thống kê (Kard)
        // =============================================
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalUsers { get; set; }
        public int TotalProductSKUs { get; set; }

        // =============================================
        // Thuộc tính cho các Bảng
        // =============================================
        public List<Order> RecentOrders { get; set; } = new List<Order>();
        public List<ProductVariants> LowStockProducts { get; set; } = new List<ProductVariants>();

        public async Task OnGetAsync()
        {
            // 1. Lấy dữ liệu cho các Kard
            // Lưu ý: Tạm thời tính tổng doanh thu của tất cả đơn hàng. 
            // Bạn có thể muốn lọc thêm theo Status (ví dụ: "Completed" hoặc "Paid")
            TotalRevenue = await _context.Orders.SumAsync(o => o.Total);
            TotalOrders = await _context.Orders.CountAsync();
            TotalUsers = await _context.Users.CountAsync(); // 'Users' từ IdentityDbContext
            TotalProductSKUs = await _context.ProductVariants.CountAsync();

            // 2. Lấy dữ liệu cho Bảng "Đơn hàng gần đây"
            // (Bao gồm User để lấy Email/Tên)
            RecentOrders = await _context.Orders
                .Include(o => o.User) 
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();

            // 3. Lấy dữ liệu cho Bảng "Sản phẩm sắp hết hàng"
            // (Bao gồm Product để lấy Tên)
            LowStockProducts = await _context.ProductVariants
                .Include(v => v.Product)
                .Where(v => v.Quantity <= 10) // Lấy các sản phẩm có SL <= 10
                .OrderBy(v => v.Quantity)
                .Take(5)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IActionResult> OnGetChartDataAsync()
        {
            // --- 1. Dữ liệu cho Biểu đồ Doanh thu (7 ngày qua) ---
            var today = DateTime.Today;
            var last7Days = Enumerable.Range(0, 7).Select(i => today.AddDays(-i)).Reverse().ToList();
            
            var revenueData = await _context.Orders
                .Where(o => o.CreatedAt >= today.AddDays(-6))
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new {
                    Date = g.Key,
                    Total = g.Sum(o => o.Total)
                })
                .ToListAsync();

            var revenueLabels = last7Days.Select(d => d.ToString("dd/MM")).ToList();
            var revenueValues = last7Days
                .Select(d => revenueData.FirstOrDefault(r => r.Date == d)?.Total ?? 0)
                .ToList();

            // --- 2. Dữ liệu cho Biểu đồ Trạng thái Đơn hàng ---
            var orderStatusData = await _context.Orders
                .GroupBy(o => o.Status)
                .Select(g => new {
                    Status = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Count)
                .ToListAsync();

            var orderStatusLabels = orderStatusData.Select(d => d.Status).ToList();
            var orderStatusValues = orderStatusData.Select(d => d.Count).ToList();

            // --- 3. Trả về JSON ---
            return new JsonResult(new 
            {
                revenue = new {
                    labels = revenueLabels,
                    data = revenueValues
                },
                orderStatus = new {
                    labels = orderStatusLabels,
                    data = orderStatusValues
                }
            });
        }
    }
}