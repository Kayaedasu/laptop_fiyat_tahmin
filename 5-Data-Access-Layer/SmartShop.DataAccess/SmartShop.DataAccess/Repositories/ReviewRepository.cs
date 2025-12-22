using Microsoft.EntityFrameworkCore;
using SmartShop.DataAccess.Data;
using SmartShop.DataAccess.Models;

namespace SmartShop.DataAccess.Repositories
{
    /// <summary>
    /// Review (Yorum) işlemleri için özel repository implementasyonu
    /// </summary>
    public class ReviewRepository : Repository<Review>, IReviewRepository
    {
        public ReviewRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
        {
            return await _context.Reviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetUserProductReviewAsync(int userId, int productId)
        {
            return await _context.Reviews
                .Where(r => r.UserId == userId && r.ProductId == productId && !r.IsDeleted)
                .Include(r => r.User)
                .Include(r => r.Product)
                .FirstOrDefaultAsync();
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId && !r.IsDeleted)
                .ToListAsync();

            if (!reviews.Any())
            {
                return 0;
            }

            return Math.Round(reviews.Average(r => r.Rating), 1);
        }
    }
}
