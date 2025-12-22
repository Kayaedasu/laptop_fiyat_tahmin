using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers
{
    /// <summary>
    /// Admin paneli controller'ı
    /// </summary>
    public class AdminController : Controller
    {
        private readonly IProductService _productService;
        private readonly IOrderService _orderService;
        private readonly IUserService _userService;
        private readonly ICategoryService _categoryService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            IProductService productService,
            IOrderService orderService,
            IUserService userService,
            ICategoryService categoryService,
            ILogger<AdminController> logger)
        {
            _productService = productService;
            _orderService = orderService;
            _userService = userService;
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Admin kontrolü - Her action'dan önce çalışır
        /// </summary>
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString("UserRole");
            _logger.LogInformation("IsAdmin check - UserRole from session: {Role}", role ?? "NULL");
            return role == "Admin";
        }

        /// <summary>
        /// Admin dashboard ana sayfa
        /// </summary>
        public async Task<IActionResult> Dashboard()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Dashboard için istatistikler
                var products = await _productService.GetAllAsync();
                var orders = await _orderService.GetAllAsync();
                var users = await _userService.GetAllAsync();

                ViewBag.TotalProducts = products.Data?.Count ?? 0;
                ViewBag.TotalOrders = orders.Data?.Count ?? 0;
                ViewBag.TotalUsers = users.Data?.Count ?? 0;
                ViewBag.TotalRevenue = orders.Data?.Sum(o => o.TotalAmount) ?? 0;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin dashboard yüklenirken hata oluştu");
                TempData["Error"] = "Dashboard yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Ürün yönetimi sayfası
        /// </summary>
        public async Task<IActionResult> Products()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _productService.GetAllAsync();
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new List<SmartShop.Business.DTOs.ProductDto>());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin ürünler sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Ürünler yüklenirken bir hata oluştu.";
                return View(new List<SmartShop.Business.DTOs.ProductDto>());
            }
        }

        /// <summary>
        /// Sipariş yönetimi sayfası
        /// </summary>
        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _orderService.GetAllAsync();
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new List<SmartShop.Business.DTOs.OrderDto>());
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin siparişler sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Siparişler yüklenirken bir hata oluştu.";
                return View(new List<SmartShop.Business.DTOs.OrderDto>());
            }
        }

        /// <summary>
        /// Kullanıcı yönetimi sayfası
        /// </summary>
        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _userService.GetAllAsync();
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new List<SmartShop.Business.DTOs.UserDto>());
                }

                // Sadece aktif kullanıcıları göster (soft delete edilen kullanıcılar hariç)
                var activeUsers = result.Data?.Where(u => u.IsActive).ToList() ?? new List<SmartShop.Business.DTOs.UserDto>();
                
                return View(activeUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin kullanıcılar sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Kullanıcılar yüklenirken bir hata oluştu.";
                return View(new List<SmartShop.Business.DTOs.UserDto>());
            }
        }

        /// <summary>
        /// Kullanıcı düzenleme sayfası - GET
        /// </summary>
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _userService.GetByIdAsync(id);
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Users));
                }

                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri yüklenirken hata oluştu: {UserId}", id);
                TempData["Error"] = "Kullanıcı bilgileri yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Users));
            }
        }

        /// <summary>
        /// Kullanıcı düzenleme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int UserId, string FirstName, string LastName, 
            string Email, string? PhoneNumber, string Role, bool IsActive)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) || 
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Role))
            {
                TempData["Error"] = "Lütfen tüm zorunlu alanları doldurun.";
                return RedirectToAction(nameof(EditUser), new { id = UserId });
            }

            try
            {
                var model = new SmartShop.Business.DTOs.UpdateUserDto
                {
                    UserId = UserId,
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    PhoneNumber = PhoneNumber,
                    Role = Role,
                    IsActive = IsActive
                };

                var result = await _userService.UpdateAsync(model);
                
                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(EditUser), new { id = UserId });
                }

                TempData["Success"] = "Kullanıcı başarıyla güncellendi.";
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı güncellenirken hata oluştu: {UserId}", UserId);
                TempData["Error"] = "Kullanıcı güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(EditUser), new { id = UserId });
            }
        }

        /// <summary>
        /// Kullanıcı silme - POST
        /// </summary>
        [HttpPost]
        // [ValidateAntiForgeryToken]  // TODO: Geçici olarak devre dışı - session problemi giderildikten sonra açılacak
        public async Task<IActionResult> DeleteUser(int id)
        {
            // Debug: Session bilgilerini logla
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            _logger.LogInformation("DeleteUser called - UserId: {UserId}, UserRole: {UserRole}, TargetId: {TargetId}", 
                userId ?? "NULL", userRole ?? "NULL", id);

            if (!IsAdmin())
            {
                _logger.LogWarning("DeleteUser - Unauthorized access attempt. UserRole: {Role}", userRole ?? "NULL");
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok. Lütfen tekrar giriş yapın." });
            }

            try
            {
                // Kendi hesabını silmesini engelle
                var currentUserId = HttpContext.Session.GetString("UserId");
                if (currentUserId == id.ToString())
                {
                    return Json(new { success = false, message = "Kendi hesabınızı silemezsiniz." });
                }

                var result = await _userService.DeleteAsync(id);
                
                if (!result.Success)
                {
                    return Json(new { success = false, message = result.Message });
                }

                _logger.LogInformation("User deleted successfully - UserId: {UserId}, DeletedBy: {AdminId}", id, currentUserId);
                return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı silinirken hata oluştu: {UserId}", id);
                return Json(new { success = false, message = "Kullanıcı silinirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// Test endpoint - Session durumunu kontrol et
        /// </summary>
        [HttpGet]
        public IActionResult TestSession()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var userRole = HttpContext.Session.GetString("UserRole");
            var userName = HttpContext.Session.GetString("UserName");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            
            return Json(new
            {
                userId = userId ?? "NULL",
                userRole = userRole ?? "NULL",
                userName = userName ?? "NULL",
                userEmail = userEmail ?? "NULL",
                sessionAvailable = HttpContext.Session.IsAvailable,
                isAdmin = IsAdmin()
            });
        }

        /// <summary>
        /// Sipariş detayı sayfası - Admin
        /// </summary>
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _orderService.GetByIdAsync(id);
                
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message ?? "Sipariş bulunamadı.";
                    return RedirectToAction(nameof(Orders));
                }

                // OrderDto'yu OrderDetailViewDto'ya dönüştür
                var orderDetail = new SmartShop.Business.DTOs.OrderDetailViewDto
                {
                    OrderId = result.Data.OrderId,
                    OrderDate = result.Data.OrderDate,
                    TotalAmount = result.Data.TotalAmount,
                    Status = result.Data.Status,
                    ShippingAddress = result.Data.ShippingAddress,
                    PaymentMethod = result.Data.PaymentMethod,
                    Items = result.Data.OrderDetails?.Select(od => new SmartShop.Business.DTOs.OrderItemDto
                    {
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        CategoryName = "", // OrderDetailDto'da kategori yok
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ImageUrl = null // OrderDetailDto'da resim yok
                    }).ToList() ?? new List<SmartShop.Business.DTOs.OrderItemDto>()
                };

                // Admin için ek bilgiler
                ViewBag.UserName = result.Data.UserName;
                ViewBag.UserEmail = result.Data.UserEmail;

                return View(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin sipariş detayı yüklenirken hata oluştu. OrderId: {OrderId}", id);
                TempData["Error"] = "Sipariş detayı yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Orders));
            }
        }

        /// <summary>
        /// Sipariş durumu güncelleme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var dto = new SmartShop.Business.DTOs.UpdateOrderStatusDto
                {
                    OrderId = orderId,
                    Status = status
                };

                var result = await _orderService.UpdateStatusAsync(dto);
                
                if (result.Success)
                {
                    TempData["Success"] = "Sipariş durumu başarıyla güncellendi.";
                }
                else
                {
                    TempData["Error"] = result.Message ?? "Sipariş durumu güncellenirken bir hata oluştu.";
                }
                
                return RedirectToAction(nameof(OrderDetails), new { id = orderId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş durumu güncellenirken hata oluştu");
                TempData["Error"] = "Sipariş durumu güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Orders));
            }
        }

        /// <summary>
        /// Ürün oluşturma sayfası - GET
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu sayfaya erişim yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                // Kategorileri yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturma sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Ürün oluşturma - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(SmartShop.Business.DTOs.CreateProductDto productDto, IFormFile? imageFile)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            // Resim dosyası yükleme
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Dosya doğrulama
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
                    var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(extension))
                    {
                        var categoriesResult = await _categoryService.GetAllAsync();
                        ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                        TempData["Error"] = "Geçersiz dosya formatı. Sadece JPG, PNG, WEBP, GIF dosyaları yüklenebilir.";
                        return View(productDto);
                    }

                    // Dosya boyutu kontrolü (5MB)
                    if (imageFile.Length > 5 * 1024 * 1024)
                    {
                        var categoriesResult = await _categoryService.GetAllAsync();
                        ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                        TempData["Error"] = "Dosya boyutu 5MB'dan küçük olmalıdır.";
                        return View(productDto);
                    }

                    // Klasör oluştur
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    // Benzersiz dosya adı oluştur
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    // Dosyayı kaydet
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // URL'i oluştur
                    productDto.ImageUrl = $"/images/products/{uniqueFileName}";
                    
                    _logger.LogInformation("Ürün resmi yüklendi: {FileName}", uniqueFileName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Resim yüklenirken hata oluştu");
                    var categoriesResult = await _categoryService.GetAllAsync();
                    ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                    TempData["Error"] = "Resim yüklenirken bir hata oluştu.";
                    return View(productDto);
                }
            }

            if (!ModelState.IsValid)
            {
                // Kategorileri tekrar yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                
                TempData["Error"] = "Lütfen tüm gerekli alanları doldurun.";
                return View(productDto);
            }

            try
            {
                var result = await _productService.CreateAsync(productDto);
                
                if (result.Success)
                {
                    TempData["Success"] = "Ürün başarıyla oluşturuldu.";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    // Kategorileri tekrar yükle
                    var categoriesResult = await _categoryService.GetAllAsync();
                    ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                    
                    TempData["Error"] = result.Message ?? "Ürün oluşturulurken bir hata oluştu.";
                    return View(productDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün oluşturulurken hata oluştu");
                
                // Kategorileri tekrar yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                
                TempData["Error"] = "Ürün oluşturulurken bir hata oluştu.";
                return View(productDto);
            }
        }

        /// <summary>
        /// Ürün düzenleme sayfası - GET
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _productService.GetByIdAsync(id);
                
                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                    return RedirectToAction(nameof(Products));
                }

                // Kategorileri yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();

                // ProductDto'dan UpdateProductDto'ya dönüştür
                var updateDto = new SmartShop.Business.DTOs.UpdateProductDto
                {
                    ProductId = result.Data.ProductId,
                    Name = result.Data.Name,
                    Description = result.Data.Description,
                    Price = result.Data.Price,
                    StockQuantity = result.Data.StockQuantity,
                    CategoryId = result.Data.CategoryId,
                    Brand = result.Data.Brand,
                    Model = result.Data.Model,
                    
                    // Teknik Özellikler
                    Processor = result.Data.Processor,
                    RAM = result.Data.RAM,
                    Storage = result.Data.Storage,
                    StorageType = result.Data.StorageType,
                    GPU = result.Data.GPU,
                    ScreenSize = result.Data.ScreenSize,
                    Resolution = result.Data.Resolution,
                    Condition = result.Data.Condition,
                    Discount = result.Data.Discount,
                    
                    ImageUrl = result.Data.ImageUrl,
                    IsActive = result.Data.IsActive
                };

                return View(updateDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün düzenleme sayfası yüklenirken hata oluştu");
                TempData["Error"] = "Sayfa yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Products));
            }
        }

        /// <summary>
        /// Ürün düzenleme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(SmartShop.Business.DTOs.UpdateProductDto productDto)
        {
            if (!IsAdmin())
            {
                TempData["Error"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                // Kategorileri tekrar yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                
                TempData["Error"] = "Lütfen tüm gerekli alanları doldurun.";
                return View(productDto);
            }

            try
            {
                var result = await _productService.UpdateAsync(productDto);
                
                if (result.Success)
                {
                    TempData["Success"] = "Ürün başarıyla güncellendi.";
                    return RedirectToAction(nameof(Products));
                }
                else
                {
                    // Kategorileri tekrar yükle
                    var categoriesResult = await _categoryService.GetAllAsync();
                    ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                    
                    TempData["Error"] = result.Message ?? "Ürün güncellenirken bir hata oluştu.";
                    return View(productDto);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün güncellenirken hata oluştu");
                
                // Kategorileri tekrar yükle
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
                
                TempData["Error"] = "Ürün güncellenirken bir hata oluştu.";
                return View(productDto);
            }
        }

        /// <summary>
        /// Ürün silme - POST
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            if (!IsAdmin())
            {
                return Json(new { success = false, message = "Bu işlem için yetkiniz yok." });
            }

            try
            {
                var result = await _productService.DeleteAsync(id);
                
                if (result.Success)
                {
                    return Json(new { success = true, message = "Ürün başarıyla silindi." });
                }
                else
                {
                    return Json(new { success = false, message = result.Message ?? "Ürün silinirken bir hata oluştu." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün silinirken hata oluştu");
                return Json(new { success = false, message = "Ürün silinirken bir hata oluştu." });
            }
        }

        /// <summary>
        /// ML Fiyat Tahmini - POST
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> PredictPrice([FromBody] PricePredictionRequest request)
        {
            try
            {
                // ML Service API URL
                const string mlServiceUrl = "http://127.0.0.1:5000/predict";
                
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(10);
                
                var jsonContent = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        ram_gb = request.ram_gb,
                        ssd_gb = request.ssd_gb,
                        islemci = request.islemci,
                        ekran_karti = request.ekran_karti,
                        marka = request.marka
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );
                
                var response = await httpClient.PostAsync(mlServiceUrl, jsonContent);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var result = System.Text.Json.JsonSerializer.Deserialize<PricePredictionResponse>(responseContent);
                    
                    return Json(new { tahmini_fiyat = result?.tahmini_fiyat ?? 0 });
                }
                else
                {
                    _logger.LogError("ML Service returned error: {StatusCode}", response.StatusCode);
                    return BadRequest(new { error = "ML Service unavailable" });
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to ML Service");
                return BadRequest(new { error = "ML Service is not running. Please start the ML service first." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during price prediction");
                return BadRequest(new { error = "An error occurred during price prediction" });
            }
        }
        
        // ML Request/Response models
        public class PricePredictionRequest
        {
            public double ram_gb { get; set; }
            public double ssd_gb { get; set; }
            public string islemci { get; set; } = string.Empty;
            public string ekran_karti { get; set; } = string.Empty;
            public string marka { get; set; } = string.Empty;
        }
        
        public class PricePredictionResponse
        {
            public double tahmini_fiyat { get; set; }
            public string? model_version { get; set; }
        }
    }
}
