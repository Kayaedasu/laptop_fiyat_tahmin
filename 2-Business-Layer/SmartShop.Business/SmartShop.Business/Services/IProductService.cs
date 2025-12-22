using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Ürün iş mantığı için servis arayüzü
    /// </summary>
    public interface IProductService
    {
        Task<ServiceResult<ProductDto>> GetByIdAsync(int productId);
        Task<ServiceResult<ProductDetailDto>> GetDetailsByIdAsync(int productId);
        Task<ServiceResult<List<ProductDto>>> GetAllAsync();
        Task<ServiceResult<List<ProductDto>>> GetByCategoryAsync(int categoryId);
        Task<ServiceResult<List<ProductDto>>> SearchAsync(ProductFilterDto filter);
        Task<ServiceResult<ProductDto>> CreateAsync(CreateProductDto dto);
        Task<ServiceResult<ProductDto>> UpdateAsync(UpdateProductDto dto);
        Task<ServiceResult> DeleteAsync(int productId);
        Task<ServiceResult> UpdateStockAsync(int productId, int quantity);
        Task<ServiceResult<List<ProductDto>>> GetTopRatedAsync(int count = 10);
        Task<ServiceResult<List<ProductDto>>> GetLowStockAsync(int threshold = 10);
        Task<ServiceResult<bool>> IsProductAvailableAsync(int productId, int quantity);
    }
}
