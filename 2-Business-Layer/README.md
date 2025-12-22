# Katman 2: Business Layer (İş Mantığı Katmanı)

## 📋 Görev
İş kurallarını, validasyonu ve iş mantığını yönetir.

## 🛠️ Teknolojiler
- ASP.NET Core MVC
- C#

## 📁 İçerik

### Controllers (En az 5)
1. **ProductController** - Ürün yönetimi
2. **OrderController** - Sipariş işlemleri
3. **UserController** - Kullanıcı işlemleri
4. **CartController** - Sepet yönetimi
5. **CategoryController** - Kategori yönetimi
6. **AdminController** - Admin paneli

### Her Controller'da En Az 3 Action
- Index (Listeleme)
- Details (Detay)
- Create (Ekleme)
- Edit (Düzenleme)
- Delete (Silme)

## 🎯 Sorumluluklar
- İş kurallarını uygulama
- Validasyon kontrolü
- Yetkilendirme
- Service Layer'a istekleri iletme
- ViewBag/ViewData/TempData ile veri aktarımı

## ⚙️ Kullanım
```csharp
// Örnek: ProductController
public class ProductController : Controller
{
    private readonly IProductService _productService;
    
    public IActionResult Index()
    {
        var products = _productService.GetAllProducts();
        return View(products);
    }
    
    [HttpPost]
    public IActionResult Create(Product product)
    {
        if (ModelState.IsValid)
        {
            _productService.AddProduct(product);
            TempData["Success"] = "Ürün eklendi!";
            return RedirectToAction("Index");
        }
        return View(product);
    }
}
```
