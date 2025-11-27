using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RazorEcom.Data;
using RazorEcom.Models;
using ClosedXML.Excel;
using RazorEcom.Helper;

namespace RazorEcom.Areas.Admin.Pages.Products
{
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được trang này
    public class IndexModel(ApplicationDbContext context) : PageModel
    {
        private readonly ApplicationDbContext _context = context;

        [BindProperty]
        public PaginatedList<RazorEcom.Models.Products> Product { get; set; } = default!;
        
        [BindProperty]
        public required IFormFile UploadedFile { get; set; }


        public async Task OnGetAsync(int? pageIndex)
        {
            var query = _context.Products
                .Include(p => p.Variants)
                .Include(p => p.Category)
                .OrderByDescending(p => p.CreatedAt)
                .AsNoTracking();

            int pageSize = 10;
            Product = await PaginatedList<RazorEcom.Models.Products>.CreateAsync(query, pageIndex ?? 1, pageSize);
        }

        // ================================================
        // HANDLER: XUẤT FILE EXCEL
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

                // Tạo Header
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
                
                // Style Header
                var headerRange = worksheet.Range("A1:K1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Đổ dữ liệu
                int row = 2;
                foreach (var product in products)
                {
                    if (product.Variants != null && product.Variants.Any())
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
                    else
                    {
                        // Sản phẩm không có variant, chỉ in thông tin chung
                        worksheet.Cell(row, 1).Value = product.Id;
                        worksheet.Cell(row, 2).Value = product.Name;
                        worksheet.Cell(row, 3).Value = product.Category?.Name;
                        worksheet.Cell(row, 4).Value = product.Brand;
                        row++;
                    }
                }

                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                string excelName = $"Products_Export_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", excelName);
            }
        }

        // ================================================
        // HANDLER: TẢI FILE MẪU (TEMPLATE)
        // ================================================
        public IActionResult OnPostDownloadTemplate()
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Template");

                // Tạo Header
                worksheet.Cell(1, 1).Value = "ProductID (Bắt buộc)";
                worksheet.Cell(1, 2).Value = "Tên SP (Tham khảo)";
                worksheet.Cell(1, 3).Value = "Danh mục (Tham khảo)";
                worksheet.Cell(1, 4).Value = "Brand (Tham khảo)";
                worksheet.Cell(1, 5).Value = "VariantID (Bỏ trống nếu mới)";
                worksheet.Cell(1, 6).Value = "SKU";
                worksheet.Cell(1, 7).Value = "Size";
                worksheet.Cell(1, 8).Value = "Màu";
                worksheet.Cell(1, 9).Value = "Giá";
                worksheet.Cell(1, 10).Value = "Số lượng";

                var headerRange = worksheet.Range("A1:J1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.Yellow;

                // Dữ liệu mẫu
                worksheet.Cell(2, 1).Value = 101;
                worksheet.Cell(2, 2).Value = "Áo Thun Basic";
                worksheet.Cell(2, 6).Value = "AT-01-L-RED";
                worksheet.Cell(2, 7).Value = "L";
                worksheet.Cell(2, 8).Value = "Đỏ";
                worksheet.Cell(2, 9).Value = 150000;
                worksheet.Cell(2, 10).Value = 50;

                worksheet.Columns().AdjustToContents();

                var stream = new MemoryStream();
                workbook.SaveAs(stream);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Product_Import_Template.xlsx");
            }
        }

        // ================================================
        // HANDLER: NHẬP EXCEL
        // ================================================
        public async Task<IActionResult> OnPostImportAsync()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file để import.";
                return RedirectToPage();
            }

            var newVariants = new List<ProductVariants>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await UploadedFile.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1); // Lấy sheet đầu tiên
                        var rows = worksheet.RowsUsed().Skip(1); // Bỏ qua header

                        int rowNum = 1;
                        foreach(var row in rows)
                        {
                            rowNum++;

                            // Đọc ProductID (cột 1)
                            var cellProductId = row.Cell(1).Value.ToString();
                            if (string.IsNullOrWhiteSpace(cellProductId) || !int.TryParse(cellProductId, out int productId))
                            {
                                // Bỏ qua dòng không có ID hoặc lỗi
                                continue;
                            }

                            // Kiểm tra Product tồn tại
                            var productExists = await _context.Products.AnyAsync(p => p.Id == productId);
                            if (!productExists)
                            {
                                throw new Exception($"Lỗi dòng {rowNum}: Không tìm thấy Sản phẩm ID = {productId}");
                            }

                            // Đọc các cột variant (Dựa trên template: 6=SKU, 7=Size, 8=Color, 9=Price, 10=Qty)
                            var newVariant = new ProductVariants
                            {
                                ProductId = productId,
                                Sku = row.Cell(6).Value.ToString(),
                                Size = row.Cell(7).Value.ToString(),
                                Color = row.Cell(8).Value.ToString(),
                                CreatedAt = DateTime.Now,
                                UpdatedAt = DateTime.Now
                            };

                            // Xử lý giá
                            if (decimal.TryParse(row.Cell(9).Value.ToString(), out decimal price))
                                newVariant.Price = price;
                            else
                                newVariant.Price = 0;

                            // Xử lý số lượng
                            if (int.TryParse(row.Cell(10).Value.ToString(), out int qty))
                                newVariant.Quantity = qty;
                            else
                                newVariant.Quantity = 0;
                            
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
                    TempData["ErrorMessage"] = "Không đọc được dữ liệu hợp lệ từ file.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi Import: {ex.Message}";
            }

            return RedirectToPage();
        }
    }
}