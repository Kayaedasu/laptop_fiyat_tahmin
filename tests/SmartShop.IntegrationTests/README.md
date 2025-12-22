# SmartShop Integration Tests

End-to-end integration tests for SmartShop SOA architecture.

## Purpose

This project tests the complete SOA flow:
1. **Presentation Layer** → Business Layer
2. **Business Layer** → Integration Layer
3. **Integration Layer** → Microservices (gRPC, REST, SOAP)
4. **Integration Layer** → External APIs (Payment, Cargo)
5. **Integration Layer** → ML Service (Python)

## Test Scenarios

### 1. User Management (gRPC)
- Register new user
- Login user
- Get user by ID
- Update user profile
- Delete user

### 2. Product Management (REST)
- Get all products
- Get product by ID
- Search products with filters
- Create new product
- Update product
- Delete product

### 3. Order Management (SOAP)
- Create order with items
- Get order by ID
- Get user orders
- Update order status
- Cancel order

### 4. External Services
- Process payment
- Create cargo shipment
- Track shipment

### 5. ML Service
- Get product recommendations
- Predict price
- Detect fraud
- Segment customer
- Find similar products

## Running Tests

```bash
dotnet run --project SmartShop.IntegrationTests
```

## Prerequisites

Ensure all services are running:
- MySQL (port 3306)
- UserService (port 50051)
- ProductService (port 3001)
- OrderService (port 3002)
- ML Service (port 5000)
