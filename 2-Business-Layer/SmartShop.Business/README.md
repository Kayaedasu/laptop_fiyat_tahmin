# 🏢 SmartShop Business Layer

## 📋 Genel Bakış

SmartShop projesinin **Business Layer** (İş Mantığı Katmanı) katmanıdır. Bu katman, uygulamanın iş kurallarını, doğrulama mantığını ve iş süreçlerini içerir.

## 🎯 Sorumluluklar

- ✅ İş kurallarının uygulanması
- ✅ Veri doğrulama (Validation)
- ✅ Data Transfer Objects (DTOs)
- ✅ Servis katmanı (Service Layer)
- ✅ İş mantığı koordinasyonu
- ✅ Transaction yönetimi

## 📁 Proje Yapısı

```
SmartShop.Business/
├── Common/
│   └── ServiceResult.cs              # Standart API yanıt yapısı
├── DTOs/
│   ├── ProductDto.cs                 # Ürün DTO'ları
│   ├── OrderDto.cs                   # Sipariş DTO'ları
│   ├── UserDto.cs                    # Kullanıcı DTO'ları
│   ├── CartDto.cs                    # Sepet DTO'ları
│   ├── CategoryDto.cs                # Kategori DTO'ları
│   └── ReviewDto.cs                  # Yorum DTO'ları
├── Services/
│   ├── IProductService.cs            # Ürün servisi interface
│   ├── ProductService.cs             # Ürün servisi implementasyonu
│   ├── IOrderService.cs              # Sipariş servisi interface
│   ├── OrderService.cs               # Sipariş servisi implementasyonu
│   ├── IUserService.cs               # Kullanıcı servisi interface
│   ├── UserService.cs                # Kullanıcı servisi implementasyonu
│   ├── ICartService.cs               # Sepet servisi interface
│   └── ICategoryService.cs           # Kategori servisi interface
└── Validators/
    ├── ProductValidator.cs           # Ürün doğrulama kuralları
    ├── OrderValidator.cs             # Sipariş doğrulama kuralları
    └── UserValidator.cs              # Kullanıcı doğrulama kuralları
```

## 🔧 Bağımlılıklar

### NuGet Paketleri
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.0.3" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="9.0.0" />
```

### Proje Referansları
- `SmartShop.DataAccess` - Data Access Layer

## 💡 Kullanım Örnekleri

### Servis Kullanımı
```csharp
var result = await _productService.GetByIdAsync(id);
if (result.Success)
{
    return Ok(result.Data);
}
return BadRequest(result.Message);
```

## 📝 Notlar

- ✅ **Build Durumu:** Başarılı ✅
- ✅ **ProductService:** Tamamlandı (11 metod)
- ✅ **OrderService:** Tamamlandı (9 metod)
- ✅ **UserService:** Tamamlandı (11 metod - BCrypt ile şifre güvenliği)
- ✅ **CartService:** Tamamlandı (7 metod - Stok kontrolü, miktar güncelleme)
- ✅ **CategoryService:** Tamamlandı (6 metod - Ürün sayısı kontrolü)
- ✅ **Toplam:** 5 servis, 44 metod, 22 DTO, 3 validator

---

**Oluşturulma Tarihi:** Aralık 2025  
**Versiyon:** 1.0.0
