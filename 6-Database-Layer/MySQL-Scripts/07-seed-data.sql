-- ============================================
-- Test Verileri (Seed Data)
-- SmartShop Database için örnek veriler
-- ============================================

USE SmartShopDB;

-- ============================================
-- 1. Users (Kullanıcılar)
-- ============================================
INSERT INTO Users (Email, Password, FirstName, LastName, Phone, Role, IsActive) VALUES
('admin@smartshop.com', 'hashed_password_123', 'Admin', 'User', '05551234567', 'Admin', TRUE),
('ahmet.yilmaz@email.com', 'hashed_password_456', 'Ahmet', 'Yılmaz', '05551234568', 'Customer', TRUE),
('ayse.demir@email.com', 'hashed_password_789', 'Ayşe', 'Demir', '05551234569', 'Customer', TRUE),
('mehmet.kaya@email.com', 'hashed_password_abc', 'Mehmet', 'Kaya', '05551234570', 'Customer', TRUE),
('zeynep.sahin@email.com', 'hashed_password_def', 'Zeynep', 'Şahin', '05551234571', 'Customer', TRUE),
('can.ozturk@email.com', 'hashed_password_ghi', 'Can', 'Öztürk', '05551234572', 'Customer', TRUE);

-- ============================================
-- 2. Categories (Kategoriler)
-- ============================================
INSERT INTO Categories (Name, Description, IsActive) VALUES
('Gaming Laptops', 'Yüksek performanslı oyun laptopları', TRUE),
('Business Laptops', 'İş ve ofis kullanımı için laptoplar', TRUE),
('Student Laptops', 'Öğrenciler için uygun fiyatlı laptoplar', TRUE),
('Ultrabooks', 'İnce, hafif ve taşınabilir laptoplar', TRUE),
('Workstations', 'Profesyonel iş istasyonları', TRUE);

-- ============================================
-- 3. Products (Laptop Ürünleri)
-- ============================================
INSERT INTO Products (Name, Brand, Model, Processor, RAM, Storage, StorageType, GPU, ScreenSize, Resolution, Price, Stock, Discount, CategoryId, Description, ImageUrl, ProductCondition, IsActive) VALUES
-- Gaming Laptops
('ASUS ROG Strix G15', 'ASUS', 'G513QM', 'AMD Ryzen 9 5900HX', 16, 512, 'SSD', 'NVIDIA RTX 3060', 15.6, '1920x1080', 28999.99, 15, 10, 1, 'Güçlü gaming laptop', '/images/asus-rog.jpg', 'New', TRUE),
('MSI Katana GF66', 'MSI', 'GF66 11UE', 'Intel Core i7-11800H', 16, 512, 'SSD', 'NVIDIA RTX 3060 Ti', 15.6, '1920x1080', 26499.99, 20, 15, 1, 'Orta segment gaming', '/images/msi-katana.jpg', 'New', TRUE),
('Lenovo Legion 5', 'Lenovo', 'Legion 5 Pro', 'AMD Ryzen 7 5800H', 16, 1024, 'SSD', 'NVIDIA RTX 3070', 16.0, '2560x1600', 32999.99, 10, 5, 1, 'Profesyonel gaming deneyimi', '/images/lenovo-legion.jpg', 'New', TRUE),
('Acer Nitro 5', 'Acer', 'AN515-57', 'Intel Core i5-11400H', 8, 512, 'SSD', 'NVIDIA GTX 1650', 15.6, '1920x1080', 18999.99, 25, 20, 1, 'Giriş seviyesi gaming', '/images/acer-nitro.jpg', 'New', TRUE),

-- Business Laptops
('Dell Latitude 5420', 'Dell', 'Latitude 5420', 'Intel Core i5-1145G7', 8, 256, 'SSD', 'Intel Iris Xe', 14.0, '1920x1080', 16999.99, 30, 0, 2, 'İş dünyası için ideal', '/images/dell-latitude.jpg', 'New', TRUE),
('HP EliteBook 840', 'HP', 'EliteBook 840 G8', 'Intel Core i7-1165G7', 16, 512, 'SSD', 'Intel Iris Xe', 14.0, '1920x1080', 24999.99, 18, 10, 2, 'Premium iş laptopu', '/images/hp-elitebook.jpg', 'New', TRUE),
('Lenovo ThinkPad E14', 'Lenovo', 'ThinkPad E14 Gen 3', 'AMD Ryzen 5 5500U', 8, 256, 'SSD', 'AMD Radeon', 14.0, '1920x1080', 14999.99, 35, 5, 2, 'Güvenilir iş arkadaşı', '/images/lenovo-thinkpad.jpg', 'New', TRUE),

