# Katman 4: Integration Layer (Entegrasyon Katmanı)

## 📋 Görev
Servisler arası iletişim ve dış API entegrasyonlarını yönetir.

## 🛠️ Teknolojiler
- C# (.NET)
- SOAP Client
- gRPC Client
- HTTP Client (REST)

## 📁 İçerik

### Service Clients
- **SoapClientService.cs** - Product Service (SOAP) ile iletişim
- **GrpcClientService.cs** - Order Service (gRPC) ile iletişim
- **RestClientService.cs** - User Service (REST) ile iletişim

### External API Clients
- **PaymentApiClient.cs** - Ödeme servisi entegrasyonu
- **CargoApiClient.cs** - Kargo takip API
- **SmsApiClient.cs** - SMS bildirimleri (opsiyonel)

### ML Integration
- **MLServiceClient.cs** - Python ML servisi ile iletişim

## 🎯 Sorumluluklar
- Mikroservislere istek gönderme
- Protokol dönüşümleri (SOAP/gRPC/REST)
- Servis orchestration
- Hata yönetimi ve retry mekanizmaları
- External API entegrasyonları

## ⚙️ Örnek Kullanım

```csharp
// SOAP Client Örneği
public class SoapClientService
{
    private readonly string _serviceUrl = "http://localhost:3001/product";
    
    public async Task<List<Product>> GetAllProductsAsync()
    {
        var client = new SoapClient(_serviceUrl);
        var response = await client.GetAllProductsAsync();
        return response.Products;
    }
}

// gRPC Client Örneği
public class GrpcClientService
{
    private readonly OrderService.OrderServiceClient _client;
    
    public async Task<Order> CreateOrderAsync(OrderRequest request)
    {
        return await _client.CreateOrderAsync(request);
    }
}

// REST Client Örneği
public class RestClientService
{
    private readonly HttpClient _httpClient;
    
    public async Task<User> GetUserAsync(int id)
    {
        var response = await _httpClient.GetAsync($"http://localhost:3003/api/users/{id}");
        return await response.Content.ReadAsAsync<User>();
    }
}
```

## 📦 Gerekli NuGet Paketleri
```xml
<PackageReference Include="Grpc.Net.Client" Version="2.59.0" />
<PackageReference Include="System.ServiceModel.Http" Version="6.0.0" />
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
```
