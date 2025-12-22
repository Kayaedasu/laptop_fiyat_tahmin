using SmartShop.Business.DTOs;
using System.Text.RegularExpressions;

namespace SmartShop.Business.Validators
{
    /// <summary>
    /// Ürün doğrulama kuralları
    /// </summary>
    public static class ProductValidator
    {
        public static List<string> ValidateCreate(CreateProductDto dto)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.Name))
                errors.Add("Ürün adı boş olamaz.");
            else if (dto.Name.Length < 3)
                errors.Add("Ürün adı en az 3 karakter olmalıdır.");
            else if (dto.Name.Length > 200)
                errors.Add("Ürün adı en fazla 200 karakter olabilir.");

            if (dto.Price <= 0)
                errors.Add("Fiyat 0'dan büyük olmalıdır.");

            if (dto.Price > 999999.99m)
                errors.Add("Fiyat çok yüksek.");

            if (dto.StockQuantity < 0)
                errors.Add("Stok miktarı negatif olamaz.");

            if (dto.CategoryId <= 0)
                errors.Add("Geçerli bir kategori seçilmelidir.");

            // Teknik özellikler validasyonu
            if (dto.RAM <= 0)
                errors.Add("RAM miktarı 0'dan büyük olmalıdır.");

            if (dto.Storage <= 0)
                errors.Add("Depolama alanı 0'dan büyük olmalıdır.");

            if (!string.IsNullOrWhiteSpace(dto.StorageType) && 
                dto.StorageType != "HDD" && dto.StorageType != "SSD" && dto.StorageType != "SSD+HDD")
                errors.Add("Depolama tipi HDD, SSD veya SSD+HDD olmalıdır.");

            if (dto.ScreenSize.HasValue && dto.ScreenSize.Value <= 0)
                errors.Add("Ekran boyutu 0'dan büyük olmalıdır.");

            if (dto.Discount < 0 || dto.Discount > 100)
                errors.Add("İndirim oranı 0-100 arasında olmalıdır.");

            if (!string.IsNullOrWhiteSpace(dto.Condition) && 
                dto.Condition != "New" && dto.Condition != "Used" && dto.Condition != "Refurbished")
                errors.Add("Ürün durumu New, Used veya Refurbished olmalıdır.");

            if (!string.IsNullOrWhiteSpace(dto.Description) && dto.Description.Length > 2000)
                errors.Add("Açıklama en fazla 2000 karakter olabilir.");

            if (!string.IsNullOrWhiteSpace(dto.Brand) && dto.Brand.Length > 100)
                errors.Add("Marka adı en fazla 100 karakter olabilir.");

            if (!string.IsNullOrWhiteSpace(dto.Model) && dto.Model.Length > 100)
                errors.Add("Model adı en fazla 100 karakter olabilir.");

            if (!string.IsNullOrWhiteSpace(dto.ImageUrl) && dto.ImageUrl.Length > 500)
                errors.Add("Resim URL'si en fazla 500 karakter olabilir.");

            return errors;
        }

        public static List<string> ValidateUpdate(UpdateProductDto dto)
        {
            var errors = ValidateCreate(new CreateProductDto
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                StockQuantity = dto.StockQuantity,
                CategoryId = dto.CategoryId,
                Brand = dto.Brand,
                Model = dto.Model,
                Processor = dto.Processor,
                RAM = dto.RAM,
                Storage = dto.Storage,
                StorageType = dto.StorageType,
                GPU = dto.GPU,
                ScreenSize = dto.ScreenSize,
                Resolution = dto.Resolution,
                Condition = dto.Condition,
                Discount = dto.Discount,
                Specifications = dto.Specifications,
                ImageUrl = dto.ImageUrl,
                IsActive = dto.IsActive
            });

            if (dto.ProductId <= 0)
                errors.Insert(0, "Geçerli bir ürün ID'si gereklidir.");

            return errors;
        }

        public static List<string> ValidateFilter(ProductFilterDto filter)
        {
            var errors = new List<string>();

            if (filter.MinPrice.HasValue && filter.MinPrice.Value < 0)
                errors.Add("Minimum fiyat negatif olamaz.");

            if (filter.MaxPrice.HasValue && filter.MaxPrice.Value < 0)
                errors.Add("Maximum fiyat negatif olamaz.");

            if (filter.MinPrice.HasValue && filter.MaxPrice.HasValue && 
                filter.MinPrice.Value > filter.MaxPrice.Value)
                errors.Add("Minimum fiyat, maximum fiyattan büyük olamaz.");

            if (filter.PageNumber < 1)
                errors.Add("Sayfa numarası 1'den küçük olamaz.");

            if (filter.PageSize < 1)
                errors.Add("Sayfa boyutu 1'den küçük olamaz.");

            if (filter.PageSize > 100)
                errors.Add("Sayfa boyutu en fazla 100 olabilir.");

            return errors;
        }
    }
}
