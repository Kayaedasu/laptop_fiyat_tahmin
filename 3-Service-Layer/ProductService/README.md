# 🛍️ SmartShop ProductService - REST API Mikroservisi

## 📋 Genel Bakış

ProductService, SmartShop e-ticaret platformunun ürün ve kategori yönetiminden sorumlu **REST API** tabanlı mikroservisidir. Express.js kullanılarak geliştirilmiştir ve MySQL veritabanı ile entegre çalışır.

## 🎯 Özellikler

### Ürün Yönetimi
- ✅ Tüm ürünleri listeleme (pagination, filtreleme, sıralama)
- ✅ Ürün detay görüntüleme
- ✅ Ürün arama (isim ve açıklamada)
- ✅ Kategori bazlı filtreleme
- ✅ Fiyat aralığı filtreleme
- ✅ Stok durumu filtreleme
- ✅ Ürün oluşturma (Admin)
- ✅ Ürün güncelleme (Admin)
- ✅ Stok güncelleme (Admin)
- ✅ Ürün silme - soft delete (Admin)
- ✅ En yüksek puanlı ürünler
- ✅ Düşük stoklu ürünler (Admin)
- ✅ Ürün yorumlarını görüntüleme

### Kategori Yönetimi
- ✅ Tüm kategorileri listeleme
- ✅ Kategori detay görüntüleme
- ✅ Kategori oluşturma (Admin)
- ✅ Kategori güncelleme (Admin)
- ✅ Kategori silme (Admin)
- ✅ Her kategorideki ürün sayısı

### Teknik Özellikler
- ✅ RESTful API standartları
- ✅ Express.js framework
- ✅ MySQL database entegrasyonu
- ✅ Request validation (express-validator)
- ✅ CORS desteği
- ✅ Logging (morgan)
- ✅ Error handling middleware
- ✅ Environment variables (.env)

## 📁 Proje Yapısı

```
ProductService/
├── controllers/
│   ├── productController.js    # Ürün işlemleri
│   └── categoryController.js   # Kategori işlemleri
├── routes/
│   └── productRoutes.js        # API route tanımları
├── db.js                       # MySQL connection pool
├── server.js                   # Express server
├── package.json
├── .env                        # Environment variables
├── test-client.js              # Test client
└── README.md
```

## 🚀 Kurulum

### 1. Bağımlılıkları Yükleyin

```bash
cd 3-Service-Layer/ProductService
npm install
```

### 2. Environment Variables

`.env` dosyasını oluşturun:

```env
PORT=3001
NODE_ENV=development

DB_HOST=localhost
DB_PORT=3306
DB_USER=root
DB_PASSWORD=your_password
DB_NAME=SmartShopDB

API_PREFIX=/api/v1
```

### 3. MySQL Veritabanı

MySQL sunucusunun çalıştığından ve `SmartShopDB` veritabanının hazır olduğundan emin olun.

### 4. Servisi Başlatın

```bash
# Production mode
npm start

# Development mode (nodemon)
npm run dev
```

Servis varsayılan olarak `http://localhost:3001` adresinde çalışacaktır.

## 📡 API Endpoints

### Base URL
```
http://localhost:3001/api/v1
```

### Kategori Endpoints

| Method | Endpoint | Açıklama | Auth |
|--------|----------|----------|------|
| GET | `/products/categories` | Tüm kategoriler | - |
| GET | `/products/categories/:id` | Kategori detay | - |
| POST | `/products/categories` | Yeni kategori | Admin |
| PUT | `/products/categories/:id` | Kategori güncelle | Admin |
| DELETE | `/products/categories/:id` | Kategori sil | Admin |

### Ürün Endpoints

| Method | Endpoint | Açıklama | Auth |
|--------|----------|----------|------|
| GET | `/products` | Tüm ürünler (filtreleme, pagination) | - |
| GET | `/products/:id` | Ürün detay | - |
| GET | `/products/:id/reviews` | Ürün yorumları | - |
| GET | `/products/featured/top-rated` | En iyi ürünler | - |
| GET | `/products/admin/low-stock` | Düşük stoklu ürünler | Admin |
| POST | `/products` | Yeni ürün | Admin |
| PUT | `/products/:id` | Ürün güncelle | Admin |
| PATCH | `/products/:id/stock` | Stok güncelle | Admin |
| DELETE | `/products/:id` | Ürün sil (soft) | Admin |

## 📝 API Kullanım Örnekleri

### 1. Tüm Kategorileri Getir

