using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace RazorEcom.Areas.Admin.Pages.Users
{
    // Bảo vệ trang, chỉ Admin được vào
    [Authorize(Roles = "Admin")]
    public class ManagementUserModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        // ViewModel nội bộ để tổ chức dữ liệu hiển thị
        public class UserViewModel
        {
            public required string UserId { get; set; }
            public required string Email { get; set; }
            public required string UserName { get; set; }
            public required IList<string> Roles { get; set; }
        }

        // Danh sách người dùng để hiển thị ra View
        public List<UserViewModel> UsersList { get; set; } = new();

        public ManagementUserModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            // Lấy tất cả người dùng từ CSDL
            var users = await _userManager.Users.ToListAsync();

            // Lặp qua từng người dùng để lấy thông tin và vai trò (Roles)
            foreach (var user in users)
            {
                var userViewModel = new UserViewModel
                {
                    UserId = user.Id,
                    Email = user.Email ?? string.Empty,
                    UserName = user.UserName ?? string.Empty,
                    // Lấy danh sách vai trò của người dùng
                    Roles = await _userManager.GetRolesAsync(user) 
                };
                UsersList.Add(userViewModel);
            }
        }

        // Trong tương lai, bạn có thể thêm các Handler OnPost... 
        // để thêm/xóa vai trò (Roles) cho người dùng tại đây.
    }
}