-- ============================================
-- Quick Fix: Set All Products to Active
-- ============================================

USE SmartShopDB;

-- Show current state BEFORE fix
SELECT 
    'BEFORE FIX' AS Status,
    IsActive,
    COUNT(*) AS ProductCount
FROM Products
GROUP BY IsActive;

-- Fix: Set all products to active
UPDATE Products SET IsActive = 1 WHERE IsActive = 0 OR IsActive IS NULL;

-- Show state AFTER fix
SELECT 
    'AFTER FIX' AS Status,
    IsActive,
    COUNT(*) AS ProductCount
FROM Products
GROUP BY IsActive;

-- Verify: Show active products
SELECT 
    ProductId,
    Name,
    Brand,
    Price,
    Stock,
    CategoryId,
    IsActive,
    'Should all be 1 (TRUE)' AS Note
FROM Products
LIMIT 10;

-- Final confirmation
SELECT 
    CONCAT('✅ Total Active Products: ', COUNT(*)) AS Result
FROM Products
WHERE IsActive = 1;
