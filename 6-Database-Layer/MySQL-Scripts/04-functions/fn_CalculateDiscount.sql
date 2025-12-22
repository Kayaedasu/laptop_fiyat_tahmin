-- ============================================
-- Function 1: fn_CalculateDiscount
-- İndirimli fiyatı hesaplar
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP FUNCTION IF EXISTS fn_CalculateDiscount//

CREATE FUNCTION fn_CalculateDiscount(
    p_Price DECIMAL(10,2),
    p_DiscountPercent DECIMAL(5,2)
)
RETURNS DECIMAL(10,2)
DETERMINISTIC
BEGIN
    DECLARE v_DiscountedPrice DECIMAL(10,2);
    
    -- İndirim hesaplama
    IF p_DiscountPercent IS NULL OR p_DiscountPercent <= 0 THEN
        SET v_DiscountedPrice = p_Price;
    ELSE
        SET v_DiscountedPrice = p_Price * (1 - p_DiscountPercent / 100);
    END IF;
    
    -- İki ondalık basamağa yuvarla
    RETURN ROUND(v_DiscountedPrice, 2);
END//

DELIMITER ;

-- Test
-- SELECT 
--     Name, 
--     Price, 
--     Discount, 
--     fn_CalculateDiscount(Price, Discount) AS DiscountedPrice
-- FROM Products
-- LIMIT 10;
