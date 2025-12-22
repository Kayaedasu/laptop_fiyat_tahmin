-- ============================================
-- View 5: vw_MonthlyRevenue
-- Aylık gelir ve sipariş istatistiklerini gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_MonthlyRevenue;

CREATE VIEW vw_MonthlyRevenue AS
SELECT 
    YEAR(o.OrderDate) AS Year,
    MONTH(o.OrderDate) AS Month,
    DATE_FORMAT(o.OrderDate, '%Y-%m') AS YearMonth,
    DATE_FORMAT(o.OrderDate, '%M %Y') AS MonthName,
    COUNT(DISTINCT o.OrderId) AS TotalOrders,
    COUNT(DISTINCT o.UserId) AS UniqueCustomers,
    SUM(o.TotalAmount) AS GrossRevenue,
    SUM(o.DiscountAmount) AS TotalDiscounts,
    SUM(o.FinalAmount) AS NetRevenue,
    AVG(o.FinalAmount) AS AverageOrderValue,
    SUM(CASE WHEN o.Status = 'Delivered' THEN o.FinalAmount ELSE 0 END) AS DeliveredRevenue,
    SUM(CASE WHEN o.Status = 'Cancelled' THEN o.FinalAmount ELSE 0 END) AS CancelledRevenue,
    COUNT(CASE WHEN o.Status = 'Delivered' THEN 1 END) AS DeliveredOrders,
    COUNT(CASE WHEN o.Status = 'Cancelled' THEN 1 END) AS CancelledOrders,
    ROUND(COUNT(CASE WHEN o.Status = 'Delivered' THEN 1 END) * 100.0 / COUNT(*), 2) AS DeliveryRate
FROM Orders o
GROUP BY YEAR(o.OrderDate), MONTH(o.OrderDate)
ORDER BY Year DESC, Month DESC;

-- Test
-- SELECT * FROM vw_MonthlyRevenue ORDER BY Year DESC, Month DESC LIMIT 12;
