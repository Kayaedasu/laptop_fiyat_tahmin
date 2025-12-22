using System.Net.Http.Json;
using Newtonsoft.Json;

namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// REST API client for ProductService
    /// Handles all product-related operations (CRUD, Search, Filter, Categories)
    /// </summary>
    public class ProductServiceClient : IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        /// <summary>
        /// Initialize ProductService REST client
        /// </summary>
        /// <param name="baseUrl">ProductService base URL (default: http://localhost:3001/api/v1)</param>
        public ProductServiceClient(string baseUrl = "http://localhost:3001/api/v1")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        /// <summary>
        /// Constructor with custom HttpClient (for dependency injection)
        /// </summary>
        public ProductServiceClient(HttpClient httpClient, string baseUrl = "http://localhost:3001/api/v1")
        {
            _httpClient = httpClient;
            _baseUrl = baseUrl;
            _httpClient.BaseAddress = new Uri(_baseUrl);
        }

        #region Product Operations

        /// <summary>
        /// Get all products with pagination
        /// </summary>
        public async Task<ProductListResponse> GetAllProductsAsync(int page = 1, int limit = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/products?page={page}&limit={limit}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductListResponse>(content);
                return result ?? new ProductListResponse { Success = false, Message = "Failed to parse response" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting products: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get product by ID
        /// </summary>
        public async Task<ProductResponse> GetProductByIdAsync(int productId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/products/{productId}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductResponse>(content);
                return result ?? new ProductResponse { Success = false, Message = "Product not found" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Create new product
        /// </summary>
        public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/products", request);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductResponse>(content);
                return result ?? new ProductResponse { Success = false, Message = "Failed to create product" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update existing product
        /// </summary>
        public async Task<ProductResponse> UpdateProductAsync(int productId, UpdateProductRequest request)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/products/{productId}", request);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductResponse>(content);
                return result ?? new ProductResponse { Success = false, Message = "Failed to update product" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete product (soft delete)
        /// </summary>
        public async Task<DeleteProductResponse> DeleteProductAsync(int productId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/products/{productId}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<DeleteProductResponse>(content);
                return result ?? new DeleteProductResponse { Success = false, Message = "Failed to delete product" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting product: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Search products by name or brand
        /// </summary>
        public async Task<ProductListResponse> SearchProductsAsync(string query, int page = 1, int limit = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/products/search?q={Uri.EscapeDataString(query)}&page={page}&limit={limit}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductListResponse>(content);
                return result ?? new ProductListResponse { Success = false, Message = "Search failed" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error searching products: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Filter products by various criteria
        /// </summary>
        public async Task<ProductListResponse> FilterProductsAsync(
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? inStock = null,
            int page = 1,
            int limit = 10)
        {
            try
            {
                var queryParams = new List<string>();
                
                if (categoryId.HasValue) queryParams.Add($"categoryId={categoryId.Value}");
                if (minPrice.HasValue) queryParams.Add($"minPrice={minPrice.Value}");
                if (maxPrice.HasValue) queryParams.Add($"maxPrice={maxPrice.Value}");
                if (inStock.HasValue) queryParams.Add($"inStock={inStock.Value}");
                queryParams.Add($"page={page}");
                queryParams.Add($"limit={limit}");
                
                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/products/filter?{queryString}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductListResponse>(content);
                return result ?? new ProductListResponse { Success = false, Message = "Filter failed" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error filtering products: {ex.Message}", ex);
            }
        }

        #endregion

        #region Category Operations

        /// <summary>
        /// Get all categories
        /// </summary>
        public async Task<CategoryListResponse> GetAllCategoriesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/categories");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<CategoryListResponse>(content);
                return result ?? new CategoryListResponse { Success = false, Message = "Failed to get categories" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting categories: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get products by category
        /// </summary>
        public async Task<ProductListResponse> GetProductsByCategoryAsync(int categoryId, int page = 1, int limit = 10)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/categories/{categoryId}/products?page={page}&limit={limit}");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<ProductListResponse>(content);
                return result ?? new ProductListResponse { Success = false, Message = "Failed to get products" };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting products by category: {ex.Message}", ex);
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Check if ProductService is available
        /// </summary>
        public async Task<bool> IsServiceAvailableAsync()
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

        #endregion

        #region IDisposable

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #endregion
    }

    #region DTOs (Data Transfer Objects)

    // Product DTOs
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Processor { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string? StorageType { get; set; }
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal Discount { get; set; }
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ProductCondition { get; set; }
        public bool IsActive { get; set; }
        public int ViewCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // Request DTOs
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? Model { get; set; }
        public string? Processor { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string StorageType { get; set; } = "SSD";
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public decimal Discount { get; set; } = 0;
        public int CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string ProductCondition { get; set; } = "New";
    }

    public class UpdateProductRequest
    {
        public string? Name { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Processor { get; set; }
        public int? RAM { get; set; }
        public int? Storage { get; set; }
        public string? StorageType { get; set; }
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public decimal? Discount { get; set; }
        public int? CategoryId { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? ProductCondition { get; set; }
        public bool? IsActive { get; set; }
    }

    // Response DTOs
    public class ProductResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Product? Product { get; set; }
    }

    public class ProductListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Product> Products { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalPages { get; set; }
    }

    public class DeleteProductResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CategoryListResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<Category> Categories { get; set; } = new();
    }

    #endregion
}
