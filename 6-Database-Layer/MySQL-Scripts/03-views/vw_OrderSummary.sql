-- ============================================
-- View 2: vw_OrderSummary
-- Sipariş özetini kullanıcı bilgileriyle gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_OrderSummary;

CREATE VIEW vw_OrderSummary AS
SELECT 
    o.OrderId,
    o.OrderDate,
    o.Status,
    o.PaymentStatus,
    o.PaymentMethod,
    u.UserId,
    CONCAT(u.FirstName, ' ', u.LastName) AS CustomerName,
    u.Email AS CustomerEmail,
    u.Phone AS CustomerPhone,
    o.ShippingAddress,
    o.ShippingCity,
    o.ShippingPostalCode,
    o.TotalAmount,
    o.DiscountAmount,
    o.FinalAmount,
    COUNT(DISTINCT od.ProductId) AS TotalItems,
    SUM(od.Quantity) AS TotalQuantity,
    o.Notes,
    o.UpdatedAt AS LastUpdated
FROM Orders o
INNER JOIN Users u ON o.UserId = u.UserId
LEFT JOIN OrderDetails od ON o.OrderId = od.OrderId
GROUP BY o.OrderId, u.UserId;

-- Test
-- SELECT * FROM vw_OrderSummary WHERE Status = 'Pending' ORDER BY OrderDate DESC;
