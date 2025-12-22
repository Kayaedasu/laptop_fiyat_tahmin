using SmartShop.Business.Common;
using SmartShop.Business.DTOs;

namespace SmartShop.Business.Services.Integration
{
    /// <summary>
    /// Entegre kullanıcı servisi - gRPC UserService mikroservisi ile iletişim kurar
    /// </summary>
    public interface IIntegratedUserService
    {
        Task<ServiceResult<UserDto>> RegisterViaGrpcAsync(RegisterUserDto dto);
        Task<ServiceResult<UserDto>> LoginViaGrpcAsync(LoginUserDto dto);
        Task<ServiceResult<UserDto>> GetByIdViaGrpcAsync(int userId);
        Task<ServiceResult<UserDto>> GetByEmailViaGrpcAsync(string email);
        Task<ServiceResult<UserDto>> UpdateViaGrpcAsync(UpdateUserDto dto);
        Task<ServiceResult> DeleteViaGrpcAsync(int userId);
    }
}
