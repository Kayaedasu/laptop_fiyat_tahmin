namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// Kargo işlemleri için DTO
    /// </summary>
    public class ShippingRequestDto
    {
        public int OrderId { get; set; }
        public string RecipientName { get; set; } = "";
        public string RecipientPhone { get; set; } = "";
        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string District { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public decimal PackageWeight { get; set; }
        public string PackageSize { get; set; } = "Medium"; // Small, Medium, Large
    }

    public class ShippingResponseDto
    {
        public bool Success { get; set; }
        public string TrackingNumber { get; set; } = "";
        public string CargoCompany { get; set; } = "";
        public string Message { get; set; } = "";
        public decimal ShippingCost { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class TrackingInfoDto
    {
        public string TrackingNumber { get; set; } = "";
        public string Status { get; set; } = ""; // Preparing, InTransit, OutForDelivery, Delivered
        public string CurrentLocation { get; set; } = "";
        public List<TrackingEventDto> Events { get; set; } = new();
        public DateTime? DeliveredAt { get; set; }
    }

    public class TrackingEventDto
    {
        public DateTime EventDate { get; set; }
        public string Status { get; set; } = "";
        public string Location { get; set; } = "";
        public string Description { get; set; } = "";
    }

    /// <summary>
    /// Harici kargo API'si ile iletişim client'ı (Simüle edilmiş)
    /// </summary>
    public class CargoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public CargoApiClient(string apiUrl = "https://api.cargo-company.com", string apiKey = "test-cargo-key")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _apiKey = apiKey;
        }

        /// <summary>
        /// Kargo gönderimi oluşturur
        /// </summary>
        public async Task<ShippingResponseDto> CreateShipmentAsync(ShippingRequestDto request)
        {
            try
            {
                // GERÇEK ENTEGRASYONDA: Gerçek kargo API'sine istek at
                // var response = await _httpClient.PostAsJsonAsync("/v1/shipments", request);
                // response.EnsureSuccessStatusCode();
                // return await response.Content.ReadFromJsonAsync<ShippingResponseDto>();

                // SİMÜLASYON: Test için otomatik oluşturma
                await Task.Delay(600);

                // Basit validasyon
                if (string.IsNullOrWhiteSpace(request.Address))
                {
                    return new ShippingResponseDto
                    {
                        Success = false,
                        Message = "Geçersiz adres bilgisi",
                        CreatedAt = DateTime.UtcNow
                    };
                }

                // Kargo ücreti hesaplama (basit simülasyon)
                decimal shippingCost = request.PackageSize switch
                {
                    "Small" => 15.00m,
                    "Medium" => 25.00m,
                    "Large" => 40.00m,
                    _ => 25.00m
                };

                shippingCost += (request.PackageWeight * 2); // Kg başına 2 TL

                return new ShippingResponseDto
                {
                    Success = true,
                    TrackingNumber = $"TRK{Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper()}",
                    CargoCompany = "Express Cargo",
                    Message = "Kargo gönderimi oluşturuldu",
                    ShippingCost = shippingCost,
                    EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new ShippingResponseDto
                {
                    Success = false,
                    Message = $"Kargo oluşturma sırasında hata: {ex.Message}",
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Kargo takip bilgisini getirir
        /// </summary>
        public async Task<TrackingInfoDto> TrackShipmentAsync(string trackingNumber)
        {
            try
            {
                await Task.Delay(300);

                // Simüle edilmiş takip bilgisi
                var events = new List<TrackingEventDto>
                {
                    new() {
                        EventDate = DateTime.UtcNow.AddDays(-2),
                        Status = "Preparing",
                        Location = "İstanbul Deposu",
                        Description = "Kargo hazırlanıyor"
                    },
                    new() {
                        EventDate = DateTime.UtcNow.AddDays(-1),
                        Status = "InTransit",
                        Location = "Ankara Transfer Merkezi",
                        Description = "Kargo yolda"
                    },
                    new() {
                        EventDate = DateTime.UtcNow.AddHours(-6),
                        Status = "OutForDelivery",
                        Location = "İzmir Dağıtım Merkezi",
                        Description = "Kargo dağıtımda"
                    }
                };

                return new TrackingInfoDto
                {
                    TrackingNumber = trackingNumber,
                    Status = "OutForDelivery",
                    CurrentLocation = "İzmir Dağıtım Merkezi",
                    Events = events
                };
            }
            catch (Exception ex)
            {
                return new TrackingInfoDto
                {
                    TrackingNumber = trackingNumber,
                    Status = "Error",
                    CurrentLocation = $"Hata: {ex.Message}",
                    Events = new List<TrackingEventDto>()
                };
            }
        }

        /// <summary>
        /// Kargo teslimat durumunu günceller
        /// </summary>
        public async Task<bool> UpdateDeliveryStatusAsync(string trackingNumber, string status, string location)
        {
            try
            {
                await Task.Delay(200);
                return true; // Simüle edilmiş başarılı güncelleme
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Kargo iptali yapar
        /// </summary>
        public async Task<bool> CancelShipmentAsync(string trackingNumber, string reason)
        {
            try
            {
                await Task.Delay(400);
                return true; // Simüle edilmiş başarılı iptal
            }
            catch
            {
                return false;
            }
        }
    }
}
