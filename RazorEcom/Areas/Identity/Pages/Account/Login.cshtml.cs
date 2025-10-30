using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorEcom.Models; 
using Microsoft.Extensions.Logging; 
using System.Threading.Tasks; 

namespace RazorEcom.Areas.Identity.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoginModel( SignInManager<ApplicationUser> signInManager, ILogger<LoginModel> logger, UserManager<ApplicationUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager; 
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(Input.Email, Input.Password, Input.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    _logger.LogInformation("Người dùng đã đăng nhập thành công.");

                    // === KIỂM TRA ADMIN ===
                    var user = await _userManager.FindByEmailAsync(Input.Email);
                    if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        // Nếu là Admin, luôn chuyển đến trang Dashboard của Admin
                        _logger.LogInformation("Phát hiện Admin, đang chuyển hướng đến trang Admin.");
                        // (Đảm bảo đường dẫn /Admin/Products/Index của bạn là chính xác)
                        return LocalRedirect(Url.Content("~/Admin/Products/Index"));
                    }

                    _logger.LogInformation("Người dùng thường, đang chuyển hướng đến returnUrl.");
                    return LocalRedirect(returnUrl); // Chuyển về trang cũ nếu là user thường
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                }
            }

            return Page();
        }
    }
}

