using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.DTOs;
using SmartShop.Business.Services;
using System.Text.Json;

namespace SmartShop.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartService cartService,
            IProductService productService,
            ILogger<CartController> logger)
        {
            _cartService = cartService;
            _productService = productService;
            _logger = logger;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    TempData["Warning"] = "Sepetinizi görüntülemek için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _cartService.GetUserCartAsync(userId);

                if (!result.Success || result.Data == null)
                {
                    TempData["Error"] = result.Message;
                    return View(new CartViewDto());
                }

                // CartSummaryDto'yu CartViewDto'ya dönüştür
                var cartViewDto = new CartViewDto
                {
                    Items = result.Data.Items?.Select(item => new CartItemDto
                    {
                        CartId = item.CartId,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        CategoryName = item.CategoryName,
                        Price = item.ProductPrice,
                        Quantity = item.Quantity,
                        ImageUrl = item.ProductImageUrl
                    }).ToList() ?? new List<CartItemDto>(),
                    TotalPrice = result.Data.TotalAmount,
                    TotalItems = result.Data.TotalItems
                };

                return View(cartViewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet görüntülenirken hata oluştu");
                TempData["Error"] = "Sepet yüklenirken bir hata oluştu.";
                return View(new CartViewDto());
            }
        }

        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    TempData["Warning"] = "Sepete ürün eklemek için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Request.Headers["Referer"].ToString() });
                }

                var dto = new AddToCartDto
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity
                };

                var result = await _cartService.AddToCartAsync(dto);

                if (result.Success)
                {
                    TempData["Success"] = "Ürün sepete eklendi.";
                    await UpdateCartItemCountAsync(userId);
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                // Referer varsa oraya dön, yoksa Products/Index'e git
                var returnUrl = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrEmpty(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                return RedirectToAction("Index", "Products");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepete ürün eklenirken hata oluştu. ProductId: {ProductId}", productId);
                TempData["Error"] = "Ürün sepete eklenirken bir hata oluştu.";
                return RedirectToAction("Index", "Products");
            }
        }

        // POST: Cart/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    TempData["Warning"] = "İşlem için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account");
                }

                // Önce kullanıcının sepetindeki bu ürünü bul
                var cartResult = await _cartService.GetUserCartAsync(userId);
                if (!cartResult.Success)
                {
                    TempData["Error"] = "Sepet bilgisi alınamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var cartItem = cartResult.Data?.Items?.FirstOrDefault(c => c.ProductId == productId);
                if (cartItem == null)
                {
                    TempData["Error"] = "Ürün sepetinizde bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                var result = await _cartService.RemoveFromCartAsync(cartItem.CartId);

                if (result.Success)
                {
                    TempData["Success"] = "Ürün sepetten kaldırıldı.";
                    await UpdateCartItemCountAsync(userId);
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün sepetten kaldırılırken hata oluştu. ProductId: {ProductId}", productId);
                TempData["Error"] = "Ürün sepetten kaldırılırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cart/UpdateQuantity  
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    TempData["Warning"] = "İşlem için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account");
                }

                if (quantity <= 0)
                {
                    TempData["Error"] = "Miktar 0'dan büyük olmalıdır.";
                    return RedirectToAction(nameof(Index));
                }

                var dto = new UpdateCartDto
                {
                    CartId = cartId,
                    Quantity = quantity
                };

                var result = await _cartService.UpdateCartItemAsync(dto);

                if (result.Success)
                {
                    TempData["Success"] = "Sepet güncellendi.";
                    await UpdateCartItemCountAsync(userId);
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet güncellenirken hata oluştu. CartId: {CartId}", cartId);
                TempData["Error"] = "Sepet güncellenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var userId = GetUserId();
                if (userId == 0)
                {
                    TempData["Warning"] = "İşlem için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account");
                }

                var result = await _cartService.ClearCartAsync(userId);

                if (result.Success)
                {
                    TempData["Success"] = "Sepet temizlendi.";
                }
                else
                {
                    TempData["Error"] = result.Message;
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepet temizlenirken hata oluştu");
                TempData["Error"] = "Sepet temizlenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // Helper method to get user ID from session
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
