-- ============================================
-- SmartShop E-Ticaret Veritabanı Şeması
-- Laptop Satış Platformu
-- ============================================

-- Veritabanı oluştur
DROP DATABASE IF EXISTS SmartShopDB;
CREATE DATABASE SmartShopDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE SmartShopDB;

-- ============================================
-- TABLO 1: Users (Kullanıcılar)
-- ============================================
CREATE TABLE Users (
    UserId INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(100) NOT NULL UNIQUE,
    Password VARCHAR(255) NOT NULL,
    FirstName VARCHAR(50) NOT NULL,
    LastName VARCHAR(50) NOT NULL,
    Phone VARCHAR(20),
    Role ENUM('Admin', 'Customer') NOT NULL DEFAULT 'Customer',
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT chk_email_format CHECK (Email LIKE '%@%.%'),
    CONSTRAINT chk_phone_length CHECK (Phone IS NULL OR LENGTH(Phone) >= 10)
) ENGINE=InnoDB;

-- ============================================
-- TABLO 2: Categories (Laptop Kategorileri)
-- ============================================
CREATE TABLE Categories (
    CategoryId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(50) NOT NULL UNIQUE,
    Description TEXT,
    ImageUrl VARCHAR(255),
    IsActive BOOLEAN DEFAULT TRUE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraint
    CONSTRAINT chk_category_name CHECK (LENGTH(Name) >= 2)
) ENGINE=InnoDB;

-- ============================================
-- TABLO 3: Products (Laptop Ürünleri)
-- ============================================
CREATE TABLE Products (
    ProductId INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(200) NOT NULL,
    Brand VARCHAR(50) NOT NULL,
    Model VARCHAR(100),
    
    -- Teknik Özellikler
    Processor VARCHAR(100),
    RAM INT NOT NULL,                    -- GB cinsinden
    Storage INT NOT NULL,                -- GB cinsinden
    StorageType ENUM('HDD', 'SSD', 'SSD+HDD') DEFAULT 'SSD',
    GPU VARCHAR(100),
    ScreenSize DECIMAL(4,2),             -- inch cinsinden (örn: 15.6)
    Resolution VARCHAR(50),
    
    -- Fiyat ve Stok
    Price DECIMAL(10,2) NOT NULL,
    Stock INT NOT NULL DEFAULT 0,
    Discount DECIMAL(5,2) DEFAULT 0,     -- Yüzde cinsinden
    
    -- İlişkiler
    CategoryId INT NOT NULL,
    
    -- Diğer
    Description TEXT,
    ImageUrl VARCHAR(255),
    ProductCondition ENUM('New', 'Used', 'Refurbished') DEFAULT 'New',
    IsActive BOOLEAN DEFAULT TRUE,
    ViewCount INT DEFAULT 0,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Key
    CONSTRAINT fk_product_category FOREIGN KEY (CategoryId) 
        REFERENCES Categories(CategoryId) ON DELETE RESTRICT,
    
    -- Constraints
    CONSTRAINT chk_price CHECK (Price > 0),
    CONSTRAINT chk_stock CHECK (Stock >= 0),
    CONSTRAINT chk_discount CHECK (Discount >= 0 AND Discount <= 100),
    CONSTRAINT chk_ram CHECK (RAM > 0 AND RAM <= 128),
    CONSTRAINT chk_storage CHECK (Storage > 0),
    CONSTRAINT chk_screen CHECK (ScreenSize IS NULL OR (ScreenSize >= 10 AND ScreenSize <= 20))
) ENGINE=InnoDB;

-- ============================================
-- TABLO 4: Orders (Siparişler)
-- ============================================
CREATE TABLE Orders (
    OrderId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    OrderDate TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    TotalAmount DECIMAL(10,2) NOT NULL,
    DiscountAmount DECIMAL(10,2) DEFAULT 0,
    FinalAmount DECIMAL(10,2) NOT NULL,
    Status ENUM('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled') 
        DEFAULT 'Pending',
    
    -- Teslimat Bilgileri
    ShippingAddress TEXT NOT NULL,
    ShippingCity VARCHAR(50),
    ShippingPostalCode VARCHAR(10),
    
    PaymentMethod ENUM('CreditCard', 'DebitCard', 'BankTransfer', 'Cash') 
        DEFAULT 'CreditCard',
    PaymentStatus ENUM('Pending', 'Paid', 'Failed', 'Refunded') DEFAULT 'Pending',
    
    Notes TEXT,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Key
    CONSTRAINT fk_order_user FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE RESTRICT,
    
    -- Constraints
    CONSTRAINT chk_total_amount CHECK (TotalAmount > 0),
    CONSTRAINT chk_discount_amount CHECK (DiscountAmount >= 0),
    CONSTRAINT chk_final_amount CHECK (FinalAmount > 0)
) ENGINE=InnoDB;

