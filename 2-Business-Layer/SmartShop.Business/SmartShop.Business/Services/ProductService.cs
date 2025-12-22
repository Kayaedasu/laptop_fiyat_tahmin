using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.Business.Validators;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Ürün iş mantığı servisi
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<ProductDto>> GetByIdAsync(int productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetProductWithDetailsAsync(productId);
                
                if (product == null)
                    return ServiceResult<ProductDto>.FailureResult("Ürün bulunamadı.");

                var dto = MapToDto(product);
                return ServiceResult<ProductDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "Ürün getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<ProductDetailDto>> GetDetailsByIdAsync(int productId)
        {
            try
            {
                _logger.LogInformation($"GetDetailsByIdAsync called for ProductId: {productId}");

                var product = await _unitOfWork.Products.GetAllQuery()
                    .Include(p => p.Category)
                    .Include(p => p.Reviews)
                        .ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(p => p.ProductId == productId);
                
                if (product == null)
                {
                    _logger.LogWarning($"Product not found: {productId}");
                    return ServiceResult<ProductDetailDto>.FailureResult("Ürün bulunamadı.");
                }

                // Filter out deleted reviews
                var activeReviews = product.Reviews?.Where(r => !r.IsDeleted).ToList() ?? new List<Review>();

                var dto = new ProductDetailDto
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Description = product.Description,
                    Price = product.Price,
                    StockQuantity = product.Stock,
                    CategoryId = product.CategoryId,
                    CategoryName = product.Category?.Name ?? string.Empty,
                    Brand = product.Brand,
                    Model = product.Model,
                    Specifications = BuildSpecifications(product),
                    ImageUrl = product.ImageUrl,
                    AverageRating = activeReviews.Any() ? 
                        activeReviews.Average(r => r.Rating) : null,
                    ReviewCount = activeReviews.Count,
                    Reviews = activeReviews.Select(r => new ReviewDto
                    {
                        ReviewId = r.ReviewId,
                        ProductId = r.ProductId,
                        UserId = r.UserId,
                        UserName = $"{r.User?.FirstName} {r.User?.LastName}".Trim(),
                        Rating = r.Rating,
                        Comment = r.Comment ?? string.Empty,
                        CreatedAt = r.CreatedAt
                    }).OrderByDescending(r => r.CreatedAt).ToList()
                };

                _logger.LogInformation($"Product details retrieved: {dto.Name}, Reviews: {dto.Reviews.Count}");
                return ServiceResult<ProductDetailDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in GetDetailsByIdAsync for ProductId: {productId}");
                return ServiceResult<ProductDetailDto>.FailureResult(
                    "Ürün detayı getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        private string? BuildSpecifications(Product product)
        {
            var specs = new List<string>();
            
            if (!string.IsNullOrEmpty(product.Processor))
                specs.Add($"Processor: {product.Processor}");
            
            if (product.RAM > 0)
                specs.Add($"RAM: {product.RAM} GB");
            
            if (product.Storage > 0)
                specs.Add($"Storage: {product.Storage} GB {product.StorageType}");
            
            if (!string.IsNullOrEmpty(product.GPU))
                specs.Add($"GPU: {product.GPU}");
            
            if (product.ScreenSize.HasValue)
                specs.Add($"Screen: {product.ScreenSize}\" {product.Resolution}");
            
            if (!string.IsNullOrEmpty(product.Condition))
                specs.Add($"Condition: {product.Condition}");

            return specs.Any() ? string.Join("\n", specs) : null;
        }

        public async Task<ServiceResult<List<ProductDto>>> GetAllAsync()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllProductsWithDetailsAsync();
                var dtos = products.Select(MapToDto).ToList();
                
                return ServiceResult<List<ProductDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "Ürünler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> GetByCategoryAsync(int categoryId)
        {
            try
            {
                var products = await _unitOfWork.Products.GetProductsByCategoryAsync(categoryId);
                var dtos = products.Select(MapToDto).ToList();
                
                return ServiceResult<List<ProductDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "Kategoriye ait ürünler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> SearchAsync(ProductFilterDto filter)
        {
            try
            {
                _logger.LogInformation($"SearchAsync called with filter: CategoryId={filter.CategoryId}, SearchTerm={filter.SearchTerm}, IsActive={filter.IsActive}, PageNumber={filter.PageNumber}, PageSize={filter.PageSize}");

                var validationErrors = ProductValidator.ValidateFilter(filter);
                if (validationErrors.Any())
                {
                    _logger.LogWarning($"Validation errors: {string.Join(", ", validationErrors)}");
                    return ServiceResult<List<ProductDto>>.FailureResult(
                        "Filtreleme parametreleri geçersiz.", 
                        validationErrors);
                }

                var query = _unitOfWork.Products.GetAllQuery()
                    .Include(p => p.Category)
                    .AsQueryable();

                _logger.LogInformation($"Initial query created, before filters");

                // Filtreleme
                if (filter.CategoryId.HasValue)
                {
                    query = query.Where(p => p.CategoryId == filter.CategoryId.Value);
                    _logger.LogInformation($"Applied CategoryId filter: {filter.CategoryId.Value}");
                }

                if (filter.MinPrice.HasValue)
                    query = query.Where(p => p.Price >= filter.MinPrice.Value);

                if (filter.MaxPrice.HasValue)
                    query = query.Where(p => p.Price <= filter.MaxPrice.Value);

                if (!string.IsNullOrWhiteSpace(filter.Brand))
                    query = query.Where(p => p.Brand == filter.Brand);

                if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
                {
                    var searchLower = filter.SearchTerm.ToLower();
                    query = query.Where(p => 
                        p.Name.ToLower().Contains(searchLower) ||
                        (p.Description != null && p.Description.ToLower().Contains(searchLower)) ||
                        (p.Brand != null && p.Brand.ToLower().Contains(searchLower)));
                    _logger.LogInformation($"Applied search term filter: {filter.SearchTerm}");
                }

                if (filter.IsActive.HasValue)
                {
                    query = query.Where(p => p.IsActive == filter.IsActive.Value);
                    _logger.LogInformation($"Applied IsActive filter: {filter.IsActive.Value}");
                }

                // Sayıyı al (sayfalamadan önce)
                var totalCount = await query.CountAsync();
                _logger.LogInformation($"Total products matching filter: {totalCount}");

                // Sıralama
                query = filter.SortBy?.ToLower() switch
                {
                    "price" => filter.IsDescending ? 
                        query.OrderByDescending(p => p.Price) : 
                        query.OrderBy(p => p.Price),
                    "createdat" => filter.IsDescending ? 
                        query.OrderByDescending(p => p.CreatedAt) : 
                        query.OrderBy(p => p.CreatedAt),
                    _ => filter.IsDescending ? 
                        query.OrderByDescending(p => p.Name) : 
                        query.OrderBy(p => p.Name)
                };

                // Sayfalama
                var products = await query
                    .Skip((filter.PageNumber - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {products.Count} products from database");

                var dtos = products.Select(MapToDto).ToList();
                _logger.LogInformation($"Mapped {dtos.Count} DTOs");

                return ServiceResult<List<ProductDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in SearchAsync - Message: {ex.Message}, StackTrace: {ex.StackTrace}");
                _logger.LogError($"Inner Exception: {ex.InnerException?.Message}");
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "Ürün araması yapılırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto)
        {
            try
            {
                var validationErrors = ProductValidator.ValidateCreate(dto);
                if (validationErrors.Any())
                    return ServiceResult<ProductDto>.FailureResult(
                        "Ürün oluşturma verileri geçersiz.", 
                        validationErrors);

                // Kategori kontrolü
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                    return ServiceResult<ProductDto>.FailureResult("Belirtilen kategori bulunamadı.");

                var product = new Product
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Price = dto.Price,
                    Stock = dto.StockQuantity,
                    CategoryId = dto.CategoryId,
                    Brand = dto.Brand ?? string.Empty,
                    Model = dto.Model,
                    ImageUrl = dto.ImageUrl,
                    IsActive = dto.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    // Teknik Özellikler
                    Processor = dto.Processor,
                    RAM = dto.RAM,
                    Storage = dto.Storage,
                    StorageType = dto.StorageType ?? "SSD",
                    GPU = dto.GPU,
                    ScreenSize = dto.ScreenSize,
                    Resolution = dto.Resolution,
                    Condition = dto.Condition ?? "New",
                    Discount = dto.Discount
                };

                await _unitOfWork.Products.AddAsync(product);
                await _unitOfWork.CommitAsync();

                product.Category = category;
                var resultDto = MapToDto(product);
                
                return ServiceResult<ProductDto>.SuccessResult(resultDto, "Ürün başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "Ürün oluşturulurken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<ProductDto>> UpdateAsync(UpdateProductDto dto)
        {
            try
            {
                var validationErrors = ProductValidator.ValidateUpdate(dto);
                if (validationErrors.Any())
                    return ServiceResult<ProductDto>.FailureResult(
                        "Ürün güncelleme verileri geçersiz.", 
                        validationErrors);

                var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                if (product == null)
                    return ServiceResult<ProductDto>.FailureResult("Güncellenecek ürün bulunamadı.");

                // Kategori kontrolü
                var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId);
                if (category == null)
                    return ServiceResult<ProductDto>.FailureResult("Belirtilen kategori bulunamadı.");

                product.Name = dto.Name;
                product.Description = dto.Description;
                product.Price = dto.Price;
                product.Stock = dto.StockQuantity;
                product.CategoryId = dto.CategoryId;
                product.Brand = dto.Brand ?? string.Empty;
                product.Model = dto.Model;
                product.ImageUrl = dto.ImageUrl;
                product.IsActive = dto.IsActive;
                
                // Teknik Özellikler
                product.Processor = dto.Processor;
                product.RAM = dto.RAM;
                product.Storage = dto.Storage;
                product.StorageType = dto.StorageType ?? "SSD";
                product.GPU = dto.GPU;
                product.ScreenSize = dto.ScreenSize;
                product.Resolution = dto.Resolution;
                product.Condition = dto.Condition ?? "New";
                product.Discount = dto.Discount;

                _unitOfWork.Products.Update(product);
                await _unitOfWork.CommitAsync();

                product.Category = category;
                var resultDto = MapToDto(product);
                
                return ServiceResult<ProductDto>.SuccessResult(resultDto, "Ürün başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "Ürün güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> DeleteAsync(int productId)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                    return ServiceResult.FailureResult("Silinecek ürün bulunamadı.");

                // Soft delete
                product.IsActive = false;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Ürün başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Ürün silinirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> UpdateStockAsync(int productId, int quantity)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                    return ServiceResult.FailureResult("Ürün bulunamadı.");

                if (product.Stock + quantity < 0)
                    return ServiceResult.FailureResult("Yetersiz stok.");

                product.Stock += quantity;
                _unitOfWork.Products.Update(product);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Stok başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Stok güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> GetTopRatedAsync(int count = 10)
        {
            try
            {
                var products = await _unitOfWork.Products.GetTopRatedProductsAsync(count);
                var dtos = products.Select(MapToDto).ToList();
                
                return ServiceResult<List<ProductDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "En yüksek puanlı ürünler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> GetLowStockAsync(int threshold = 10)
        {
            try
            {
                var products = await _unitOfWork.Products.GetLowStockProductsAsync(threshold);
                var dtos = products.Select(MapToDto).ToList();
                
                return ServiceResult<List<ProductDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "Düşük stoklu ürünler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> IsProductAvailableAsync(int productId, int quantity)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                
                if (product == null)
                    return ServiceResult<bool>.FailureResult("Ürün bulunamadı.");

                var isAvailable = product.IsActive && product.Stock >= quantity;
                return ServiceResult<bool>.SuccessResult(isAvailable);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult(
                    "Ürün müsaitlik kontrolü yapılırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        private ProductDto MapToDto(Product product)
        {
            return new ProductDto
            {
                ProductId = product.ProductId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.Stock,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                Brand = product.Brand,
                Model = product.Model,
                
                // Teknik Özellikler
                Processor = product.Processor,
                RAM = product.RAM,
                Storage = product.Storage,
                StorageType = product.StorageType,
                GPU = product.GPU,
                ScreenSize = product.ScreenSize,
                Resolution = product.Resolution,
                Condition = product.Condition,
                Discount = product.Discount,
                
                Specifications = null, // Product modelinde Specifications yok
                ImageUrl = product.ImageUrl,
                IsActive = product.IsActive,
                AverageRating = product.Reviews?.Any() == true ? 
                    product.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = product.Reviews?.Count ?? 0,
                CreatedAt = product.CreatedAt
            };
        }
    }
}
