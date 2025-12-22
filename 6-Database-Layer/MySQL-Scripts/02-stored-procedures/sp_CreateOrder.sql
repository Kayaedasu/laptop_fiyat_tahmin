-- ============================================
-- Stored Procedure 3: sp_CreateOrder
-- Sepetten sipariş oluştur ve stokları güncelle
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_CreateOrder//

CREATE PROCEDURE sp_CreateOrder(
    IN p_UserId INT,
    IN p_ShippingAddress TEXT,
    IN p_ShippingCity VARCHAR(50),
    IN p_ShippingPostalCode VARCHAR(10),
    IN p_PaymentMethod VARCHAR(20),
    OUT p_OrderId INT,
    OUT p_Success BOOLEAN,
    OUT p_Message VARCHAR(255)
)
BEGIN
    DECLARE v_TotalAmount DECIMAL(10,2) DEFAULT 0;
    DECLARE v_DiscountAmount DECIMAL(10,2) DEFAULT 0;
    DECLARE v_FinalAmount DECIMAL(10,2) DEFAULT 0;
    DECLARE v_CartCount INT DEFAULT 0;
    
    -- Hata kontrolü
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_Success = FALSE;
        SET p_Message = 'Hata: Sipariş oluşturulamadı!';
        SET p_OrderId = NULL;
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Sepetteki ürün sayısını kontrol et
    SELECT COUNT(*) INTO v_CartCount
    FROM Cart
    WHERE UserId = p_UserId;
    
    IF v_CartCount = 0 THEN
        SET p_Success = FALSE;
        SET p_Message = 'Hata: Sepet boş!';
        SET p_OrderId = NULL;
        ROLLBACK;
    ELSE
        -- Toplam tutarı hesapla
        SELECT 
            SUM(p.Price * c.Quantity) AS Total,
            SUM((p.Price * p.Discount / 100) * c.Quantity) AS Discount
        INTO v_TotalAmount, v_DiscountAmount
        FROM Cart c
        INNER JOIN Products p ON c.ProductId = p.ProductId
        WHERE c.UserId = p_UserId;
        
        SET v_FinalAmount = v_TotalAmount - v_DiscountAmount;
        
        -- Sipariş oluştur
        INSERT INTO Orders (
            UserId, TotalAmount, DiscountAmount, FinalAmount,
            ShippingAddress, ShippingCity, ShippingPostalCode,
            PaymentMethod, Status, PaymentStatus
        ) VALUES (
            p_UserId, v_TotalAmount, v_DiscountAmount, v_FinalAmount,
            p_ShippingAddress, p_ShippingCity, p_ShippingPostalCode,
            p_PaymentMethod, 'Pending', 'Pending'
        );
        
        SET p_OrderId = LAST_INSERT_ID();
        
        -- Sipariş detaylarını ekle ve stokları güncelle
        INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Discount, Subtotal)
        SELECT 
            p_OrderId,
            c.ProductId,
            c.Quantity,
            p.Price,
            p.Discount,
            (p.Price * c.Quantity) - ((p.Price * p.Discount / 100) * c.Quantity)
        FROM Cart c
        INNER JOIN Products p ON c.ProductId = p.ProductId
        WHERE c.UserId = p_UserId;
        
        -- Stokları güncelle
        UPDATE Products p
        INNER JOIN Cart c ON p.ProductId = c.ProductId
        SET p.Stock = p.Stock - c.Quantity
        WHERE c.UserId = p_UserId;
        
        -- Sepeti temizle
        DELETE FROM Cart WHERE UserId = p_UserId;
        
        SET p_Success = TRUE;
        SET p_Message = CONCAT('Sipariş başarıyla oluşturuldu! Sipariş No: ', p_OrderId);
        COMMIT;
    END IF;
    
END//

DELIMITER ;

-- Test
-- CALL sp_CreateOrder(1, 'Test Adres', 'İstanbul', '34000', 'CreditCard', @orderId, @success, @message);
-- SELECT @orderId, @success, @message;
