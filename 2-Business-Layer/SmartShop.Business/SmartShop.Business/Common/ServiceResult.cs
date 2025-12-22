namespace SmartShop.Business.Common
{
    /// <summary>
    /// API'den dönen standart yanıt yapısı
    /// </summary>
    /// <typeparam name="T">Dönen veri tipi</typeparam>
    public class ServiceResult<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new();

        public static ServiceResult<T> SuccessResult(T data, string message = "İşlem başarılı")
        {
            return new ServiceResult<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ServiceResult<T> FailureResult(string message, List<string>? errors = null)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ServiceResult<T> FailureResult(string message, string error)
        {
            return new ServiceResult<T>
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }

    /// <summary>
    /// Veri döndürmeyen işlemler için sonuç yapısı
    /// </summary>
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();

        public static ServiceResult SuccessResult(string message = "İşlem başarılı")
        {
            return new ServiceResult
            {
                Success = true,
                Message = message
            };
        }

        public static ServiceResult FailureResult(string message, List<string>? errors = null)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ServiceResult FailureResult(string message, string error)
        {
            return new ServiceResult
            {
                Success = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
    }
}
