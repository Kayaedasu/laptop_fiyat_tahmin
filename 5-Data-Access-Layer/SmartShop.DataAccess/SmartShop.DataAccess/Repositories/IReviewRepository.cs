using SmartShop.DataAccess.Models;

namespace SmartShop.DataAccess.Repositories
{
    /// <summary>
    /// Review (Yorum) işlemleri için özel repository interface'i
    /// </summary>
    public interface IReviewRepository : IRepository<Review>
    {
        /// <summary>
        /// Belirli bir ürünün tüm yorumlarını getirir
        /// </summary>
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId);

        /// <summary>
        /// Kullanıcının belirli bir ürün için yaptığı yorumu getirir
        /// </summary>
        Task<Review?> GetUserProductReviewAsync(int userId, int productId);

        /// <summary>
        /// Bir ürünün ortalama puanını hesaplar
        /// </summary>
        Task<double> GetAverageRatingAsync(int productId);
    }
}
