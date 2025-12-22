-- ============================================
-- View 3: vw_CustomerReviews
-- Müşteri yorumlarını ürün ve kullanıcı bilgileriyle gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_CustomerReviews;

CREATE VIEW vw_CustomerReviews AS
SELECT 
    r.ReviewId,
    r.Rating,
    r.Title,
    r.Comment,
    r.IsVerifiedPurchase,
    r.HelpfulCount,
    r.CreatedAt,
    r.UpdatedAt,
    -- Ürün bilgileri
    p.ProductId,
    p.Name AS ProductName,
    p.Brand AS ProductBrand,
    p.Model AS ProductModel,
    -- Kullanıcı bilgileri
    u.UserId,
    CONCAT(u.FirstName, ' ', SUBSTRING(u.LastName, 1, 1), '.') AS ReviewerName,
    -- Gizlilik için email'i gizle
    CONCAT(SUBSTRING(u.Email, 1, 3), '***@', 
           SUBSTRING_INDEX(u.Email, '@', -1)) AS ReviewerEmail
FROM Reviews r
INNER JOIN Products p ON r.ProductId = p.ProductId
INNER JOIN Users u ON r.UserId = u.UserId
ORDER BY r.CreatedAt DESC;

-- Test
-- SELECT * FROM vw_CustomerReviews WHERE Rating >= 4 LIMIT 10;
