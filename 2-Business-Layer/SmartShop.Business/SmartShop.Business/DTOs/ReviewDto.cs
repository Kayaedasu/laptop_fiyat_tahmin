namespace SmartShop.Business.DTOs
{
    /// <summary>
    /// Yorum bilgilerini taşımak için kullanılan DTO
    /// </summary>
    public class ReviewDto
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Yorum oluşturma için kullanılan DTO
    /// </summary>
    public class CreateReviewDto
    {
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public int Rating { get; set; } // 1-5 arası
        public string? Comment { get; set; }
    }
}
