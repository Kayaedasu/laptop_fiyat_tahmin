# UserService - gRPC Mikroservis

## 📋 Açıklama
SmartShop platformu için kullanıcı yönetimi gRPC mikroservisi.

## 🚀 Kurulum

```bash
# Bağımlılıkları yükle
npm install

# Sunucuyu başlat
npm start

# Development mode (nodemon)
npm run dev
```

## 🔌 Port
- **gRPC Port:** 50051

## 📡 gRPC Metotları

### 1. RegisterUser
Yeni kullanıcı kaydı oluşturur.

### 2. LoginUser
Kullanıcı girişi yapar ve JWT token döner.

### 3. GetUser
Kullanıcı bilgilerini getirir.

### 4. UpdateUser
Kullanıcı bilgilerini günceller.

### 5. DeleteUser
Kullanıcıyı soft delete yapar.

### 6. ListUsers
Tüm kullanıcıları listeler (sayfalama ile).

## 🔐 Güvenlik
- Şifreler bcrypt ile hash'leniyor
- JWT token authentication
- Input validation

## 🗄️ Veritabanı
MySQL (SmartShopDB)

## ⚙️ Environment Variables
`.env` dosyasını kontrol edin.
