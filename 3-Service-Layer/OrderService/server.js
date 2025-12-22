const express = require('express');
const soap = require('soap');
const fs = require('fs');
const path = require('path');
const bodyParser = require('body-parser');
const db = require('./db');
require('dotenv').config();

const app = express();
const PORT = process.env.PORT || 3002;

// Middleware
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

// Load WSDL file
const wsdlPath = path.join(__dirname, 'order.wsdl');
const wsdlXML = fs.readFileSync(wsdlPath, 'utf8');

// ============================================
// SOAP Service Implementation
// ============================================

const orderService = {
  OrderService: {
    OrderServicePort: {
      
      /**
       * CreateOrder - Yeni sipariş oluşturur
       */
      CreateOrder: async (args) => {
        console.log('\n🔵 CreateOrder called with:', JSON.stringify(args, null, 2));
        
        const connection = await db.getConnection();
        
        try {
          await connection.beginTransaction();
          
          const { UserId, ShippingAddress, PaymentMethod, Items } = args;
          
          // Validation
          if (!UserId || !ShippingAddress || !PaymentMethod || !Items || !Items.OrderItem || Items.OrderItem.length === 0) {
            return {
              Success: false,
              Message: 'Missing required fields: UserId, ShippingAddress, PaymentMethod, and Items are required'
            };
          }
          
          // Calculate total amount
          let totalAmount = 0;
          const orderItems = Array.isArray(Items.OrderItem) ? Items.OrderItem : [Items.OrderItem];
          
          // Validate products and calculate total
          for (const item of orderItems) {
            if (!item.ProductId || !item.Quantity || !item.UnitPrice) {
              throw new Error('Invalid order item: ProductId, Quantity, and UnitPrice are required');
            }
            
            // Check product exists and stock
            const [product] = await connection.query(
              'SELECT ProductId, Name, Price, Stock, IsActive FROM Products WHERE ProductId = ?',
              [item.ProductId]
            );
            
            if (!product || product.length === 0 || !product[0].IsActive) {
              throw new Error(`Product with ID ${item.ProductId} not found`);
            }
            
            if (product[0].Stock < item.Quantity) {
              throw new Error(`Insufficient stock for product ${product[0].Name}. Available: ${product[0].Stock}, Requested: ${item.Quantity}`);
            }
            
            const subtotal = parseFloat(item.UnitPrice) * parseInt(item.Quantity);
            totalAmount += subtotal;
          }
          
          // Insert Order
          const [orderResult] = await connection.query(
            `INSERT INTO Orders (UserId, OrderDate, TotalAmount, DiscountAmount, FinalAmount, Status, ShippingAddress, PaymentMethod, UpdatedAt)
             VALUES (?, NOW(), ?, 0, ?, 'Pending', ?, ?, NOW())`,
            [UserId, totalAmount.toFixed(2), totalAmount.toFixed(2), ShippingAddress, PaymentMethod]
          );
          
          const orderId = orderResult.insertId;
          
          // Insert OrderDetails and update stock
          for (const item of orderItems) {
            const subtotal = parseFloat(item.UnitPrice) * parseInt(item.Quantity);
            
            // Insert OrderDetail
            await connection.query(
              `INSERT INTO OrderDetails (OrderId, ProductId, Quantity, UnitPrice, Discount, Subtotal)
               VALUES (?, ?, ?, ?, 0, ?)`,
              [orderId, item.ProductId, item.Quantity, item.UnitPrice, subtotal.toFixed(2)]
            );
            
            // Update product stock
            await connection.query(
              'UPDATE Products SET Stock = Stock - ?, UpdatedAt = NOW() WHERE ProductId = ?',
              [item.Quantity, item.ProductId]
            );
          }
          
          await connection.commit();
          
          console.log(`✅ Order created successfully: OrderId=${orderId}, Total=${totalAmount.toFixed(2)}`);
          
          return {
            Success: true,
            Message: 'Order created successfully',
            OrderId: orderId
          };
          
        } catch (error) {
          await connection.rollback();
          console.error('❌ CreateOrder error:', error.message);
          return {
            Success: false,
            Message: `Error creating order: ${error.message}`
          };
        } finally {
          connection.release();
        }
      },
      
      /**
       * GetOrder - Sipariş detaylarını getirir
       */
      GetOrder: async (args) => {
        console.log('\n🔵 GetOrder called with:', JSON.stringify(args, null, 2));
        
        try {
          const { OrderId } = args;
          
          if (!OrderId) {
            return {
              Success: false,
              Message: 'OrderId is required'
            };
          }
          
          // Get order with user info
          const [orders] = await db.query(
            `SELECT o.*, CONCAT(u.FirstName, ' ', u.LastName) as UserName, u.Email
             FROM Orders o
             INNER JOIN Users u ON o.UserId = u.UserId
             WHERE o.OrderId = ?`,
            [OrderId]
          );
          
          if (!orders || orders.length === 0) {
            return {
              Success: false,
              Message: `Order with ID ${OrderId} not found`
            };
          }
          
          const order = orders[0];
          
          // Get order items with product info
          const [items] = await db.query(
            `SELECT od.*, p.Name as ProductName
             FROM OrderDetails od
             INNER JOIN Products p ON od.ProductId = p.ProductId
             WHERE od.OrderId = ?
             ORDER BY od.OrderDetailId`,
            [OrderId]
          );
          
          // Format order items
          const orderItems = items.map(item => ({
            OrderItemId: item.OrderDetailId,
            OrderId: item.OrderId,
            ProductId: item.ProductId,
            ProductName: item.ProductName,
            Quantity: item.Quantity,
            UnitPrice: parseFloat(item.UnitPrice),
            Subtotal: parseFloat(item.Subtotal)
          }));
          
          const orderDetail = {
            OrderId: order.OrderId,
            UserId: order.UserId,
            UserName: order.UserName,
            OrderDate: order.OrderDate.toISOString(),
            TotalAmount: parseFloat(order.FinalAmount),
            Status: order.Status,
            ShippingAddress: order.ShippingAddress,
            PaymentMethod: order.PaymentMethod,
            Items: { OrderItem: orderItems },
            CreatedAt: order.OrderDate.toISOString()
          };
          
          console.log(`✅ Order retrieved: OrderId=${OrderId}`);
          
          return {
            Success: true,
            Message: 'Order retrieved successfully',
            Order: orderDetail
          };
          
        } catch (error) {
          console.error('❌ GetOrder error:', error.message);
          return {
            Success: false,
            Message: `Error retrieving order: ${error.message}`
          };
        }
      },
      
      /**
       * GetUserOrders - Kullanıcının tüm siparişlerini getirir
       */
      GetUserOrders: async (args) => {
        console.log('\n🔵 GetUserOrders called with:', JSON.stringify(args, null, 2));
        
        try {
          const { UserId } = args;
          
          if (!UserId) {
            return {
              Success: false,
              Message: 'UserId is required'
            };
          }
          
          // Get user's orders
          const [orders] = await db.query(
            `SELECT OrderId, UserId, OrderDate, FinalAmount as TotalAmount, Status, 
                    ShippingAddress, PaymentMethod, OrderDate as CreatedAt
             FROM Orders
             WHERE UserId = ?
             ORDER BY OrderDate DESC`,
            [UserId]
          );
          
          if (!orders || orders.length === 0) {
            return {
              Success: true,
              Message: `No orders found for user ${UserId}`,
              Orders: { Order: [] }
            };
          }
          
          // Format orders
          const formattedOrders = orders.map(order => ({
            OrderId: order.OrderId,
            UserId: order.UserId,
            OrderDate: order.OrderDate.toISOString(),
            TotalAmount: parseFloat(order.TotalAmount),
            Status: order.Status,
            ShippingAddress: order.ShippingAddress,
            PaymentMethod: order.PaymentMethod,
            CreatedAt: order.CreatedAt.toISOString()
          }));
          
          console.log(`✅ Retrieved ${orders.length} orders for user ${UserId}`);
          
          return {
            Success: true,
            Message: `Found ${orders.length} orders`,
            Orders: { Order: formattedOrders }
          };
          
        } catch (error) {
          console.error('❌ GetUserOrders error:', error.message);
          return {
            Success: false,
            Message: `Error retrieving user orders: ${error.message}`
          };
        }
      },
      
      /**
       * UpdateOrderStatus - Sipariş durumunu günceller
       */
      UpdateOrderStatus: async (args) => {
        console.log('\n🔵 UpdateOrderStatus called with:', JSON.stringify(args, null, 2));
        
        try {
          const { OrderId, Status } = args;
          
          if (!OrderId || !Status) {
            return {
              Success: false,
              Message: 'OrderId and Status are required'
            };
          }
          
          // Validate status
          const validStatuses = ['Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled'];
          if (!validStatuses.includes(Status)) {
            return {
              Success: false,
              Message: `Invalid status. Valid values: ${validStatuses.join(', ')}`
            };
          }
          
          // Check if order exists
          const [orders] = await db.query(
            'SELECT OrderId, Status FROM Orders WHERE OrderId = ?',
            [OrderId]
          );
          
          if (!orders || orders.length === 0) {
            return {
              Success: false,
              Message: `Order with ID ${OrderId} not found`
            };
          }
          
          const currentStatus = orders[0].Status;
          
          // Prevent updating cancelled orders
          if (currentStatus === 'Cancelled') {
            return {
              Success: false,
              Message: 'Cannot update status of a cancelled order'
            };
          }
          
          // Update order status
          const [result] = await db.query(
            'UPDATE Orders SET Status = ?, UpdatedAt = NOW() WHERE OrderId = ?',
            [Status, OrderId]
          );
          
          if (result.affectedRows === 0) {
            return {
              Success: false,
              Message: 'Failed to update order status'
            };
          }
          
          console.log(`✅ Order status updated: OrderId=${OrderId}, Status=${currentStatus} -> ${Status}`);
          
          return {
            Success: true,
            Message: `Order status updated from ${currentStatus} to ${Status}`
          };
          
        } catch (error) {
          console.error('❌ UpdateOrderStatus error:', error.message);
          return {
            Success: false,
            Message: `Error updating order status: ${error.message}`
          };
        }
      },
      
      /**
       * CancelOrder - Siparişi iptal eder ve stokları geri ekler
       */
      CancelOrder: async (args) => {
        console.log('\n🔵 CancelOrder called with:', JSON.stringify(args, null, 2));
        
        const connection = await db.getConnection();
        
        try {
          await connection.beginTransaction();
          
          const { OrderId } = args;
          
          if (!OrderId) {
            return {
              Success: false,
              Message: 'OrderId is required'
            };
          }
          
          // Check if order exists and get status
          const [orders] = await connection.query(
            'SELECT OrderId, Status FROM Orders WHERE OrderId = ?',
            [OrderId]
          );
          
          if (!orders || orders.length === 0) {
            return {
              Success: false,
              Message: `Order with ID ${OrderId} not found`
            };
          }
          
          const currentStatus = orders[0].Status;
          
          // Check if order can be cancelled
          if (currentStatus === 'Cancelled') {
            return {
              Success: false,
              Message: 'Order is already cancelled'
            };
          }
          
          if (currentStatus === 'Delivered') {
            return {
              Success: false,
              Message: 'Cannot cancel a delivered order'
            };
          }
          
          // Get order items to restore stock
          const [items] = await connection.query(
            'SELECT ProductId, Quantity FROM OrderDetails WHERE OrderId = ?',
            [OrderId]
          );
          
          // Restore stock for each item
          for (const item of items) {
            await connection.query(
              'UPDATE Products SET Stock = Stock + ?, UpdatedAt = NOW() WHERE ProductId = ?',
              [item.Quantity, item.ProductId]
            );
          }
          
          // Update order status to Cancelled
          await connection.query(
            'UPDATE Orders SET Status = ?, UpdatedAt = NOW() WHERE OrderId = ?',
            ['Cancelled', OrderId]
          );
          
          await connection.commit();
          
          console.log(`✅ Order cancelled: OrderId=${OrderId}, restored stock for ${items.length} items`);
          
          return {
            Success: true,
            Message: `Order cancelled successfully. Stock restored for ${items.length} items.`
          };
          
        } catch (error) {
          await connection.rollback();
          console.error('❌ CancelOrder error:', error.message);
          return {
            Success: false,
            Message: `Error cancelling order: ${error.message}`
          };
        } finally {
          connection.release();
        }
      }
      
    }
  }
};

