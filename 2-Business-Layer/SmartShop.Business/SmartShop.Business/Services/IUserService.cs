using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Kullanıcı iş mantığı için servis arayüzü
    /// </summary>
    public interface IUserService
    {
        Task<ServiceResult<UserDto>> GetByIdAsync(int userId);
        Task<ServiceResult<UserDto>> GetByEmailAsync(string email);
        Task<ServiceResult<List<UserDto>>> GetAllAsync();
        Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto);
        Task<ServiceResult<UserDto>> LoginAsync(LoginUserDto dto);
        Task<ServiceResult<UserDto>> UpdateAsync(UpdateUserDto dto);
        Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto);
        Task<ServiceResult> DeleteAsync(int userId);
        Task<ServiceResult<bool>> IsEmailExistsAsync(string email);
        Task<ServiceResult> ActivateUserAsync(int userId);
        Task<ServiceResult> DeactivateUserAsync(int userId);
    }
}
