# 🏗️ SmartShop - Mimari Dokümantasyonu

## 6 Katmanlı SOA Mimarisi

### Katman 1: Presentation Layer (Sunum Katmanı)
**Klasör:** `1-Presentation-Layer/SmartShop.Web/`

**Görev:** Kullanıcı arayüzü ve etkileşim

**Teknolojiler:**
- ASP.NET Core MVC (Razor Views)
- React Components
- HTML5, CSS3, JavaScript
- Bootstrap 5

**İçerik:**
- Views (Razor .cshtml)
- Layouts
- PartialViews
- ViewComponents
- Static files (CSS, JS, Images)

---

### Katman 2: Business Layer (İş Mantığı Katmanı)
**Klasör:** `2-Business-Layer/SmartShop.Business/`

**Görev:** İş kuralları ve validasyon

**Teknolojiler:**
- ASP.NET Core MVC Controllers
- C#

**İçerik:**
- Controllers (5+)
  - ProductController
  - OrderController
  - UserController
  - CartController
  - CategoryController
  - AdminController
- Business Logic Classes
- Validation Rules
- Models/DTOs

---

### Katman 3: Service Layer (Servis Katmanı)
**Klasör:** `3-Service-Layer/`

**Görev:** Mikroservisler ve servis sağlama

**Servisler:**

#### 3.1 Product Service (SOAP)
- **Port:** 3001
- **Protokol:** SOAP/XML
- **Görev:** Ürün yönetimi (CRUD)

#### 3.2 Order Service (gRPC)
- **Port:** 3002
- **Protokol:** gRPC/Protocol Buffers
- **Görev:** Sipariş yönetimi

#### 3.3 User Service (REST)
- **Port:** 3003
- **Protokol:** REST/JSON
- **Görev:** Kullanıcı yönetimi

---

### Katman 4: Integration Layer (Entegrasyon Katmanı)
**Klasör:** `4-Integration-Layer/SmartShop.Integration/`

**Görev:** Servisler arası iletişim ve dış API entegrasyonu

**İçerik:**
- SOAP Client
- gRPC Client
- REST Client
- External API Clients (Ödeme, Kargo vb.)
- ML Service Client

---

### Katman 5: Data Access Layer (Veri Erişim Katmanı)
**Klasör:** `5-Data-Access-Layer/SmartShop.DataAccess/`

**Görev:** Veritabanı işlemleri

**Teknolojiler:**
- Entity Framework Core
- Repository Pattern
- Unit of Work Pattern

**İçerik:**
- DbContext
- Repositories
- Migrations
- LINQ Queries

---

### Katman 6: Database Layer (Veritabanı Katmanı)
**Klasör:** `6-Database-Layer/MySQL-Scripts/`

**Görev:** Veri saklama ve yönetim

**Teknoloji:** MySQL 8.0+

**İçerik:**
- Tables (7+ tablo)
- Views (5+)
- Stored Procedures (2+)
- Functions (2+)
- Constraints (5+)
- Indexes

---

## Ek Servis: ML Service
**Klasör:** `ML-Service/`

**Görev:** Makine öğrenmesi tahminleri

**Teknolojiler:**
- Python Flask/FastAPI
- Scikit-learn
- Pandas, NumPy

**Özellikler:**
1. Fiyat Tahmini
2. Ürün Öneri Sistemi
3. Akıllı Arama

---

## İletişim Akışı

```
Kullanıcı İsteği
      ↓
[1. Presentation] → Razor View + React
      ↓
[2. Business] → Controller + Validation
      ↓
[3. Service] → SOAP/gRPC/REST Service
      ↓
[4. Integration] → Service Clients
      ↓
[5. Data Access] → EF Core + Repository
      ↓
[6. Database] → MySQL
```

---

## Port Yapısı

| Servis | Port | Protokol |
|--------|------|----------|
| ASP.NET MVC | 5000/5001 | HTTP/HTTPS |
| Product Service | 3001 | SOAP |
| Order Service | 3002 | gRPC |
| User Service | 3003 | REST |
| ML Service | 5050 | REST |
| MySQL | 3306 | MySQL |

---

**Son Güncelleme:** Aralık 2025
