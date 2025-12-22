using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Sipariş iş mantığı için servis arayüzü
    /// </summary>
    public interface IOrderService
    {
        Task<ServiceResult<OrderDto>> GetByIdAsync(int orderId);
        Task<ServiceResult<List<OrderDto>>> GetByUserIdAsync(int userId);
        Task<ServiceResult<List<OrderDto>>> GetAllAsync();
        Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto);
        Task<ServiceResult> UpdateStatusAsync(UpdateOrderStatusDto dto);
        Task<ServiceResult> CancelOrderAsync(int orderId, int userId);
        Task<ServiceResult<decimal>> CalculateOrderTotalAsync(CreateOrderDto dto);
        Task<ServiceResult<List<OrderDto>>> GetOrdersByStatusAsync(string status);
        Task<ServiceResult<List<OrderDto>>> GetRecentOrdersAsync(int count = 10);
    }
}
