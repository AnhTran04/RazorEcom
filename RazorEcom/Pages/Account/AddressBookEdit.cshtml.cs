using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Threading.Tasks;

namespace RazorEcom.Pages.Account
{
    [Authorize]
    public class AddressBookEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressBookEditModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Model chứa địa chỉ đang được sửa
        [BindProperty]
        public AddressBook AddressToEdit { get; set; } = new AddressBook { UserId = string.Empty };

        // Id của địa chỉ cần sửa (từ route)
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var address = await _context.AddressBooks
                .FirstOrDefaultAsync(a => a.Id == Id && a.UserId == user.Id);

            if (address == null)
            {
                TempData["error"] = "Không tìm thấy địa chỉ hoặc bạn không có quyền sửa.";
                return RedirectToPage("AddressBook"); // Quay về trang danh sách
            }

            AddressToEdit = address;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Kiểm tra xem Id trong form có khớp với Id trong route không
            // và người dùng có sở hữu địa chỉ này không
            var addressFromDb = await _context.AddressBooks
                .AsNoTracking() // Không cần theo dõi vì sẽ cập nhật ngay sau
                .FirstOrDefaultAsync(a => a.Id == AddressToEdit.Id && a.UserId == user.Id);

            if (addressFromDb == null)
            {
                TempData["error"] = "Không tìm thấy địa chỉ hoặc bạn không có quyền sửa.";
                return RedirectToPage("AddressBook");
            }

            // Cần gán lại UserId vì nó không được gửi từ form
            AddressToEdit.UserId = user.Id;
            // Giữ nguyên CreatedAt
            AddressToEdit.CreatedAt = addressFromDb.CreatedAt;
            // Cập nhật UpdatedAt
            AddressToEdit.UpdatedAt = DateTime.UtcNow;


            if (!ModelState.IsValid)
            {
                // Nếu model không hợp lệ, hiển thị lại form với lỗi
                return Page();
            }

            // Xử lý logic IsDefault
            if (AddressToEdit.IsDefault)
            {
                // Tìm địa chỉ mặc định cũ (khác địa chỉ đang sửa) và bỏ đánh dấu
                var currentDefault = await _context.AddressBooks
                    .FirstOrDefaultAsync(a => a.UserId == user.Id && a.IsDefault && a.Id != AddressToEdit.Id);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                    _context.AddressBooks.Update(currentDefault);
                }
            }
            else
            {
                // Nếu người dùng bỏ check mặc định, và đây LÀ địa chỉ mặc định cũ
                // thì cần đặt một địa chỉ khác làm mặc định (nếu có)
                if (addressFromDb.IsDefault) // Kiểm tra trạng thái cũ từ DB
                {
                    var anotherAddress = await _context.AddressBooks
                        .FirstOrDefaultAsync(a => a.UserId == user.Id && a.Id != AddressToEdit.Id);
                    if (anotherAddress != null)
                    {
                        anotherAddress.IsDefault = true;
                        _context.AddressBooks.Update(anotherAddress);
                    }
                    else
                    {
                        // Nếu không còn địa chỉ nào khác, buộc phải giữ là mặc định
                        AddressToEdit.IsDefault = true;
                        ModelState.AddModelError(string.Empty, "Bạn không thể bỏ địa chỉ mặc định duy nhất.");
                        return Page(); // Hiển thị lại form với lỗi
                    }
                }
            }


            try
            {
                _context.Attach(AddressToEdit).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                TempData["success"] = "Đã cập nhật địa chỉ thành công!";
                return RedirectToPage("AddressBook"); // Quay về trang danh sách
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi cập nhật. Vui lòng thử lại.");
                return Page(); // Hiển thị lại form với lỗi
            }
        }
    }
}

