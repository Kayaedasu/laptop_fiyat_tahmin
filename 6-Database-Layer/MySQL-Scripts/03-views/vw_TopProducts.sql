-- ============================================
-- View 4: vw_TopProducts
-- En çok satan ve en yüksek puanlı ürünleri gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_TopProducts;

CREATE VIEW vw_TopProducts AS
SELECT 
    p.ProductId,
    p.Name,
    p.Brand,
    p.Model,
    p.Price,
    p.Discount,
    ROUND(p.Price * (1 - p.Discount / 100), 2) AS DiscountedPrice,
    p.Stock,
    c.Name AS CategoryName,
    p.ImageUrl,
    p.ViewCount,
    -- Satış bilgileri
    COALESCE(SUM(od.Quantity), 0) AS TotalSold,
    COALESCE(SUM(od.Subtotal), 0) AS TotalRevenue,
    -- Yorum bilgileri
    COALESCE(AVG(r.Rating), 0) AS AverageRating,
    COUNT(DISTINCT r.ReviewId) AS ReviewCount,
    -- Popülerlik skoru (satış + görüntülenme + puan)
    (COALESCE(SUM(od.Quantity), 0) * 10 + 
     p.ViewCount + 
     COALESCE(AVG(r.Rating), 0) * 20) AS PopularityScore
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.CategoryId
LEFT JOIN OrderDetails od ON p.ProductId = od.ProductId
LEFT JOIN Reviews r ON p.ProductId = r.ProductId
WHERE p.IsActive = TRUE AND p.Stock > 0
GROUP BY p.ProductId, c.Name
ORDER BY PopularityScore DESC;

-- Test
-- SELECT * FROM vw_TopProducts LIMIT 20;