-- Student Laptops
('ASUS VivoBook 15', 'ASUS', 'X515EA', 'Intel Core i3-1115G4', 8, 256, 'SSD', 'Intel UHD', 15.6, '1920x1080', 9999.99, 50, 15, 3, 'Öğrenciler için ekonomik', '/images/asus-vivobook.jpg', 'New', TRUE),
('HP 250 G8', 'HP', '250 G8', 'Intel Core i5-1035G7', 8, 256, 'SSD', 'Intel UHD', 15.6, '1920x1080', 11999.99, 40, 10, 3, 'Her gün kullanım', '/images/hp-250.jpg', 'New', TRUE),
('Acer Aspire 5', 'Acer', 'A515-56', 'Intel Core i5-1135G7', 8, 512, 'SSD', 'Intel Iris Xe', 15.6, '1920x1080', 12999.99, 45, 12, 3, 'Çok yönlü kullanım', '/images/acer-aspire.jpg', 'New', TRUE),

-- Ultrabooks
('Dell XPS 13', 'Dell', 'XPS 13 9310', 'Intel Core i7-1185G7', 16, 512, 'SSD', 'Intel Iris Xe', 13.4, '1920x1200', 32999.99, 12, 0, 4, 'Premium ultrabook', '/images/dell-xps13.jpg', 'New', TRUE),
('MacBook Air M2', 'Apple', 'MacBook Air', 'Apple M2', 8, 256, 'SSD', 'Apple M2 GPU', 13.6, '2560x1664', 29999.99, 20, 5, 4, 'Apple Silicon güç', '/images/macbook-air.jpg', 'New', TRUE),
('Huawei MateBook X Pro', 'Huawei', 'MateBook X Pro', 'Intel Core i7-1165G7', 16, 512, 'SSD', 'Intel Iris Xe', 13.9, '3000x2000', 27999.99, 15, 8, 4, 'Zarif tasarım', '/images/huawei-matebook.jpg', 'New', TRUE),

-- Workstations
('HP ZBook Studio G8', 'HP', 'ZBook Studio G8', 'Intel Core i9-11950H', 32, 1024, 'SSD', 'NVIDIA RTX A3000', 15.6, '1920x1080', 54999.99, 8, 0, 5, 'Profesyonel iş istasyonu', '/images/hp-zbook.jpg', 'New', TRUE),
('Dell Precision 5560', 'Dell', 'Precision 5560', 'Intel Core i7-11850H', 32, 512, 'SSD', 'NVIDIA RTX A2000', 15.6, '1920x1080', 48999.99, 10, 5, 5, 'Güçlü workstation', '/images/dell-precision.jpg', 'New', TRUE),

-- İkinci El Ürünler
('Lenovo ThinkPad T490 (İkinci El)', 'Lenovo', 'ThinkPad T490', 'Intel Core i5-8265U', 8, 256, 'SSD', 'Intel UHD 620', 14.0, '1920x1080', 8999.99, 5, 0, 2, 'Temiz ikinci el', '/images/lenovo-t490.jpg', 'Used', TRUE),
('MacBook Pro 2019 (İkinci El)', 'Apple', 'MacBook Pro', 'Intel Core i5', 8, 256, 'SSD', 'Intel Iris Plus', 13.3, '2560x1600', 18999.99, 3, 10, 4, 'Az kullanılmış', '/images/macbook-pro-2019.jpg', 'Used', TRUE);

-- ============================================
-- 4. Reviews (Yorumlar)
-- ============================================
INSERT INTO Reviews (ProductId, UserId, Rating, Title, Comment, IsVerifiedPurchase, HelpfulCount) VALUES
(1, 2, 5, 'Mükemmel performans!', 'Gaming için harika bir laptop, RTX 3060 ile tüm oyunları sorunsuz oynuyorum.', TRUE, 15),
(1, 3, 4, 'İyi ama biraz gürültülü', 'Performans çok iyi ancak fanlar biraz gürültülü çalışıyor.', TRUE, 8),
(2, 4, 5, 'Fiyat/performans şampiyonu', 'Bu fiyata bu performans harika!', TRUE, 22),
(3, 5, 5, 'Legion serisi harika', 'Lenovo Legion serisi gerçekten kaliteli, ekranı muhteşem.', FALSE, 12),
(4, 6, 3, 'Giriş seviyesi için iyi', 'İlk gaming laptobum, orta seviye oyunlar için yeterli.', TRUE, 5),
(5, 2, 4, 'İş için ideal', 'Ofis çalışması için mükemmel, pil ömrü de iyi.', TRUE, 7),
(8, 3, 5, 'Öğrenci dostu', 'Fiyatı uygun, performansı yeterli. Öğrenciler için harika.', TRUE, 18),
(10, 4, 4, 'Günlük kullanım için süper', 'Her türlü işimi görebiliyorum, önerim var.', FALSE, 9),
(11, 5, 5, 'XPS serisi efsane', 'Dell XPS 13 gerçekten premium bir deneyim sunuyor.', TRUE, 25),
(12, 6, 5, 'M2 çipi harika!', 'MacBook Air M2 ile çalışmak çok keyifli, batarya süresi inanılmaz.', TRUE, 31);

