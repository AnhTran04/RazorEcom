using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RazorEcom.Pages.Account
{
    [Authorize] // Bắt buộc đăng nhập
    public class AddressBookModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressBookModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dùng để hiển thị danh sách địa chỉ
        public List<AddressBook> Addresses { get; set; } = new List<AddressBook>();

        // Dùng cho biểu mẫu (form) thêm địa chỉ mới
        [BindProperty]
        public AddressBook NewAddress { get; set; } = new AddressBook();

        // Tải danh sách địa chỉ của người dùng
        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge(); // Hoặc RedirectToPage("/Identity/Account/Login")
            }

            Addresses = await _context.AddressBooks
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            return Page();
        }

        // Xử lý thêm địa chỉ mới
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            // Đặt UserId cho địa chỉ mới
            NewAddress.UserId = userId;

            // Xóa lỗi validation cho User, vì chúng ta không bind nó từ form
            ModelState.Remove("NewAddress.User");
            // Xóa lỗi validation cho UserId, vì chúng ta gán nó thủ công
            ModelState.Remove("NewAddress.UserId");


            if (!ModelState.IsValid)
            {
                // ----- BẮT ĐẦU CODE DEBUG -----
                // Thêm code này để xem lỗi validation cụ thể trong cửa sổ Output (Debug)
                var errors = ModelState
                    .Where(a => a.Value.Errors.Count > 0)
                    .Select(b => $"Lỗi ở trường: {b.Key}, Thông báo: {b.Value.Errors.FirstOrDefault()?.ErrorMessage}")
                    .ToList();

                foreach (var error in errors)
                {
                    // In lỗi ra cửa sổ Output (Debug)
                    System.Diagnostics.Debug.WriteLine($"[VALIDATION ERROR] {error}");
                }
                // ----- KẾT THÚC CODE DEBUG -----


                // Nếu model không hợp lệ, tải lại danh sách địa chỉ để hiển thị trang
                Addresses = await _context.AddressBooks
                    .Where(a => a.UserId == userId)
                    .OrderByDescending(a => a.IsDefault)
                    .ThenByDescending(a => a.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();

                TempData["error"] = "Thêm địa chỉ thất bại. Vui lòng kiểm tra lại thông tin.";
                return Page();
            }

            // Xử lý logic IsDefault: Nếu địa chỉ mới là Default,
            // bỏ Default của các địa chỉ cũ
            if (NewAddress.IsDefault)
            {
                var currentDefault = await _context.AddressBooks
                    .Where(a => a.UserId == userId && a.IsDefault)
                    .FirstOrDefaultAsync();

                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                    _context.Update(currentDefault);
                }
            }

            _context.AddressBooks.Add(NewAddress);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã thêm địa chỉ mới thành công!";
            return RedirectToPage(); // Tải lại trang để hiển thị danh sách mới
        }

        // Xử lý xóa địa chỉ
        // Chúng ta dùng một handler riêng (OnPostDeleteAsync)
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
            {
                return Challenge();
            }

            var addressToDelete = await _context.AddressBooks
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (addressToDelete == null)
            {
                TempData["error"] = "Không tìm thấy địa chỉ hoặc bạn không có quyền xóa.";
                return RedirectToPage();
            }

            // Ngăn người dùng xóa địa chỉ mặc định? (Tùy chọn)
            if (addressToDelete.IsDefault)
            {
                TempData["error"] = "Không thể xóa địa chỉ mặc định.";
                return RedirectToPage();
            }

            _context.AddressBooks.Remove(addressToDelete);
            await _context.SaveChangesAsync();

            TempData["success"] = "Đã xóa địa chỉ thành công.";
            return RedirectToPage();
        }
    }
}

