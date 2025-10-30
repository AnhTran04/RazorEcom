using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được trang này
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Models.Products> Product { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Product = await _context.Products
                .Include(p => p.Category) // Lấy cả thông tin Category
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
