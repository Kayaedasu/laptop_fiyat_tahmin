using Microsoft.EntityFrameworkCore;
using SmartShop.Business.Common;
using SmartShop.Business.DTOs;
using SmartShop.Business.Validators;
using SmartShop.DataAccess.Models;
using SmartShop.DataAccess.UnitOfWork;

namespace SmartShop.Business.Services
{
    /// <summary>
    /// Sipariş iş mantığı servisi
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrderService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ServiceResult<OrderDto>> GetByIdAsync(int orderId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                
                if (order == null)
                    return ServiceResult<OrderDto>.FailureResult("Sipariş bulunamadı.");

                var dto = MapToDto(order);
                return ServiceResult<OrderDto>.SuccessResult(dto);
            }
            catch (Exception ex)
            {
                return ServiceResult<OrderDto>.FailureResult(
                    "Sipariş getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<OrderDto>>> GetByUserIdAsync(int userId)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetUserOrdersAsync(userId);
                var dtos = orders.Select(MapToDto).ToList();
                
                return ServiceResult<List<OrderDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<OrderDto>>.FailureResult(
                    "Kullanıcı siparişleri getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<OrderDto>>> GetAllAsync()
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllQuery()
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var dtos = orders.Select(MapToDto).ToList();
                return ServiceResult<List<OrderDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<OrderDto>>.FailureResult(
                    "Siparişler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<OrderDto>> CreateAsync(CreateOrderDto dto)
        {
            try
            {
                var validationErrors = OrderValidator.ValidateCreate(dto);
                if (validationErrors.Any())
                    return ServiceResult<OrderDto>.FailureResult(
                        "Sipariş oluşturma verileri geçersiz.", 
                        validationErrors);

                // Kullanıcı kontrolü
                var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId);
                if (user == null)
                    return ServiceResult<OrderDto>.FailureResult("Kullanıcı bulunamadı.");

                if (!user.IsActive)
                    return ServiceResult<OrderDto>.FailureResult("Kullanıcı hesabı aktif değil.");

                // Toplam tutarı hesapla ve stok kontrolü yap
                decimal totalAmount = 0;
                var orderDetails = new List<OrderDetail>();

                foreach (var detail in dto.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                    
                    if (product == null)
                        return ServiceResult<OrderDto>.FailureResult($"Ürün bulunamadı (ID: {detail.ProductId}).");

                    if (!product.IsActive)
                        return ServiceResult<OrderDto>.FailureResult($"{product.Name} artık satışta değil.");

                    if (product.Stock < detail.Quantity)
                        return ServiceResult<OrderDto>.FailureResult(
                            $"{product.Name} için yetersiz stok. Mevcut: {product.Stock}");

                    var subtotal = product.Price * detail.Quantity;
                    totalAmount += subtotal;

                    orderDetails.Add(new OrderDetail
                    {
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        UnitPrice = product.Price,
                        Subtotal = subtotal
                    });
                }

                // Sipariş oluştur
                var order = new Order
                {
                    UserId = dto.UserId,
                    OrderDate = DateTime.UtcNow,
                    TotalAmount = totalAmount,
                    DiscountAmount = 0, // Şimdilik indirim yok
                    FinalAmount = totalAmount, // FinalAmount = TotalAmount - DiscountAmount
                    Status = "Pending", // ENUM: Pending, Processing, Shipped, Delivered, Cancelled
                    ShippingAddress = dto.ShippingAddress,
                    PaymentMethod = dto.PaymentMethod,
                    PaymentStatus = "Pending",
                    OrderDetails = orderDetails
                };

                await _unitOfWork.Orders.AddAsync(order);

                // Stokları güncelle
                foreach (var detail in dto.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                    product!.Stock -= detail.Quantity;
                    _unitOfWork.Products.Update(product);
                }

                await _unitOfWork.CommitAsync();

                // Detaylı sipariş bilgisini getir
                var createdOrder = await _unitOfWork.Orders.GetOrderWithDetailsAsync(order.OrderId);
                var resultDto = MapToDto(createdOrder!);
                
                return ServiceResult<OrderDto>.SuccessResult(resultDto, "Sipariş başarıyla oluşturuldu.");
            }
            catch (Exception ex)
            {
                return ServiceResult<OrderDto>.FailureResult(
                    "Sipariş oluşturulurken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> UpdateStatusAsync(UpdateOrderStatusDto dto)
        {
            try
            {
                var validationErrors = OrderValidator.ValidateUpdateStatus(dto);
                if (validationErrors.Any())
                    return ServiceResult.FailureResult(
                        "Sipariş durumu güncelleme verileri geçersiz.", 
                        validationErrors);

                var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId);
                if (order == null)
                    return ServiceResult.FailureResult("Sipariş bulunamadı.");

                // İptal edilmiş siparişin durumu değiştirilemez
                if (order.Status == "Cancelled")
                    return ServiceResult.FailureResult("İptal edilmiş siparişin durumu değiştirilemez.");

                order.Status = dto.Status;
                
                // TrackingNumber Order modelinde yok, şimdilik yorum satırına aldık
                // if (!string.IsNullOrWhiteSpace(dto.TrackingNumber))
                //     order.TrackingNumber = dto.TrackingNumber;

                _unitOfWork.Orders.Update(order);
                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Sipariş durumu başarıyla güncellendi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Sipariş durumu güncellenirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult> CancelOrderAsync(int orderId, int userId)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetOrderWithDetailsAsync(orderId);
                
                if (order == null)
                    return ServiceResult.FailureResult("Sipariş bulunamadı.");

                if (order.UserId != userId)
                    return ServiceResult.FailureResult("Bu siparişi iptal etme yetkiniz yok.");

                if (order.Status == "İptal Edildi")
                    return ServiceResult.FailureResult("Sipariş zaten iptal edilmiş.");

                if (order.Status == "Kargoda" || order.Status == "Teslim Edildi")
                    return ServiceResult.FailureResult("Bu aşamadaki sipariş iptal edilemez.");

                order.Status = "İptal Edildi";
                _unitOfWork.Orders.Update(order);

                // Stokları geri ekle
                foreach (var detail in order.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                    if (product != null)
                    {
                        product.Stock += detail.Quantity;
                        _unitOfWork.Products.Update(product);
                    }
                }

                await _unitOfWork.CommitAsync();

                return ServiceResult.SuccessResult("Sipariş başarıyla iptal edildi.");
            }
            catch (Exception ex)
            {
                return ServiceResult.FailureResult(
                    "Sipariş iptal edilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<decimal>> CalculateOrderTotalAsync(CreateOrderDto dto)
        {
            try
            {
                decimal total = 0;

                foreach (var detail in dto.OrderDetails)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(detail.ProductId);
                    if (product == null)
                        return ServiceResult<decimal>.FailureResult($"Ürün bulunamadı (ID: {detail.ProductId}).");

                    total += product.Price * detail.Quantity;
                }

                return ServiceResult<decimal>.SuccessResult(total);
            }
            catch (Exception ex)
            {
                return ServiceResult<decimal>.FailureResult(
                    "Sipariş tutarı hesaplanırken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<OrderDto>>> GetOrdersByStatusAsync(string status)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetAllQuery()
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .Where(o => o.Status == status)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                var dtos = orders.Select(MapToDto).ToList();
                return ServiceResult<List<OrderDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<OrderDto>>.FailureResult(
                    "Siparişler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        public async Task<ServiceResult<List<OrderDto>>> GetRecentOrdersAsync(int count = 10)
        {
            try
            {
                var orders = await _unitOfWork.Orders.GetRecentOrdersAsync(count);
                var dtos = orders.Select(MapToDto).ToList();
                
                return ServiceResult<List<OrderDto>>.SuccessResult(dtos);
            }
            catch (Exception ex)
            {
                return ServiceResult<List<OrderDto>>.FailureResult(
                    "Son siparişler getirilirken bir hata oluştu.", 
                    ex.Message);
            }
        }

        private OrderDto MapToDto(Order order)
        {
            return new OrderDto
            {
                OrderId = order.OrderId,
                UserId = order.UserId,
                UserName = order.User != null ? 
                    $"{order.User.FirstName} {order.User.LastName}" : string.Empty,
                UserEmail = order.User?.Email ?? string.Empty,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                PaymentMethod = order.PaymentMethod,
                TrackingNumber = null, // Order modelinde TrackingNumber yok
                OrderDetails = order.OrderDetails?.Select(od => new OrderDetailDto
                {
                    OrderDetailId = od.OrderDetailId,
                    ProductId = od.ProductId,
                    ProductName = od.Product?.Name ?? string.Empty,
                    Quantity = od.Quantity,
                    UnitPrice = od.UnitPrice,
                    Subtotal = od.Subtotal
                }).ToList() ?? new List<OrderDetailDto>()
            };
        }
    }
}