// ============================================
// Start Server
// ============================================

const startServer = () => {
  const server = app.listen(PORT, () => {
    console.log('╔════════════════════════════════════════════════════════╗');
    console.log('║       🛒 SmartShop Order Service (SOAP)               ║');
    console.log('╠════════════════════════════════════════════════════════╣');
    console.log(`║  🌐 Server running on: http://localhost:${PORT}         ║`);
    console.log(`║  📄 WSDL available at: http://localhost:${PORT}/order?wsdl`);
    console.log(`║  🔌 SOAP endpoint:     http://localhost:${PORT}/order      ║`);
    console.log('║                                                        ║');
    console.log('║  📦 Available Operations:                              ║');
    console.log('║     • CreateOrder       - Create new order             ║');
    console.log('║     • GetOrder          - Get order details            ║');
    console.log('║     • GetUserOrders     - Get user orders              ║');
    console.log('║     • UpdateOrderStatus - Update order status          ║');
    console.log('║     • CancelOrder       - Cancel order & restore stock ║');
    console.log('╚════════════════════════════════════════════════════════╝');
  });

  // Create SOAP service
  soap.listen(server, '/order', orderService, wsdlXML, () => {
    console.log('✅ SOAP service initialized successfully');
  });
};

// Health check endpoint
app.get('/health', (req, res) => {
  res.json({ status: 'OK', service: 'OrderService', timestamp: new Date().toISOString() });
});

// Start the server
startServer();

module.exports = app;
