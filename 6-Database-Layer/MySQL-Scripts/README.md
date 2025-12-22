# 🗄️ SmartShop Veritabanı Kurulum Rehberi

## ✅ Tamamlanan SQL Scriptler

### 1️⃣ Schema (01-schema.sql)
- ✅ 7 Tablo (Users, Categories, Products, Orders, OrderDetails, Reviews, Cart)
- ✅ 15+ Foreign Key
- ✅ 20+ Constraint (PRIMARY KEY, FOREIGN KEY, UNIQUE, CHECK, NOT NULL, DEFAULT)
- ✅ 15+ Index (Performance optimization)

### 2️⃣ Stored Procedures (02-stored-procedures/)
- ✅ `sp_GetUserOrders` - Kullanıcı siparişlerini getir
- ✅ `sp_UpdateProductStock` - Stok güncelleme
- ✅ `sp_CreateOrder` - Sepetten sipariş oluştur

### 3️⃣ Views (03-views/)
- ✅ `vw_ProductDetails` - Ürün detayları + kategori + ortalama puan
- ✅ `vw_OrderSummary` - Sipariş özeti + kullanıcı bilgileri
- ✅ `vw_CustomerReviews` - Müşteri yorumları + ürün/kullanıcı
- ✅ `vw_TopProducts` - En popüler ürünler (satış + görüntülenme + puan)
- ✅ `vw_MonthlyRevenue` - Aylık gelir raporları
- ✅ `vw_LowStockProducts` - Stok azalan ürünler (Bonus)

### 4️⃣ Functions (04-functions/)
- ✅ `fn_CalculateDiscount` - İndirimli fiyat hesaplama
- ✅ `fn_GetAverageRating` - Ortalama puan hesaplama
- ✅ `fn_GetTotalCartValue` - Sepet toplam tutarı (Bonus)

### 5️⃣ Seed Data (07-seed-data.sql)
- ✅ 6 Kullanıcı (1 Admin, 5 Customer)
- ✅ 5 Kategori
- ✅ 17 Laptop ürünü
- ✅ 10 Yorum
- ✅ 5 Sipariş
- ✅ 5 Sepet kaydı

---

## 🚀 Kurulum Adımları

### Adım 1: MySQL Kurulumu
```bash
# MySQL 8.0+ yüklü olmalı
mysql --version
```

### Adım 2: MySQL'e Bağlan
```bash
# Windows PowerShell
mysql -u root -p
```

### Adım 3: Veritabanını Oluştur
```bash
# MySQL içinde
cd "c:\Users\durgu\Desktop\PROJEDENEME\6-Database-Layer\MySQL-Scripts"

# 1. Schema oluştur
source 01-schema.sql

# 2. Stored Procedures
source 02-stored-procedures/sp_GetUserOrders.sql
source 02-stored-procedures/sp_UpdateProductStock.sql
source 02-stored-procedures/sp_CreateOrder.sql

# 3. Views
source 03-views/vw_ProductDetails.sql
source 03-views/vw_OrderSummary.sql
source 03-views/vw_CustomerReviews.sql
source 03-views/vw_TopProducts.sql
source 03-views/vw_MonthlyRevenue.sql
source 03-views/vw_LowStockProducts.sql

# 4. Functions
source 04-functions/fn_CalculateDiscount.sql
source 04-functions/fn_GetAverageRating.sql
source 04-functions/fn_GetTotalCartValue.sql

# 5. Test verileri
source 07-seed-data.sql
```

### Alternatif Kurulum (Tek komutla)
```bash
# PowerShell'de
Get-Content "01-schema.sql", `
  "02-stored-procedures/*.sql", `
  "03-views/*.sql", `
  "04-functions/*.sql", `
  "07-seed-data.sql" | mysql -u root -p
```

---

## 🧪 Test Sorguları

### Tabloları Kontrol Et
```sql
USE SmartShopDB;

SHOW TABLES;
-- Çıktı: 7 tablo görmeli

DESCRIBE Products;
-- Products tablosu yapısını gösterir
```

