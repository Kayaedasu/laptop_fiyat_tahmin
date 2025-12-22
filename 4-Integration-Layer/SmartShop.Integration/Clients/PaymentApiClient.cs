namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// Ödeme işlemleri için DTO
    /// </summary>
    public class PaymentRequestDto
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "TRY";
        public string CardNumber { get; set; } = "";
        public string CardHolderName { get; set; } = "";
        public string ExpiryMonth { get; set; } = "";
        public string ExpiryYear { get; set; } = "";
        public string CVV { get; set; } = "";
    }

    public class PaymentResponseDto
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime ProcessedAt { get; set; }
    }

    /// <summary>
    /// Harici ödeme API'si ile iletişim client'ı (Simüle edilmiş)
    /// </summary>
    public class PaymentApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public PaymentApiClient(string apiUrl = "https://api.payment-gateway.com", string apiKey = "test-api-key")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(apiUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            _apiKey = apiKey;
        }

        /// <summary>
        /// Ödeme işlemini gerçekleştirir
        /// </summary>
        public async Task<PaymentResponseDto> ProcessPaymentAsync(PaymentRequestDto request)
        {
            try
            {
                // GERÇEK ENTEGRASYONDA: Gerçek ödeme gateway API'sine istek at
                // var response = await _httpClient.PostAsJsonAsync("/v1/payments", request);
                // response.EnsureSuccessStatusCode();
                // return await response.Content.ReadFromJsonAsync<PaymentResponseDto>();

                // SİMÜLASYON: Test için otomatik onay
                await Task.Delay(500); // Simulated API latency

                // Basit validasyon
                if (string.IsNullOrWhiteSpace(request.CardNumber) || request.CardNumber.Length < 16)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Geçersiz kart numarası",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                if (request.Amount <= 0)
                {
                    return new PaymentResponseDto
                    {
                        Success = false,
                        Message = "Geçersiz tutar",
                        ProcessedAt = DateTime.UtcNow
                    };
                }

                // Simüle edilmiş başarılı ödeme
                return new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = $"TXN-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                    Message = "Ödeme başarıyla gerçekleştirildi",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Ödeme işlemi sırasında hata: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Ödeme durumunu sorgular
        /// </summary>
        public async Task<PaymentResponseDto> CheckPaymentStatusAsync(string transactionId)
        {
            try
            {
                await Task.Delay(200);

                return new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = transactionId,
                    Message = "İşlem tamamlandı",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"Durum sorgulaması sırasında hata: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }

        /// <summary>
        /// Ödeme iadesi yapar
        /// </summary>
        public async Task<PaymentResponseDto> RefundPaymentAsync(string transactionId, decimal amount)
        {
            try
            {
                await Task.Delay(500);

                return new PaymentResponseDto
                {
                    Success = true,
                    TransactionId = $"REFUND-{transactionId}",
                    Message = $"{amount} TL iade işlemi başarılı",
                    ProcessedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                return new PaymentResponseDto
                {
                    Success = false,
                    Message = $"İade işlemi sırasında hata: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
    }
}
