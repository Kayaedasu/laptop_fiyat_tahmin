using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Yorum (Review) işlemleri için servis interface'i
    /// </summary>
    public interface IReviewService
    {
        /// <summary>
        /// Belirli bir ürünün tüm yorumlarını getirir
        /// </summary>
        Task<ServiceResult<List<ReviewDto>>> GetProductReviewsAsync(int productId);

        /// <summary>
        /// Belirli bir yorumun detaylarını getirir
        /// </summary>
        Task<ServiceResult<ReviewDto>> GetByIdAsync(int reviewId);

        /// <summary>
        /// Yeni bir yorum oluşturur
        /// </summary>
        Task<ServiceResult<ReviewDto>> CreateAsync(CreateReviewDto createReviewDto);

        /// <summary>
        /// Bir yorumu günceller
        /// </summary>
        Task<ServiceResult<ReviewDto>> UpdateAsync(int reviewId, CreateReviewDto updateReviewDto);

        /// <summary>
        /// Bir yorumu siler (soft delete)
        /// </summary>
        Task<ServiceResult<bool>> DeleteAsync(int reviewId);

        /// <summary>
        /// Bir ürünün ortalama puanını hesaplar
        /// </summary>
        Task<ServiceResult<double>> GetAverageRatingAsync(int productId);

        /// <summary>
        /// Kullanıcının belirli bir ürün için yorum yapıp yapmadığını kontrol eder
        /// </summary>
        Task<ServiceResult<bool>> HasUserReviewedProductAsync(int userId, int productId);
    }
}
