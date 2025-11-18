using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using ClosedXML.Excel;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được trang này
    public class IndexModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public IList<RazorEcom.Models.Products> Product { get; set; } = default!;
        [BindProperty]
        public required IFormFile UploadedFile { get; set; }


        public async Task OnGetAsync(int id)
        {
            Product = await _context.Products
       .Include(p => p.Variants)
       .Include(p => p.Category)
       .OrderByDescending(p => p.CreatedAt)
       .AsNoTracking()
       .ToListAsync();
        }
        // ================================================
        // HANDLER MỚI: XUẤT FILE EXCEL
        // ================================================
       public async Task<IActionResult> OnPostExportAsync()
        {
            var products = await _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking()
                .ToListAsync();            
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Products");

                // 2. Tạo Header
                worksheet.Cell(1, 1).Value = "ProductID";
                worksheet.Cell(1, 2).Value = "Tên sản phẩm";
                worksheet.Cell(1, 3).Value = "Danh mục";
                worksheet.Cell(1, 4).Value = "Thương hiệu";
                worksheet.Cell(1, 5).Value = "VariantID";
                worksheet.Cell(1, 6).Value = "SKU";
                worksheet.Cell(1, 7).Value = "Size";
                worksheet.Cell(1, 8).Value = "Màu";
                worksheet.Cell(1, 9).Value = "Giá";
                worksheet.Cell(1, 10).Value = "Số lượng";
                worksheet.Cell(1, 11).Value = "Ngày tạo";
                
                // (Tùy chọn) Làm đậm header
                worksheet.Row(1).Style.Font.Bold = true;

                // 3. Đổ dữ liệu
                int row = 2;
                foreach (var product in products)
                {
                    foreach (var variant in product.Variants)
                    {
                        worksheet.Cell(row, 1).Value = product.Id;
                        worksheet.Cell(row, 2).Value = product.Name;
                        worksheet.Cell(row, 3).Value = product.Category?.Name;
                        worksheet.Cell(row, 4).Value = product.Brand;
                        worksheet.Cell(row, 5).Value = variant.Id;
                        worksheet.Cell(row, 6).Value = variant.Sku;
                        worksheet.Cell(row, 7).Value = variant.Size;
                        worksheet.Cell(row, 8).Value = variant.Color;
                        worksheet.Cell(row, 9).Value = variant.Price;
                        worksheet.Cell(row, 10).Value = variant.Quantity;
                        worksheet.Cell(row, 11).Value = variant.CreatedAt.ToString("yyyy-MM-dd");
                        row++;
                    }
                }

                // 4. Lưu và Trả về file
                var stream = new MemoryStream();
                // THAY ĐỔI 3: Dùng hàm SaveAs() của ClosedXML
                workbook.SaveAs(stream); 
                stream.Position = 0;
                string excelName = $"Products-{DateTime.Now:yyyyMMddHHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // ================================================
        // HANDLER NHẬP EXCEL (Viết lại bằng ClosedXML)
        // ================================================
        public async Task<IActionResult> OnPostImportAsync()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file để import.";
                return RedirectToPage();
            }

            // THAY ĐỔI 4: Không cần License
            // ExcelPackage.License = OfficeOpenXml.LicenseContext.NonCommercial; // Bỏ
            var newVariants = new List<ProductVariants>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await UploadedFile.CopyToAsync(stream);
                    // THAY ĐỔI 5: Dùng XLWorkbook
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        // Lấy các hàng có dữ liệu, bỏ qua hàng 1 (header)
                        var rows = worksheet.RowsUsed().Skip(1);

                        int rowNum = 1; // Bắt đầu từ 1 (vì đã skip 1)
                        foreach(var row in rows)
                        {
                            rowNum++; // rowNum bây giờ là 2

                            // Đọc ProductID (cột 1)
                            if (!int.TryParse(row.Cell(1).Value.ToString(), out int productId))
                            {
                                throw new Exception($"Lỗi ở hàng {rowNum}: ProductID không hợp lệ.");
                            }

                            // Kiểm tra xem Product có tồn tại không
                            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
                            if (!productExists)
                            {
                                throw new Exception($"Lỗi ở hàng {rowNum}: Không tìm thấy Product với ID = {productId}.");
                            }

                            // Đọc các cột variant
                            var newVariant = new ProductVariants
                            {
                                ProductId = productId,
                                Sku = row.Cell(6).Value.ToString(),
                                Size = row.Cell(7).Value.ToString(),
                                Color = row.Cell(8).Value.ToString(),
                                Price = decimal.Parse(row.Cell(9).Value.ToString() ?? "0"),
                                Quantity = int.Parse(row.Cell(10).Value.ToString() ?? "0"),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };
                            
                            newVariants.Add(newVariant);
                        }
                    }
                }

                if (newVariants.Any())
                {
                    await _context.ProductVariants.AddRangeAsync(newVariants);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Đã import thành công {newVariants.Count} biến thể sản phẩm mới.";
                }
                else
                {
                    TempData["InfoMessage"] = "File không có dữ liệu để import.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
            }

            return RedirectToPage();
        }

    }
}
