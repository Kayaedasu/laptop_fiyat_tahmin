using SmartShop.Business.DTOs;

namespace SmartShop.Business.Validators
{
    /// <summary>
    /// Sipariş doğrulama kuralları
    /// </summary>
    public static class OrderValidator
    {
        public static List<string> ValidateCreate(CreateOrderDto dto)
        {
            var errors = new List<string>();

            if (dto.UserId <= 0)
                errors.Add("Geçerli bir kullanıcı ID'si gereklidir.");

            if (string.IsNullOrWhiteSpace(dto.ShippingAddress))
                errors.Add("Teslimat adresi boş olamaz.");
            else if (dto.ShippingAddress.Length < 10)
                errors.Add("Teslimat adresi en az 10 karakter olmalıdır.");
            else if (dto.ShippingAddress.Length > 500)
                errors.Add("Teslimat adresi en fazla 500 karakter olabilir.");

            if (string.IsNullOrWhiteSpace(dto.PaymentMethod))
                errors.Add("Ödeme yöntemi boş olamaz.");
            else
            {
                // Veritabanındaki ENUM değerleriyle eşleşen değerler
                var validPaymentMethods = new[] { 
                    "CreditCard", "DebitCard", "BankTransfer", "Cash",
                    // Geriye dönük uyumluluk için Türkçe değerler
                    "Kredi Kartı", "Banka Kartı", "Kapıda Ödeme", "Havale"
                };
                if (!validPaymentMethods.Contains(dto.PaymentMethod))
                    errors.Add("Geçersiz ödeme yöntemi.");
            }

            if (dto.OrderDetails == null || !dto.OrderDetails.Any())
                errors.Add("Sipariş en az bir ürün içermelidir.");
            else
            {
                foreach (var detail in dto.OrderDetails)
                {
                    if (detail.ProductId <= 0)
                        errors.Add($"Geçersiz ürün ID'si: {detail.ProductId}");

                    if (detail.Quantity <= 0)
                        errors.Add($"Ürün miktarı 0'dan büyük olmalıdır (Ürün ID: {detail.ProductId})");

                    if (detail.Quantity > 100)
                        errors.Add($"Ürün miktarı en fazla 100 olabilir (Ürün ID: {detail.ProductId})");
                }

                // Aynı üründen birden fazla var mı kontrol et
                var duplicates = dto.OrderDetails
                    .GroupBy(x => x.ProductId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key);

                if (duplicates.Any())
                    errors.Add($"Aynı ürün birden fazla kez eklenemez: {string.Join(", ", duplicates)}");
            }

            return errors;
        }

        public static List<string> ValidateUpdateStatus(UpdateOrderStatusDto dto)
        {
            var errors = new List<string>();

            if (dto.OrderId <= 0)
                errors.Add("Geçerli bir sipariş ID'si gereklidir.");

            if (string.IsNullOrWhiteSpace(dto.Status))
                errors.Add("Sipariş durumu boş olamaz.");
            else
            {
                // İngilizce durum isimleri (Order entity ile uyumlu)
                var validStatuses = new[] { "Pending", "Processing", "Shipped", "Delivered", "Cancelled" };
                if (!validStatuses.Contains(dto.Status))
                    errors.Add($"Geçersiz sipariş durumu. Geçerli değerler: {string.Join(", ", validStatuses)}");
            }

            // Kargo durumu kontrolü (opsiyonel - TrackingNumber Order modelinde yok)
            // if (dto.Status == "Shipped" && string.IsNullOrWhiteSpace(dto.TrackingNumber))
            //     errors.Add("Kargo durumu için takip numarası gereklidir.");

            return errors;
        }
    }
}
