# SmartShop OrderService - SOAP Mikroservis

## 📋 Genel Bakış

OrderService, SmartShop e-ticaret platformunun sipariş yönetimi için geliştirilmiş **SOAP protokolünü kullanan** bir mikroservistir. Node.js, Express ve `soap` kütüphanesi ile geliştirilmiştir.

## 🎯 Özellikler

### Temel Fonksiyonlar
- ✅ **CreateOrder**: Yeni sipariş oluşturma ve stok kontrolü
- ✅ **GetOrder**: Sipariş detaylarını getirme (items + user bilgisi)
- ✅ **GetUserOrders**: Kullanıcının tüm siparişlerini listeleme
- ✅ **UpdateOrderStatus**: Sipariş durumu güncelleme
- ✅ **CancelOrder**: Sipariş iptali ve stok geri yükleme

### Öne Çıkan Özellikler
- 🔒 **Transaction Management**: MySQL transaction desteği ile veri tutarlılığı
- 📦 **Stock Management**: Sipariş oluşturma/iptal sırasında otomatik stok yönetimi
- ✔️ **Validation**: Kapsamlı input validation ve business rule kontrolü
- 🚫 **Error Handling**: Detaylı hata mesajları ve rollback mekanizması
- 📊 **Detailed Responses**: Tüm operasyonlarda detaylı başarı/hata bilgisi
- 🔗 **Foreign Key Support**: Product ve User ilişkileri kontrol edilir

## 🏗️ Mimari

```
OrderService/
├── server.js           # SOAP server ve service implementation
├── db.js              # MySQL connection pool
├── order.wsdl         # SOAP service definition (WSDL)
├── test-client.js     # Comprehensive test client (11 tests)
├── package.json       # Dependencies
├── .env               # Configuration
└── README.md          # Documentation
```

## 🛠️ Kurulum

### 1. Dependencies Yükleme
```bash
cd 3-Service-Layer/OrderService
npm install
```

### 2. Environment Configuration
`.env` dosyasını düzenleyin:
```env
# Database
DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=your_password
DB_NAME=SmartShopDB

# Server
PORT=3002
NODE_ENV=development

# External Services
PRODUCT_SERVICE_URL=http://localhost:3001/api/v1
USER_SERVICE_URL=http://localhost:50051
```

### 3. Veritabanı Kontrolü
MySQL'de gerekli tablolar var mı kontrol edin:
```sql
USE SmartShopDB;
SHOW TABLES; -- Orders, OrderItems, Products, Users tabloları olmalı
```

## 🚀 Çalıştırma

### Production Mode
```bash
npm start
```

### Development Mode (with auto-reload)
```bash
npm run dev
```

### Test Client
```bash
npm test
```

Server başarıyla başlatıldığında:
```
╔════════════════════════════════════════════════════════╗
║       🛒 SmartShop Order Service (SOAP)               ║
╠════════════════════════════════════════════════════════╣
║  🌐 Server running on: http://localhost:3002          ║
║  📄 WSDL available at: http://localhost:3002/order?wsdl
║  🔌 SOAP endpoint:     http://localhost:3002/order     ║
║                                                        ║
║  📦 Available Operations:                              ║
║     • CreateOrder       - Create new order             ║
║     • GetOrder          - Get order details            ║
║     • GetUserOrders     - Get user orders              ║
║     • UpdateOrderStatus - Update order status          ║
║     • CancelOrder       - Cancel order & restore stock ║
╚════════════════════════════════════════════════════════╝
```

## 📡 SOAP Operations

### 1. CreateOrder
Yeni sipariş oluşturur, stok kontrolü yapar ve stokları azaltır.

**Request:**
```xml
<CreateOrderRequest>
  <UserId>1</UserId>
  <ShippingAddress>123 Main St, City, 12345</ShippingAddress>
  <PaymentMethod>CreditCard</PaymentMethod>
  <Items>
    <OrderItem>
      <ProductId>1</ProductId>
      <Quantity>2</Quantity>
      <UnitPrice>999.99</UnitPrice>
    </OrderItem>
    <OrderItem>
      <ProductId>2</ProductId>
      <Quantity>1</Quantity>
      <UnitPrice>899.99</UnitPrice>
    </OrderItem>
  </Items>
</CreateOrderRequest>
```

