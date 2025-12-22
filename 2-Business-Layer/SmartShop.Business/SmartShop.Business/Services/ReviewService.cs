using Microsoft.Extensions.Logging;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Yorum (Review) işlemleri için servis implementasyonu
    /// </summary>
    public class ReviewService : IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IUnitOfWork unitOfWork, ILogger<ReviewService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ServiceResult<List<ReviewDto>>> GetProductReviewsAsync(int productId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetProductReviewsAsync(productId);

                var reviewDtos = reviews.Select(r => new ReviewDto
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    ProductName = r.Product?.Name ?? "",
                    UserId = r.UserId,
                    UserName = r.User?.FullName ?? "",
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                }).ToList();

                return ServiceResult<List<ReviewDto>>.SuccessResult(reviewDtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün yorumları getirilirken hata oluştu. ProductId: {ProductId}", productId);
                return ServiceResult<List<ReviewDto>>.FailureResult("Yorumlar yüklenirken bir hata oluştu.");
            }
        }

        public async Task<ServiceResult<ReviewDto>> GetByIdAsync(int reviewId)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

                if (review == null)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Yorum bulunamadı.");
                }

                var reviewDto = new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    ProductId = review.ProductId,
                    ProductName = review.Product?.Name ?? "",
                    UserId = review.UserId,
                    UserName = review.User?.FullName ?? "",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                return ServiceResult<ReviewDto>.SuccessResult(reviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum getirilirken hata oluştu. ReviewId: {ReviewId}", reviewId);
                return ServiceResult<ReviewDto>.FailureResult("Yorum yüklenirken bir hata oluştu.");
            }
        }

        public async Task<ServiceResult<ReviewDto>> CreateAsync(CreateReviewDto createReviewDto)
        {
            try
            {
                // Validasyon
                if (createReviewDto.Rating < 1 || createReviewDto.Rating > 5)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Puan 1 ile 5 arasında olmalıdır.");
                }

                // Kullanıcı daha önce yorum yaptı mı kontrol et
                var hasReviewed = await HasUserReviewedProductAsync(createReviewDto.UserId, createReviewDto.ProductId);
                if (hasReviewed.Data)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Bu ürüne daha önce yorum yaptınız.");
                }

                // Ürün var mı kontrol et
                var product = await _unitOfWork.Products.GetByIdAsync(createReviewDto.ProductId);
                if (product == null)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Ürün bulunamadı.");
                }

                // Kullanıcı var mı kontrol et
                var user = await _unitOfWork.Users.GetByIdAsync(createReviewDto.UserId);
                if (user == null)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Kullanıcı bulunamadı.");
                }

                var review = new Review
                {
                    ProductId = createReviewDto.ProductId,
                    UserId = createReviewDto.UserId,
                    Rating = createReviewDto.Rating,
                    Comment = createReviewDto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                await _unitOfWork.Reviews.AddAsync(review);
                await _unitOfWork.SaveChangesAsync();

                var reviewDto = new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    ProductId = review.ProductId,
                    ProductName = product.Name,
                    UserId = review.UserId,
                    UserName = user.FullName,
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                _logger.LogInformation("Yeni yorum oluşturuldu. ReviewId: {ReviewId}, ProductId: {ProductId}, UserId: {UserId}",
                    review.ReviewId, review.ProductId, review.UserId);

                return ServiceResult<ReviewDto>.SuccessResult(reviewDto, "Yorumunuz başarıyla eklendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum oluşturulurken hata oluştu. ProductId: {ProductId}, UserId: {UserId}",
                    createReviewDto.ProductId, createReviewDto.UserId);
                return ServiceResult<ReviewDto>.FailureResult("Yorum eklenirken bir hata oluştu.");
            }
        }

        public async Task<ServiceResult<ReviewDto>> UpdateAsync(int reviewId, CreateReviewDto updateReviewDto)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

                if (review == null)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Yorum bulunamadı.");
                }

                // Validasyon
                if (updateReviewDto.Rating < 1 || updateReviewDto.Rating > 5)
                {
                    return ServiceResult<ReviewDto>.FailureResult("Puan 1 ile 5 arasında olmalıdır.");
                }

                review.Rating = updateReviewDto.Rating;
                review.Comment = updateReviewDto.Comment;

                _unitOfWork.Reviews.Update(review);
                await _unitOfWork.SaveChangesAsync();

                var reviewDto = new ReviewDto
                {
                    ReviewId = review.ReviewId,
                    ProductId = review.ProductId,
                    ProductName = review.Product?.Name ?? "",
                    UserId = review.UserId,
                    UserName = review.User?.FullName ?? "",
                    Rating = review.Rating,
                    Comment = review.Comment,
                    CreatedAt = review.CreatedAt
                };

                _logger.LogInformation("Yorum güncellendi. ReviewId: {ReviewId}", reviewId);

                return ServiceResult<ReviewDto>.SuccessResult(reviewDto, "Yorumunuz başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum güncellenirken hata oluştu. ReviewId: {ReviewId}", reviewId);
                return ServiceResult<ReviewDto>.FailureResult("Yorum güncellenirken bir hata oluştu.");
            }
        }

        public async Task<ServiceResult<bool>> DeleteAsync(int reviewId)
        {
            try
            {
                var review = await _unitOfWork.Reviews.GetByIdAsync(reviewId);

                if (review == null)
                {
                    return ServiceResult<bool>.FailureResult("Yorum bulunamadı.");
                }

                // Soft delete
                review.IsDeleted = true;
                _unitOfWork.Reviews.Update(review);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Yorum silindi. ReviewId: {ReviewId}", reviewId);

                return ServiceResult<bool>.SuccessResult(true, "Yorumunuz başarıyla silindi.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum silinirken hata oluştu. ReviewId: {ReviewId}", reviewId);
                return ServiceResult<bool>.FailureResult("Yorum silinirken bir hata oluştu.");
            }
        }

        public async Task<ServiceResult<double>> GetAverageRatingAsync(int productId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetProductReviewsAsync(productId);

                if (!reviews.Any())
                {
                    return ServiceResult<double>.SuccessResult(0);
                }

                var averageRating = reviews.Average(r => r.Rating);
                return ServiceResult<double>.SuccessResult(Math.Round(averageRating, 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ortalama puan hesaplanırken hata oluştu. ProductId: {ProductId}", productId);
                return ServiceResult<double>.FailureResult("Ortalama puan hesaplanamadı.");
            }
        }

        public async Task<ServiceResult<bool>> HasUserReviewedProductAsync(int userId, int productId)
        {
            try
            {
                var reviews = await _unitOfWork.Reviews.GetProductReviewsAsync(productId);
                var hasReviewed = reviews.Any(r => r.UserId == userId && !r.IsDeleted);
                return ServiceResult<bool>.SuccessResult(hasReviewed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcının yorum kontrolü yapılırken hata oluştu. UserId: {UserId}, ProductId: {ProductId}",
                    userId, productId);
                return ServiceResult<bool>.FailureResult("Yorum kontrolü yapılamadı.");
            }
        }
    }
}
