using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json; // Cần cho việc serialize JSON

namespace RazorEcom.Pages.Products
{
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // === CÁC THUỘC TÍNH ĐỂ HIỂN THỊ DỮ LIỆU ===

        // 1. Dùng để lưu sản phẩm gốc (tên, mô tả)
        public RazorEcom.Models.Products Product { get; set; } = null!;

        // 2. Dùng để hiển thị giá/ảnh/sku/material mặc định
        public ProductVariants DefaultVariant { get; set; } = null!;

        // 3. Dùng để tạo dropdown (ví dụ: "Size M - Trắng")
        public List<SelectListItem> VariantOptions { get; set; } = new();


        // === CÁC THUỘC TÍNH BINDING VỚI FORM ===

        // 4. Nhận ID từ URL (ví dụ: /Products/Detail/1)
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // Phải khớp với asp-for="Id"

        // 5. Nhận ID biến thể được chọn từ dropdown khi POST
        [BindProperty]
        [Required(ErrorMessage = "Vui lòng chọn một phiên bản")]
        public int SelectedVariantId { get; set; } // Phải khớp với asp-for="SelectedVariantId"

        // 6. Nhận số lượng từ input
        [BindProperty]
        [Range(1, 100, ErrorMessage = "Số lượng phải từ 1 đến 100")]
        public int Quantity { get; set; } = 1; // Phải khớp với asp-for="Quantity"


        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Tải sản phẩm VÀ tất cả các biến thể của nó
            Product = await _context.Products
                .Include(p => p.Variants) // Rất quan trọng
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null || !Product.Variants.Any())
            {
                // Nếu không có sản phẩm hoặc sản phẩm không có biến thể
                return NotFound();
            }

            // Lấy biến thể đầu tiên làm mặc định
            DefaultVariant = Product.Variants.First();

            // Tạo danh sách cho dropdown
            VariantOptions = Product.Variants.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(), // Giá trị là ID của biến thể
                Text = $"{v.Size} - {v.Color} - {v.Material}" // Text hiển thị (ĐÃ CẬP NHẬT)
            }).ToList();

            // Đặt giá trị mặc định cho dropdown
            SelectedVariantId = DefaultVariant.Id;
            Id = Product.Id; // Đặt ID cho form POST

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Kiểm tra xem các giá trị gửi lên (SelectedVariantId, Quantity) có hợp lệ không
            if (!ModelState.IsValid)
            {
                // Nếu không hợp lệ, tải lại dữ liệu cho trang
                // (Phải làm lại logic của OnGetAsync)
                await ReLoadPageData();
                return Page();
            }

            // 2. Logic thêm vào giỏ hàng
            // ...
            // var cartService = ...;
            // await cartService.AddToCart(SelectedVariantId, Quantity);
            // ...

            // Tạm thời chỉ thông báo
            TempData["SuccessMessage"] = $"Đã thêm sản phẩm (ID: {SelectedVariantId}) x {Quantity} vào giỏ!";

            // Tải lại dữ liệu trang sau khi POST
            await ReLoadPageData();
            return Page();
            // Hoặc chuyển hướng đến trang giỏ hàng
            // return RedirectToPage("/Cart/Index");
        }

        // Hàm tiện ích để tải lại dữ liệu cho OnPostAsync
        private async Task ReLoadPageData()
        {
            Product = await _context.Products
                .Include(p => p.Variants)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == this.Id); // Dùng this.Id (từ hidden input)

            if (Product != null && Product.Variants.Any())
            {
                DefaultVariant = Product.Variants.FirstOrDefault(v => v.Id == SelectedVariantId) ?? Product.Variants.First();

                VariantOptions = Product.Variants.Select(v => new SelectListItem
                {
                    Value = v.Id.ToString(),
                    Text = $"{v.Size} - {v.Color} - {v.Material}" // Text hiển thị (ĐÃ CẬP NHẬT)
                }).ToList();
            }
        }
    }
}


