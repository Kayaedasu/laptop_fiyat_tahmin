using Grpc.Net.Client;
using Userservice;

namespace SmartShop.Integration.Clients
{
    /// <summary>
    /// gRPC client for UserService
    /// Handles all user-related operations (Register, Login, Get, Update, Delete, List)
    /// </summary>
    public class UserServiceClient : IDisposable
    {
        private readonly GrpcChannel _channel;
        private readonly UserService.UserServiceClient _client;
        private readonly string _serviceUrl;

        /// <summary>
        /// Initialize UserService gRPC client
        /// </summary>
        /// <param name="serviceUrl">UserService gRPC endpoint (default: http://localhost:50051)</param>
        public UserServiceClient(string serviceUrl = "http://localhost:50051")
        {
            _serviceUrl = serviceUrl;
            
            // Create gRPC channel
            _channel = GrpcChannel.ForAddress(_serviceUrl);
            
            // Create client
            _client = new UserService.UserServiceClient(_channel);
        }

        #region User Operations

        /// <summary>
        /// Register a new user
        /// </summary>
        public async Task<UserResponse> RegisterUserAsync(string email, string password, 
            string firstName, string lastName, string? phone = null)
        {
            try
            {
                var request = new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone ?? string.Empty
                };

                var response = await _client.RegisterUserAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error registering user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Login user with email and password
        /// </summary>
        public async Task<LoginResponse> LoginUserAsync(string email, string password)
        {
            try
            {
                var request = new LoginRequest
                {
                    Email = email,
                    Password = password
                };

                var response = await _client.LoginUserAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error logging in user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        public async Task<UserResponse> GetUserAsync(int userId)
        {
            try
            {
                var request = new GetUserRequest
                {
                    UserId = userId
                };

                var response = await _client.GetUserAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Update user information
        /// </summary>
        public async Task<UserResponse> UpdateUserAsync(int userId, string firstName, 
            string lastName, string? phone = null)
        {
            try
            {
                var request = new UpdateUserRequest
                {
                    UserId = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    Phone = phone ?? string.Empty
                };

                var response = await _client.UpdateUserAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Delete user (soft delete)
        /// </summary>
        public async Task<DeleteResponse> DeleteUserAsync(int userId)
        {
            try
            {
                var request = new DeleteUserRequest
                {
                    UserId = userId
                };

                var response = await _client.DeleteUserAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting user: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// List all users (Admin only)
        /// </summary>
        public async Task<UserListResponse> ListUsersAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var request = new ListUsersRequest
                {
                    Page = page,
                    PageSize = pageSize
                };

                var response = await _client.ListUsersAsync(request);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error listing users: {ex.Message}", ex);
            }
        }

        #endregion

        #region Health Check

        /// <summary>
        /// Check if UserService is available
        /// </summary>
        public async Task<bool> IsServiceAvailableAsync()
        {
            try
            {
                // Try to get a non-existent user to check connectivity
                var request = new GetUserRequest { UserId = 0 };
                await _client.GetUserAsync(request);
                return true;
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
            _channel?.Dispose();
        }

        #endregion
    }
}
