using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartShop.DataAccess.Models
{
    [Table("Products")]
    public class Product
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Model { get; set; }

        // Teknik Özellikler
        [MaxLength(100)]
        public string? Processor { get; set; }

        [Required]
        public int RAM { get; set; }

        [Required]
        public int Storage { get; set; }

        [MaxLength(20)]
        public string StorageType { get; set; } = "SSD"; // HDD, SSD, SSD+HDD

        [MaxLength(100)]
        public string? GPU { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? ScreenSize { get; set; }

        [MaxLength(50)]
        public string? Resolution { get; set; }

        // Fiyat ve Stok
        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Required]
        public int Stock { get; set; } = 0;

        [Column(TypeName = "decimal(5,2)")]
        public decimal Discount { get; set; } = 0;

        // İlişkiler
        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Diğer
        [Column(TypeName = "text")]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? ImageUrl { get; set; }

        [MaxLength(20)]
        [Column("ProductCondition")]
        public string Condition { get; set; } = "New"; // New, Used, Refurbished

        public bool IsActive { get; set; } = true;

        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation Properties
        public virtual Category Category { get; set; } = null!;
        public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Cart> CartItems { get; set; } = new List<Cart>();
    }
}
