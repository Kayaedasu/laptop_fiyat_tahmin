-- ============================================
-- Stored Procedure 1: sp_GetUserOrders
-- Kullanıcının tüm siparişlerini detaylı getir
-- ============================================

USE SmartShopDB;

DELIMITER //

DROP PROCEDURE IF EXISTS sp_GetUserOrders//

CREATE PROCEDURE sp_GetUserOrders(
    IN p_UserId INT
)
BEGIN
    -- Kullanıcının siparişlerini getir
    SELECT 
        o.OrderId,
        o.OrderDate,
        o.TotalAmount,
        o.DiscountAmount,
        o.FinalAmount,
        o.Status,
        o.PaymentStatus,
        o.ShippingAddress,
        o.ShippingCity,
        COUNT(od.OrderDetailId) AS TotalItems,
        SUM(od.Quantity) AS TotalQuantity
    FROM Orders o
    LEFT JOIN OrderDetails od ON o.OrderId = od.OrderId
    WHERE o.UserId = p_UserId
    GROUP BY o.OrderId
    ORDER BY o.OrderDate DESC;
    
    -- İsteğe bağlı: Son siparişin detaylarını da getir
    SELECT 
        od.OrderDetailId,
        od.OrderId,
        p.Name AS ProductName,
        p.Brand,
        p.Model,
        od.Quantity,
        od.UnitPrice,
        od.Discount,
        od.Subtotal
    FROM OrderDetails od
    INNER JOIN Products p ON od.ProductId = p.ProductId
    WHERE od.OrderId = (
        SELECT OrderId 
        FROM Orders 
        WHERE UserId = p_UserId 
        ORDER BY OrderDate DESC 
        LIMIT 1
    );
END//

DELIMITER ;

-- Test
-- CALL sp_GetUserOrders(1);
