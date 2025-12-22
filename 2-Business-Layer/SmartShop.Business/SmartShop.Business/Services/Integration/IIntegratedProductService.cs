using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services.Integration
{
    /// <summary>
    /// Entegre ürün servisi - REST ProductService mikroservisi ile iletişim kurar
    /// </summary>
    public interface IIntegratedProductService
    {
        Task<ServiceResult<ProductDto>> GetByIdViaRestAsync(int productId);
        Task<ServiceResult<List<ProductDto>>> GetAllViaRestAsync();
        Task<ServiceResult<List<ProductDto>>> GetByCategoryViaRestAsync(int categoryId);
        Task<ServiceResult<List<ProductDto>>> SearchViaRestAsync(ProductFilterDto filter);
        Task<ServiceResult<ProductDto>> CreateViaRestAsync(CreateProductDto dto);
        Task<ServiceResult<ProductDto>> UpdateViaRestAsync(UpdateProductDto dto);
        Task<ServiceResult> DeleteViaRestAsync(int productId);
    }
}