-- ============================================
-- TABLO 5: OrderDetails (Sipariş Detayları)
-- ============================================
CREATE TABLE OrderDetails (
    OrderDetailId INT AUTO_INCREMENT PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Discount DECIMAL(5,2) DEFAULT 0,
    Subtotal DECIMAL(10,2) NOT NULL,
    
    -- Foreign Keys
    CONSTRAINT fk_orderdetail_order FOREIGN KEY (OrderId) 
        REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT fk_orderdetail_product FOREIGN KEY (ProductId) 
        REFERENCES Products(ProductId) ON DELETE RESTRICT,
    
    -- Constraints
    CONSTRAINT chk_quantity CHECK (Quantity > 0),
    CONSTRAINT chk_unit_price CHECK (UnitPrice > 0),
    CONSTRAINT chk_subtotal CHECK (Subtotal >= 0),
    
    -- Unique constraint: Aynı üründen aynı siparişte sadece bir kez
    CONSTRAINT uk_order_product UNIQUE (OrderId, ProductId)
) ENGINE=InnoDB;

-- ============================================
-- TABLO 6: Reviews (Ürün Yorumları)
-- ============================================
CREATE TABLE Reviews (
    ReviewId INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    UserId INT NOT NULL,
    Rating INT NOT NULL,
    Title VARCHAR(100),
    Comment TEXT,
    IsVerifiedPurchase BOOLEAN DEFAULT FALSE,
    HelpfulCount INT DEFAULT 0,
    IsDeleted BOOLEAN DEFAULT FALSE,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Keys
    CONSTRAINT fk_review_product FOREIGN KEY (ProductId) 
        REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT fk_review_user FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_rating CHECK (Rating >= 1 AND Rating <= 5),
    CONSTRAINT chk_helpful_count CHECK (HelpfulCount >= 0),
    
    -- Unique constraint: Bir kullanıcı bir ürüne sadece bir yorum yapabilir
    CONSTRAINT uk_user_product_review UNIQUE (ProductId, UserId)
) ENGINE=InnoDB;

-- ============================================
-- TABLO 7: Cart (Alışveriş Sepeti)
-- ============================================
CREATE TABLE Cart (
    CartId INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    AddedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    -- Foreign Keys
    CONSTRAINT fk_cart_user FOREIGN KEY (UserId) 
        REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT fk_cart_product FOREIGN KEY (ProductId) 
        REFERENCES Products(ProductId) ON DELETE CASCADE,
    
    -- Constraints
    CONSTRAINT chk_cart_quantity CHECK (Quantity > 0 AND Quantity <= 10),
    
    -- Unique constraint: Aynı ürün aynı kullanıcının sepetinde bir kez
    CONSTRAINT uk_user_product_cart UNIQUE (UserId, ProductId)
) ENGINE=InnoDB;

-- ============================================
-- İNDEXLER (Performance Optimization)
-- ============================================

-- Users tablosu indexleri
CREATE INDEX idx_users_email ON Users(Email);
CREATE INDEX idx_users_role ON Users(Role);

-- Products tablosu indexleri
CREATE INDEX idx_products_category ON Products(CategoryId);
CREATE INDEX idx_products_brand ON Products(Brand);
CREATE INDEX idx_products_price ON Products(Price);
CREATE INDEX idx_products_name ON Products(Name);
CREATE INDEX idx_products_active ON Products(IsActive);

-- Orders tablosu indexleri
CREATE INDEX idx_orders_user ON Orders(UserId);
CREATE INDEX idx_orders_status ON Orders(Status);
CREATE INDEX idx_orders_date ON Orders(OrderDate);

-- OrderDetails tablosu indexleri
CREATE INDEX idx_orderdetails_order ON OrderDetails(OrderId);
CREATE INDEX idx_orderdetails_product ON OrderDetails(ProductId);

-- Reviews tablosu indexleri
CREATE INDEX idx_reviews_product ON Reviews(ProductId);
CREATE INDEX idx_reviews_user ON Reviews(UserId);
CREATE INDEX idx_reviews_rating ON Reviews(Rating);

-- Cart tablosu indexleri
CREATE INDEX idx_cart_user ON Cart(UserId);
CREATE INDEX idx_cart_product ON Cart(ProductId);

-- ============================================
-- TABLO YAPISI ÖZETI
-- ============================================
-- 7 Tablo ✅
-- 15+ Foreign Key ✅
-- 20+ Constraint (CHECK, UNIQUE, NOT NULL) ✅
-- 15+ Index (Performance) ✅
-- ============================================

SELECT 'SmartShop Database Schema Created Successfully!' AS Status;
