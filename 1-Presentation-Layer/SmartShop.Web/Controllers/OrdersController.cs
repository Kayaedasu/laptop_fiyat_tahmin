using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.DTOs;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(
            IOrderService orderService,
            ICartService cartService,
            ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _cartService = cartService;
            _logger = logger;
        }

        // GET: Orders/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Siparişlerinizi görüntülemek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _orderService.GetByUserIdAsync(userId);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return View(new List<OrderDto>());
                }

                // OrderDto'yu OrderListDto'ya dönüştür
                var orderList = result.Data?.Select(o => new OrderListDto
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    ItemCount = o.OrderDetails?.Count ?? 0
                }).ToList() ?? new List<OrderListDto>();

                return View(orderList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Siparişler listelenirken hata oluştu. UserId: {UserId}", userId);
                TempData["Error"] = "Siparişler yüklenirken bir hata oluştu.";
                return View(new List<OrderListDto>());
            }
        }

        // GET: Orders/OrderDetails/5
        public async Task<IActionResult> OrderDetails(int id)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Sipariş detayını görüntülemek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _orderService.GetByIdAsync(id);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(MyOrders));
                }

                // Siparişin kullanıcıya ait olup olmadığını kontrol et
                if (result.Data!.UserId != userId)
                {
                    TempData["Error"] = "Bu siparişi görüntüleme yetkiniz yok.";
                    return RedirectToAction(nameof(MyOrders));
                }

                // OrderDto'yu OrderDetailViewDto'ya dönüştür
                var orderDetail = new OrderDetailViewDto
                {
                    OrderId = result.Data.OrderId,
                    OrderDate = result.Data.OrderDate,
                    TotalAmount = result.Data.TotalAmount,
                    Status = result.Data.Status,
                    ShippingAddress = result.Data.ShippingAddress,
                    PaymentMethod = result.Data.PaymentMethod,
                    Items = result.Data.OrderDetails?.Select(od => new OrderItemDto
                    {
                        ProductId = od.ProductId,
                        ProductName = od.ProductName,
                        CategoryName = "", // OrderDetailDto'da kategori yok
                        Quantity = od.Quantity,
                        Price = od.UnitPrice,
                        ImageUrl = null // OrderDetailDto'da resim yok
                    }).ToList() ?? new List<OrderItemDto>()
                };

                return View(orderDetail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş detayı getirilirken hata oluştu. OrderId: {OrderId}", id);
                TempData["Error"] = "Sipariş detayı yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(MyOrders));
            }
        }

        // GET: Orders/Checkout
        public async Task<IActionResult> Checkout()
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Sipariş vermek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders/Checkout" });
            }

            try
            {
                // Sepet bilgilerini getir
                var cartResult = await _cartService.GetUserCartAsync(userId);

                if (!cartResult.Success || cartResult.Data!.Items.Count == 0)
                {
                    TempData["Warning"] = "Sepetiniz boş. Sipariş vermek için ürün ekleyin.";
                    return RedirectToAction("Index", "Cart");
                }

                // CheckoutDto oluştur
                var checkoutDto = new CheckoutDto
                {
                    TotalAmount = cartResult.Data.TotalAmount,
                    Items = cartResult.Data.Items.Select(c => new CartItemDto
                    {
                        ProductId = c.ProductId,
                        ProductName = c.ProductName,
                        CategoryName = "", // CartDto'da kategori yok
                        Price = c.ProductPrice,
                        Quantity = c.Quantity,
                        ImageUrl = c.ProductImageUrl
                    }).ToList()
                };

                // Kullanıcı bilgilerini session'dan al
                ViewBag.UserName = HttpContext.Session.GetString("UserName") ?? "Kullanıcı";
                ViewBag.UserEmail = HttpContext.Session.GetString("UserEmail") ?? "";

                return View(checkoutDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Checkout sayfası yüklenirken hata oluştu. UserId: {UserId}", userId);
                TempData["Error"] = "Ödeme sayfası yüklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // POST: Orders/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string shippingAddress, string paymentMethod, string? notes)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "Sipariş vermek için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account");
            }

            if (string.IsNullOrWhiteSpace(shippingAddress))
            {
                TempData["Error"] = "Teslimat adresi boş olamaz.";
                return RedirectToAction(nameof(Checkout));
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                TempData["Error"] = "Ödeme yöntemi seçilmelidir.";
                return RedirectToAction(nameof(Checkout));
            }

            try
            {
                // Sepetteki ürünleri al
                var cartResult = await _cartService.GetUserCartAsync(userId);

                if (!cartResult.Success || cartResult.Data!.Items.Count == 0)
                {
                    TempData["Warning"] = "Sepetiniz boş.";
                    return RedirectToAction("Index", "Cart");
                }

                // Sipariş DTO'sunu oluştur
                var orderDto = new CreateOrderDto
                {
                    UserId = userId,
                    ShippingAddress = shippingAddress,
                    PaymentMethod = paymentMethod,
                    OrderDetails = cartResult.Data.Items.Select(item => new CreateOrderDetailDto
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity
                    }).ToList()
                };

                // Siparişi oluştur
                var result = await _orderService.CreateAsync(orderDto);

                if (!result.Success)
                {
                    TempData["Error"] = result.Message;
                    if (result.Errors.Any())
                    {
                        foreach (var error in result.Errors)
                        {
                            _logger.LogWarning("Sipariş oluşturma hatası: {Error}", error);
                        }
                    }
                    return RedirectToAction(nameof(Checkout));
                }

                // Sipariş başarılı, sepeti temizle
                await _cartService.ClearCartAsync(userId);

                // Session'daki sepet sayısını sıfırla
                HttpContext.Session.SetInt32("CartItemCount", 0);

                ViewBag.OrderId = result.Data!.OrderId;
                return View("OrderConfirmation");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş oluşturulurken hata oluştu. UserId: {UserId}", userId);
                TempData["Error"] = "Sipariş oluşturulurken bir hata oluştu. Lütfen tekrar deneyin.";
                return RedirectToAction(nameof(Checkout));
            }
        }

        // POST: Orders/CancelOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = GetUserId();
            if (userId == 0)
            {
                TempData["Warning"] = "İşlem için giriş yapmalısınız.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var result = await _orderService.CancelOrderAsync(id, userId);

                if (result.Success)
                {
                    TempData["Success"] = "Siparişiniz iptal edildi.";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(OrderDetails), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sipariş iptal edilirken hata oluştu. OrderId: {OrderId}", id);
                TempData["Error"] = "Sipariş iptal edilirken bir hata oluştu.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }
        }

        // Helper method
        private int GetUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }
    }
}
