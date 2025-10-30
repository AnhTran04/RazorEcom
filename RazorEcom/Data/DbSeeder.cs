using Microsoft.AspNetCore.Identity;
using RazorEcom.Models; 

namespace RazorEcom.Data
{
    public static class DbSeeder
    {
        // Phương thức này sẽ được gọi từ Program.cs
        public static async Task SeedRolesAndAdminAsync(IServiceProvider services)
        {
            // Lấy các dịch vụ cần thiết
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            // --- 1. Tạo Roles (Vai trò) ---
            // Kiểm tra xem vai trò "Admin" đã tồn tại chưa
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            // (Tùy chọn) Tạo vai trò "User"
            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // --- 2. Tạo User Admin ---
            string adminEmail = "admin@app.com";
            string adminPassword = "Password123!"; // Đổi mật khẩu này!

            // Kiểm tra xem user admin đã tồn tại chưa
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                // Nếu chưa, tạo user mới
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Quản trị viên", // Thêm FullName hoặc các trường tùy chỉnh
                    EmailConfirmed = true // Xác thực email luôn
                };

                // Tạo user với mật khẩu đã định
                var result = await userManager.CreateAsync(adminUser, adminPassword);

                if (result.Succeeded)
                {
                    // --- 3. Gán Role "Admin" cho User đó ---
                    // Chỉ gán role nếu user được tạo thành công
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
                // (Bạn có thể thêm log lỗi nếu result.Succeeded == false)
            }
            else
            {
                // (Tùy chọn) Nếu user admin đã tồn tại, kiểm tra xem họ đã ở trong role Admin chưa
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    // Nếu chưa, gán role
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // (Tùy chọn) Bạn có thể tạo một user mẫu (khách hàng)
            string customerEmail = "customer@app.com";
            string customerPassword = "Password123!";

            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Khách hàng mẫu",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(customerUser, customerPassword);
                if (result.Succeeded)
                {
                    // Gán role "User"
                    await userManager.AddToRoleAsync(customerUser, "User");
                }
            }
        }
    }
}
