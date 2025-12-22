-- ============================================
-- View 6: vw_LowStockProducts (Bonus)
-- Stok azalan ürünleri gösterir
-- ============================================

USE SmartShopDB;

DROP VIEW IF EXISTS vw_LowStockProducts;

CREATE VIEW vw_LowStockProducts AS
SELECT 
    p.ProductId,
    p.Name,
    p.Brand,
    p.Model,
    p.Stock,
    p.Price,
    c.Name AS CategoryName,
    -- Son 30 gündeki satış miktarı
    COALESCE(SUM(CASE 
        WHEN o.OrderDate >= DATE_SUB(CURRENT_DATE, INTERVAL 30 DAY) 
        THEN od.Quantity 
        ELSE 0 
    END), 0) AS SoldLast30Days,
    -- Tahmini tükenme günü
    CASE 
        WHEN COALESCE(SUM(CASE 
            WHEN o.OrderDate >= DATE_SUB(CURRENT_DATE, INTERVAL 30 DAY) 
            THEN od.Quantity 
            ELSE 0 
        END), 0) > 0 
        THEN ROUND(p.Stock / (COALESCE(SUM(CASE 
            WHEN o.OrderDate >= DATE_SUB(CURRENT_DATE, INTERVAL 30 DAY) 
            THEN od.Quantity 
            ELSE 0 
        END), 0) / 30), 0)
        ELSE NULL
    END AS EstimatedDaysToStockout,
    p.UpdatedAt AS LastUpdated
FROM Products p
INNER JOIN Categories c ON p.CategoryId = c.CategoryId
LEFT JOIN OrderDetails od ON p.ProductId = od.ProductId
LEFT JOIN Orders o ON od.OrderId = o.OrderId
WHERE p.IsActive = TRUE AND p.Stock <= 10
GROUP BY p.ProductId, c.Name
HAVING p.Stock > 0
ORDER BY p.Stock ASC, SoldLast30Days DESC;

-- Test
-- SELECT * FROM vw_LowStockProducts LIMIT 20;
