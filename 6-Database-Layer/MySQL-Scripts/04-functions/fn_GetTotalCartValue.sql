-- ============================================
-- Function 3: fn_GetTotalCartValue (Bonus)
-- Kullanıcının sepet toplamını hesaplar
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP FUNCTION IF EXISTS fn_GetTotalCartValue//

CREATE FUNCTION fn_GetTotalCartValue(
    p_UserId INT
)
RETURNS DECIMAL(10,2)
READS SQL DATA
BEGIN
    DECLARE v_TotalValue DECIMAL(10,2);
    
    -- Sepet toplamını hesapla (indirimleri uygula)
    SELECT COALESCE(
        SUM(
            p.Price * c.Quantity * (1 - p.Discount / 100)
        ), 
        0
    )
    INTO v_TotalValue
    FROM Cart c
    INNER JOIN Products p ON c.ProductId = p.ProductId
    WHERE c.UserId = p_UserId;
    
    RETURN ROUND(v_TotalValue, 2);
END//

DELIMITER ;

-- Test
-- SELECT 
--     UserId,
--     fn_GetTotalCartValue(UserId) AS CartTotal
-- FROM Users
-- WHERE Role = 'Customer'
-- LIMIT 10;
