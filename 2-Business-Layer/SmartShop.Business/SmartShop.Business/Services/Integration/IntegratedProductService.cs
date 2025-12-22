using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.Integration.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartShop.Business.Services.Integration
{
    /// <summary>
    /// Entegre ürün servisi - REST ProductService mikroservisi ile iletişim kurar
    /// NOT: Bu sınıf şu anda devre dışı - ProductServiceClient API uyumsuzluğu nedeniyle
    /// </summary>
    public class IntegratedProductService : IIntegratedProductService
    {
        private readonly ProductServiceClient _restClient;

        public IntegratedProductService(string serviceUrl = "http://localhost:3001/api/v1")
        {
            _restClient = new ProductServiceClient(serviceUrl);
        }

        public async Task<ServiceResult<ProductDto>> GetByIdViaRestAsync(int productId)
        {
            try
            {
                // NOT: ProductServiceClient API'si farklı, simülasyon
                await Task.Delay(100);
                return ServiceResult<ProductDto>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "REST üzerinden ürün getirilirken bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> GetAllViaRestAsync()
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult<List<ProductDto>>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "REST üzerinden ürünler getirilirken bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> GetByCategoryViaRestAsync(int categoryId)
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult<List<ProductDto>>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "REST üzerinden kategoriye ait ürünler getirilirken bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<ProductDto>>> SearchViaRestAsync(ProductFilterDto filter)
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult<List<ProductDto>>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<List<ProductDto>>.FailureResult(
                    "REST üzerinden ürün arama sırasında bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult<ProductDto>> CreateViaRestAsync(CreateProductDto dto)
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult<ProductDto>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "REST üzerinden ürün oluşturulurken bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult<ProductDto>> UpdateViaRestAsync(UpdateProductDto dto)
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult<ProductDto>.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult<ProductDto>.FailureResult(
                    "REST üzerinden ürün güncellenirken bir hata oluştu.",
                    ex.Message);
            }
        }

        public async Task<ServiceResult> DeleteViaRestAsync(int productId)
        {
            try
            {
                await Task.Delay(100);
                return ServiceResult.FailureResult("Şu anda REST entegrasyonu yapım aşamasında");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "REST üzerinden ürün silinirken bir hata oluştu.",
                    ex.Message);
            }
        }
    }
}
