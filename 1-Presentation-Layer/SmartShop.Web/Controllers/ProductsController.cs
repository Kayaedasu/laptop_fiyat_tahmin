using Microsoft.AspNetCore.Mvc;
using SmartShop.Business.DTOs;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;
        private readonly ICartService _cartService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IReviewService reviewService,
            ICartService cartService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
            _cartService = cartService;
            _logger = logger;
        }

        // GET: Products
        public async Task<IActionResult> Index(int? categoryId, string? searchTerm, int pageNumber = 1)
        {
            try
            {
                var filter = new ProductFilterDto
                {
                    CategoryId = categoryId,
                    SearchTerm = searchTerm,
                    IsActive = true,
                    PageNumber = pageNumber,
                    PageSize = 12,
                    SortBy = "Name"
                };

                var result = await _productService.SearchAsync(filter);

                _logger.LogInformation($"ProductService.SearchAsync - Success: {result.Success}, Data Count: {result.Data?.Count ?? 0}");

                if (!result.Success)
                {
                    _logger.LogWarning($"Product search failed: {result.Message}");
                    TempData["Error"] = result.Message;
                    return View(new List<ProductDto>());
                }

                // Kategorileri getir
                var categoriesResult = await _categoryService.GetAllAsync();
                ViewBag.Categories = categoriesResult.Data ?? new List<CategoryDto>();
                ViewBag.CurrentCategoryId = categoryId;
                ViewBag.SearchTerm = searchTerm;

                var products = result.Data ?? new List<ProductDto>();
                _logger.LogInformation($"Returning {products.Count} products to view");

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürünler listelenirken hata oluştu");
                TempData["Error"] = "Ürünler yüklenirken bir hata oluştu.";
                return View(new List<ProductDto>());
            }
        }

        // GET: Products/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation($"Details page requested for ProductId: {id}");

                var result = await _productService.GetDetailsByIdAsync(id);

                if (!result.Success)
                {
                    _logger.LogWarning($"Product details not found: {id}");
                    TempData["Error"] = result.Message;
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation($"Returning product details: {result.Data?.Name}");
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün detayı getirilirken hata oluştu. ID: {ProductId}", id);
                TempData["Error"] = "Ürün detayı yüklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Products/Search
        public IActionResult Search(string searchTerm)
        {
            return RedirectToAction(nameof(Index), new { searchTerm });
        }

        // GET: Products/Category/5
        public IActionResult Category(int id)
        {
            return RedirectToAction(nameof(Index), new { categoryId = id });
        }

        // GET: Products/Compare
        public async Task<IActionResult> Compare(string ids)
        {
            try
            {
                if (string.IsNullOrEmpty(ids))
                {
                    TempData["Warning"] = "Karşılaştırılacak ürün seçilmedi.";
                    return RedirectToAction(nameof(Index));
                }

                var productIds = ids.Split(',').Select(int.Parse).ToList();
                var products = new List<ProductDto>();

                foreach (var id in productIds)
                {
                    var result = await _productService.GetByIdAsync(id);
                    if (result.Success && result.Data != null)
                    {
                        products.Add(result.Data);
                    }
                }

                if (products.Count < 2)
                {
                    TempData["Warning"] = "Karşılaştırma için en az 2 ürün seçilmelidir.";
                    return RedirectToAction(nameof(Index));
                }

                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ürün karşılaştırma hatası. IDs: {Ids}", ids);
                TempData["Error"] = "Ürünler karşılaştırılırken bir hata oluştu.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Products/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kullanıcı kontrolü
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Warning"] = "Sepete ürün eklemek için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", new { id = productId }) });
                }

                // Ürünü kontrol et
                var productResult = await _productService.GetByIdAsync(productId);
                if (!productResult.Success || productResult.Data == null)
                {
                    TempData["Error"] = "Ürün bulunamadı.";
                    return RedirectToAction(nameof(Index));
                }

                // Stok kontrolü
                if (productResult.Data.StockQuantity < quantity)
                {
                    TempData["Error"] = "Yeterli stok yok.";
                    return RedirectToAction(nameof(Details), new { id = productId });
                }

                // Sepete ekle
                var addToCartDto = new AddToCartDto
                {
                    UserId = userId.Value,
                    ProductId = productId,
                    Quantity = quantity
                };

                var result = await _cartService.AddToCartAsync(addToCartDto);

                if (result.Success)
                {
                    // Session'daki sepet sayısını güncelle
                    var cartCountResult = await _cartService.GetCartItemCountAsync(userId.Value);
                    if (cartCountResult.Success)
                    {
                        HttpContext.Session.SetInt32("CartItemCount", cartCountResult.Data);
                    }

                    TempData["Success"] = $"{productResult.Data.Name} sepete eklendi.";
                    return RedirectToAction("Index", "Cart");
                }
                else
                {
                    TempData["Error"] = result.Message ?? "Ürün sepete eklenirken bir hata oluştu.";
                    return RedirectToAction(nameof(Details), new { id = productId });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sepete ürün eklenirken hata oluştu. ProductId: {ProductId}", productId);
                TempData["Error"] = "Ürün sepete eklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }
        }

        // POST: Products/AddReview
        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment)
        {
            try
            {
                // Kullanıcı kontrolü
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId == null)
                {
                    TempData["Error"] = "Yorum yapabilmek için giriş yapmalısınız.";
                    return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Details", new { id = productId }) });
                }

                var createReviewDto = new CreateReviewDto
                {
                    ProductId = productId,
                    UserId = userId.Value,
                    Rating = rating,
                    Comment = comment
                };

                var result = await _reviewService.CreateAsync(createReviewDto);

                if (result.Success)
                {
                    TempData["Success"] = "Yorumunuz başarıyla eklendi.";
                }
                else
                {
                    TempData["Error"] = result.Message ?? "Yorum eklenirken bir hata oluştu.";
                }

                return RedirectToAction(nameof(Details), new { id = productId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Yorum eklenirken hata oluştu. ProductId: {ProductId}, UserId: {UserId}",
                    productId, HttpContext.Session.GetInt32("UserId"));
                TempData["Error"] = "Yorum eklenirken bir hata oluştu.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }
        }
    }
}
