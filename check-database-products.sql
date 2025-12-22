-- ============================================
-- Veritabanı Ürün Kontrolü
-- Ürünlerin ve kategorilerin veritabanında olup olmadığını kontrol eder
-- ============================================

USE SmartShopDB;

-- Tablo var mı kontrol
SHOW TABLES;

-- Ürün sayısını göster
SELECT COUNT(*) AS TotalProducts FROM Products;

-- Aktif ürün sayısını göster
SELECT COUNT(*) AS ActiveProducts FROM Products WHERE IsActive = TRUE;

-- Kategori başına ürün sayısı
SELECT 
    c.Name AS CategoryName,
    COUNT(p.ProductId) AS ProductCount
FROM Categories c
LEFT JOIN Products p ON c.CategoryId = p.CategoryId
GROUP BY c.CategoryId, c.Name;

-- Örnek ürünleri göster (ilk 10 ürün)
SELECT 
    p.ProductId,
    p.Name,
    p.Brand,
    p.Price,
    p.Stock,
    c.Name AS CategoryName,
    p.IsActive
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.CategoryId
LIMIT 10;

-- Kategorileri göster
SELECT CategoryId, Name, Description, IsActive FROM Categories;
