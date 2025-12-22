namespace SmartShop.DataAccess.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        // Repositories
        SmartShop.DataAccess.Repositories.IProductRepository Products { get; }
        SmartShop.DataAccess.Repositories.IOrderRepository Orders { get; }
        SmartShop.DataAccess.Repositories.IRepository<Models.User> Users { get; }
        SmartShop.DataAccess.Repositories.IRepository<Models.Category> Categories { get; }
        SmartShop.DataAccess.Repositories.IReviewRepository Reviews { get; }
        SmartShop.DataAccess.Repositories.IRepository<Models.Cart> Carts { get; }
        
        // Transaction Yönetimi
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
