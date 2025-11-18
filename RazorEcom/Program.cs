using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RazorEcom.Data;
using RazorEcom.Models;
using RazorEcom.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Đăng ký DbContext với SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<CategoryService>();
// Đăng ký ASP.NET Core Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.Password.RequireDigit = true;           // Yêu cầu phải có số
    options.Password.RequireLowercase = true;       // Yêu cầu phải có chữ thường
    options.Password.RequireUppercase = true;       // Yêu cầu phải có chữ hoa
    options.Password.RequiredLength = 8;            // Độ dài tối thiểu (bạn nên đổi từ 6 thành 8)
    options.Password.RequiredUniqueChars = 1;         // Số ký tự khác nhau tối thiểu trong mật khẩu
    options.SignIn.RequireConfirmedAccount = false; // Không yêu cầu xác nhận email khi đăng ký
    options.Password.RequireNonAlphanumeric = false; // Không yêu cầu ký tự đặc biệt
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();
// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddRazorPages()
   .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

var app = builder.Build();

// == GỌI SEEDER ==
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        try
        {
            // Gọi hàm seeder  từ DbSeeder.cs
            await RazorEcom.Data.DbSeeder.SeedRolesAndAdminAsync(services);
        }
        catch (Exception ex)
        {
            var logger = loggerFactory.CreateLogger<Program>();
            logger.LogError(ex, "Đã xảy ra lỗi khi seeding database.");
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Đã xảy ra lỗi khi tạo service scope cho seeder.");
}
if (app.Environment.IsDevelopment())
{
    // ...
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();


// Dòng này quan trọng, đảm bảo các trang Identity (Login, Register...) có thể được truy cập
app.MapRazorPages();

app.Run();

