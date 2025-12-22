-- ============================================
-- View 1: vw_ProductDetails
-- Ürün detaylarını kategori bilgisiyle birlikte gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_ProductDetails;

CREATE VIEW vw_ProductDetails AS
SELECT 
    p.ProductId,
    p.Name,
    p.Brand,
    p.Model,
    p.Processor,
    p.RAM,
    p.Storage,
    p.StorageType,
    p.GPU,
    p.ScreenSize,
    p.Resolution,
    p.Price,
    p.Discount,
    ROUND(p.Price * (1 - p.Discount / 100), 2) AS DiscountedPrice,
    p.Stock,
    p.Condition,
    p.ViewCount,
    c.CategoryId,
    c.Name AS CategoryName,
    c.Description AS CategoryDescription,
    p.ImageUrl,
    p.Description,
    p.IsActive,
    p.CreatedAt,
    p.UpdatedAt,
    -- Ortalama puan hesaplama
    COALESCE(AVG(r.Rating), 0) AS AverageRating,
    COUNT(r.ReviewId) AS ReviewCount
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.CategoryId
LEFT JOIN Reviews r ON p.ProductId = r.ProductId
GROUP BY p.ProductId, c.CategoryId;

-- Test
-- SELECT * FROM vw_ProductDetails WHERE IsActive = TRUE ORDER BY AverageRating DESC LIMIT 10;
