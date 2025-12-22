# Katman 6: Database Layer (Veritabanı Katmanı)

## 📋 Görev
Veri saklama, yönetim ve veritabanı nesneleri.

## 🛠️ Teknoloji
- MySQL 8.0+

## 📁 İçerik

### Tablolar (En az 6)
1. **Users** - Kullanıcılar
2. **Products** - Laptop ürünleri
3. **Categories** - Ürün kategorileri
4. **Orders** - Siparişler
5. **OrderDetails** - Sipariş detayları
6. **Reviews** - Ürün yorumları
7. **Cart** - Sepet

### Views (En az 5)
1. `vw_ProductDetails` - Ürün detayları (kategori ile birlikte)
2. `vw_OrderSummary` - Sipariş özeti
3. `vw_CustomerReviews` - Müşteri yorumları
4. `vw_TopProducts` - En çok satan ürünler
5. `vw_MonthlyRevenue` - Aylık gelir raporu

### Stored Procedures (En az 2)
1. `sp_GetUserOrders` - Kullanıcının siparişlerini getir
2. `sp_UpdateProductStock` - Ürün stok güncelleme
3. `sp_CalculateOrderTotal` - Sipariş toplam hesaplama (opsiyonel)

### Functions (En az 2)
1. `fn_CalculateDiscount` - İndirim hesaplama
2. `fn_GetAverageRating` - Ortalama puan hesaplama

### Constraints (En az 5 farklı tipte)
1. **PRIMARY KEY** - Her tabloda
2. **FOREIGN KEY** - İlişkiler için
3. **UNIQUE** - Email, username vb.
4. **CHECK** - Fiyat > 0, Rating 1-5 arası
5. **NOT NULL** - Zorunlu alanlar
6. **DEFAULT** - Varsayılan değerler

### Indexes
- Performance için gerekli indexler
- Primary key'ler otomatik index
- Foreign key'ler için index
- Sık aranan alanlar için index

## 📊 Tablo Yapıları

### Users
```sql
- UserId (PK, INT, AUTO_INCREMENT)
- Email (UNIQUE, NOT NULL)
- Password (NOT NULL)
- Role (ENUM: 'Admin', 'Customer')
- CreatedAt (TIMESTAMP, DEFAULT CURRENT_TIMESTAMP)
```

### Products
```sql
- ProductId (PK, INT, AUTO_INCREMENT)
- Name (VARCHAR, NOT NULL)
- Brand (VARCHAR) - Dell, HP, Lenovo, Apple vb.
- Processor (VARCHAR) - i5, i7, Ryzen 5 vb.
- RAM (INT) - 8, 16, 32 GB
- Storage (INT) - 256, 512, 1024 GB
- GPU (VARCHAR) - Integrated, GTX, RTX vb.
- ScreenSize (DECIMAL) - 13.3, 15.6, 17.3
- Price (DECIMAL, CHECK Price > 0)
- Stock (INT, DEFAULT 0)
- CategoryId (FK → Categories)
- ImageUrl (VARCHAR)
- CreatedAt (TIMESTAMP)
```

### Categories
```sql
- CategoryId (PK, INT, AUTO_INCREMENT)
- Name (VARCHAR, NOT NULL) - Gaming, Business, Student vb.
- Description (TEXT)
```

### Orders
```sql
- OrderId (PK, INT, AUTO_INCREMENT)
- UserId (FK → Users)
- OrderDate (TIMESTAMP, DEFAULT CURRENT_TIMESTAMP)
- TotalAmount (DECIMAL)
- Status (ENUM: 'Pending', 'Completed', 'Cancelled')
```

### OrderDetails
```sql
- OrderDetailId (PK, INT, AUTO_INCREMENT)
- OrderId (FK → Orders)
- ProductId (FK → Products)
- Quantity (INT, CHECK Quantity > 0)
- UnitPrice (DECIMAL)
- Subtotal (DECIMAL)
```

### Reviews
```sql
- ReviewId (PK, INT, AUTO_INCREMENT)
- ProductId (FK → Products)
- UserId (FK → Users)
- Rating (INT, CHECK Rating BETWEEN 1 AND 5)
- Comment (TEXT)
- CreatedAt (TIMESTAMP)
```

### Cart
```sql
- CartId (PK, INT, AUTO_INCREMENT)
- UserId (FK → Users)
- ProductId (FK → Products)
- Quantity (INT, CHECK Quantity > 0)
- AddedAt (TIMESTAMP)
```

## 🚀 Kurulum
```bash
# MySQL'e bağlan
mysql -u root -p

# Veritabanı oluştur
CREATE DATABASE SmartShopDB;

# SQL scriptlerini çalıştır
mysql -u root -p SmartShopDB < schema.sql
```

## 📂 SQL Dosya Yapısı
```
MySQL-Scripts/
├── 01-schema.sql
├── 02-stored-procedures/
│   ├── sp_GetUserOrders.sql
│   └── sp_UpdateProductStock.sql
├── 03-views/
│   ├── vw_ProductDetails.sql
│   ├── vw_OrderSummary.sql
│   ├── vw_CustomerReviews.sql
│   ├── vw_TopProducts.sql
│   └── vw_MonthlyRevenue.sql
├── 04-functions/
│   ├── fn_CalculateDiscount.sql
│   └── fn_GetAverageRating.sql
├── 05-constraints.sql
├── 06-indexes.sql
└── 07-seed-data.sql
```
