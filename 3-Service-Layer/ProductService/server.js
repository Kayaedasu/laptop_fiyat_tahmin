const express = require('express');
const cors = require('cors');
const morgan = require('morgan');
require('dotenv').config();

const productRoutes = require('./routes/productRoutes');

const app = express();
const PORT = process.env.PORT || 3001;

// Middleware
app.use(cors());
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(morgan('dev'));

// Routes
app.get('/', (req, res) => {
  res.json({
    message: 'SmartShop ProductService REST API',
    version: '1.0.0',
    endpoints: {
      products: '/api/v1/products',
      categories: '/api/v1/categories'
    }
  });
});

app.use('/api/v1/products', productRoutes);

// Error handling middleware
app.use((err, req, res, next) => {
  console.error(err.stack);
  res.status(500).json({
    success: false,
    message: 'Sunucu hatası',
    error: process.env.NODE_ENV === 'development' ? err.message : undefined
  });
});

// 404 handler
app.use((req, res) => {
  res.status(404).json({
    success: false,
    message: 'Endpoint bulunamadı'
  });
});

// Start server
app.listen(PORT, () => {
  console.log(`🚀 ProductService (REST API) is running on http://localhost:${PORT}`);
  console.log(`📦 API Base URL: http://localhost:${PORT}/api/v1`);
});

module.exports = app;
