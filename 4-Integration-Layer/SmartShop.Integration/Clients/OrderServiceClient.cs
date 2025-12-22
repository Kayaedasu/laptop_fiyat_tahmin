using System.ServiceModel;
using System.ServiceModel.Channels;

namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// SOAP client for OrderService
    /// Handles all order-related operations (Create, Get, List, Update Status, Cancel)
    /// </summary>
    public class OrderServiceClient : IDisposable
    {
        private readonly string _serviceUrl;
        private readonly BasicHttpBinding _binding;

        /// <summary>
        /// Initialize OrderService SOAP client
        /// </summary>
        /// <param name="serviceUrl">OrderService SOAP endpoint (default: http://localhost:3002/order)</param>
        public OrderServiceClient(string serviceUrl = "http://localhost:3002/order")
        {
            _serviceUrl = serviceUrl;
            
            // Configure SOAP binding
            _binding = new BasicHttpBinding
            {
                MaxReceivedMessageSize = 2147483647,
                MaxBufferSize = 2147483647,
                SendTimeout = TimeSpan.FromMinutes(5),
                ReceiveTimeout = TimeSpan.FromMinutes(5),
                OpenTimeout = TimeSpan.FromMinutes(1),
                CloseTimeout = TimeSpan.FromMinutes(1)
            };
        }

        #region Order Operations

        /// <summary>
        /// Create a new order
        /// </summary>
        public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var endpoint = new EndpointAddress(_serviceUrl);
            var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
            var channel = channelFactory.CreateChannel();

            try
            {
                var response = await channel.CreateOrderAsync(request);
                return response;
            }
            finally
            {
                CloseChannel(channel);
                channelFactory.Close();
            }
        }

        /// <summary>
        /// Get order details by ID
        /// </summary>
        public async Task<GetOrderResponse> GetOrderAsync(int orderId)
        {
            var endpoint = new EndpointAddress(_serviceUrl);
            var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
            var channel = channelFactory.CreateChannel();

            try
            {
                var request = new GetOrderRequest { OrderId = orderId };
                var response = await channel.GetOrderAsync(request);
                return response;
            }
            finally
            {
                CloseChannel(channel);
                channelFactory.Close();
            }
        }

        /// <summary>
        /// Get all orders for a user
        /// </summary>
        public async Task<GetUserOrdersResponse> GetUserOrdersAsync(int userId)
        {
            var endpoint = new EndpointAddress(_serviceUrl);
            var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
            var channel = channelFactory.CreateChannel();

            try
            {
                var request = new GetUserOrdersRequest { UserId = userId };
                var response = await channel.GetUserOrdersAsync(request);
                return response;
            }
            finally
            {
                CloseChannel(channel);
                channelFactory.Close();
            }
        }

        /// <summary>
        /// Update order status
        /// </summary>
        public async Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(int orderId, string status)
        {
            var endpoint = new EndpointAddress(_serviceUrl);
            var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
            var channel = channelFactory.CreateChannel();

            try
            {
                var request = new UpdateOrderStatusRequest
                {
                    OrderId = orderId,
                    Status = status
                };
                var response = await channel.UpdateOrderStatusAsync(request);
                return response;
            }
            finally
            {
                CloseChannel(channel);
                channelFactory.Close();
            }
        }

        /// <summary>
        /// Cancel an order
        /// </summary>
        public async Task<CancelOrderResponse> CancelOrderAsync(int orderId)
        {
            var endpoint = new EndpointAddress(_serviceUrl);
            var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
            var channel = channelFactory.CreateChannel();

            try
            {
                var request = new CancelOrderRequest { OrderId = orderId };
                var response = await channel.CancelOrderAsync(request);
                return response;
            }
            finally
            {
                CloseChannel(channel);
                channelFactory.Close();
            }
        }

        #endregion

        #region Helper Methods

        private void CloseChannel(IOrderService channel)
        {
            try
            {
                ((IClientChannel)channel).Close();
            }
            catch
            {
                ((IClientChannel)channel).Abort();
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Check if OrderService is available
        /// </summary>
        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                var endpoint = new EndpointAddress(_serviceUrl);
                var channelFactory = new ChannelFactory<IOrderService>(_binding, endpoint);
                var channel = channelFactory.CreateChannel();

                try
                {
                    // Try to get a non-existent order to check connectivity
                    var request = new GetOrderRequest { OrderId = 0 };
                    await channel.GetOrderAsync(request);
                    return true;
                }
                finally
                {
                    CloseChannel(channel);
                    channelFactory.Close();
                }
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            // Nothing to dispose as we create and close channels per request
        }

        #endregion
    }

    #region SOAP Service Contract

    [ServiceContract(Namespace = "http://smartshop.com/order")]
    public interface IOrderService
    {
        [OperationContract(Action = "CreateOrder")]
        Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request);

        [OperationContract(Action = "GetOrder")]
        Task<GetOrderResponse> GetOrderAsync(GetOrderRequest request);

        [OperationContract(Action = "GetUserOrders")]
        Task<GetUserOrdersResponse> GetUserOrdersAsync(GetUserOrdersRequest request);

        [OperationContract(Action = "UpdateOrderStatus")]
        Task<UpdateOrderStatusResponse> UpdateOrderStatusAsync(UpdateOrderStatusRequest request);

        [OperationContract(Action = "CancelOrder")]
        Task<CancelOrderResponse> CancelOrderAsync(CancelOrderRequest request);
    }

    #endregion

    #region SOAP DTOs

    // Order Item
    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class OrderItem
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderItemId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int ProductId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string ProductName { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public int Quantity { get; set; }

        [System.Runtime.Serialization.DataMember]
        public decimal UnitPrice { get; set; }

        [System.Runtime.Serialization.DataMember]
        public decimal Subtotal { get; set; }
    }

    // Order Detail
    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class OrderDetail
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int UserId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string UserName { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string OrderDate { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public decimal TotalAmount { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Status { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string ShippingAddress { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string PaymentMethod { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public List<OrderItem> Items { get; set; } = new();

        [System.Runtime.Serialization.DataMember]
        public string CreatedAt { get; set; } = string.Empty;
    }

    // Order (Summary)
    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class Order
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public int UserId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string OrderDate { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public decimal TotalAmount { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Status { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string ShippingAddress { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string PaymentMethod { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string CreatedAt { get; set; } = string.Empty;
    }

    // Requests
    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class CreateOrderRequest
    {
        [System.Runtime.Serialization.DataMember]
        public int UserId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string ShippingAddress { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public string PaymentMethod { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public List<OrderItem> Items { get; set; } = new();
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class GetOrderRequest
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class GetUserOrdersRequest
    {
        [System.Runtime.Serialization.DataMember]
        public int UserId { get; set; }
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class UpdateOrderStatusRequest
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Status { get; set; } = string.Empty;
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class CancelOrderRequest
    {
        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }
    }

    // Responses
    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class CreateOrderResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public int OrderId { get; set; }
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class GetOrderResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public OrderDetail? Order { get; set; }
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class GetUserOrdersResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; } = string.Empty;

        [System.Runtime.Serialization.DataMember]
        public List<Order> Orders { get; set; } = new();
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class UpdateOrderStatusResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; } = string.Empty;
    }

    [System.Runtime.Serialization.DataContract(Namespace = "http://smartshop.com/order")]
    public class CancelOrderResponse
    {
        [System.Runtime.Serialization.DataMember]
        public bool Success { get; set; }

        [System.Runtime.Serialization.DataMember]
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
