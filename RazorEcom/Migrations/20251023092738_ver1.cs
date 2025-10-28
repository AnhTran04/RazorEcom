using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RazorEcom.Migrations
{
    /// <inheritdoc />
    public partial class ver1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentId",
                        column: x => x.ParentId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddressBooks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AddressLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Ward = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    District = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressBooks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AddressBooks_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Carts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Brand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingAddressId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AddressBooks_ShippingAddressId",
                        column: x => x.ShippingAddressId,
                        principalTable: "AddressBooks",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                       
                });

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Sku = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CartId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CartItems_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    VariantId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_ProductVariants_VariantId",
                        column: x => x.VariantId,
                        principalTable: "ProductVariants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Name", "ParentId", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(530), "Các loại áo thun nam nữ", "Áo Thun", null, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(532) },
                    { 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(535), "Các loại quần jeans thời trang", "Quần Jeans", null, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(535) }
                });

            migrationBuilder.InsertData(
                 table: "Products",
                 columns: new[] { "Id", "Brand", "CategoryId", "CreatedAt", "Description", "ImageUrl", "Name", "Status", "UpdatedAt" },
                 values: new object[,]
                 {
                    { 1, "Coolmate", 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(655), "Áo thun cotton 100%, thoáng mát.", "https://source.unsplash.com/300x300/?basic-t-shirt", "Áo Thun Cổ Tròn Basic", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(655) },
                    { 2, "Uniqlo", 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(660), "Áo polo lịch lãm, phù hợp công sở.", "https://source.unsplash.com/300x300/?polo-shirt", "Áo Thun Polo", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(660) },
                    { 3, "H&M", 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(661), "Áo thun in hình độc đáo, cá tính.", "https://source.unsplash.com/300x300/?graphic-t-shirt", "Áo Thun In Hình", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(662) },
                    { 4, "Nike", 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(663), "Áo ba lỗ thể thao, năng động.", "https://source.unsplash.com/300x300/?tank-top", "Áo Thun Ba Lỗ", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(663) },
                    { 5, "Zara", 1, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(664), "Áo thun dài tay giữ ấm nhẹ.", "https://source.unsplash.com/300x300/?long-sleeve-shirt", "Áo Thun Dài Tay", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(664) },
                    { 6, "Levi's", 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(666), "Quần jeans ống côn, tôn dáng.", "https://source.unsplash.com/300x300/?slim-fit-jeans", "Quần Jeans Slim-fit", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(666) },
                    { 7, "Topman", 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(667), "Quần jeans rách gối phong cách.", "https://source.unsplash.com/300x300/?ripped-jeans", "Quần Jeans Rách Gối", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(667) },
                    { 8, "Pull&Bear", 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(668), "Quần jeans ống rộng thoải mái.", "https://source.unsplash.com/300x300/?wide-leg-jeans", "Quần Jeans Ống Rộng", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(669) },
                    { 9, "Calvin Klein", 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(670), "Quần jeans đen cơ bản, dễ phối đồ.", "https://source.unsplash.com/300x300/?black-jeans", "Quần Jeans Đen Trơn", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(670) },
                    { 10, "ASOS", 2, new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(671), "Quần short jeans năng động cho mùa hè.", "https://source.unsplash.com/300x300/?jean-shorts", "Quần Short Jeans", "active", new DateTime(2025, 10, 23, 9, 27, 37, 576, DateTimeKind.Utc).AddTicks(672) }
                 }
            );
            migrationBuilder.InsertData(
                    table: "ProductVariants",
                    columns: new[] { "Id", "ProductId", "Size", "Color", "Price", "Quantity", "Sku", "ImageUrl", "CreatedAt", "Material", "UpdatedAt" },
                    values: new object[,]
                    {
                        // Thời gian cố định
                        // Đã thêm "null" (cho Material) và giá trị DateTime (cho UpdatedAt)
                        { 1, 1, "M", "Trắng", 250000m, 100, "AOT-CL-M-TR", "https://source.unsplash.com/600x600?tshirt,white", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 2, 1, "L", "Đen", 250000m, 100, "AOT-CL-L-DE", "https://source.unsplash.com/600x600?tshirt,black", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 3, 2, "S", "Xanh Navy", 450000m, 80, "AOP-UQ-S-XA", "https://source.unsplash.com/600x600?polo,navy", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 4, 2, "M", "Xám", 450000m, 80, "AOP-UQ-M-XA", "https://source.unsplash.com/600x600?polo,grey", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 5, 3, "M", "Vàng", 320000m, 50, "AOI-HM-M-VA", "https://source.unsplash.com/600x600?graphic,tshirt,yellow", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 6, 4, "L", "Đen", 280000m, 70, "AOB-NK-L-DE", "https://source.unsplash.com/600x600?tanktop,black", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 7, 4, "XL", "Trắng", 280000m, 70, "AOB-NK-XL-TR", "https://source.unsplash.com/600x600?tanktop,white", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 8, 5, "M", "Rêu", 380000m, 60, "AOD-ZR-M-RE", "https://source.unsplash.com/600x600?longsleeve,shirt,olive", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 9, 5, "L", "Be", 380000m, 60, "AOD-ZR-L-BE", "https://source.unsplash.com/600x600?longsleeve,shirt,beige", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 10, 6, "30", "Xanh Đậm", 750000m, 100, "QJS-LV-30-XD", "https://source.unsplash.com/600x600?slim,jeans,darkblue", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 11, 6, "32", "Xanh Đậm", 750000m, 100, "QJS-LV-32-XD", "https://source.unsplash.com/600x600?slim,jeans,darkblue", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 12, 7, "31", "Xanh Nhạt", 800000m, 50, "QJR-TP-31-XN", "https://source.unsplash.com/600x600?ripped,jeans,lightblue", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 13, 7, "32", "Đen", 820000m, 40, "QJR-TP-32-DE", "https://source.unsplash.com/600x600?ripped,jeans,black", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 14, 8, "M", "Trắng", 650000m, 60, "QJO-PB-M-TR", "https://source.unsplash.com/600x600?wide,jeans,white", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 15, 8, "L", "Be", 650000m, 60, "QJO-PB-L-BE", "https://source.unsplash.com/600x600?wide,jeans,beige", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 16, 9, "30", "Đen", 700000m, 90, "QJD-CK-30-DE", "https://source.unsplash.com/600x600?black,jeans,plain", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 17, 9, "32", "Đen", 700000m, 90, "QJD-CK-32-DE", "httpss://source.unsplash.com/600x600?black,jeans", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 18, 9, "34", "Đen", 700000m, 90, "QJD-CK-34-DE", "https://source.unsplash.com/600x600?plain,black,jeans", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 19, 10, "M", "Xanh Nhạt", 380000m, 100, "AQS-AS-M-XN", "https://source.unsplash.com/600x600?denim,shorts,light", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) },
                        { 20, 10, "L", "Xanh Đậm", 380000m, 100, "AQS-AS-L-XD", "https://source.unsplash.com/600x600?denim,shorts,dark", new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc), null, new DateTime(2025, 10, 25, 10, 0, 0, DateTimeKind.Utc) }
                    }
            );
            migrationBuilder.CreateIndex(
                name: "IX_AddressBooks_UserId",
                table: "AddressBooks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_VariantId",
                table: "CartItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentId",
                table: "Categories",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_VariantId",
                table: "OrderItems",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ShippingAddressId",
                table: "Orders",
                column: "ShippingAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_OrderId",
                table: "Payments",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId",
                table: "ProductVariants",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "AddressBooks");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
