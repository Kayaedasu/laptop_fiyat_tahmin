using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Kategori iş mantığı için servis arayüzü
    /// </summary>
    public interface ICategoryService
    {
        Task<ServiceResult<CategoryDto>> GetByIdAsync(int categoryId);
        Task<ServiceResult<List<CategoryDto>>> GetAllAsync();
        Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto);
        Task<ServiceResult<CategoryDto>> UpdateAsync(int categoryId, CreateCategoryDto dto);
        Task<ServiceResult> DeleteAsync(int categoryId);
        Task<ServiceResult<bool>> IsCategoryExistsAsync(string name);
    }
}
