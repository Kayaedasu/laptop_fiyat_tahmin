using Microsoft.EntityFrameworkCore;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.Business.Validators;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;
using BCrypt.Net;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Kullanıcı iş mantığı servisi
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<UserDto>> GetByIdAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                
                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("Kullanıcı bulunamadı.");

                var dto = MapToDto(user);
                return ServiceResult<UserDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Kullanıcı getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<UserDto>> GetByEmailAsync(string email)
        {
            try
            {
                var user = await _unitOfWork.Users.GetAllQuery()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
                
                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("Kullanıcı bulunamadı.");

                var dto = MapToDto(user);
                return ServiceResult<UserDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Kullanıcı getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<UserDto>>> GetAllAsync()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAllQuery()
                    .OrderBy(u => u.FirstName)
                    .ThenBy(u => u.LastName)
                    .ToListAsync();

                var dtos = users.Select(MapToDto).ToList();
                return ServiceResult<List<UserDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<UserDto>>.FailureResult(
                    "Kullanıcılar getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<UserDto>> RegisterAsync(RegisterUserDto dto)
        {
            try
            {
                var validationErrors = UserValidator.ValidateRegister(dto);
                if (validationErrors.Any())
                    return ServiceResult<UserDto>.FailureResult(
                        "Kayıt verileri geçersiz.", 
                        validationErrors);

                // E-posta kontrolü
                var existingUser = await _unitOfWork.Users.GetAllQuery()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                if (existingUser != null)
                    return ServiceResult<UserDto>.FailureResult("Bu e-posta adresi zaten kullanımda.");

                // Şifreyi hashle
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Password = passwordHash,
                    Phone = dto.PhoneNumber,
                    Role = "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Users.AddAsync(user);
                await _unitOfWork.CommitAsync();

                var resultDto = MapToDto(user);
                return ServiceResult<UserDto>.SuccessResult(resultDto, "Kayıt başarıyla tamamlandı.");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Kayıt işlemi sırasında bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<UserDto>> LoginAsync(LoginUserDto dto)
        {
            try
            {
                var validationErrors = UserValidator.ValidateLogin(dto);
                if (validationErrors.Any())
                    return ServiceResult<UserDto>.FailureResult(
                        "Giriş verileri geçersiz.", 
                        validationErrors);

                var user = await _unitOfWork.Users.GetAllQuery()
                    .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("E-posta veya şifre hatalı.");

                if (!user.IsActive)
                    return ServiceResult<UserDto>.FailureResult("Hesabınız aktif değil.");

                // Şifre kontrolü
                if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                    return ServiceResult<UserDto>.FailureResult("E-posta veya şifre hatalı.");

                var resultDto = MapToDto(user);
                return ServiceResult<UserDto>.SuccessResult(resultDto, "Giriş başarılı.");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Giriş işlemi sırasında bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<UserDto>> UpdateAsync(UpdateUserDto dto)
        {
            try
            {
                var validationErrors = UserValidator.ValidateUpdate(dto);
                if (validationErrors.Any())
                    return ServiceResult<UserDto>.FailureResult(
                        "Güncelleme verileri geçersiz.", 
                        validationErrors);

                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult<UserDto>.FailureResult("Kullanıcı bulunamadı.");

                // Temel alanları güncelle
                user.FirstName = dto.FirstName;
                user.LastName = dto.LastName;
                user.Phone = dto.PhoneNumber;

                // Admin tarafından güncelleme yapılıyorsa (Email, Role, IsActive gönderilmişse)
                if (!string.IsNullOrEmpty(dto.Email))
                {
                    // E-posta değişikliği kontrolü
                    if (dto.Email.ToLower() != user.Email.ToLower())
                    {
                        var emailExists = await _unitOfWork.Users.GetAllQuery()
                            .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.UserId != dto.UserId);
                        
                        if (emailExists)
                            return ServiceResult<UserDto>.FailureResult("Bu e-posta adresi başka bir kullanıcı tarafından kullanılıyor.");
                        
                        user.Email = dto.Email;
                    }
                }

                if (!string.IsNullOrEmpty(dto.Role))
                {
                    user.Role = dto.Role;
                }

                if (dto.IsActive.HasValue)
                {
                    user.IsActive = dto.IsActive.Value;
                }

                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                var resultDto = MapToDto(user);
                return ServiceResult<UserDto>.SuccessResult(resultDto, "Profil başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<UserDto>.FailureResult(
                    "Profil güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> ChangePasswordAsync(ChangePasswordDto dto)
        {
            try
            {
                var validationErrors = UserValidator.ValidateChangePassword(dto);
                if (validationErrors.Any())
                    return ServiceResult.FailureResult(
                        "Şifre değiştirme verileri geçersiz.", 
                        validationErrors);

                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult.FailureResult("Kullanıcı bulunamadı.");

                // Mevcut şifre kontrolü
                if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.Password))
                    return ServiceResult.FailureResult("Mevcut şifre hatalı.");

                // Yeni şifreyi hashle ve kaydet
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Şifre başarıyla değiştirildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Şifre değiştirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> DeleteAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult.FailureResult("Kullanıcı bulunamadı.");

                // Soft delete
                user.IsActive = false;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Kullanıcı başarıyla silindi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Kullanıcı silinirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> IsEmailExistsAsync(string email)
        {
            try
            {
                var exists = await _unitOfWork.Users.GetAllQuery()
                    .AnyAsync(u => u.Email.ToLower() == email.ToLower());

                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult(
                    "E-posta kontrolü yapılırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> ActivateUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult.FailureResult("Kullanıcı bulunamadı.");

                user.IsActive = true;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Kullanıcı aktif edildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Kullanıcı aktif edilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> DeactivateUserAsync(int userId)
        {
            try
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId);
                if (user == null)
                    return ServiceResult.FailureResult("Kullanıcı bulunamadı.");

                user.IsActive = false;
                _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Kullanıcı pasif edildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Kullanıcı pasif edilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.Phone,
                Address = null, // Veritabanında Address alanı yok
                Role = user.Role,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
