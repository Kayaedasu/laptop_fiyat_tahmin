using SmartShop.DataAccess.Models;

namespace SmartShop.DataAccess.Repositories
{
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);
        Task<IEnumerable<Product>> GetProductsByBrandAsync(string brand);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        Task<Product?> GetProductWithDetailsAsync(int productId);
        Task<IEnumerable<Product>> GetAllProductsWithDetailsAsync();
        Task<IEnumerable<Product>> GetTopRatedProductsAsync(int count);
        Task<IEnumerable<Product>> GetLowStockProductsAsync(int threshold);
    }
}