```bash
GET http://localhost:3001/api/v1/products/categories
```

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "CategoryID": 1,
      "Name": "Elektronik",
      "Description": "Elektronik ürünler",
      "ProductCount": 15
    }
  ],
  "count": 10
}
```

### 2. Ürünleri Filtrele ve Listele

```bash
GET http://localhost:3001/api/v1/products?page=1&limit=10&categoryId=1&minPrice=100&maxPrice=5000&search=laptop&sortBy=price&order=ASC&inStock=true
```

**Query Parameters:**
- `page`: Sayfa numarası (default: 1)
- `limit`: Sayfa başına ürün (default: 10, max: 100)
- `categoryId`: Kategori ID
- `minPrice`: Minimum fiyat
- `maxPrice`: Maximum fiyat
- `inStock`: Stokta olan (true/false)
- `search`: Arama terimi (isim veya açıklama)
- `sortBy`: Sıralama (name, price, rating, createdAt)
- `order`: Sıralama yönü (ASC, DESC)

**Response:**
```json
{
  "success": true,
  "data": [
    {
      "ProductID": 1,
      "Name": "Laptop XYZ",
      "Description": "Yüksek performanslı laptop",
      "Price": 1299.99,
      "StockQuantity": 50,
      "CategoryID": 1,
      "CategoryName": "Elektronik",
      "ImageURL": "https://...",
      "SKU": "LAP-001",
      "Rating": 4.5
    }
  ],
  "pagination": {
    "page": 1,
    "limit": 10,
    "total": 45,
    "totalPages": 5
  }
}
```

### 3. Ürün Detayı

```bash
GET http://localhost:3001/api/v1/products/1
```

**Response:**
```json
{
  "success": true,
  "data": {
    "ProductID": 1,
    "Name": "Laptop XYZ",
    "Description": "Yüksek performanslı laptop",
    "Price": 1299.99,
    "StockQuantity": 50,
    "CategoryID": 1,
    "CategoryName": "Elektronik",
    "ImageURL": "https://...",
    "Rating": 4.5,
    "ReviewCount": 120,
    "AvgRating": 4.52
  }
}
```

### 4. Yeni Ürün Oluştur (Admin)

```bash
POST http://localhost:3001/api/v1/products
Content-Type: application/json

{
  "name": "Yeni Ürün",
  "description": "Ürün açıklaması",
  "price": 999.99,
  "stockQuantity": 100,
  "categoryId": 1,
  "imageUrl": "https://example.com/image.jpg",
  "sku": "PROD-001",
  "weight": 2.5
}
```

### 5. Ürün Stoğunu Güncelle

```bash
PATCH http://localhost:3001/api/v1/products/1/stock
Content-Type: application/json

{
  "quantity": -5
}
```

Pozitif değer stok ekler, negatif değer stok azaltır.

### 6. En İyi Ürünler

```bash
GET http://localhost:3001/api/v1/products/featured/top-rated?limit=10
```

### 7. Düşük Stoklu Ürünler (Admin)

```bash
GET http://localhost:3001/api/v1/products/admin/low-stock?threshold=10
```

### 8. Ürün Yorumları

```bash
GET http://localhost:3001/api/v1/products/1/reviews?page=1&limit=10
```

## 🧪 Test

Otomatik test client'ı çalıştırın:

```bash
node test-client.js
```

Test client şunları test eder:
- ✅ Kategori CRUD işlemleri
- ✅ Ürün CRUD işlemleri
- ✅ Filtreleme ve arama
- ✅ Pagination
- ✅ Stok güncelleme
- ✅ Validation hataları
- ✅ Top rated ürünler
- ✅ Low stock ürünler

## 🔧 Teknolojiler

- **Node.js** - Runtime environment
- **Express.js 4.18+** - Web framework
- **MySQL2** - Database driver
- **express-validator** - Request validation
- **cors** - CORS middleware
- **morgan** - HTTP request logger
- **dotenv** - Environment variables
- **nodemon** - Development auto-reload

## 📊 Database Schema

### Products Table
```sql
- ProductID (PK)
- Name
- Description
- Price
- StockQuantity
- CategoryID (FK)
- ImageURL
- SKU (UNIQUE)
- Weight
- Rating
- IsActive
- CreatedAt
- UpdatedAt
```

### Categories Table
```sql
- CategoryID (PK)
- Name (UNIQUE)
- Description
- CreatedAt
- UpdatedAt
```

## 🔐 Güvenlik Notları

- ⚠️ Şu an authentication/authorization implementasyonu yok
- ⚠️ Production'da Admin endpoint'leri için JWT token kontrolü eklenmelidir
- ⚠️ Rate limiting eklenmelidir
- ⚠️ Input sanitization geliştirilmelidir

## 🚦 Error Handling

Tüm endpoint'ler standart error response döner:

```json
{
  "success": false,
  "message": "Hata mesajı",
  "errors": [
    {
      "field": "price",
      "message": "Geçerli bir fiyat giriniz"
    }
  ]
}
```

HTTP Status Codes:
- `200`: Success
- `201`: Created
- `400`: Bad Request (Validation error)
- `404`: Not Found
- `500`: Internal Server Error

## 📈 Performans

- Connection pooling ile veritabanı bağlantı yönetimi
- Pagination ile büyük veri setlerinde performans optimizasyonu
- Index'ler ile hızlı arama

## 🔄 Versiyon Geçmişi

- **v1.0.0** (2024) - İlk kararlı sürüm
  - Tüm CRUD operasyonları
  - Filtreleme ve arama
  - Pagination
  - Validation

## 👥 Integration Layer

Bu servis, Integration Layer tarafından ASP.NET MVC uygulamasından çağrılacaktır:

```csharp
// C# client örneği
var response = await httpClient.GetAsync(
    "http://localhost:3001/api/v1/products?page=1&limit=10"
);
```

## 📞 Destek

Sorunlar için GitHub Issues kullanın.

---

**SmartShop Team** - SOA Microservices Architecture
