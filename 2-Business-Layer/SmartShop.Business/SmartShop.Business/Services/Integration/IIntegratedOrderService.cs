using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services.Integration
{
    /// <summary>
    /// Entegre sipariş servisi - SOAP OrderService mikroservisi ile iletişim kurar
    /// </summary>
    public interface IIntegratedOrderService
    {
        Task<ServiceResult<OrderDto>> GetByIdViaSoapAsync(int orderId);
        Task<ServiceResult<List<OrderDto>>> GetByUserIdViaSoapAsync(int userId);
        Task<ServiceResult<OrderDto>> CreateViaSoapAsync(CreateOrderDto dto);
        Task<ServiceResult<OrderDto>> UpdateStatusViaSoapAsync(int orderId, string status);
        Task<ServiceResult> CancelViaSoapAsync(int orderId, string reason);
    }
}
