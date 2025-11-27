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
    [Authorize(Roles = "Admin")]
    public class EditUserModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager) : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;

        // Lấy ID từ URL
        [BindProperty(SupportsGet = true)]
        public required string Id { get; set; }

        [BindProperty]
        public required string Email { get; set; }

        [BindProperty]
        public required string UserName { get; set; }
        
        // Dùng để hiển thị tất cả các vai trò
        public required List<IdentityRole> AllRoles { get; set; }
        
        // Dùng để nhận các vai trò được chọn từ form
        [BindProperty]
        public required List<string> SelectedRoles { get; set; }

        // Dùng để kiểm tra các vai trò hiện tại của user
        private IList<string>? _currentUserRoles;

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{id}'.");
            }

            // Lấy thông tin cơ bản
            Id = user.Id;
            Email = user.Email!;
            UserName = user.UserName ?? string.Empty;

            // Tải tất cả vai trò trong hệ thống
            AllRoles = await _roleManager.Roles.ToListAsync();
            
            // Tải các vai trò hiện tại của người dùng
            _currentUserRoles = await _userManager.GetRolesAsync(user);

            // Gán các vai trò hiện tại cho SelectedRoles để Checkbox tự động chọn
            SelectedRoles = _currentUserRoles.ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.FindByIdAsync(Id);
            if (user == null)
            {
                return NotFound($"Không tìm thấy người dùng với ID '{Id}'.");
            }

            // Lấy vai trò hiện tại của user từ CSDL
            var currentUserRoles = await _userManager.GetRolesAsync(user);

            // 1. Tính toán các vai trò cần THÊM
            // (là các vai trò có trong SelectedRoles nhưng không có trong currentUserRoles)
            var rolesToAdd = SelectedRoles.Except(currentUserRoles).ToList();
            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                {
                    // Xử lý lỗi nếu thêm không thành công
                    ModelState.AddModelError("", "Lỗi khi thêm vai trò mới.");
                    return Page();
                }
            }

            // 2. Tính toán các vai trò cần XÓA
            // (là các vai trò có trong currentUserRoles nhưng không có trong SelectedRoles)
            var rolesToRemove = currentUserRoles.Except(SelectedRoles).ToList();
            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    // Xử lý lỗi nếu xóa không thành công
                    ModelState.AddModelError("", "Lỗi khi xóa vai trò cũ.");
                    return Page();
                }
            }

            // Quay lại trang danh sách người dùng
            return RedirectToPage("./Index");
        }
    }
}