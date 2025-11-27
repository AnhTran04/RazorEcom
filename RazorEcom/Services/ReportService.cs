using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RazorEcom.Data;
using RazorEcom.Models;
using System.Globalization;

namespace RazorEcom.Services
{
    public class ReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ReportService> _logger;

        public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
        {
            _context = context;
            _logger = logger;
            
            // Configure QuestPDF license (Community edition is free for non-commercial use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // =============================================
        // SALES REPORT DATA
        // =============================================
        public async Task<SalesReportData> GenerateSalesReportDataAsync(DateTime startDate, DateTime endDate)
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Variant)
                .ThenInclude(v => v.Product)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .AsNoTracking()
                .ToListAsync();

            var totalRevenue = orders.Sum(o => o.Total);
            var totalOrders = orders.Count;
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            // Top products
            var topProducts = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.Variant.Product.Name)
                .Select(g => new TopProductData
                {
                    ProductName = g.Key,
                    Quantity = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.UnitPrice * oi.Quantity)
                })
                .OrderByDescending(p => p.Revenue)
                .Take(10)
                .ToList();

            // Daily revenue
            var dailyRevenue = orders
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new DailyRevenueData
                {
                    Date = g.Key,
                    Revenue = g.Sum(o => o.Total),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            return new SalesReportData
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                AverageOrderValue = averageOrderValue,
                TopProducts = topProducts,
                DailyRevenue = dailyRevenue
            };
        }

        // =============================================
        // INVENTORY REPORT DATA
        // =============================================
        public async Task<InventoryReportData> GenerateInventoryReportDataAsync()
        {
            var variants = await _context.ProductVariants
                .Include(v => v.Product)
                .ThenInclude(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            var totalProducts = variants.Count;
            var lowStockCount = variants.Count(v => v.Quantity <= 10);
            var outOfStockCount = variants.Count(v => v.Quantity == 0);
            var totalStockValue = variants.Sum(v => v.Price * v.Quantity);

            var stockByCategory = variants
                .GroupBy(v => v.Product.Category?.Name ?? "Uncategorized")
                .Select(g => new CategoryStockData
                {
                    CategoryName = g.Key,
                    ProductCount = g.Count(),
                    TotalQuantity = g.Sum(v => v.Quantity),
                    TotalValue = g.Sum(v => v.Price * v.Quantity)
                })
                .OrderByDescending(c => c.TotalValue)
                .ToList();

            var lowStockProducts = variants
                .Where(v => v.Quantity <= 10 && v.Quantity > 0)
                .OrderBy(v => v.Quantity)
                .Select(v => new LowStockProductData
                {
                    ProductName = v.Product.Name,
                    Variant = $"{v.Size} - {v.Color}",
                    Quantity = v.Quantity,
                    Price = v.Price
                })
                .ToList();

            return new InventoryReportData
            {
                TotalProducts = totalProducts,
                LowStockCount = lowStockCount,
                OutOfStockCount = outOfStockCount,
                TotalStockValue = totalStockValue,
                StockByCategory = stockByCategory,
                LowStockProducts = lowStockProducts
            };
        }

        // =============================================
        // CUSTOMER REPORT DATA
        // =============================================
        public async Task<CustomerReportData> GenerateCustomerReportDataAsync(DateTime startDate, DateTime endDate)
        {
            var totalCustomers = await _context.Users.CountAsync();
            
            var newCustomers = await _context.Users
                .Where(u => u.EmailConfirmed && u.LockoutEnabled == false)
                .CountAsync();

            var topCustomers = await _context.Orders
                .Include(o => o.User)
                .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
                .GroupBy(o => new { o.UserId, o.User.Email, o.User.FullName })
                .Select(g => new TopCustomerData
                {
                    CustomerName = g.Key.FullName ?? g.Key.Email ?? "Unknown",
                    Email = g.Key.Email ?? "",
                    OrderCount = g.Count(),
                    TotalSpent = g.Sum(o => o.Total)
                })
                .OrderByDescending(c => c.TotalSpent)
                .Take(10)
                .ToListAsync();

            return new CustomerReportData
            {
                StartDate = startDate,
                EndDate = endDate,
                TotalCustomers = totalCustomers,
                NewCustomers = newCustomers,
                TopCustomers = topCustomers
            };
        }

        // =============================================
        // PDF GENERATION - SALES REPORT
        // =============================================
        public byte[] GenerateSalesPdf(SalesReportData data)
        {
            var culture = new CultureInfo("vi-VN");
            
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Blue.Darken3)
                        .Padding(20)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("RazorEcom").FontSize(24).FontColor(Colors.White).Bold();
                                column.Item().Text("BÁO CÁO DOANH THU").FontSize(16).FontColor(Colors.Grey.Lighten2);
                            });
                            
                            row.ConstantItem(100).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy}").FontColor(Colors.White);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Period
                            column.Item().Text($"Từ ngày: {data.StartDate:dd/MM/yyyy} - Đến ngày: {data.EndDate:dd/MM/yyyy}")
                                .FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
                            
                            column.Item().PaddingTop(20);

                            // Summary Cards
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Tổng Doanh Thu").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.TotalRevenue.ToString("C0", culture)).FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                                
                                row.Spacing(10);
                                
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Tổng Đơn Hàng").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.TotalOrders.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                                
                                row.Spacing(10);
                                
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Giá Trị TB/Đơn").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.AverageOrderValue.ToString("C0", culture)).FontSize(18).Bold().FontColor(Colors.Orange.Darken2);
                                });
                            });

                            column.Item().PaddingTop(30);

                            // Top Products Table
                            column.Item().Text("SẢN PHẨM BÁN CHẠY").FontSize(14).Bold();
                            column.Item().PaddingTop(10);
                            
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Sản phẩm").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("SL").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Doanh thu").Bold();
                                });

                                foreach (var product in data.TopProducts.Take(10))
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(product.ProductName);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(product.Quantity.ToString("N0"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).AlignRight().Text(product.Revenue.ToString("C0", culture));
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }

        // =============================================
        // PDF GENERATION - INVENTORY REPORT
        // =============================================
        public byte[] GenerateInventoryPdf(InventoryReportData data)
        {
            var culture = new CultureInfo("vi-VN");
            
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Purple.Darken3)
                        .Padding(20)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("RazorEcom").FontSize(24).FontColor(Colors.White).Bold();
                                column.Item().Text("BÁO CÁO TỒN KHO").FontSize(16).FontColor(Colors.Grey.Lighten2);
                            });
                            
                            row.ConstantItem(100).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy}").FontColor(Colors.White);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Summary
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Tổng Sản Phẩm").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.TotalProducts.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                                
                                row.Spacing(10);
                                
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col  =>
                                {
                                    col.Item().Text("Sắp Hết Hàng").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.LowStockCount.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Orange.Darken2);
                                });
                                
                                row.Spacing(10);
                                
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Giá Trị Tồn").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.TotalStockValue.ToString("C0", culture)).FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                            });

                            column.Item().PaddingTop(30);

                            // Stock by Category
                            column.Item().Text("TỒN KHO THEO DANH MỤC").FontSize(14).Bold();
                            column.Item().PaddingTop(10);
                            
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Danh mục").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("SP").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("SL").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Giá trị").Bold();
                                });

                                foreach (var cat in data.StockByCategory)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(cat.CategoryName);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(cat.ProductCount.ToString("N0"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(cat.TotalQuantity.ToString("N0"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).AlignRight().Text(cat.TotalValue.ToString("C0", culture));
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }


        // =============================================
        // PDF GENERATION - CUSTOMER REPORT
        // =============================================
        public byte[] GenerateCustomerPdf(CustomerReportData data)
        {
            var culture = new CultureInfo("vi-VN");
            
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    page.Header()
                        .Height(100)
                        .Background(Colors.Teal.Darken3)
                        .Padding(20)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("RazorEcom").FontSize(24).FontColor(Colors.White).Bold();
                                column.Item().Text("BÁO CÁO KHÁCH HÀNG").FontSize(16).FontColor(Colors.Grey.Lighten2);
                            });
                            
                            row.ConstantItem(100).AlignRight().Text($"{DateTime.Now:dd/MM/yyyy}").FontColor(Colors.White);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Period
                            column.Item().Text($"Từ ngày: {data.StartDate:dd/MM/yyyy} - Đến ngày: {data.EndDate:dd/MM/yyyy}")
                                .FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
                            
                            column.Item().PaddingTop(20);

                            // Summary Cards
                            column.Item().Row(row =>
                            {
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Tổng Khách Hàng").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.TotalCustomers.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Blue.Darken2);
                                });
                                
                                row.Spacing(10);
                                
                                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(col =>
                                {
                                    col.Item().Text("Khách Hàng Mới").FontSize(10).FontColor(Colors.Grey.Darken1);
                                    col.Item().Text(data.NewCustomers.ToString("N0")).FontSize(18).Bold().FontColor(Colors.Green.Darken2);
                                });
                            });

                            column.Item().PaddingTop(30);

                            // Top Customers Table
                            column.Item().Text("TOP 10 KHÁCH HÀNG CHI TIÊU CAO NHẤT").FontSize(14).Bold();
                            column.Item().PaddingTop(10);
                            
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Tên khách hàng").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Email").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Đơn hàng").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Tổng chi tiêu").Bold();
                                });

                                foreach (var customer in data.TopCustomers)
                                {
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(customer.CustomerName);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(customer.Email);
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).Text(customer.OrderCount.ToString("N0"));
                                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).Padding(5).AlignRight().Text(customer.TotalSpent.ToString("C0", culture));
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Trang ");
                            x.CurrentPageNumber();
                            x.Span(" / ");
                            x.TotalPages();
                        });
                });
            }).GeneratePdf();
        }
    }

    // =============================================
    // DATA MODELS
    // =============================================
    public class SalesReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public List<TopProductData> TopProducts { get; set; } = new();
        public List<DailyRevenueData> DailyRevenue { get; set; } = new();
    }

    public class TopProductData
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DailyRevenueData
    {
        public DateTime Date { get; set; }
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class InventoryReportData
    {
        public int TotalProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public decimal TotalStockValue { get; set; }
        public List<CategoryStockData> StockByCategory { get; set; } = new();
        public List<LowStockProductData> LowStockProducts { get; set; } = new();
    }

    public class CategoryStockData
    {
        public string CategoryName { get; set; } = "";
        public int ProductCount { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class LowStockProductData
    {
        public string ProductName { get; set; } = "";
        public string Variant { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class CustomerReportData
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TotalCustomers { get; set; }
        public int NewCustomers { get; set; }
        public List<TopCustomerData> TopCustomers { get; set; } = new();
    }

    public class TopCustomerData
    {
        public string CustomerName { get; set; } = "";
        public string Email { get; set; } = "";
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }
}
