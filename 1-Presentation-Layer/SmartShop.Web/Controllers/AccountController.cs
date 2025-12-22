using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.DTOs;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly ICartService _cartService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IUserService userService, ICartService cartService, ILogger<AccountController> logger)
        {
            _userService = userService;
            _cartService = cartService;
            _logger = logger;
        }

        // GET: Account/Login
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginUserDto model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.LoginAsync(model);

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, result.Message);
                    return View(model);
                }

                // Session'a kullanıcı bilgilerini kaydet
                HttpContext.Session.SetInt32("UserId", result.Data!.UserId);
                HttpContext.Session.SetString("UserName", $"{result.Data.FirstName} {result.Data.LastName}");
                HttpContext.Session.SetString("UserEmail", result.Data.Email);
                HttpContext.Session.SetString("UserRole", result.Data.Role);

                // Sepetteki ürün sayısını güncelle (sadece müşteriler için)
                if (result.Data.Role != "Admin")
                {
                    await UpdateCartItemCountAsync(result.Data.UserId);
                }

                TempData["Success"] = "Hoş geldiniz, " + result.Data.FirstName + "!";

                // ReturnUrl varsa oraya yönlendir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                // Rol bazlı yönlendirme
                if (result.Data.Role == "Admin")
                {
                    return RedirectToAction("Dashboard", "Admin");
                }
                else
                {
                    return RedirectToAction("Dashboard", "Customer");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login hatası: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Giriş yapılırken bir hata oluştu.");
                return View(model);
            }
        }

        // GET: Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.RegisterAsync(model);

                if (!result.Success)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }

                TempData["Success"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kayıt hatası: {Email}", model.Email);
                ModelState.AddModelError(string.Empty, "Kayıt sırasında bir hata oluştu.");
                return View(model);
            }
        }

        // POST: Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Çıkış yapıldı.";
            return RedirectToAction("Index", "Home");
        }

        // GET: Account/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Profili görüntülemek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var result = await _userService.GetByIdAsync(userId);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction("Index", "Home");
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil görüntüleme hatası. UserId: {UserId}", userId);
                TempData["Error"] = "Profil yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Account/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Profili düzenlemek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var result = await _userService.GetByIdAsync(userId);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Profile));
                }

                var model = new UpdateUserDto
                {
                    UserId = result.Data!.UserId,
                    FirstName = result.Data.FirstName,
                    LastName = result.Data.LastName,
                    PhoneNumber = result.Data.PhoneNumber
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil düzenleme hatası. UserId: {UserId}", userId);
                TempData["Error"] = "Profil yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: Account/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateUserDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.UpdateAsync(model);

                if (!result.Success)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }

                // Session'daki kullanıcı adını güncelle
                HttpContext.Session.SetString("UserName", $"{result.Data!.FirstName} {result.Data.LastName}");

                TempData["Success"] = "Profiliniz güncellendi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncelleme hatası. UserId: {UserId}", model.UserId);
                ModelState.AddModelError(string.Empty, "Profil güncellenirken bir hata oluştu.");
                return View(model);
            }
        }

        // GET: Account/ChangePassword
        public IActionResult ChangePassword()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Şifre değiştirmek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        // POST: Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto model)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Şifre değiştirmek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            model.UserId = userId;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var result = await _userService.ChangePasswordAsync(model);

                if (!result.Success)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                    return View(model);
                }

                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme hatası. UserId: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "Şifre değiştirilirken bir hata oluştu.");
                return View(model);
            }
        }

        // POST: Account/UpdateProfile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(string firstName, string lastName, string? phone)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Profili güncellemek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            try
            {
                var model = new UpdateUserDto
                {
                    UserId = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phone
                };

                var result = await _userService.UpdateAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Profil güncellenirken bir hata oluştu.";
                    return RedirectToAction(nameof(Profile));
                }

                // Session'daki kullanıcı adını güncelle
                HttpContext.Session.SetString("UserName", $"{result.Data!.FirstName} {result.Data.LastName}");

                TempData["Success"] = "Profiliniz başarıyla güncellendi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncelleme hatası. UserId: {UserId}", userId);
                TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // POST: Account/ChangePassword (Form post için)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmNewPassword)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Şifre değiştirmek için giriş yapmalısınız.";
                return RedirectToAction(nameof(Login));
            }

            if (newPassword != confirmNewPassword)
            {
                TempData["Error"] = "Yeni şifreler eşleşmiyor.";
                return RedirectToAction(nameof(Profile));
            }

            try
            {
                var model = new ChangePasswordDto
                {
                    UserId = userId,
                    CurrentPassword = currentPassword,
                    NewPassword = newPassword
                };

                var result = await _userService.ChangePasswordAsync(model);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message ?? "Şifre değiştirilirken bir hata oluştu.";
                    return RedirectToAction(nameof(Profile));
                }

                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction(nameof(Profile));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme hatası. UserId: {UserId}", userId);
                TempData["Error"] = "Şifre değiştirilirken bir hata oluştu.";
                return RedirectToAction(nameof(Profile));
            }
        }

        // Helper method
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        // Helper method to update cart item count in session
        private async Task UpdateCartItemCountAsync(int userId)
        {
            try
            {
                var cartResult = await _cartService.GetUserCartAsync(userId);
                if (cartResult.Success && cartResult.Data != null)
                {
                    HttpContext.Session.SetInt32("CartItemCount", cartResult.Data.TotalItems);
                }
                else
                {
                    HttpContext.Session.SetInt32("CartItemCount", 0);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet item sayısı güncellenirken hata oluştu");
                HttpContext.Session.SetInt32("CartItemCount", 0);
            }
        }
    }
}
