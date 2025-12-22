-- ============================================
-- MASTER SCRIPT
-- Tüm veritabanı scriptlerini tek seferde çalıştırır
-- ============================================

-- KULLANIM:
-- mysql -u root -p < 00-master-setup.sql

-- ============================================
-- 1. SCHEMA - Tabloları oluştur
-- ============================================
SOURCE 01-schema.sql;

-- ============================================
-- 2. STORED PROCEDURES
-- ============================================
SOURCE 02-stored-procedures/sp_GetUserOrders.sql;
SOURCE 02-stored-procedures/sp_UpdateProductStock.sql;
SOURCE 02-stored-procedures/sp_CreateOrder.sql;

-- ============================================
-- 3. VIEWS
-- ============================================
SOURCE 03-views/vw_ProductDetails.sql;
SOURCE 03-views/vw_OrderSummary.sql;
SOURCE 03-views/vw_CustomerReviews.sql;
SOURCE 03-views/vw_TopProducts.sql;
SOURCE 03-views/vw_MonthlyRevenue.sql;
SOURCE 03-views/vw_LowStockProducts.sql;

-- ============================================
-- 4. FUNCTIONS
-- ============================================
SOURCE 04-functions/fn_CalculateDiscount.sql;
SOURCE 04-functions/fn_GetAverageRating.sql;
SOURCE 04-functions/fn_GetTotalCartValue.sql;

-- ============================================
-- 5. SEED DATA - Test verileri
-- ============================================
SOURCE 07-seed-data.sql;

-- ============================================
-- TAMAMLANDI!
-- ============================================
SELECT '============================================' AS '';
SELECT '✅ SmartShop Veritabanı Başarıyla Kuruldu!' AS Status;
SELECT '============================================' AS '';
SELECT '' AS '';

-- Özet bilgiler
SELECT 'VERITABANI ÖZETİ' AS '';
SELECT '------------' AS '';
USE SmartShopDB;
SELECT 'Toplam Tablolar:' AS Info, COUNT(*) AS Sayi FROM information_schema.tables WHERE table_schema = 'SmartShopDB' AND table_type = 'BASE TABLE';
SELECT 'Toplam Views:' AS Info, COUNT(*) AS Sayi FROM information_schema.views WHERE table_schema = 'SmartShopDB';
SELECT 'Toplam Stored Procedures:' AS Info, COUNT(*) AS Sayi FROM information_schema.routines WHERE routine_schema = 'SmartShopDB' AND routine_type = 'PROCEDURE';
SELECT 'Toplam Functions:' AS Info, COUNT(*) AS Sayi FROM information_schema.routines WHERE routine_schema = 'SmartShopDB' AND routine_type = 'FUNCTION';

SELECT '' AS '';
SELECT 'VERİ ÖZETİ' AS '';
SELECT '------------' AS '';
SELECT 'Kullanıcılar:' AS Tablo, COUNT(*) AS Toplam FROM Users;
SELECT 'Kategoriler:' AS Tablo, COUNT(*) AS Toplam FROM Categories;
SELECT 'Ürünler:' AS Tablo, COUNT(*) AS Toplam FROM Products;
SELECT 'Siparişler:' AS Tablo, COUNT(*) AS Toplam FROM Orders;
SELECT 'Yorumlar:' AS Tablo, COUNT(*) AS Toplam FROM Reviews;
SELECT 'Sepet:' AS Tablo, COUNT(*) AS Toplam FROM Cart;

SELECT '' AS '';
SELECT '🎉 Kurulum tamamlandı! Artık uygulamayı geliştirebilirsiniz.' AS Mesaj;
