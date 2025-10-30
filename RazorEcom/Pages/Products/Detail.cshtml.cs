using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System;

namespace RazorEcom.Pages.Products
{
    // Chúng ta nên yêu cầu Authorize, vì chỉ người dùng đăng nhập mới có thể thêm vào giỏ
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DetailModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // === CÁC THUỘC TÍNH ĐỂ HIỂN THỊ ===
        public Models.Products Product { get; set; } = null!;
        public ProductVariants DefaultVariant { get; set; } = null!;
        public SelectList VariantOptions { get; set; } = null!;
        public string VariantsJson { get; set; } = "{}"; // Dùng cho JavaScript

        // === CÁC THUỘC TÍNH ĐỂ BINDING (NHẬN DỮ LIỆU) ===
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // ID của Product (gốc)

        [BindProperty]
        public int SelectedVariantId { get; set; } // ID của biến thể được chọn từ dropdown

        [BindProperty]
        public int Quantity { get; set; } = 1; // Số lượng từ ô input

        // =================================================================
        // HÀM TẢI TRANG (GET)
        // =================================================================
        public async Task<IActionResult> OnGetAsync()
        {
            // Tải sản phẩm gốc và TẤT CẢ các biến thể của nó
            Product = await _context.Products
                .Include(p => p.Variants) // Tải các biến thể liên quan
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == Id);

            // Nếu không tìm thấy sản phẩm, hoặc sản phẩm không có biến thể nào
            if (Product == null || !Product.Variants.Any())
            {
                return NotFound("Không tìm thấy sản phẩm hoặc sản phẩm không có biến thể.");
            }

            // Tải lại dữ liệu (để chuẩn bị dropdown, json, v.v.)
            return await ReLoadPageData();
        }

        // =================================================================
        // HÀM XỬ LÝ THÊM VÀO GIỎ (POST)
        // =================================================================
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin không hợp lệ.";
                return await ReLoadPageData(); // Tải lại trang với lỗi
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                // Điều này không nên xảy ra do có [Authorize], nhưng vẫn kiểm tra
                return Challenge();
            }

            // 1. Tìm biến thể (variant) mà người dùng đã chọn
            var selectedVariant = await _context.ProductVariants
                                        .FirstOrDefaultAsync(v => v.Id == SelectedVariantId && v.ProductId == Id);

            if (selectedVariant == null)
            {
                TempData["ErrorMessage"] = "Phiên bản sản phẩm không hợp lệ.";
                return await ReLoadPageData();
            }

            // 2. Kiểm tra tồn kho (Quantity là số lượng người dùng muốn mua)
            if (selectedVariant.Quantity < Quantity)
            {
                TempData["ErrorMessage"] = $"Xin lỗi, chỉ còn {selectedVariant.Quantity} sản phẩm trong kho.";
                return await ReLoadPageData();
            }

            // 3. Tìm giỏ hàng của người dùng (hoặc tạo mới)
            var cart = await _context.Carts
                            .Include(c => c.Items) // Tải các CartItem hiện có
                            .FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart == null)
            {
                // Nếu người dùng chưa có giỏ hàng, tạo mới
                // SỬA LỖI: Chỉ định rõ RazorEcom.Models.Cart
                cart = new RazorEcom.Models.Cart { UserId = user.Id };
                _context.Carts.Add(cart);
                // Cần SaveChanges 1 lần để cart có Id
                await _context.SaveChangesAsync();
            }

            // 4. Kiểm tra xem sản phẩm (biến thể) đã có trong giỏ chưa
            var cartItem = cart.Items.FirstOrDefault(i => i.VariantId == SelectedVariantId);

            if (cartItem != null)
            {
                // Đã có -> Cập nhật số lượng
                // Kiểm tra lại tồn kho tổng (số lượng hiện có + số lượng thêm mới)
                if (selectedVariant.Quantity < cartItem.Quantity + Quantity)
                {
                    TempData["ErrorMessage"] = $"Số lượng trong giỏ ({cartItem.Quantity}) + số lượng thêm ({Quantity}) vượt quá tồn kho ({selectedVariant.Quantity}).";
                    return await ReLoadPageData();
                }
                cartItem.Quantity += Quantity;
                cartItem.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Chưa có -> Thêm mới
                // SỬA LỖI: Chỉ định rõ RazorEcom.Models.CartItem
                cartItem = new RazorEcom.Models.CartItem
                {
                    CartId = cart.Id,
                    VariantId = SelectedVariantId,
                    Quantity = Quantity
                    // UnitPrice đã được xóa vì không có trong model CartItem
                };
                cart.Items.Add(cartItem);
            }

            cart.UpdatedAt = DateTime.UtcNow;

            // 5. Lưu vào CSDL
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            // Chuyển hướng về chính trang đó (để tránh lỗi F5 gửi lại form)
            return RedirectToPage(new { id = Id });
        }

        // =================================================================
        // HÀM TRỢ GIÚP
        // =================================================================
        // Hàm này dùng để tải lại dữ liệu trang (dùng cho cả OnGet và OnPost khi có lỗi)
        private async Task<IActionResult> ReLoadPageData()
        {
            // Đảm bảo Product đã được tải
            if (Product == null)
            {
                Product = await _context.Products
                   .Include(p => p.Variants)
                   .AsNoTracking()
                   .FirstOrDefaultAsync(p => p.Id == Id);
                if (Product == null) return NotFound();
            }

            var allVariants = Product.Variants.ToList();
            var culture = new CultureInfo("vi-VN");

            // Lấy biến thể đầu tiên làm mặc định
            DefaultVariant = allVariants.FirstOrDefault()!;
            if (DefaultVariant == null) return NotFound("Sản phẩm không có biến thể.");

            // Tạo SelectList cho dropdown
            VariantOptions = new SelectList(allVariants.Select(v => new
            {
                v.Id,
                // Hiển thị: "M - Trắng - Cotton"
                DisplayText = $"{v.Size} - {v.Color}" + (string.IsNullOrEmpty(v.Material) ? "" : $" - {v.Material}")
            }), "Id", "DisplayText", DefaultVariant.Id);

            // Tạo JSON cho JavaScript (để cập nhật giá/ảnh/sku/material khi đổi dropdown)
            var variantsData = allVariants.ToDictionary(
                v => v.Id,
                v => new
                {
                    v.Id,
                    Price = v.Price.ToString("C0", culture), // Định dạng tiền
                    v.ImageUrl,
                    v.Sku,
                    v.Material
                });
            VariantsJson = JsonSerializer.Serialize(variantsData);

            return Page();
        }
    }
}

