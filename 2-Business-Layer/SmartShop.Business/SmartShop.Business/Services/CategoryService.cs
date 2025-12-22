using Microsoft.EntityFrameworkCore;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Kategori iş mantığı servisi
    /// </summary>
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<CategoryDto>> GetByIdAsync(int categoryId)
        {
            try
            {
                var category = await _unitOfWork.Categories.GetAllQuery()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                    return ServiceResult<CategoryDto>.FailureResult("Kategori bulunamadı.");

                var dto = MapToDto(category);
                return ServiceResult<CategoryDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<CategoryDto>.FailureResult(
                    "Kategori getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<CategoryDto>>> GetAllAsync()
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllQuery()
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .OrderBy(c => c.Name)
                    .ToListAsync();

                var dtos = categories.Select(MapToDto).ToList();
                return ServiceResult<List<CategoryDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<CategoryDto>>.FailureResult(
                    "Kategoriler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<CategoryDto>> CreateAsync(CreateCategoryDto dto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(dto.Name))
                    return ServiceResult<CategoryDto>.FailureResult("Kategori adı boş olamaz.");

                if (dto.Name.Length < 2 || dto.Name.Length > 100)
                    return ServiceResult<CategoryDto>.FailureResult("Kategori adı 2-100 karakter arasında olmalıdır.");

                if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 500)
                    return ServiceResult<CategoryDto>.FailureResult("Açıklama en fazla 500 karakter olabilir.");

                // Aynı isimde kategori var mı kontrol et
                var exists = await _unitOfWork.Categories.GetAllQuery()
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower());

                if (exists)
                    return ServiceResult<CategoryDto>.FailureResult("Bu isimde bir kategori zaten mevcut.");

                var category = new Category
                {
                    Name = dto.Name,
                    Description = dto.Description
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CommitAsync();

                var resultDto = MapToDto(category);
                return ServiceResult<CategoryDto>.SuccessResult(resultDto, "Kategori başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return ServiceResult<CategoryDto>.FailureResult(
                    "Kategori oluşturulurken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<CategoryDto>> UpdateAsync(int categoryId, CreateCategoryDto dto)
        {
            try
            {
                if (categoryId <= 0)
                    return ServiceResult<CategoryDto>.FailureResult("Geçerli bir kategori ID'si gereklidir.");

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return ServiceResult<CategoryDto>.FailureResult("Kategori adı boş olamaz.");

                if (dto.Name.Length < 2 || dto.Name.Length > 100)
                    return ServiceResult<CategoryDto>.FailureResult("Kategori adı 2-100 karakter arasında olmalıdır.");

                if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 500)
                    return ServiceResult<CategoryDto>.FailureResult("Açıklama en fazla 500 karakter olabilir.");

                var category = await _unitOfWork.Categories.GetAllQuery()
                    .Include(c => c.Products.Where(p => p.IsActive))
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                    return ServiceResult<CategoryDto>.FailureResult("Kategori bulunamadı.");

                // Aynı isimde başka kategori var mı kontrol et
                var exists = await _unitOfWork.Categories.GetAllQuery()
                    .AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.CategoryId != categoryId);

                if (exists)
                    return ServiceResult<CategoryDto>.FailureResult("Bu isimde bir kategori zaten mevcut.");

                category.Name = dto.Name;
                category.Description = dto.Description;

                _unitOfWork.Categories.Update(category);
                await _unitOfWork.CommitAsync();

                var resultDto = MapToDto(category);
                return ServiceResult<CategoryDto>.SuccessResult(resultDto, "Kategori başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<CategoryDto>.FailureResult(
                    "Kategori güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> DeleteAsync(int categoryId)
        {
            try
            {
                if (categoryId <= 0)
                    return ServiceResult.FailureResult("Geçerli bir kategori ID'si gereklidir.");

                var category = await _unitOfWork.Categories.GetAllQuery()
                    .Include(c => c.Products)
                    .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

                if (category == null)
                    return ServiceResult.FailureResult("Kategori bulunamadı.");

                // Kategoriye ait ürün var mı kontrol et
                if (category.Products.Any())
                    return ServiceResult.FailureResult(
                        $"Bu kategoriye ait {category.Products.Count} adet ürün bulunmaktadır. Önce ürünleri silmeniz veya başka kategoriye taşımanız gerekir.");

                _unitOfWork.Categories.Remove(category);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Kategori başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Kategori silinirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> IsCategoryExistsAsync(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return ServiceResult<bool>.FailureResult("Kategori adı boş olamaz.");

                var exists = await _unitOfWork.Categories.GetAllQuery()
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower());

                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult(
                    "Kategori kontrolü yapılırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        private CategoryDto MapToDto(Category category)
        {
            return new CategoryDto
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description,
                ProductCount = category.Products?.Count(p => p.IsActive) ?? 0
            };
        }
    }
}
