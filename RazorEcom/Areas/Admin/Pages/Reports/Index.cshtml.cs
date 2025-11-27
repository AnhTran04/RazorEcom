using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorEcom.Services;
using ClosedXML.Excel;
using System.Globalization;

namespace RazorEcom.Areas.Admin.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ReportService _reportService;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ReportService reportService, ILogger<IndexModel> logger)
        {
            _reportService = reportService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string ReportType { get; set; } = "sales"; // sales, inventory, customer

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public SalesReportData? SalesData { get; set; }
        public InventoryReportData? InventoryData { get; set; }
        public CustomerReportData? CustomerData { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Set default dates if not provided
            EndDate ??= DateTime.Now;
            StartDate ??= DateTime.Now.AddDays(-30);

            // Generate report based on type
            switch (ReportType.ToLower())
            {
                case "sales":
                    SalesData = await _reportService.GenerateSalesReportDataAsync(StartDate.Value, EndDate.Value);
                    break;
                case "inventory":
                    InventoryData = await _reportService.GenerateInventoryReportDataAsync();
                    break;
                case "customer":
                    CustomerData = await _reportService.GenerateCustomerReportDataAsync(StartDate.Value, EndDate.Value);
                    break;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostExportPdfAsync()
        {
            try
            {
                EndDate ??= DateTime.Now;
                StartDate ??= DateTime.Now.AddDays(-30);

                byte[] pdfBytes;
                string filename;

                switch (ReportType.ToLower())
                {
                    case "sales":
                        var salesData = await _reportService.GenerateSalesReportDataAsync(StartDate.Value, EndDate.Value);
                        pdfBytes = _reportService.GenerateSalesPdf(salesData);
                        filename = $"Sales_Report_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.pdf";
                        break;

                    case "inventory":
                        var inventoryData = await _reportService.GenerateInventoryReportDataAsync();
                        pdfBytes = _reportService.GenerateInventoryPdf(inventoryData);
                        filename = $"Inventory_Report_{DateTime.Now:yyyyMMdd}.pdf";
                        break;                    default:
                        TempData["error"] = "Loại báo cáo không hợp lệ";
                        return RedirectToPage();
                }

                return File(pdfBytes, "application/pdf", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF report");
                TempData["error"] = "Lỗi khi tạo báo cáo PDF: " + ex.Message;
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostExportExcelAsync()
        {
            try
            {
                EndDate ??= DateTime.Today;
                StartDate ??= DateTime.Today.AddDays(-30);

                var culture = new CultureInfo("vi-VN");

                using var workbook = new XLWorkbook();
                string filename;

                switch (ReportType.ToLower())
                {
                    case "sales":
                        var salesData = await _reportService.GenerateSalesReportDataAsync(StartDate.Value, EndDate.Value);
                        var salesSheet = workbook.Worksheets.Add("Báo cáo Doanh thu");
                        
                        // Header
                        salesSheet.Cell(1, 1).Value = "BÁO CÁO DOANH THU";
                        salesSheet.Cell(1, 1).Style.Font.FontSize = 16;
                        salesSheet.Cell(1, 1).Style.Font.Bold = true;
                        
                        salesSheet.Cell(2, 1).Value = $"Từ {StartDate:dd/MM/yyyy} đến {EndDate:dd/MM/yyyy}";
                        
                        // Summary
                        salesSheet.Cell(4, 1).Value = "Tổng doanh thu:";
                        salesSheet.Cell(4, 2).Value = salesData.TotalRevenue;
                        salesSheet.Cell(4, 2).Style.NumberFormat.Format = "#,##0 ₫";
                        
                        salesSheet.Cell(5, 1).Value = "Tổng đơn hàng:";
                        salesSheet.Cell(5, 2).Value = salesData.TotalOrders;
                        
                        salesSheet.Cell(6, 1).Value = "Giá trị TB/Đơn:";
                        salesSheet.Cell(6, 2).Value = salesData.AverageOrderValue;
                        salesSheet.Cell(6, 2).Style.NumberFormat.Format = "#,##0 ₫";
                        
                        // Top Products Table
                        salesSheet.Cell(8, 1).Value = "SẢN PHẨM BÁN CHẠY";
                        salesSheet.Cell(8, 1).Style.Font.Bold = true;
                        
                        salesSheet.Cell(9, 1).Value = "Sản phẩm";
                        salesSheet.Cell(9, 2).Value = "Số lượng";
                        salesSheet.Cell(9, 3).Value = "Doanh thu";
                        salesSheet.Range(9, 1, 9, 3).Style.Font.Bold = true;
                        salesSheet.Range(9, 1, 9, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
                        
                        int row = 10;
                        foreach (var product in salesData.TopProducts)
                        {
                            salesSheet.Cell(row, 1).Value = product.ProductName;
                            salesSheet.Cell(row, 2).Value = product.Quantity;
                            salesSheet.Cell(row, 3).Value = product.Revenue;
                            salesSheet.Cell(row, 3).Style.NumberFormat.Format = "#,##0 ₫";
                            row++;
                        }
                        
                        salesSheet.Columns().AdjustToContents();
                        filename = $"Sales_Report_{StartDate:yyyyMMdd}_{EndDate:yyyyMMdd}.xlsx";
                        break;

                    case "inventory":
                        var inventoryData = await _reportService.GenerateInventoryReportDataAsync();
                        var inventorySheet = workbook.Worksheets.Add("Báo cáo Tồn kho");
                        
                        // Header
                        inventorySheet.Cell(1, 1).Value = "BÁO CÁO TỒN KHO";
                        inventorySheet.Cell(1, 1).Style.Font.FontSize = 16;
                        inventorySheet.Cell(1, 1).Style.Font.Bold = true;
                        
                        // Summary
                        inventorySheet.Cell(3, 1).Value = "Tổng sản phẩm:";
                        inventorySheet.Cell(3, 2).Value = inventoryData.TotalProducts;
                        
                        inventorySheet.Cell(4, 1).Value = "Sắp hết hàng:";
                        inventorySheet.Cell(4, 2).Value = inventoryData.LowStockCount;
                        
                        inventorySheet.Cell(5, 1).Value = "Hết hàng:";
                        inventorySheet.Cell(5, 2).Value = inventoryData.OutOfStockCount;
                        
                        inventorySheet.Cell(6, 1).Value = "Giá trị tồn kho:";
                        inventorySheet.Cell(6, 2).Value = inventoryData.TotalStockValue;
                        inventorySheet.Cell(6, 2).Style.NumberFormat.Format = "#,##0 ₫";
                        
                        // Category Table
                        inventorySheet.Cell(8, 1).Value = "TỒN KHO THEO DANH MỤC";
                        inventorySheet.Cell(8, 1).Style.Font.Bold = true;
                        
                        inventorySheet.Cell(9, 1).Value = "Danh mục";
                        inventorySheet.Cell(9, 2).Value = "Số SP";
                        inventorySheet.Cell(9, 3).Value = "Số lượng";
                        inventorySheet.Cell(9, 4).Value = "Giá trị";
                        inventorySheet.Range(9, 1, 9, 4).Style.Font.Bold = true;
                        inventorySheet.Range(9, 1, 9, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
                        
                        row = 10;
                        foreach (var cat in inventoryData.StockByCategory)
                        {
                            inventorySheet.Cell(row, 1).Value = cat.CategoryName;
                            inventorySheet.Cell(row, 2).Value = cat.ProductCount;
                            inventorySheet.Cell(row, 3).Value = cat.TotalQuantity;
                            inventorySheet.Cell(row, 4).Value = cat.TotalValue;
                            inventorySheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0 ₫";
                            row++;
                        }
                        
                        inventorySheet.Columns().AdjustToContents();
                        filename = $"Inventory_Report_{DateTime.Now:yyyyMMdd}.xlsx";
                        break;

                    default:
                        TempData["error"] = "Loại báo cáo không hợp lệ";
                        return RedirectToPage();
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var excelBytes = stream.ToArray();

                return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Excel report");
                TempData["error"] = "Lỗi khi tạo báo cáo Excel: " + ex.Message;
                return RedirectToPage();
            }
        }
    }
}
