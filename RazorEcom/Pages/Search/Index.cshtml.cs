using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;

using System.Security.Claims;
using CartModel = global::RazorEcom.Models.Cart;
namespace RazorEcom.Pages
{
    public class SearchModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public SearchModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // BindProperty(SupportsGet = true) cho phép nhận giá trị "q" từ URL
        [BindProperty(SupportsGet = true)]
        public string q { get; set; }

        public List<ProductVariants> SearchResults { get; set; } = new List<ProductVariants>();

        // Lấy logic OnPostAddToCart từ trang Index để thêm vào giỏ hàng
        // SỬA LỖI CS0246: Thêm 'global::' để giải quyết triệt để xung đột namespace
        private async Task<CartModel> GetOrCreateCartAsync(string userId) // Dùng alias
        {
            // Tìm Cart dựa trên UserId
            CartModel cart = await _context.Carts.FirstOrDefaultAsync(c => c.UserId == userId); // Dùng alias

            if (cart == null)
            {
                // Nếu không có Cart, tạo mới và gán UserId
                cart = new CartModel // Dùng alias
                {
                    CreatedAt = System.DateTime.Now,
                    UserId = userId // Gán UserId (bắt buộc)
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task OnGetAsync()
        {
            if (!string.IsNullOrEmpty(q))
            {
                // Chuyển truy vấn sang chữ thường để tìm kiếm không phân biệt hoa/thường
                var lowerQuery = q.ToLower();

                // Tìm kiếm trong ProductVariants, bao gồm cả thông tin Product
                SearchResults = await _context.ProductVariants
                    .Include(v => v.Product)
                    .Where(v =>
                        v.Product.Name.ToLower().Contains(lowerQuery) ||
                        v.Product.Description.ToLower().Contains(lowerQuery) ||
                        v.Product.Brand.ToLower().Contains(lowerQuery) ||
                        v.Sku.ToLower().Contains(lowerQuery) ||
                        v.Color.ToLower().Contains(lowerQuery) ||
                        v.Size.ToLower().Contains(lowerQuery)
                    )
                    .AsNoTracking() // Dùng AsNoTracking để tăng hiệu suất đọc
                    .ToListAsync();
            }
            // Nếu 'q' rỗng, SearchResults sẽ là danh sách rỗng (mặc định)
        }

        // Tái sử dụng logic OnPostAddToCart từ IndexModel
        public async Task<IActionResult> OnPostAddToCart(int variantId)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            CartModel cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.VariantId == variantId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    VariantId = variantId,
                    Quantity = 1,
                    CreatedAt = System.DateTime.Now
                };
                _context.CartItems.Add(newItem);
            }

            await _context.SaveChangesAsync();
            
            // Thêm TempData để thông báo thành công
            TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng!";
            
            // Quay lại trang tìm kiếm với truy vấn cũ
            return RedirectToPage(new { q = this.q });
        }
    }
}