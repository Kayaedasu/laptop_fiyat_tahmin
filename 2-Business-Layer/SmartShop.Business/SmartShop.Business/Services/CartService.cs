using Microsoft.EntityFrameworkCore;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Sepet iş mantığı servisi
    /// </summary>
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CartService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<CartSummaryDto>> GetUserCartAsync(int userId)
        {
            try
            {
                var cartItems = await _unitOfWork.Carts.GetAllQuery()
                    .Include(c => c.Product)
                        .ThenInclude(p => p.Category)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                var cartDtos = cartItems.Select(c => new CartDto
                {
                    CartId = c.CartId,
                    UserId = c.UserId,
                    ProductId = c.ProductId,
                    ProductName = c.Product?.Name ?? string.Empty,
                    ProductPrice = c.Product?.Price ?? 0,
                    Quantity = c.Quantity,
                    Subtotal = (c.Product?.Price ?? 0) * c.Quantity,
                    ProductImageUrl = c.Product?.ImageUrl,
                    AvailableStock = c.Product?.Stock ?? 0,
                    AddedAt = c.AddedAt,
                    CategoryName = c.Product?.Category?.Name ?? string.Empty // Category bilgisi eklendi
                }).ToList();

                var summary = new CartSummaryDto
                {
                    UserId = userId,
                    Items = cartDtos,
                    TotalItems = cartDtos.Sum(c => c.Quantity),
                    TotalAmount = cartDtos.Sum(c => c.Subtotal)
                };

                return ServiceResult<CartSummaryDto>.SuccessResult(summary);
            }
            catch (Exception ex)
            {
                return ServiceResult<CartSummaryDto>.FailureResult(
                    "Sepet getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<CartDto>> AddToCartAsync(AddToCartDto dto)
        {
            try
            {
                if (dto.UserId <= 0)
                    return ServiceResult<CartDto>.FailureResult("Geçerli bir kullanıcı ID'si gereklidir.");

                if (dto.ProductId <= 0)
                    return ServiceResult<CartDto>.FailureResult("Geçerli bir ürün ID'si gereklidir.");

                if (dto.Quantity <= 0)
                    return ServiceResult<CartDto>.FailureResult("Miktar 0'dan büyük olmalıdır.");

                // Kullanıcı kontrolü
                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult<CartDto>.FailureResult("Kullanıcı bulunamadı.");

                if (!user.IsActive)
                    return ServiceResult<CartDto>.FailureResult("Kullanıcı hesabı aktif değil.");

                // Ürün kontrolü
                var product = await _unitOfWork.Products.GetByIdAsync(dto.ProductId);
                if (product == null)
                    return ServiceResult<CartDto>.FailureResult("Ürün bulunamadı.");

                if (!product.IsActive)
                    return ServiceResult<CartDto>.FailureResult("Bu ürün artık satışta değil.");

                if (product.Stock < dto.Quantity)
                    return ServiceResult<CartDto>.FailureResult($"Yetersiz stok. Mevcut: {product.Stock}");

                // Sepette zaten var mı kontrol et
                var existingCartItem = await _unitOfWork.Carts.GetAllQuery()
                    .FirstOrDefaultAsync(c => c.UserId == dto.UserId && c.ProductId == dto.ProductId);

                Cart cartItem;

                if (existingCartItem != null)
                {
                    // Sepette varsa miktarı artır
                    var newQuantity = existingCartItem.Quantity + dto.Quantity;

                    if (product.Stock < newQuantity)
                        return ServiceResult<CartDto>.FailureResult(
                            $"Yetersiz stok. Sepetinizde {existingCartItem.Quantity} adet var. Toplam {newQuantity} adet istediniz. Mevcut: {product.Stock}");

                    existingCartItem.Quantity = newQuantity;
                    _unitOfWork.Carts.Update(existingCartItem);
                    cartItem = existingCartItem;
                }
                else
                {
                    // Yeni ekle
                    cartItem = new Cart
                    {
                        UserId = dto.UserId,
                        ProductId = dto.ProductId,
                        Quantity = dto.Quantity,
                        AddedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Carts.AddAsync(cartItem);
                }

                await _unitOfWork.CommitAsync();

                // Ürün bilgilerini tekrar yükle
                cartItem = await _unitOfWork.Carts.GetAllQuery()
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.CartId == cartItem.CartId);

                var resultDto = new CartDto
                {
                    CartId = cartItem!.CartId,
                    UserId = cartItem.UserId,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product?.Name ?? string.Empty,
                    ProductPrice = cartItem.Product?.Price ?? 0,
                    Quantity = cartItem.Quantity,
                    Subtotal = (cartItem.Product?.Price ?? 0) * cartItem.Quantity,
                    ProductImageUrl = cartItem.Product?.ImageUrl,
                    AvailableStock = cartItem.Product?.Stock ?? 0,
                    AddedAt = cartItem.AddedAt
                };

                return ServiceResult<CartDto>.SuccessResult(resultDto, "Ürün sepete eklendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<CartDto>.FailureResult(
                    "Ürün sepete eklenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<CartDto>> UpdateCartItemAsync(UpdateCartDto dto)
        {
            try
            {
                if (dto.CartId <= 0)
                    return ServiceResult<CartDto>.FailureResult("Geçerli bir sepet ID'si gereklidir.");

                if (dto.Quantity <= 0)
                    return ServiceResult<CartDto>.FailureResult("Miktar 0'dan büyük olmalıdır.");

                var cartItem = await _unitOfWork.Carts.GetAllQuery()
                    .Include(c => c.Product)
                    .FirstOrDefaultAsync(c => c.CartId == dto.CartId);

                if (cartItem == null)
                    return ServiceResult<CartDto>.FailureResult("Sepet öğesi bulunamadı.");

                // Ürün kontrolü
                if (cartItem.Product == null)
                    return ServiceResult<CartDto>.FailureResult("Ürün bulunamadı.");

                if (!cartItem.Product.IsActive)
                    return ServiceResult<CartDto>.FailureResult("Bu ürün artık satışta değil.");

                if (cartItem.Product.Stock < dto.Quantity)
                    return ServiceResult<CartDto>.FailureResult($"Yetersiz stok. Mevcut: {cartItem.Product.Stock}");

                cartItem.Quantity = dto.Quantity;
                _unitOfWork.Carts.Update(cartItem);
                await _unitOfWork.CommitAsync();

                var resultDto = new CartDto
                {
                    CartId = cartItem.CartId,
                    UserId = cartItem.UserId,
                    ProductId = cartItem.ProductId,
                    ProductName = cartItem.Product.Name,
                    ProductPrice = cartItem.Product.Price,
                    Quantity = cartItem.Quantity,
                    Subtotal = cartItem.Product.Price * cartItem.Quantity,
                    ProductImageUrl = cartItem.Product.ImageUrl,
                    AvailableStock = cartItem.Product.Stock,
                    AddedAt = cartItem.AddedAt
                };

                return ServiceResult<CartDto>.SuccessResult(resultDto, "Sepet güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult<CartDto>.FailureResult(
                    "Sepet güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> RemoveFromCartAsync(int cartId)
        {
            try
            {
                if (cartId <= 0)
                    return ServiceResult.FailureResult("Geçerli bir sepet ID'si gereklidir.");

                var cartItem = await _unitOfWork.Carts.GetByIdAsync(cartId);
                if (cartItem == null)
                    return ServiceResult.FailureResult("Sepet öğesi bulunamadı.");

                _unitOfWork.Carts.Remove(cartItem);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Ürün sepetten kaldırıldı.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Ürün sepetten kaldırılırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> ClearCartAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return ServiceResult.FailureResult("Geçerli bir kullanıcı ID'si gereklidir.");

                var cartItems = await _unitOfWork.Carts.GetAllQuery()
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                if (!cartItems.Any())
                    return ServiceResult.SuccessResult("Sepet zaten boş.");

                _unitOfWork.Carts.RemoveRange(cartItems);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Sepet temizlendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Sepet temizlenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<int>> GetCartItemCountAsync(int userId)
        {
            try
            {
                if (userId <= 0)
                    return ServiceResult<int>.FailureResult("Geçerli bir kullanıcı ID'si gereklidir.");

                var count = await _unitOfWork.Carts.GetAllQuery()
                    .Where(c => c.UserId == userId)
                    .SumAsync(c => c.Quantity);

                return ServiceResult<int>.SuccessResult(count);
            }
            catch (Exception ex)
            {
                return ServiceResult<int>.FailureResult(
                    "Sepet ürün sayısı hesaplanırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<bool>> IsProductInCartAsync(int userId, int productId)
        {
            try
            {
                if (userId <= 0)
                    return ServiceResult<bool>.FailureResult("Geçerli bir kullanıcı ID'si gereklidir.");

                if (productId <= 0)
                    return ServiceResult<bool>.FailureResult("Geçerli bir ürün ID'si gereklidir.");

                var exists = await _unitOfWork.Carts.GetAllQuery()
                    .AnyAsync(c => c.UserId == userId && c.ProductId == productId);

                return ServiceResult<bool>.SuccessResult(exists);
            }
            catch (Exception ex)
            {
                return ServiceResult<bool>.FailureResult(
                    "Sepet kontrolü yapılırken bir hata oluştu.", 
                    ex.Message);
            }
        }
    }
}
