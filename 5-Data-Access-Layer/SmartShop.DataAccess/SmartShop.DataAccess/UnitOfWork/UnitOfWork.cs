using Microsoft.EntityFrameworkCore.Storage;
using SmartShop.DataAccess.Data;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.Repositories;

namespace SmartShop.DataAccess.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        // Repositories
        public IProductRepository Products { get; private set; }
        public IOrderRepository Orders { get; private set; }
        public IRepository<User> Users { get; private set; }
        public IRepository<Category> Categories { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IRepository<Cart> Carts { get; private set; }

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;

            // Initialize Repositories
            Products = new ProductRepository(_context);
            Orders = new OrderRepository(_context);
            Users = new Repository<User>(_context);
            Categories = new Repository<Category>(_context);
            Reviews = new ReviewRepository(_context);
            Carts = new Repository<Cart>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            try
            {
                await SaveChangesAsync();
                
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}
