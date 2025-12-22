namespace SmartShop.Business.DTOs
{
    /// <summary>
    /// Ürün bilgilerini taşımak için kullanılan DTO
    /// </summary>
    public class ProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        
        // Teknik Özellikler
        public string? Processor { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string? StorageType { get; set; }
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public string? Condition { get; set; }
        public decimal Discount { get; set; }
        
        public string? Specifications { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Ürün oluşturma/güncelleme için kullanılan DTO
    /// </summary>
    public class CreateProductDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        
        // Teknik Özellikler (Product entity ile uyumlu)
        public string? Processor { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string? StorageType { get; set; } = "SSD";
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public string? Condition { get; set; } = "New";
        public decimal Discount { get; set; } = 0;
        
        public string? Specifications { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;
    }

    /// <summary>
    /// Ürün güncelleme için kullanılan DTO
    /// </summary>
    public class UpdateProductDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        
        // Teknik Özellikler (CreateProductDto ile uyumlu)
        public string? Processor { get; set; }
        public int RAM { get; set; }
        public int Storage { get; set; }
        public string? StorageType { get; set; } = "SSD";
        public string? GPU { get; set; }
        public decimal? ScreenSize { get; set; }
        public string? Resolution { get; set; }
        public string? Condition { get; set; } = "New";
        public decimal Discount { get; set; } = 0;
        
        public string? Specifications { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }

    /// <summary>
    /// Ürün filtreleme için kullanılan DTO
    /// </summary>
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Brand { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsActive { get; set; } = true;
        public string? SortBy { get; set; } = "Name"; // Name, Price, CreatedAt
        public bool IsDescending { get; set; } = false;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

    /// <summary>
    /// Ürün listesi için kullanılan basitleştirilmiş DTO
    /// </summary>
    public class ProductListDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double? AverageRating { get; set; }
    }

    /// <summary>
    /// Ürün detay sayfası için kullanılan DTO
    /// </summary>
    public class ProductDetailDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? Specifications { get; set; }
        public string? ImageUrl { get; set; }
        public double? AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public List<ReviewDto> Reviews { get; set; } = new();
    }
}
