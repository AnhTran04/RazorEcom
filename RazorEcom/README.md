# RazorEcom - E-Commerce Web Application

A full-featured e-commerce platform built with ASP.NET Core Razor Pages, featuring product management, shopping cart, order processing, and an admin dashboard.

## 🚀 Features

### Customer Features

- **Product Browsing**: Browse products by category with search and filtering
- **Product Details**: View detailed product information with images and variants (size, color)
- **Shopping Cart**: Add, update, and remove items from cart with real-time updates
- **Checkout**: Secure checkout process with address management
- **Order Tracking**: View order history and order details
- **User Profile**: Manage personal information and addresses
- **User Authentication**: Register, login, and secure authentication with ASP.NET Core Identity

### Admin Features

- **Dashboard**: Overview of sales, orders, and products
- **Product Management**:
  - CRUD operations for products
  - Manage product variants (SKU, size, color, price, quantity)
  - Excel import/export functionality for bulk operations
- **Order Management**: View and manage customer orders
- **User Management**: Manage users and assign roles
- **Modern Dark-Themed UI**: Professional admin interface with responsive sidebar

## 🛠️ Tech Stack

- **Framework**: ASP.NET Core 8.0 (Razor Pages)
- **Database**: SQL Server with Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity with role-based authorization
- **Frontend**:
  - Bootstrap 5.3.3
  - Bootstrap Icons
  - JavaScript (Vanilla JS)
  - SweetAlert2 for notifications
- **Excel Operations**: ClosedXML
- **Styling**: Custom CSS with dark theme support

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- A code editor (Visual Studio 2022, VS Code, or Rider)

## 🔧 Installation & Setup

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd RazorEcom
   ```

2. **Update Database Connection String**

   Open `appsettings.json` and update the connection string if needed:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=RazorEcomDB;Trusted_Connection=True;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Apply Database Migrations**

   ```bash
   dotnet ef database update
   ```

   This will create the database and seed initial data (Admin user and roles).

4. **Run the Application**

   ```bash
   dotnet run
   ```

   The application will be available at `https://localhost:5001` or `http://localhost:5000`

## 👤 Default Credentials

After running the application for the first time, use these credentials:

**Admin Account:**

- Email: `admin@app.com`
- Password: `Password123!`

**Customer Account:**

- Email: `customer@app.com`
- Password: `Password123!`

> ⚠️ **Important**: Change these default passwords in production!

## 📁 Project Structure

```
RazorEcom/
├── Areas/
│   ├── Admin/              # Admin area pages
│   │   ├── Pages/
│   │   │   ├── Dashboard/  # Admin dashboard
│   │   │   ├── Products/   # Product management
│   │   │   ├── Orders/     # Order management
│   │   │   └── Users/      # User management
│   │   └── _AdminLayout.cshtml
│   └── Identity/           # Identity scaffolded pages
├── Data/
│   ├── ApplicationDbContext.cs
│   └── DbSeeder.cs         # Database seeding
├── Models/                 # Data models
│   ├── ApplicationUser.cs
│   ├── Products.cs
│   ├── ProductVariants.cs
│   ├── Category.cs
│   ├── Orders.cs
│   └── ...
├── Pages/                  # Customer-facing pages
│   ├── Account/            # Profile, orders, address book
│   ├── Cart/               # Shopping cart
│   ├── Checkout/           # Checkout process
│   ├── Products/           # Product listings and details
│   └── Index.cshtml        # Home page
├── Services/               # Business logic services
├── wwwroot/                # Static files
│   ├── css/
│   │   ├── site.css
│   │   └── admin.css       # Admin dark theme
│   ├── js/
│   │   ├── site.js         # Global notifications
│   │   └── product.js
│   └── lib/                # External libraries
└── Program.cs              # Application configuration
```

## 🔐 Authorization & Roles

The application uses ASP.NET Core Identity with role-based authorization:

- **Admin Role**: Full access to admin area (dashboard, product management, order management, user management)
- **User Role**: Standard customer access (browse, shop, view orders, manage profile)

All admin pages are protected with `[Authorize(Roles = "Admin")]` attribute.

## 💡 Key Features Implementation

### Global Notification System

- SweetAlert2-based notification system
- Consistent feedback across all pages
- Toast-style notifications (top-right corner)

### Admin Interface

- Modern dark-themed design
- Responsive sidebar with toggle functionality
- Professional table styling
- Excel import/export for product variants

### Product Variants

- Each product can have multiple variants (SKU, size, color, price, quantity)
- Inventory tracking per variant
- Low stock warnings

### Shopping Cart

- Session-based cart management
- Real-time quantity updates
- Stock validation at checkout

## 🔨 Development

### Database Migrations

Create a new migration:

```bash
dotnet ef migrations add MigrationName
```

Update database:

```bash
dotnet ef database update
```

### Build the Project

```bash
dotnet build
```

### Run in Development Mode

```bash
dotnet run --environment Development
```

## 📝 Configuration

### Password Requirements

Password requirements can be configured in `Program.cs`:

```csharp
options.Password.RequireDigit = true;
options.Password.RequireLowercase = true;
options.Password.RequireUppercase = true;
options.Password.RequiredLength = 8;
options.Password.RequireNonAlphanumeric = false;
```

### Session Configuration

Session timeout and cookie settings in `Program.cs`:

```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

## 🎨 Styling

- **User-facing pages**: Bootstrap 5 with custom CSS
- **Admin pages**: Custom dark theme (`admin.css`)
- **Responsive design**: Mobile-friendly layout
- **Icons**: Bootstrap Icons

## 🚧 Troubleshooting

### Database Connection Issues

- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Check if database exists: `dotnet ef database update`

### Build Errors

- Clean and rebuild: `dotnet clean && dotnet build`
- Restore packages: `dotnet restore`

### Admin Access Issues

- Ensure DbSeeder ran successfully (check console logs on first run)
- Verify admin user exists in database
- Check role assignment

## 📄 License

This project is licensed under the MIT License.

## 👨‍💻 Author

Developed as an e-commerce learning project showcasing ASP.NET Core Razor Pages, Entity Framework Core, and modern web development practices.

---

For questions or issues, please create an issue in the repository.
