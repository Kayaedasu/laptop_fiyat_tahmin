using System.Net.Http.Json;

namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// ML Service DTOs
    /// </summary>
    public class RecommendationRequestDto
    {
        public int UserId { get; set; }
        public int Limit { get; set; } = 10;
    }

    public class RecommendationDto
    {
        public int ProductId { get; set; }
        public double Score { get; set; }
    }

    public class RecommendationResponseDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public List<RecommendationDto> Recommendations { get; set; } = new();
        public string Timestamp { get; set; } = "";
    }

    public class PriceRequestDto
    {
        public Dictionary<string, object> Features { get; set; } = new();
    }

    public class PriceResponseDto
    {
        public bool Success { get; set; }
        public decimal PredictedPrice { get; set; }
        public string Currency { get; set; } = "TRY";
        public string Timestamp { get; set; } = "";
    }

    public class FraudRequestDto
    {
        public Dictionary<string, object> TransactionData { get; set; } = new();
    }

    public class FraudResponseDto
    {
        public bool Success { get; set; }
        public bool IsFraud { get; set; }
        public double RiskScore { get; set; }
        public double Confidence { get; set; }
        public string Timestamp { get; set; } = "";
    }

    public class SegmentRequestDto
    {
        public Dictionary<string, object> CustomerData { get; set; } = new();
    }

    public class SegmentResponseDto
    {
        public bool Success { get; set; }
        public string Segment { get; set; } = "";
        public double Confidence { get; set; }
        public string Timestamp { get; set; } = "";
    }

    public class SimilarProductsRequestDto
    {
        public int ProductId { get; set; }
        public int Limit { get; set; } = 5;
    }

    public class SimilarProductDto
    {
        public int ProductId { get; set; }
        public double SimilarityScore { get; set; }
    }

    public class SimilarProductsResponseDto
    {
        public bool Success { get; set; }
        public int ProductId { get; set; }
        public List<SimilarProductDto> SimilarProducts { get; set; } = new();
        public string Timestamp { get; set; } = "";
    }

    /// <summary>
    /// ML Service client - Python/Flask ML mikroservisi ile iletişim kurar
    /// </summary>
    public class MLServiceClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public MLServiceClient(string serviceUrl = "http://localhost:5000")
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(serviceUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Kullanıcı için ürün önerileri alır
        /// </summary>
        public async Task<RecommendationResponseDto?> GetRecommendationsAsync(int userId, int limit = 10)
        {
            try
            {
                var request = new RecommendationRequestDto
                {
                    UserId = userId,
                    Limit = limit
                };

                var response = await _httpClient.PostAsJsonAsync("/api/ml/recommendations", new { user_id = userId, limit = limit });
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<RecommendationResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Service - Recommendations error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Ürün fiyat tahmini alır
        /// </summary>
        public async Task<PriceResponseDto?> PredictPriceAsync(Dictionary<string, object> productFeatures)
        {
            try
            {
                var request = new { features = productFeatures };

                var response = await _httpClient.PostAsJsonAsync("/api/ml/predict-price", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<PriceResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Service - Price prediction error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Dolandırıcılık tespiti yapar
        /// </summary>
        public async Task<FraudResponseDto?> DetectFraudAsync(Dictionary<string, object> transactionData)
        {
            try
            {
                var request = new { transaction_data = transactionData };

                var response = await _httpClient.PostAsJsonAsync("/api/ml/detect-fraud", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<FraudResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Service - Fraud detection error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Müşteri segmentasyonu yapar
        /// </summary>
        public async Task<SegmentResponseDto?> SegmentCustomerAsync(Dictionary<string, object> customerData)
        {
            try
            {
                var request = new { customer_data = customerData };

                var response = await _httpClient.PostAsJsonAsync("/api/ml/segment-customer", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SegmentResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Service - Customer segmentation error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Benzer ürünleri bulur
        /// </summary>
        public async Task<SimilarProductsResponseDto?> FindSimilarProductsAsync(int productId, int limit = 5)
        {
            try
            {
                var request = new { product_id = productId, limit = limit };

                var response = await _httpClient.PostAsJsonAsync("/api/ml/similar-products", request);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<SimilarProductsResponseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ML Service - Similar products error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// ML Service health check
        /// </summary>
        public async Task<bool> HealthCheckAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/health");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