-- ============================================
-- 5. Cart (Sepet - Örnek veriler)
-- ============================================
INSERT INTO Cart (UserId, ProductId, Quantity) VALUES
(2, 1, 1),  -- Ahmet'in sepetinde ASUS ROG
(2, 8, 1),  -- Ahmet'in sepetinde ASUS VivoBook
(3, 12, 1), -- Ayşe'nin sepetinde MacBook Air
(4, 5, 2),  -- Mehmet'in sepetinde Dell Latitude (2 adet)
(5, 3, 1);  -- Zeynep'in sepetinde Lenovo Legion

-- ============================================
-- 6. Orders (Siparişler - Geçmiş siparişler)
-- ============================================
INSERT INTO Orders (UserId, OrderDate, TotalAmount, DiscountAmount, FinalAmount, Status, ShippingAddress, ShippingCity, ShippingPostalCode, PaymentMethod, PaymentStatus) VALUES
(2, '2024-12-01 10:30:00', 28999.99, 2900.00, 26099.99, 'Delivered', 'Atatürk Cad. No:123', 'İstanbul', '34000', 'CreditCard', 'Paid'),
(3, '2024-12-05 14:15:00', 9999.99, 1500.00, 8499.99, 'Delivered', 'İnönü Sok. No:45', 'Ankara', '06000', 'DebitCard', 'Paid'),
(4, '2024-12-08 09:20:00', 16999.99, 0, 16999.99, 'Shipped', 'Cumhuriyet Mah. No:67', 'İzmir', '35000', 'CreditCard', 'Paid'),
(5, '2024-12-10 16:45:00', 32999.99, 1650.00, 31349.99, 'Processing', 'Kültür Cad. No:89', 'Bursa', '16000', 'BankTransfer', 'Paid'),
(6, '2024-12-12 11:00:00', 24999.99, 2500.00, 22499.99, 'Pending', 'Zafer Mah. No:34', 'Antalya', '07000', 'CreditCard', 'Pending');

-- ============================================
-- 7. OrderDetails (Sipariş Detayları)
-- ============================================
INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Discount, Subtotal) VALUES
-- Sipariş 1
(1, 1, 1, 28999.99, 10, 26099.99),
-- Sipariş 2
(2, 8, 1, 9999.99, 15, 8499.99),
-- Sipariş 3
(3, 5, 1, 16999.99, 0, 16999.99),
-- Sipariş 4
(4, 3, 1, 32999.99, 5, 31349.99),
-- Sipariş 5
(5, 6, 1, 24999.99, 10, 22499.99);

-- ============================================
-- ViewCount güncelleme (Simüle edilen görüntülenmeler)
-- ============================================
UPDATE Products SET ViewCount = 150 WHERE ProductId = 1;
UPDATE Products SET ViewCount = 120 WHERE ProductId = 2;
UPDATE Products SET ViewCount = 200 WHERE ProductId = 3;
UPDATE Products SET ViewCount = 95 WHERE ProductId = 4;
UPDATE Products SET ViewCount = 180 WHERE ProductId = 11;
UPDATE Products SET ViewCount = 250 WHERE ProductId = 12;

-- ============================================
-- Başarılı mesaj
-- ============================================
SELECT 'Test verileri başarıyla eklendi!' AS Status;
SELECT 'Toplam Kullanıcılar:' AS Info, COUNT(*) AS Count FROM Users;
SELECT 'Toplam Kategoriler:' AS Info, COUNT(*) AS Count FROM Categories;
SELECT 'Toplam Ürünler:' AS Info, COUNT(*) AS Count FROM Products;
SELECT 'Toplam Siparişler:' AS Info, COUNT(*) AS Count FROM Orders;
SELECT 'Toplam Yorumlar:' AS Info, COUNT(*) AS Count FROM Reviews;
SELECT 'Sepetteki Ürünler:' AS Info, COUNT(*) AS Count FROM Cart;
