namespace SmartShop.Business.DTOs
{
    /// <summary>
    /// Sepet bilgilerini taşımak için kullanılan DTO
    /// </summary>
    public class CartDto
    {
        public int CartId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal ProductPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }
        public string? ProductImageUrl { get; set; }
        public int AvailableStock { get; set; }
        public DateTime AddedAt { get; set; }
    }

    /// <summary>
    /// Sepete ürün ekleme için kullanılan DTO
    /// </summary>
    public class AddToCartDto
    {
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    /// <summary>
    /// Sepet güncelleme için kullanılan DTO
    /// </summary>
    public class UpdateCartDto
    {
        public int CartId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>
    /// Kullanıcının sepet özeti
    /// </summary>
    public class CartSummaryDto
    {
        public int UserId { get; set; }
        public List<CartDto> Items { get; set; } = new();
        public int TotalItems { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Sepet öğesi bilgileri için DTO (view için optimize edilmiş)
    /// </summary>
    public class CartItemDto
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// Sepet görünümü için kullanılan DTO
    /// </summary>
    public class CartViewDto
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal TotalPrice { get; set; }
        public int TotalItems { get; set; }
    }
}
