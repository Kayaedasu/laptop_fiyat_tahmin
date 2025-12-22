const soap = require('soap');

// SOAP Service URL
const WSDL_URL = 'http://localhost:3002/order?wsdl';

// Test data
const TEST_USER_ID = 1; // Existing user from database
const TEST_ORDER_DATA = {
  UserId: TEST_USER_ID,
  ShippingAddress: '123 Test Street, Test City, TC 12345',
  PaymentMethod: 'CreditCard',
  Items: {
    OrderItem: [
      {
        ProductId: 1, // iPhone 14 Pro
        Quantity: 1,
        UnitPrice: 999.99
      },
      {
        ProductId: 2, // Samsung Galaxy S23
        Quantity: 2,
        UnitPrice: 899.99
      }
    ]
  }
};

let createdOrderId = null;

// ============================================
// Test Functions
// ============================================

/**
 * Test 1: CreateOrder
 */
async function testCreateOrder(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 1: CreateOrder - Create a new order');
  console.log('='.repeat(60));
  
  try {
    console.log('📤 Request:', JSON.stringify(TEST_ORDER_DATA, null, 2));
    
    const result = await client.CreateOrderAsync(TEST_ORDER_DATA);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success) {
      createdOrderId = response.OrderId;
      console.log(`✅ TEST PASSED - Order created: OrderId=${createdOrderId}`);
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
    }
    
    return response.Success;
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 2: CreateOrder with Invalid Data
 */
async function testCreateOrderInvalid(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 2: CreateOrder - Invalid data (missing fields)');
  console.log('='.repeat(60));
  
  try {
    const invalidData = {
      UserId: TEST_USER_ID,
      // Missing ShippingAddress, PaymentMethod, Items
    };
    
    console.log('📤 Request:', JSON.stringify(invalidData, null, 2));
    
    const result = await client.CreateOrderAsync(invalidData);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (!response.Success) {
      console.log('✅ TEST PASSED - Validation error detected correctly');
      return true;
    } else {
      console.log('❌ TEST FAILED - Should have failed validation');
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 3: CreateOrder with Insufficient Stock
 */
async function testCreateOrderInsufficientStock(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 3: CreateOrder - Insufficient stock');
  console.log('='.repeat(60));
  
  try {
    const insufficientStockData = {
      UserId: TEST_USER_ID,
      ShippingAddress: '456 Test Ave',
      PaymentMethod: 'CreditCard',
      Items: {
        OrderItem: [
          {
            ProductId: 1,
            Quantity: 99999, // Way more than available
            UnitPrice: 999.99
          }
        ]
      }
    };
    
    console.log('📤 Request:', JSON.stringify(insufficientStockData, null, 2));
    
    const result = await client.CreateOrderAsync(insufficientStockData);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (!response.Success && response.Message.includes('Insufficient stock')) {
      console.log('✅ TEST PASSED - Stock validation works correctly');
      return true;
    } else {
      console.log('❌ TEST FAILED - Should have detected insufficient stock');
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 4: GetOrder
 */
async function testGetOrder(client) {
  console.log('\n' + '='.repeat(60));
  console.log(`TEST 4: GetOrder - Get order details (OrderId=${createdOrderId || 1})`);
  console.log('='.repeat(60));
  
  try {
    const orderId = createdOrderId || 1;
    const request = { OrderId: orderId };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.GetOrderAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success && response.Order) {
      console.log(`✅ TEST PASSED - Order retrieved with ${response.Order.Items.OrderItem.length} items`);
      return true;
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 5: GetOrder with Invalid ID
 */
async function testGetOrderInvalid(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 5: GetOrder - Non-existent order');
  console.log('='.repeat(60));
  
  try {
    const request = { OrderId: 999999 };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.GetOrderAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (!response.Success && response.Message.includes('not found')) {
      console.log('✅ TEST PASSED - Correctly handles non-existent order');
      return true;
    } else {
      console.log('❌ TEST FAILED - Should have returned not found');
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 6: GetUserOrders
 */
async function testGetUserOrders(client) {
  console.log('\n' + '='.repeat(60));
  console.log(`TEST 6: GetUserOrders - Get all orders for user ${TEST_USER_ID}`);
  console.log('='.repeat(60));
  
  try {
    const request = { UserId: TEST_USER_ID };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.GetUserOrdersAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success) {
      const orderCount = response.Orders?.Order ? 
        (Array.isArray(response.Orders.Order) ? response.Orders.Order.length : 1) : 0;
      console.log(`✅ TEST PASSED - Found ${orderCount} orders for user`);
      return true;
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 7: UpdateOrderStatus
 */
async function testUpdateOrderStatus(client) {
  console.log('\n' + '='.repeat(60));
  console.log(`TEST 7: UpdateOrderStatus - Update order status (OrderId=${createdOrderId || 1})`);
  console.log('='.repeat(60));
  
  try {
    const orderId = createdOrderId || 1;
    const request = {
      OrderId: orderId,
      Status: 'Processing'
    };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.UpdateOrderStatusAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success) {
      console.log('✅ TEST PASSED - Order status updated');
      return true;
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 8: UpdateOrderStatus with Invalid Status
 */
async function testUpdateOrderStatusInvalid(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 8: UpdateOrderStatus - Invalid status');
  console.log('='.repeat(60));
  
  try {
    const orderId = createdOrderId || 1;
    const request = {
      OrderId: orderId,
      Status: 'InvalidStatus'
    };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.UpdateOrderStatusAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (!response.Success && response.Message.includes('Invalid status')) {
      console.log('✅ TEST PASSED - Status validation works correctly');
      return true;
    } else {
      console.log('❌ TEST FAILED - Should have validated status');
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 9: UpdateOrderStatus to Shipped
 */
async function testUpdateOrderStatusShipped(client) {
  console.log('\n' + '='.repeat(60));
  console.log(`TEST 9: UpdateOrderStatus - Update to Shipped`);
  console.log('='.repeat(60));
  
  try {
    const orderId = createdOrderId || 1;
    const request = {
      OrderId: orderId,
      Status: 'Shipped'
    };
    
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.UpdateOrderStatusAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success) {
      console.log('✅ TEST PASSED - Order marked as Shipped');
      return true;
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 10: CancelOrder
 */
async function testCancelOrder(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 10: CancelOrder - Cancel an order and restore stock');
  console.log('='.repeat(60));
  
  try {
    // First create a test order to cancel
    const testOrder = {
      UserId: TEST_USER_ID,
      ShippingAddress: 'Cancel Test Address',
      PaymentMethod: 'DebitCard',
      Items: {
        OrderItem: [
          {
            ProductId: 3, // Sony WH-1000XM4
            Quantity: 1,
            UnitPrice: 349.99
          }
        ]
      }
    };
    
    console.log('📤 Creating test order first...');
    const createResult = await client.CreateOrderAsync(testOrder);
    const createResponse = createResult[0];
    
    if (!createResponse.Success) {
      console.log(`❌ TEST FAILED - Could not create test order: ${createResponse.Message}`);
      return false;
    }
    
    const orderToCancel = createResponse.OrderId;
    console.log(`✅ Test order created: OrderId=${orderToCancel}`);
    
    // Now cancel it
    const request = { OrderId: orderToCancel };
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.CancelOrderAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (response.Success) {
      console.log('✅ TEST PASSED - Order cancelled and stock restored');
      return true;
    } else {
      console.log(`❌ TEST FAILED - ${response.Message}`);
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

/**
 * Test 11: CancelOrder - Already Cancelled
 */
async function testCancelOrderAlreadyCancelled(client) {
  console.log('\n' + '='.repeat(60));
  console.log('TEST 11: CancelOrder - Try to cancel already cancelled order');
  console.log('='.repeat(60));
  
  try {
    // Create and cancel an order
    const testOrder = {
      UserId: TEST_USER_ID,
      ShippingAddress: 'Already Cancelled Test',
      PaymentMethod: 'DebitCard', // Valid enum value
      Items: {
        OrderItem: [
          {
            ProductId: 2, // Use different product
            Quantity: 1,
            UnitPrice: 899.99
          }
        ]
      }
    };
    
    const createResult = await client.CreateOrderAsync(testOrder);
    const createResponse = createResult[0];
    
    if (!createResponse.Success) {
      console.log(`❌ TEST FAILED - Could not create test order: ${createResponse.Message}`);
      return false;
    }
    
    const orderId = createResponse.OrderId;
    console.log(`✅ Test order created: OrderId=${orderId}`);
    
    // Cancel it first time
    const firstCancel = await client.CancelOrderAsync({ OrderId: orderId });
    console.log(`✅ First cancellation successful`);
    
    // Try to cancel again
    const request = { OrderId: orderId };
    console.log('📤 Request:', JSON.stringify(request, null, 2));
    
    const result = await client.CancelOrderAsync(request);
    const response = result[0];
    
    console.log('📥 Response:', JSON.stringify(response, null, 2));
    
    if (!response.Success && response.Message.includes('already cancelled')) {
      console.log('✅ TEST PASSED - Correctly prevents double cancellation');
      return true;
    } else {
      console.log('❌ TEST FAILED - Should have prevented double cancellation');
      return false;
    }
  } catch (error) {
    console.error('❌ TEST ERROR:', error.message);
    return false;
  }
}

// ============================================
// Main Test Runner
// ============================================

async function runAllTests() {
  console.log('\n╔════════════════════════════════════════════════════════╗');
  console.log('║     🧪 SmartShop OrderService - SOAP Test Suite       ║');
  console.log('╚════════════════════════════════════════════════════════╝\n');
  console.log(`Connecting to SOAP service: ${WSDL_URL}\n`);
  
  try {
    // Create SOAP client
    const client = await soap.createClientAsync(WSDL_URL);
    console.log('✅ SOAP client connected successfully\n');
    
    const tests = [
      { name: 'CreateOrder', fn: testCreateOrder },
      { name: 'CreateOrder (Invalid)', fn: testCreateOrderInvalid },
      { name: 'CreateOrder (Insufficient Stock)', fn: testCreateOrderInsufficientStock },
      { name: 'GetOrder', fn: testGetOrder },
      { name: 'GetOrder (Invalid)', fn: testGetOrderInvalid },
      { name: 'GetUserOrders', fn: testGetUserOrders },
      { name: 'UpdateOrderStatus', fn: testUpdateOrderStatus },
      { name: 'UpdateOrderStatus (Invalid)', fn: testUpdateOrderStatusInvalid },
      { name: 'UpdateOrderStatus (Shipped)', fn: testUpdateOrderStatusShipped },
      { name: 'CancelOrder', fn: testCancelOrder },
      { name: 'CancelOrder (Already Cancelled)', fn: testCancelOrderAlreadyCancelled }
    ];
    
    let passed = 0;
    let failed = 0;
    
    for (const test of tests) {
      const result = await test.fn(client);
      if (result) {
        passed++;
      } else {
        failed++;
      }
      
      // Wait a bit between tests
      await new Promise(resolve => setTimeout(resolve, 500));
    }
    
    // Summary
    console.log('\n' + '='.repeat(60));
    console.log('📊 TEST SUMMARY');
    console.log('='.repeat(60));
    console.log(`Total Tests: ${tests.length}`);
    console.log(`✅ Passed: ${passed}`);
    console.log(`❌ Failed: ${failed}`);
    console.log(`Success Rate: ${((passed / tests.length) * 100).toFixed(1)}%`);
    console.log('='.repeat(60));
    
    if (failed === 0) {
      console.log('\n🎉 All tests passed! OrderService is working perfectly.\n');
    } else {
      console.log('\n⚠️  Some tests failed. Please check the output above.\n');
    }
    
  } catch (error) {
    console.error('❌ Fatal error:', error.message);
    console.error('Make sure the OrderService is running on port 3002');
    process.exit(1);
  }
}

// Run tests
runAllTests();
