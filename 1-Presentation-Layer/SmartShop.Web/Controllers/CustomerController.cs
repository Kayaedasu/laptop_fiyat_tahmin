using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers
{
    /// <summary>
    /// Customer (Müşteri) paneli controller'ı
    /// </summary>
    public class CustomerController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IUserService _userService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(
            IProductService productService,
            IOrderService orderService,
            ICartService cartService,
            IUserService userService,
            ILogger<CustomerController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _cartService = cartService;
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Kullanıcı giriş kontrolü
        /// </summary>
        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserId").HasValue;
        }

        /// <summary>
        /// Customer ID'yi al
        /// </summary>
        private int? GetCustomerId()
        {
            return HttpContext.Session.GetInt32("UserId");
        }

        /// <summary>
        /// Customer dashboard ana sayfa
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userId = GetCustomerId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                // Kullanıcı bilgileri
                var userResult = await _productService.GetAllAsync();
                
                // Son siparişler
                var ordersResult = await _orderService.GetAllAsync();
                var userOrders = ordersResult.Data?.Where(o => o.UserId == userId.Value).OrderByDescending(o => o.OrderDate).Take(5).ToList();

                ViewBag.UserName = HttpContext.Session.GetString("UserName");
                ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail");
                ViewBag.RecentOrders = userOrders ?? new List<SmartShop.Business.DTOs.OrderDto>();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Customer dashboard yüklenirken hata oluştu");
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Müşteri siparişleri
        /// </summary>
        public async Task<IActionResult> MyOrders()
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userId = GetCustomerId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _orderService.GetAllAsync();
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new List<SmartShop.Business.DTOs.OrderDto>());
                }

                var userOrders = result.Data?.Where(o => o.UserId == userId.Value).OrderByDescending(o => o.OrderDate).ToList();
                return View(userOrders ?? new List<SmartShop.Business.DTOs.OrderDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler yüklenirken hata oluştu");
                TempData["Error"] = "Siparişler yüklenirken bir hata oluştu.";
                return View(new List<SmartShop.Business.DTOs.OrderDto>());
            }
        }

        /// <summary>
        /// Profil sayfası - GET
        /// </summary>
        public async Task<IActionResult> Profile()
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userId = GetCustomerId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                // Kullanıcı bilgilerini getir
                var userResult = await _userService.GetByIdAsync(userId.Value);
                
                if (!userResult.Success || userResult.Data == null)
                {
                    TempData["Error"] = "Kullanıcı bilgileri yüklenemedi.";
                    return RedirectToAction("Dashboard");
                }

                // UpdateUserDto modeli oluştur
                var model = new SmartShop.Business.DTOs.UpdateUserDto
                {
                    UserId = userResult.Data.UserId,
                    FirstName = userResult.Data.FirstName,
                    LastName = userResult.Data.LastName,
                    PhoneNumber = userResult.Data.PhoneNumber
                };

                ViewBag.UserName = $"{userResult.Data.FirstName} {userResult.Data.LastName}";
                ViewBag.UserEmail = userResult.Data.Email;

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Profil bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction("Dashboard");
            }
        }

        /// <summary>
        /// Profil güncelleme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(SmartShop.Business.DTOs.UpdateUserDto model)
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userId = GetCustomerId();
                if (!userId.HasValue || userId.Value != model.UserId)
                {
                    TempData["Error"] = "Yetkisiz işlem.";
                    return RedirectToAction("Profile");
                }

                // Model validation
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Lütfen tüm alanları doğru şekilde doldurun.";
                    return View("Profile", model);
                }

                // UserService ile güncelleme yap (Email, Role, IsActive null bırakılır - customer güncelleme)
                model.Email = null;
                model.Role = null;
                model.IsActive = null;
                
                var result = await _userService.UpdateAsync(model);
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View("Profile", model);
                }
                
                // Session'ı güncelle
                HttpContext.Session.SetString("UserName", $"{model.FirstName} {model.LastName}");
                
                TempData["Success"] = "Profil bilgileriniz başarıyla güncellendi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncellenirken hata oluştu");
                TempData["Error"] = "Profil güncellenirken bir hata oluştu.";
                return View("Profile", model);
            }
        }

        /// <summary>
        /// Şifre değiştirme sayfası - GET
        /// </summary>
        public IActionResult ChangePassword()
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        /// <summary>
        /// Şifre değiştirme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(SmartShop.Business.DTOs.ChangePasswordDto model)
        {
            if (!IsLoggedIn())
            {
                TempData["Error"] = "Lütfen önce giriş yapın.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var userId = GetCustomerId();
                if (!userId.HasValue)
                {
                    TempData["Error"] = "Kullanıcı bilgisi bulunamadı.";
                    return RedirectToAction("Login", "Account");
                }

                model.UserId = userId.Value;

                // Model validation
                if (!ModelState.IsValid)
                {
                    TempData["Error"] = "Lütfen tüm alanları doğru şekilde doldurun.";
                    return View(model);
                }

                // UserService ile şifre değiştir
                var result = await _userService.ChangePasswordAsync(model);
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(model);
                }
                
                TempData["Success"] = "Şifreniz başarıyla değiştirildi.";
                return RedirectToAction("Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirilirken hata oluştu");
                TempData["Error"] = "Şifre değiştirilirken bir hata oluştu.";
                return View(model);
            }
        }
    }
}
