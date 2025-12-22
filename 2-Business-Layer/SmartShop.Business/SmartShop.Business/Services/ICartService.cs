using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Sepet iş mantığı için servis arayüzü
    /// </summary>
    public interface ICartService
    {
        Task<ServiceResult<CartSummaryDto>> GetUserCartAsync(int userId);
        Task<ServiceResult<CartDto>> AddToCartAsync(AddToCartDto dto);
        Task<ServiceResult<CartDto>> UpdateCartItemAsync(UpdateCartDto dto);
        Task<ServiceResult> RemoveFromCartAsync(int cartId);
        Task<ServiceResult> ClearCartAsync(int userId);
        Task<ServiceResult<int>> GetCartItemCountAsync(int userId);
        Task<ServiceResult<bool>> IsProductInCartAsync(int userId, int productId);
    }
}
