-- ============================================
-- Stored Procedure 2: sp_UpdateProductStock
-- Sipariş sonrası ürün stoğunu güncelle
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_UpdateProductStock//

CREATE PROCEDURE sp_UpdateProductStock(
    IN p_ProductId INT,
    IN p_Quantity INT,
    IN p_Operation VARCHAR(10),  -- 'ADD' veya 'SUBTRACT'
    OUT p_Success BOOLEAN,
    OUT p_Message VARCHAR(255)
)
BEGIN
    DECLARE v_CurrentStock INT;
    DECLARE v_NewStock INT;
    
    -- Hata kontrolü için handler
    DECLARE EXIT HANDLER FOR SQLEXCEPTION
    BEGIN
        SET p_Success = FALSE;
        SET p_Message = 'Hata: Stok güncellenemedi!';
        ROLLBACK;
    END;
    
    START TRANSACTION;
    
    -- Mevcut stok miktarını al
    SELECT Stock INTO v_CurrentStock
    FROM Products
    WHERE ProductId = p_ProductId
    FOR UPDATE;  -- Kilitle
    
    -- Ürün bulunamadı kontrolü
    IF v_CurrentStock IS NULL THEN
        SET p_Success = FALSE;
        SET p_Message = 'Hata: Ürün bulunamadı!';
        ROLLBACK;
    ELSE
        -- İşlem tipine göre yeni stok hesapla
        IF p_Operation = 'ADD' THEN
            SET v_NewStock = v_CurrentStock + p_Quantity;
        ELSEIF p_Operation = 'SUBTRACT' THEN
            SET v_NewStock = v_CurrentStock - p_Quantity;
            
            -- Negatif stok kontrolü
            IF v_NewStock < 0 THEN
                SET p_Success = FALSE;
                SET p_Message = CONCAT('Hata: Yetersiz stok! Mevcut: ', v_CurrentStock);
                ROLLBACK;
            END IF;
        ELSE
            SET p_Success = FALSE;
            SET p_Message = 'Hata: Geçersiz işlem tipi! (ADD veya SUBTRACT)';
            ROLLBACK;
        END IF;
        
        -- Stok güncelleme
        IF v_NewStock >= 0 THEN
            UPDATE Products
            SET Stock = v_NewStock,
                UpdatedAt = CURRENT_TIMESTAMP
            WHERE ProductId = p_ProductId;
            
            SET p_Success = TRUE;
            SET p_Message = CONCAT('Başarılı! Yeni stok: ', v_NewStock);
            COMMIT;
        END IF;
    END IF;
    
END//

DELIMITER ;

-- Test
-- CALL sp_UpdateProductStock(1, 5, 'SUBTRACT', @success, @message);
-- SELECT @success, @message;
