using SmartShop.Business.DTOs;
using System.Text.RegularExpressions;

namespace SmartShop.Business.Validators
{
    /// <summary>
    /// Kullanıcı doğrulama kuralları
    /// </summary>
    public static class UserValidator
    {
        private static readonly Regex EmailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PhoneRegex = new Regex(
            @"^[\d\s\-\+\(\)]{10,20}$",
            RegexOptions.Compiled);

        public static List<string> ValidateRegister(RegisterUserDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                errors.Add("Ad boş olamaz.");
            else if (dto.FirstName.Length < 2 || dto.FirstName.Length > 50)
                errors.Add("Ad 2-50 karakter arasında olmalıdır.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                errors.Add("Soyad boş olamaz.");
            else if (dto.LastName.Length < 2 || dto.LastName.Length > 50)
                errors.Add("Soyad 2-50 karakter arasında olmalıdır.");

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("E-posta boş olamaz.");
            else if (!EmailRegex.IsMatch(dto.Email))
                errors.Add("Geçerli bir e-posta adresi giriniz.");
            else if (dto.Email.Length > 100)
                errors.Add("E-posta en fazla 100 karakter olabilir.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("Şifre boş olamaz.");
            else
            {
                var passwordErrors = ValidatePassword(dto.Password);
                errors.AddRange(passwordErrors);
            }

            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword))
                errors.Add("Şifre tekrarı boş olamaz.");
            else if (dto.Password != dto.ConfirmPassword)
                errors.Add("Şifreler eşleşmiyor.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !PhoneRegex.IsMatch(dto.PhoneNumber))
                errors.Add("Geçerli bir telefon numarası giriniz.");

            if (!string.IsNullOrWhiteSpace(dto.Address) && dto.Address.Length > 500)
                errors.Add("Adres en fazla 500 karakter olabilir.");

            return errors;
        }

        public static List<string> ValidateLogin(LoginUserDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Email))
                errors.Add("E-posta boş olamaz.");
            else if (!EmailRegex.IsMatch(dto.Email))
                errors.Add("Geçerli bir e-posta adresi giriniz.");

            if (string.IsNullOrWhiteSpace(dto.Password))
                errors.Add("Şifre boş olamaz.");

            return errors;
        }

        public static List<string> ValidateUpdate(UpdateUserDto dto)
        {
            var errors = new List<string>();

            if (dto.UserId <= 0)
                errors.Add("Geçerli bir kullanıcı ID'si gereklidir.");

            if (string.IsNullOrWhiteSpace(dto.FirstName))
                errors.Add("Ad boş olamaz.");
            else if (dto.FirstName.Length < 2 || dto.FirstName.Length > 50)
                errors.Add("Ad 2-50 karakter arasında olmalıdır.");

            if (string.IsNullOrWhiteSpace(dto.LastName))
                errors.Add("Soyad boş olamaz.");
            else if (dto.LastName.Length < 2 || dto.LastName.Length > 50)
                errors.Add("Soyad 2-50 karakter arasında olmalıdır.");

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber) && !PhoneRegex.IsMatch(dto.PhoneNumber))
                errors.Add("Geçerli bir telefon numarası giriniz.");

            return errors;
        }

        public static List<string> ValidateChangePassword(ChangePasswordDto dto)
        {
            var errors = new List<string>();

            if (dto.UserId <= 0)
                errors.Add("Geçerli bir kullanıcı ID'si gereklidir.");

            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                errors.Add("Mevcut şifre boş olamaz.");

            if (string.IsNullOrWhiteSpace(dto.NewPassword))
                errors.Add("Yeni şifre boş olamaz.");
            else
            {
                var passwordErrors = ValidatePassword(dto.NewPassword);
                errors.AddRange(passwordErrors);
            }

            if (dto.CurrentPassword == dto.NewPassword)
                errors.Add("Yeni şifre mevcut şifre ile aynı olamaz.");

            return errors;
        }

        private static List<string> ValidatePassword(string password)
        {
            var errors = new List<string>();

            if (password.Length < 6)
                errors.Add("Şifre en az 6 karakter olmalıdır.");

            if (password.Length > 100)
                errors.Add("Şifre en fazla 100 karakter olabilir.");

            if (!password.Any(char.IsUpper))
                errors.Add("Şifre en az bir büyük harf içermelidir.");

            if (!password.Any(char.IsLower))
                errors.Add("Şifre en az bir küçük harf içermelidir.");

            if (!password.Any(char.IsDigit))
                errors.Add("Şifre en az bir rakam içermelidir.");

            return errors;
        }
    }
}