**Response:**
```xml
<CreateOrderResponse>
  <Success>true</Success>
  <Message>Order created successfully</Message>
  <OrderId>15</OrderId>
</CreateOrderResponse>
```

**Business Rules:**
- Tüm alanlar zorunludur (UserId, ShippingAddress, PaymentMethod, Items)
- En az 1 ürün olmalıdır
- Ürün stokları kontrol edilir
- Yetersiz stok varsa hata döner
- Tüm işlemler transaction içinde yapılır

### 2. GetOrder
Sipariş detaylarını ve item'larını getirir.

**Request:**
```xml
<GetOrderRequest>
  <OrderId>15</OrderId>
</GetOrderRequest>
```

**Response:**
```xml
<GetOrderResponse>
  <Success>true</Success>
  <Message>Order retrieved successfully</Message>
  <Order>
    <OrderId>15</OrderId>
    <UserId>1</UserId>
    <UserName>john_doe</UserName>
    <OrderDate>2024-01-15T10:30:00Z</OrderDate>
    <TotalAmount>2899.97</TotalAmount>
    <Status>Pending</Status>
    <ShippingAddress>123 Main St, City, 12345</ShippingAddress>
    <PaymentMethod>CreditCard</PaymentMethod>
    <Items>
      <OrderItem>
        <OrderItemId>25</OrderItemId>
        <OrderId>15</OrderId>
        <ProductId>1</ProductId>
        <ProductName>iPhone 14 Pro</ProductName>
        <Quantity>2</Quantity>
        <UnitPrice>999.99</UnitPrice>
        <Subtotal>1999.98</Subtotal>
      </OrderItem>
      <OrderItem>
        <OrderItemId>26</OrderItemId>
        <OrderId>15</OrderId>
        <ProductId>2</ProductId>
        <ProductName>Samsung Galaxy S23</ProductName>
        <Quantity>1</Quantity>
        <UnitPrice>899.99</UnitPrice>
        <Subtotal>899.99</Subtotal>
      </OrderItem>
    </Items>
    <CreatedAt>2024-01-15T10:30:00Z</CreatedAt>
  </Order>
</GetOrderResponse>
```

### 3. GetUserOrders
Kullanıcının tüm siparişlerini listeler.

**Request:**
```xml
<GetUserOrdersRequest>
  <UserId>1</UserId>
</GetUserOrdersRequest>
```

**Response:**
```xml
<GetUserOrdersResponse>
  <Success>true</Success>
  <Message>Found 5 orders</Message>
  <Orders>
    <Order>
      <OrderId>15</OrderId>
      <UserId>1</UserId>
      <OrderDate>2024-01-15T10:30:00Z</OrderDate>
      <TotalAmount>2899.97</TotalAmount>
      <Status>Pending</Status>
      <ShippingAddress>123 Main St</ShippingAddress>
      <PaymentMethod>CreditCard</PaymentMethod>
      <CreatedAt>2024-01-15T10:30:00Z</CreatedAt>
    </Order>
    <!-- More orders... -->
  </Orders>
</GetUserOrdersResponse>
```

### 4. UpdateOrderStatus
Sipariş durumunu günceller.

**Request:**
```xml
<UpdateOrderStatusRequest>
  <OrderId>15</OrderId>
  <Status>Processing</Status>
</UpdateOrderStatusRequest>
```

**Valid Status Values:**
- `Pending` - Beklemede
- `Processing` - İşleniyor
- `Shipped` - Kargoya verildi
- `Delivered` - Teslim edildi
- `Cancelled` - İptal edildi

**Response:**
```xml
<UpdateOrderStatusResponse>
  <Success>true</Success>
  <Message>Order status updated from Pending to Processing</Message>
</UpdateOrderStatusResponse>
```

**Business Rules:**
- Status değeri valid olmalı (yukardaki listeden)
- Cancelled siparişler güncellenemez

### 5. CancelOrder
Siparişi iptal eder ve stokları geri yükler.

**Request:**
```xml
<CancelOrderRequest>
  <OrderId>15</OrderId>
</CancelOrderRequest>
```

**Response:**
```xml
<CancelOrderResponse>
  <Success>true</Success>
  <Message>Order cancelled successfully. Stock restored for 2 items.</Message>
</CancelOrderResponse>
```

