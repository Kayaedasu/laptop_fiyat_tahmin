-- ============================================
-- Function 2: fn_GetAverageRating
-- Ürünün ortalama puanını hesaplar
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP FUNCTION IF EXISTS fn_GetAverageRating//

CREATE FUNCTION fn_GetAverageRating(
    p_ProductId INT
)
RETURNS DECIMAL(3,2)
READS SQL DATA
BEGIN
    DECLARE v_AverageRating DECIMAL(3,2);
    
    -- Ortalama puanı hesapla
    SELECT COALESCE(AVG(Rating), 0)
    INTO v_AverageRating
    FROM Reviews
    WHERE ProductId = p_ProductId;
    
    -- İki ondalık basamağa yuvarla
    RETURN ROUND(v_AverageRating, 2);
END//

DELIMITER ;

-- Test
-- SELECT 
--     ProductId,
--     Name,
--     fn_GetAverageRating(ProductId) AS AverageRating
-- FROM Products
-- LIMIT 10;
