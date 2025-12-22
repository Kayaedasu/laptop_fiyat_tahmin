# Katman 5: Data Access Layer (Veri Erişim Katmanı)

## 📋 Görev
Veritabanı işlemlerini yönetir (CRUD).

## 🛠️ Teknolojiler
- Entity Framework Core
- MySQL (Pomelo.EntityFrameworkCore.MySql)
- Repository Pattern
- Unit of Work Pattern

## 📁 İçerik

### DbContext
**ApplicationDbContext.cs** - Ana veritabanı bağlamı

### Entities (Models)
- User
- Product
- Category
- Order
- OrderDetail
- Review
- Cart

### Repositories
- IRepository<T> (Generic interface)
- ProductRepository
- OrderRepository
- UserRepository
- CategoryRepository
- ReviewRepository
- CartRepository

### Unit of Work
- IUnitOfWork
- UnitOfWork

## 🎯 Sorumluluklar
- CRUD işlemleri
- LINQ sorguları
- Transaction yönetimi
- Database migrations
- İlişkisel veri yönetimi

## ⚙️ Örnek Kullanım

```csharp
// DbContext
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Cart> Carts { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişkileri tanımlama
        modelBuilder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);
    }
}

// Repository Pattern
public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

// Product Repository
public class ProductRepository : IRepository<Product>
{
    private readonly ApplicationDbContext _context;
    
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .ToListAsync();
    }
}
```

## 📦 Gerekli NuGet Paketleri
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
```

## 🚀 Migration Komutları
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```
