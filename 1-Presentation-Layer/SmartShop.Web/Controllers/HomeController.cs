using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SmartShop.Web.Models;
using SmartShop.Business.Services;

namespace SmartShop.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public HomeController(
        ILogger<HomeController> logger, 
        IProductService productService,
        ICategoryService categoryService)
    {
        _logger = logger;
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            // Get featured/top rated products
            var topProducts = await _productService.GetTopRatedAsync(8);
            
            // Get all categories
            var categories = await _categoryService.GetAllAsync();
            
            ViewBag.FeaturedProducts = topProducts.Data ?? new List<SmartShop.Business.DTOs.ProductDto>();
            ViewBag.Categories = categories.Data ?? new List<SmartShop.Business.DTOs.CategoryDto>();
            
            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page");
            return View();
        }
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