**Business Rules:**
- Zaten iptal edilmiş siparişler tekrar iptal edilemez
- Delivered (Teslim edilmiş) siparişler iptal edilemez
- Stoklar otomatik olarak geri yüklenir
- Tüm işlem transaction içinde yapılır

## 🧪 Testing

Test client 11 kapsamlı test içerir:

```bash
npm test
```

### Test Coverage
1. ✅ CreateOrder - Normal sipariş oluşturma
2. ✅ CreateOrder - Invalid data (validation)
3. ✅ CreateOrder - Insufficient stock
4. ✅ GetOrder - Sipariş detayları
5. ✅ GetOrder - Non-existent order
6. ✅ GetUserOrders - Kullanıcı siparişleri
7. ✅ UpdateOrderStatus - Status güncelleme
8. ✅ UpdateOrderStatus - Invalid status
9. ✅ UpdateOrderStatus - Shipped'e güncelleme
10. ✅ CancelOrder - Sipariş iptali
11. ✅ CancelOrder - Already cancelled

### Expected Output
```
📊 TEST SUMMARY
============================================================
Total Tests: 11
✅ Passed: 11
❌ Failed: 0
Success Rate: 100.0%
============================================================

🎉 All tests passed! OrderService is working perfectly.
```

## 🔧 Troubleshooting

### MySQL Connection Failed
```
❌ MySQL connection failed: Access denied
```
**Çözüm:** `.env` dosyasındaki `DB_PASSWORD` doğru mu kontrol edin.

### Port Already in Use
```
Error: listen EADDRINUSE: address already in use :::3002
```
**Çözüm:** 
```bash
# Windows
netstat -ano | findstr :3002
taskkill /PID <PID> /F

# Linux/Mac
lsof -ti:3002 | xargs kill -9
```

### WSDL Not Loading
- `order.wsdl` dosyasının OrderService klasöründe olduğundan emin olun
- WSDL'de port numarası doğru mu kontrol edin (3002)

## 🔗 Integration

### C# SOAP Client Example
```csharp
using System.ServiceModel;

var binding = new BasicHttpBinding();
var endpoint = new EndpointAddress("http://localhost:3002/order");
var client = new OrderServiceClient(binding, endpoint);

var response = await client.CreateOrderAsync(new CreateOrderRequest
{
    UserId = 1,
    ShippingAddress = "123 Main St",
    PaymentMethod = "CreditCard",
    Items = new[]
    {
        new OrderItem { ProductId = 1, Quantity = 2, UnitPrice = 999.99m }
    }
});
```

### Python SOAP Client Example
```python
from zeep import Client

client = Client('http://localhost:3002/order?wsdl')

response = client.service.CreateOrder(
    UserId=1,
    ShippingAddress="123 Main St",
    PaymentMethod="CreditCard",
    Items={
        'OrderItem': [
            {'ProductId': 1, 'Quantity': 2, 'UnitPrice': 999.99}
        ]
    }
)
print(response)
```

## 📊 Database Schema

```sql
-- Orders Table
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY AUTO_INCREMENT,
    UserId INT NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    ShippingAddress VARCHAR(255) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    UpdatedAt DATETIME NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);

-- OrderItems Table
CREATE TABLE OrderItems (
    OrderItemId INT PRIMARY KEY AUTO_INCREMENT,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    Subtotal DECIMAL(10,2) NOT NULL,
    CreatedAt DATETIME NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(OrderId),
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
```

## 🎯 Next Steps

1. ✅ OrderService %100 tamamlandı
2. ⏭️ Integration Layer: C# SOAP client oluşturulacak
3. ⏭️ ML Service (Python/Flask) geliştirilecek
4. ⏭️ Presentation Layer'dan mikroservis entegrasyonu

## 📝 Notes

- SOAP protokolü XML tabanlı iletişim kullanır
- WSDL dosyası servis contract'ını tanımlar
- Transaction yönetimi ile veri tutarlılığı garanti edilir
- Tüm business rule'lar servis katmanında uygulanır
- Stock management otomatik olarak yapılır

## 🤝 Related Services

- **UserService** (gRPC): http://localhost:50051
- **ProductService** (REST): http://localhost:3001/api/v1
- **Integration Layer**: C# client'lar (geliştirilecek)

---

**SmartShop OrderService** - SOAP Mikroservis Architecture  
Version: 1.0.0  
Node.js + Express + MySQL + SOAP
