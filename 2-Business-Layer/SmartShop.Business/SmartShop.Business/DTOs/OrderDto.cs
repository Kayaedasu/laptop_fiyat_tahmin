namespace SmartShop.Business.DTOs
{
    /// <summary>
    /// Sipariş bilgilerini taşımak için kullanılan DTO
    /// </summary>
    public class OrderDto
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TrackingNumber { get; set; }
        public List<OrderDetailDto> OrderDetails { get; set; } = new();
    }

    /// <summary>
    /// Sipariş listesi için kullanılan basitleştirilmiş DTO
    /// </summary>
    public class OrderListDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
    }

    /// <summary>
    /// Sipariş detay bilgileri için DTO (OrderService için)
    /// </summary>
    public class OrderDetailDto
    {
        public int OrderDetailId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }
    }

    /// <summary>
    /// Sipariş detay sayfası için kullanılan DTO (View için)
    /// </summary>
    public class OrderDetailViewDto
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public List<OrderItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Sipariş öğesi bilgileri için DTO
    /// </summary>
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// Checkout sayfası için kullanılan DTO
    /// </summary>
    public class CheckoutDto
    {
        public decimal TotalAmount { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
    }

    /// <summary>
    /// Sipariş oluşturma için kullanılan DTO
    /// </summary>
    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<CreateOrderDetailDto> OrderDetails { get; set; } = new();
    }

    /// <summary>
    /// Sipariş detay oluşturma için DTO
    /// </summary>
    public class CreateOrderDetailDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Sipariş durumu güncelleme için DTO
    /// </summary>
    public class UpdateOrderStatusDto
    {
        public int OrderId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
    }
}
