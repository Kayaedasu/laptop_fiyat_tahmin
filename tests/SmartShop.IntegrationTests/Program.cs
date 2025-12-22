using SmartShop.Integration.Clients;
using System;
using System.Threading.Tasks;

namespace SmartShop.IntegrationTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
            Console.WriteLine("║    SmartShop SOA Integration Tests                   ║");
            Console.WriteLine("║    6-Layer Architecture End-to-End Tests             ║");
            Console.WriteLine("╚═══════════════════════════════════════════════════════╝\n");

            var testRunner = new IntegrationTestRunner();
            
            while (true)
            {
                Console.WriteLine("\n📋 Test Categories:");
                Console.WriteLine("1. User Service Tests (gRPC)");
                Console.WriteLine("2. Product Service Tests (REST)");
                Console.WriteLine("3. Order Service Tests (SOAP)");
                Console.WriteLine("4. External API Tests (Payment & Cargo)");
                Console.WriteLine("5. ML Service Tests (Python/Flask)");
                Console.WriteLine("6. Run All Tests");
                Console.WriteLine("0. Exit");
                
                Console.Write("\n➤ Select option: ");
                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        await testRunner.RunUserServiceTests();
                        break;
                    case "2":
                        await testRunner.RunProductServiceTests();
                        break;
                    case "3":
                        await testRunner.RunOrderServiceTests();
                        break;
                    case "4":
                        await testRunner.RunExternalApiTests();
                        break;
                    case "5":
                        await testRunner.RunMLServiceTests();
                        break;
                    case "6":
                        await testRunner.RunAllTests();
                        break;
                    case "0":
                        Console.WriteLine("\n👋 Exiting...");
                        return;
                    default:
                        Console.WriteLine("\n❌ Invalid option!");
                        break;
                }

                Console.WriteLine("\n" + new string('─', 60));
            }
        }
    }

    class IntegrationTestRunner
    {
        private int passedTests = 0;
        private int failedTests = 0;

        public async Task RunAllTests()
        {
            Console.WriteLine("\n🚀 Running ALL Integration Tests...\n");
            passedTests = 0;
            failedTests = 0;

            await RunUserServiceTests();
            await RunProductServiceTests();
            await RunOrderServiceTests();
            await RunExternalApiTests();
            await RunMLServiceTests();

            Console.WriteLine("\n" + new string('═', 60));
            Console.WriteLine($"✅ Passed: {passedTests} | ❌ Failed: {failedTests}");
            Console.WriteLine(new string('═', 60));
        }

        public async Task RunUserServiceTests()
        {
            Console.WriteLine("\n🔹 USER SERVICE TESTS (gRPC)");
            Console.WriteLine(new string('─', 60));

            var client = new UserServiceClient("http://localhost:50051");

            // Test 1: Register User
            await RunTest("Register User", async () =>
            {
                var email = $"test_{Guid.NewGuid().ToString().Substring(0, 8)}@example.com";
                var result = await client.RegisterUserAsync("John", "Doe", email, "Test123!", "5551234567");
                return result != null && result.UserId > 0;
            });

            // Test 2: Login User
            await RunTest("Login User", async () =>
            {
                var result = await client.LoginUserAsync("admin@smartshop.com", "Admin123!");
                return result != null && !string.IsNullOrEmpty(result.Email);
            });

            // Test 3: Get User by ID
            await RunTest("Get User by ID", async () =>
            {
                var result = await client.GetUserAsync(1);
                return result != null && result.UserId == 1;
            });

            // Test 4: Update User
            await RunTest("Update User", async () =>
            {
                var result = await client.UpdateUserAsync(1, "Updated", "Name", "5559876543");
                return result != null;
            });

            // Test 5: Delete User
            await RunTest("Delete User (Soft)", async () =>
            {
                var result = await client.DeleteUserAsync(999); // Non-existent user
                return true; // Should not throw
            });
        }

        public async Task RunProductServiceTests()
        {
            Console.WriteLine("\n🔹 PRODUCT SERVICE TESTS (REST API)");
            Console.WriteLine(new string('─', 60));

            var client = new ProductServiceClient("http://localhost:3001");

            // Test 1: Get All Products
            await RunTest("Get All Products", async () =>
            {
                var result = await client.GetAllProductsAsync();
                return result != null && result.Count > 0;
            });

            // Test 2: Get Product by ID
            await RunTest("Get Product by ID", async () =>
            {
                var result = await client.GetProductByIdAsync(1);
                return result != null && result.ProductId == 1;
            });

            // Test 3: Search Products
            await RunTest("Search Products", async () =>
            {
                var result = await client.SearchProductsAsync(searchTerm: "laptop");
                return result != null;
            });

            // Test 4: Get Products by Category
            await RunTest("Get Products by Category", async () =>
            {
                var result = await client.GetProductsByCategoryAsync(1);
                return result != null;
            });

            // Test 5: Create Product
            await RunTest("Create Product", async () =>
            {
                var result = await client.CreateProductAsync(
                    1, "Test Product", "Test Description", 999.99m, 10, "test.jpg", "TestBrand");
                return result != null && result.ProductId > 0;
            });
        }

        public async Task RunOrderServiceTests()
        {
            Console.WriteLine("\n🔹 ORDER SERVICE TESTS (SOAP)");
            Console.WriteLine(new string('─', 60));

            var client = new OrderServiceClient("http://localhost:3002");

            // Test 1: Get Order by ID
            await RunTest("Get Order by ID", async () =>
            {
                var result = await client.GetOrderAsync(1);
                return result != null;
            });

            // Test 2: Get User Orders
            await RunTest("Get User Orders", async () =>
            {
                var result = await client.GetUserOrdersAsync(1);
                return result != null;
            });

            // Test 3: Create Order
            await RunTest("Create Order", async () =>
            {
                var items = new List<OrderItemDto>
                {
                    new OrderItemDto { ProductId = 1, Quantity = 2, UnitPrice = 100.00m }
                };
                var result = await client.CreateOrderAsync(1, items, "Test Address", "CreditCard");
                return result != null && result.OrderId > 0;
            });

            // Test 4: Update Order Status
            await RunTest("Update Order Status", async () =>
            {
                var result = await client.UpdateOrderStatusAsync(1, "Shipped");
                return result != null;
            });

            // Test 5: Cancel Order
            await RunTest("Cancel Order", async () =>
            {
                var result = await client.CancelOrderAsync(999, "Test cancellation");
                return true; // Should not throw
            });
        }

        public async Task RunExternalApiTests()
        {
            Console.WriteLine("\n🔹 EXTERNAL API TESTS (Payment & Cargo)");
            Console.WriteLine(new string('─', 60));

            // Payment API Tests
            var paymentClient = new PaymentApiClient();

            await RunTest("Process Payment", async () =>
            {
                var request = new PaymentRequestDto
                {
                    OrderId = 1,
                    Amount = 250.50m,
                    CardNumber = "4111111111111111",
                    CardHolderName = "Test User",
                    ExpiryMonth = "12",
                    ExpiryYear = "2025",
                    CVV = "123"
                };
                var result = await paymentClient.ProcessPaymentAsync(request);
                return result.Success;
            });

            await RunTest("Check Payment Status", async () =>
            {
                var result = await paymentClient.CheckPaymentStatusAsync("TXN-12345678");
                return result.Success;
            });

            // Cargo API Tests
            var cargoClient = new CargoApiClient();

            await RunTest("Create Shipment", async () =>
            {
                var request = new ShippingRequestDto
                {
                    OrderId = 1,
                    RecipientName = "John Doe",
                    RecipientPhone = "5551234567",
                    Address = "Test Address, Test District",
                    City = "Istanbul",
                    District = "Kadıköy",
                    PostalCode = "34710",
                    PackageWeight = 2.5m,
                    PackageSize = "Medium"
                };
                var result = await cargoClient.CreateShipmentAsync(request);
                return result.Success;
            });

            await RunTest("Track Shipment", async () =>
            {
                var result = await cargoClient.TrackShipmentAsync("TRK123456789");
                return result != null && !string.IsNullOrEmpty(result.Status);
            });
        }

        public async Task RunMLServiceTests()
        {
            Console.WriteLine("\n🔹 ML SERVICE TESTS (Python/Flask)");
            Console.WriteLine(new string('─', 60));

            var client = new MLServiceClient("http://localhost:5000");

            // Test 1: Health Check
            await RunTest("ML Service Health Check", async () =>
            {
                var result = await client.HealthCheckAsync();
                return result;
            });

            // Test 2: Get Recommendations
            await RunTest("Get Product Recommendations", async () =>
            {
                var result = await client.GetRecommendationsAsync(1, 10);
                return result != null && result.Success;
            });

            // Test 3: Predict Price
            await RunTest("Predict Product Price", async () =>
            {
                var features = new Dictionary<string, object>
                {
                    { "base_price", 500 },
                    { "category_id", 1 },
                    { "brand", "TestBrand" }
                };
                var result = await client.PredictPriceAsync(features);
                return result != null && result.Success;
            });

            // Test 4: Detect Fraud
            await RunTest("Detect Transaction Fraud", async () =>
            {
                var transactionData = new Dictionary<string, object>
                {
                    { "amount", 15000 },
                    { "user_id", 1 },
                    { "card_country", "TR" }
                };
                var result = await client.DetectFraudAsync(transactionData);
                return result != null && result.Success;
            });

            // Test 5: Segment Customer
            await RunTest("Segment Customer", async () =>
            {
                var customerData = new Dictionary<string, object>
                {
                    { "total_spent", 5500 },
                    { "order_count", 12 }
                };
                var result = await client.SegmentCustomerAsync(customerData);
                return result != null && result.Success;
            });

            // Test 6: Find Similar Products
            await RunTest("Find Similar Products", async () =>
            {
                var result = await client.FindSimilarProductsAsync(1, 5);
                return result != null && result.Success;
            });
        }

        private async Task RunTest(string testName, Func<Task<bool>> testFunc)
        {
            Console.Write($"  → {testName,-40}");
            try
            {
                var result = await testFunc();
                if (result)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(" ✓ PASS");
                    Console.ResetColor();
                    passedTests++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(" ✗ FAIL");
                    Console.ResetColor();
                    failedTests++;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($" ✗ ERROR: {ex.Message}");
                Console.ResetColor();
                failedTests++;
            }
        }
    }
}
