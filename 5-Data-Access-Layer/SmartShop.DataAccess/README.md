# 🗄️ SmartShop Data Access Layer

## ✅ Tamamlandı!

### 📦 Oluşturulan Yapı

```
SmartShop.DataAccess/
├── Models/                          ✅ Entity Models (7 adet)
│   ├── User.cs
│   ├── Category.cs
│   ├── Product.cs
│   ├── Order.cs
│   ├── OrderDetail.cs
│   ├── Review.cs
│   └── Cart.cs
│
├── Data/                            ✅ DbContext
│   └── ApplicationDbContext.cs
│
├── Repositories/                    ✅ Repository Pattern
│   ├── IRepository.cs              (Generic Interface)
│   ├── Repository.cs               (Generic Implementation)
│   ├── IProductRepository.cs
│   ├── ProductRepository.cs
│   ├── IOrderRepository.cs
│   └── OrderRepository.cs
│
└── UnitOfWork/                      ✅ Unit of Work Pattern
    ├── IUnitOfWork.cs
    └── UnitOfWork.cs
```

---

## 🎯 Özellikler

### ✅ Entity Models
- **Data Annotations** ile validation
- **Foreign Key** ilişkileri
- **Navigation Properties**
- 7 Model: User, Category, Product, Order, OrderDetail, Review, Cart

### ✅ DbContext
- **MySQL** bağlantısı (Pomelo.EntityFrameworkCore.MySql)
- **Fluent API** konfigürasyonları
- **Index** tanımları
- **Cascade/Restrict** delete behaviors

### ✅ Repository Pattern
- Generic `IRepository<T>` interface
- CRUD operasyonları
- Async methods
- LINQ desteği
- Özel repository'ler (Product, Order)

### ✅ Unit of Work Pattern
- Transaction yönetimi
- SaveChanges merkezi
- Multiple repository koordinasyonu
- Dispose pattern

---

## 🔧 Kullanılan Teknolojiler

- **.NET 9.0**
- **Entity Framework Core 9.0**
- **Pomelo.EntityFrameworkCore.MySql 9.0**
- **Repository Pattern**
- **Unit of Work Pattern**

---

## 📊 NuGet Paketleri

```xml
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="9.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.1" />
```

---

## 🚀 Kullanım

### 1. Connection String Ayarlama

**appsettings.json** (Web projesinde):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SmartShopDB;User=root;Password=your_password;Port=3306;"
  }
}
```

### 2. Dependency Injection Ayarlama

**Program.cs** (ASP.NET MVC):
```csharp
using Microsoft.EntityFrameworkCore;
using SmartShop.DataAccess.Data;
using SmartShop.DataAccess.UnitOfWork;

var builder = WebApplication.CreateBuilder(args);

// DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Unit of Work
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

var app = builder.Build();
```

### 3. Controller'da Kullanım

```csharp
using SmartShop.DataAccess.UnitOfWork;

public class ProductController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _unitOfWork.Products.GetProductsWithCategoryAsync();
        return View(products);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product product)
    {
        if (ModelState.IsValid)
        {
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        return View(product);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _unitOfWork.Products.GetProductWithDetailsAsync(id);
        if (product == null)
            return NotFound();
        
        return View(product);
    }
}
```

### 4. Transaction Örneği

```csharp
public async Task<IActionResult> CreateOrder(OrderViewModel model)
{
    try
    {
        await _unitOfWork.BeginTransactionAsync();

        // Sipariş oluştur
        var order = new Order { /* ... */ };
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Sipariş detaylarını ekle
        foreach (var item in model.Items)
        {
            var orderDetail = new OrderDetail { /* ... */ };
            await _unitOfWork.OrderDetails.AddAsync(orderDetail);

            // Stok güncelle
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            product.Stock -= item.Quantity;
            _unitOfWork.Products.Update(product);
        }

        // Sepeti temizle
        var cartItems = await _unitOfWork.Cart.FindAsync(c => c.UserId == model.UserId);
        _unitOfWork.Cart.RemoveRange(cartItems);

        await _unitOfWork.CommitAsync();
        return RedirectToAction("OrderSuccess");
    }
    catch
    {
        await _unitOfWork.RollbackAsync();
        return View("Error");
    }
}
```

---

## 📝 Migration Komutları

### Migration Oluşturma
```powershell
# Data Access Layer dizininde
cd "5-Data-Access-Layer/SmartShop.DataAccess/SmartShop.DataAccess"

# Migration oluştur
dotnet ef migrations add InitialCreate

# Veritabanını güncelle
dotnet ef database update
```

### Connection String ile Migration
```powershell
dotnet ef migrations add InitialCreate --connection "Server=localhost;Database=SmartShopDB;User=root;Password=your_password;"
```

---

## 🔍 Repository Methodları

### Generic Repository (IRepository<T>)
- `GetByIdAsync(int id)`
- `GetAllAsync()`
- `FindAsync(Expression<Func<T, bool>> predicate)`
- `FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)`
- `AddAsync(T entity)`
- `AddRangeAsync(IEnumerable<T> entities)`
- `Update(T entity)`
- `Remove(T entity)`
- `CountAsync()`
- `AnyAsync(Expression<Func<T, bool>> predicate)`

### Product Repository (Ek Methodlar)
- `GetProductsWithCategoryAsync()`
- `GetProductsByCategoryAsync(int categoryId)`
- `GetProductsByBrandAsync(string brand)`
- `SearchProductsAsync(string searchTerm)`
- `GetProductWithDetailsAsync(int productId)`

### Order Repository (Ek Methodlar)
- `GetUserOrdersAsync(int userId)`
- `GetOrderWithDetailsAsync(int orderId)`
- `GetOrdersByStatusAsync(string status)`

---

## ✅ Checklist

- [x] Entity Models oluşturuldu (7 adet)
- [x] Data Annotations eklendi
- [x] Navigation Properties tanımlandı
- [x] ApplicationDbContext oluşturuldu
- [x] Fluent API konfigürasyonları
- [x] Generic Repository Pattern
- [x] Özel Repository'ler (Product, Order)
- [x] Unit of Work Pattern
- [x] Transaction yönetimi
- [x] NuGet paketleri eklendi
- [x] Proje başarıyla build edildi ✅
- [ ] Migration oluşturulacak (veritabanı kurulduktan sonra)
- [ ] Web projesine entegre edilecek

---

## 🎯 Sıradaki Adım

Data Access Layer tamamlandı! Şimdi:
1. **MySQL veritabanını kur** (SQL scriptlerini çalıştır)
2. **Migration oluştur ve çalıştır**
3. **Business Layer** (Controllers) geliştir

---

**Oluşturulma Tarihi:** Aralık 2025  
**Build Status:** ✅ Başarılı