### Views Test
```sql
-- En iyi ürünler
SELECT * FROM vw_TopProducts LIMIT 10;

-- Aylık gelir
SELECT * FROM vw_MonthlyRevenue;

-- Müşteri yorumları
SELECT * FROM vw_CustomerReviews WHERE Rating >= 4;

-- Düşük stoklu ürünler
SELECT * FROM vw_LowStockProducts;
```

### Stored Procedures Test
```sql
-- Kullanıcının siparişlerini getir
CALL sp_GetUserOrders(2);

-- Stok güncelle
CALL sp_UpdateProductStock(1, 5, 'SUBTRACT', @success, @message);
SELECT @success, @message;

-- Sipariş oluştur
CALL sp_CreateOrder(2, 'Test Adres', 'İstanbul', '34000', 'CreditCard', @orderId, @success, @message);
SELECT @orderId, @success, @message;
```

### Functions Test
```sql
-- İndirimli fiyat hesapla
SELECT 
    Name, 
    Price, 
    Discount,
    fn_CalculateDiscount(Price, Discount) AS DiscountedPrice
FROM Products
LIMIT 10;

-- Ortalama puan
SELECT 
    Name,
    fn_GetAverageRating(ProductId) AS Rating
FROM Products
WHERE ProductId IN (1, 2, 3);

-- Sepet toplamı
SELECT 
    u.FirstName,
    fn_GetTotalCartValue(u.UserId) AS CartTotal
FROM Users u
WHERE u.Role = 'Customer';
```

### CRUD İşlemleri Test
```sql
-- CREATE
INSERT INTO Products (Name, Brand, Model, Processor, RAM, Storage, StorageType, GPU, ScreenSize, Price, Stock, CategoryId, Condition)
VALUES ('Test Laptop', 'Test Brand', 'Test Model', 'Test CPU', 8, 256, 'SSD', 'Test GPU', 15.6, 10000, 50, 3, 'New');

-- READ
SELECT * FROM vw_ProductDetails WHERE Brand = 'Test Brand';

-- UPDATE
UPDATE Products SET Price = 11000 WHERE Name = 'Test Laptop';

-- DELETE
DELETE FROM Products WHERE Name = 'Test Laptop';
```

---

## 📊 Veritabanı İstatistikleri

```sql
-- Toplam istatistikler
SELECT 'Kullanıcılar' AS Tablo, COUNT(*) AS Toplam FROM Users
UNION ALL
SELECT 'Kategoriler', COUNT(*) FROM Categories
UNION ALL
SELECT 'Ürünler', COUNT(*) FROM Products
UNION ALL
SELECT 'Siparişler', COUNT(*) FROM Orders
UNION ALL
SELECT 'Yorumlar', COUNT(*) FROM Reviews
UNION ALL
SELECT 'Sepet', COUNT(*) FROM Cart;
```

---

## 🔗 Bağlantı Bilgileri

### Connection String (.NET)
```csharp
"Server=localhost;Database=SmartShopDB;User=root;Password=your_password;Port=3306;"
```

### Connection String (Node.js)
```javascript
{
  host: 'localhost',
  user: 'root',
  password: 'your_password',
  database: 'SmartShopDB',
  port: 3306
}
```

---

## ✅ Checklist

- [x] Schema oluşturuldu (7 tablo)
- [x] Foreign Keys tanımlandı
- [x] Constraints eklendi (5+ farklı tip)
- [x] Indexler oluşturuldu
- [x] 3 Stored Procedure yazıldı
- [x] 6 View oluşturuldu
- [x] 3 Function yazıldı
- [x] Test verileri eklendi
- [ ] Connection test edildi
- [ ] CRUD işlemleri test edildi

---

## 🎯 Sıradaki Adım

Veritabanı hazır! Şimdi:
1. **Data Access Layer** (Entity Framework Core) oluşturulabilir
2. **Business Layer** (Controllers) geliştirilebilir

---

**Oluşturulma Tarihi:** Aralık 2025
