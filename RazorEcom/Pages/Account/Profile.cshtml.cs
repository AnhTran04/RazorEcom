using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorEcom.Data; // Giả sử ApplicationUser của bạn ở đây
using System.Threading.Tasks;

namespace RazorEcom.Pages.Account
{
    [Authorize] // Yêu cầu người dùng phải đăng nhập để xem trang này
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // Dùng một InputModel để hiển thị dữ liệu
        public class InputModel
        {
            public string Email { get; set; }
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Không thể tải người dùng với ID '{_userManager.GetUserId(User)}'.");
            }

            // Tải dữ liệu từ user vào InputModel
            Input = new InputModel
            {
                Email = await _userManager.GetEmailAsync(user),
                FullName = user.FullName, // Giả sử bạn có thuộc tính FullName trong ApplicationUser
                PhoneNumber = await _userManager.GetPhoneNumberAsync(user)
            };

            return Page();
        }

        // Xử lý khi người dùng nhấn nút Đăng xuất
        public async Task<IActionResult> OnPostLogoutAsync()
        {
            await _signInManager.SignOutAsync();
            // Chuyển hướng về trang chủ sau khi đăng xuất
            return RedirectToPage("/Index");
        }
    }
}
