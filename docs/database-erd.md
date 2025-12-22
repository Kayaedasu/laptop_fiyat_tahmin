# 📊 SmartShop Veritabanı - Entity Relationship Diagram (ERD)

## 🗂️ Tablo İlişkileri

```
┌──────────────┐
│    Users     │
│──────────────│
│ UserId (PK)  │───┐
│ Email (UQ)   │   │
│ Password     │   │
│ FirstName    │   │
│ LastName     │   │
│ Phone        │   │
│ Role         │   │
│ IsActive     │   │
└──────────────┘   │
                   │
        ┌──────────┴───────────┬──────────────┐
        │                      │              │
        ↓                      ↓              ↓
┌──────────────┐      ┌──────────────┐  ┌──────────────┐
│    Orders    │      │    Cart      │  │   Reviews    │
│──────────────│      │──────────────│  │──────────────│
│ OrderId (PK) │      │ CartId (PK)  │  │ ReviewId(PK) │
│ UserId (FK)  │      │ UserId (FK)  │  │ ProductId(FK)│
│ OrderDate    │      │ ProductId(FK)│  │ UserId (FK)  │
│ TotalAmount  │      │ Quantity     │  │ Rating       │
│ Status       │      │ AddedAt      │  │ Comment      │
└──────────────┘      └──────────────┘  └──────────────┘
        │                      │              │
        │                      │              │
        ↓                      └──────┬───────┘
┌──────────────┐                     │
│ OrderDetails │                     │
│──────────────│                     │
│ DetailId(PK) │                     │
│ OrderId (FK) │                     │
│ ProductId(FK)│◄────────────────────┘
│ Quantity     │                     │
│ UnitPrice    │                     │
└──────────────┘                     │
                                     │
                            ┌────────┴──────────┐
                            │                   │
                            ↓                   │
                    ┌──────────────┐            │
                    │   Products   │            │
                    │──────────────│            │
                    │ ProductId(PK)│◄───────────┘
                    │ Name         │
                    │ Brand        │
                    │ Processor    │
                    │ RAM          │
                    │ Storage      │
                    │ GPU          │
                    │ Price        │
                    │ Stock        │
                    │ CategoryId(FK)
                    └──────────────┘
                            │
                            ↓
                    ┌──────────────┐
                    │  Categories  │
                    │──────────────│
                    │ CategoryId(PK)
                    │ Name (UQ)    │
                    │ Description  │
                    └──────────────┘
```

---

## 📋 Tablo Detayları

### 1. Users (Kullanıcılar)
- **Primary Key:** UserId
- **Unique:** Email
- **İlişkiler:** 
  - 1 User → N Orders (One-to-Many)
  - 1 User → N Cart (One-to-Many)
  - 1 User → N Reviews (One-to-Many)

### 2. Categories (Kategoriler)
- **Primary Key:** CategoryId
- **Unique:** Name
- **İlişkiler:**
  - 1 Category → N Products (One-to-Many)

### 3. Products (Ürünler)
- **Primary Key:** ProductId
- **Foreign Keys:** CategoryId
- **İlişkiler:**
  - N Products → 1 Category (Many-to-One)
  - 1 Product → N OrderDetails (One-to-Many)
  - 1 Product → N Cart (One-to-Many)
  - 1 Product → N Reviews (One-to-Many)

### 4. Orders (Siparişler)
- **Primary Key:** OrderId
- **Foreign Keys:** UserId
- **İlişkiler:**
  - N Orders → 1 User (Many-to-One)
  - 1 Order → N OrderDetails (One-to-Many)

### 5. OrderDetails (Sipariş Detayları)
- **Primary Key:** OrderDetailId
- **Foreign Keys:** OrderId, ProductId
- **Unique:** (OrderId, ProductId)
- **İlişkiler:**
  - N OrderDetails → 1 Order (Many-to-One)
  - N OrderDetails → 1 Product (Many-to-One)

### 6. Reviews (Yorumlar)
- **Primary Key:** ReviewId
- **Foreign Keys:** ProductId, UserId
- **Unique:** (ProductId, UserId)
- **İlişkiler:**
  - N Reviews → 1 Product (Many-to-One)
  - N Reviews → 1 User (Many-to-One)

### 7. Cart (Sepet)
- **Primary Key:** CartId
- **Foreign Keys:** UserId, ProductId
- **Unique:** (UserId, ProductId)
- **İlişkiler:**
  - N Cart → 1 User (Many-to-One)
  - N Cart → 1 Product (Many-to-One)

---

## 🔐 Constraints Özeti

### Primary Keys (7)
- Users.UserId
- Categories.CategoryId
- Products.ProductId
- Orders.OrderId
- OrderDetails.OrderDetailId
- Reviews.ReviewId
- Cart.CartId

### Foreign Keys (8)
- Products.CategoryId → Categories.CategoryId
- Orders.UserId → Users.UserId
- OrderDetails.OrderId → Orders.OrderId
- OrderDetails.ProductId → Products.ProductId
- Reviews.ProductId → Products.ProductId
- Reviews.UserId → Users.UserId
- Cart.UserId → Users.UserId
- Cart.ProductId → Products.ProductId

### Unique Constraints (5)
- Users.Email
- Categories.Name
- OrderDetails(OrderId, ProductId)
- Reviews(ProductId, UserId)
- Cart(UserId, ProductId)

### Check Constraints (15+)
- Users.Email format kontrolü
- Users.Phone length kontrolü
- Products.Price > 0
- Products.Stock >= 0
- Products.Discount 0-100 arası
- Products.RAM > 0
- Products.Storage > 0
- Products.ScreenSize 10-20 arası
- Orders.TotalAmount > 0
- Orders.DiscountAmount >= 0
- Orders.FinalAmount > 0
- OrderDetails.Quantity > 0
- OrderDetails.UnitPrice > 0
- Reviews.Rating 1-5 arası
- Cart.Quantity 1-10 arası

### Default Values (10+)
- Users.Role = 'Customer'
- Users.IsActive = TRUE
- Products.Stock = 0
- Products.Discount = 0
- Products.Condition = 'New'
- Products.ViewCount = 0
- Orders.Status = 'Pending'
- Reviews.HelpfulCount = 0
- Timestamps (CURRENT_TIMESTAMP)

---

## 📈 Indexes (Performance)

### Primary Key Indexes (Otomatik)
- Tüm PK'lar otomatik index

### Foreign Key Indexes
- idx_products_category
- idx_orders_user
- idx_orderdetails_order
- idx_orderdetails_product
- idx_reviews_product
- idx_reviews_user
- idx_cart_user
- idx_cart_product

### Search Indexes
- idx_users_email
- idx_users_role
- idx_products_brand
- idx_products_price
- idx_products_name
- idx_products_active
- idx_orders_status
- idx_orders_date
- idx_reviews_rating

**Toplam:** 15+ Index

---

## 🎯 Normalizasyon

### 1NF (First Normal Form) ✅
- Her sütun atomic değer içeriyor
- Tekrarlayan gruplar yok

### 2NF (Second Normal Form) ✅
- 1NF'e uygun
- Partial dependency yok
- Her non-key attribute tamamen PK'ya bağımlı

### 3NF (Third Normal Form) ✅
- 2NF'e uygun
- Transitive dependency yok
- Non-key attributes sadece PK'ya bağımlı

**Örnek Normalizasyon:**
- OrderDetails tablosu: Order ve Product'ı ayırır
- Categories tablosu: Product'tan ayrılır
- Reviews tablosu: User ve Product'ı birleştirir

---

**Oluşturulma Tarihi:** Aralık 2025
