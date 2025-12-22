const express = require('express');
const router = express.Router();
const { body, query, param } = require('express-validator');
const productController = require('../controllers/productController');
const categoryController = require('../controllers/categoryController');

// ==========================================
// CATEGORY ROUTES
// ==========================================

// Get all categories
router.get('/categories', categoryController.getAllCategories);

// Get category by ID
router.get('/categories/:id', 
  param('id').isInt().withMessage('Geçersiz kategori ID'),
  categoryController.getCategoryById
);

// Create category (Admin)
router.post('/categories',
  body('name').trim().notEmpty().withMessage('Kategori adı gerekli'),
  body('description').optional().trim(),
  categoryController.createCategory
);

// Update category (Admin)
router.put('/categories/:id',
  param('id').isInt().withMessage('Geçersiz kategori ID'),
  body('name').optional().trim().notEmpty().withMessage('Kategori adı boş olamaz'),
  body('description').optional().trim(),
  categoryController.updateCategory
);

// Delete category (Admin)
router.delete('/categories/:id',
  param('id').isInt().withMessage('Geçersiz kategori ID'),
  categoryController.deleteCategory
);

// ==========================================
// PRODUCT ROUTES
// ==========================================

// Get all products with filters, search, pagination
router.get('/', 
  query('page').optional().isInt({ min: 1 }).withMessage('Geçersiz sayfa numarası'),
  query('limit').optional().isInt({ min: 1, max: 100 }).withMessage('Geçersiz limit'),
  query('categoryId').optional().isInt().withMessage('Geçersiz kategori ID'),
  query('minPrice').optional().isFloat({ min: 0 }).withMessage('Geçersiz minimum fiyat'),
  query('maxPrice').optional().isFloat({ min: 0 }).withMessage('Geçersiz maksimum fiyat'),
  query('inStock').optional().isBoolean().withMessage('Geçersiz stok durumu'),
  query('search').optional().trim(),
  query('sortBy').optional().isIn(['name', 'price', 'rating', 'createdAt']).withMessage('Geçersiz sıralama'),
  query('order').optional().isIn(['ASC', 'DESC']).withMessage('Geçersiz sıralama yönü'),
  productController.getAllProducts
);

// Get product by ID (with details)
router.get('/:id',
  param('id').isInt().withMessage('Geçersiz ürün ID'),
  productController.getProductById
);

// Get product reviews
router.get('/:id/reviews',
  param('id').isInt().withMessage('Geçersiz ürün ID'),
  query('page').optional().isInt({ min: 1 }).withMessage('Geçersiz sayfa numarası'),
  query('limit').optional().isInt({ min: 1, max: 50 }).withMessage('Geçersiz limit'),
  productController.getProductReviews
);

// Get top rated products
router.get('/featured/top-rated',
  query('limit').optional().isInt({ min: 1, max: 50 }).withMessage('Geçersiz limit'),
  productController.getTopRatedProducts
);

// Get low stock products (Admin)
router.get('/admin/low-stock',
  query('threshold').optional().isInt({ min: 0 }).withMessage('Geçersiz eşik değeri'),
  productController.getLowStockProducts
);

// Create product (Admin)
router.post('/',
  body('name').trim().notEmpty().withMessage('Ürün adı gerekli'),
  body('brand').trim().notEmpty().withMessage('Marka gerekli'),
  body('ram').isInt({ min: 1, max: 128 }).withMessage('Geçerli bir RAM değeri giriniz (1-128 GB)'),
  body('storage').isInt({ min: 1 }).withMessage('Geçerli bir depolama değeri giriniz'),
  body('price').isFloat({ min: 0 }).withMessage('Geçerli bir fiyat giriniz'),
  body('stock').optional().isInt({ min: 0 }).withMessage('Geçerli bir stok miktarı giriniz'),
  body('categoryId').isInt().withMessage('Geçerli bir kategori ID giriniz'),
  body('imageUrl').optional().trim().isURL().withMessage('Geçerli bir URL giriniz'),
  body('model').optional().trim(),
  body('processor').optional().trim(),
  body('storageType').optional().isIn(['HDD', 'SSD', 'SSD+HDD']).withMessage('Geçersiz depolama tipi'),
  body('gpu').optional().trim(),
  body('screenSize').optional().isFloat({ min: 10, max: 20 }).withMessage('Geçerli bir ekran boyutu giriniz'),
  body('resolution').optional().trim(),
  body('discount').optional().isFloat({ min: 0, max: 100 }).withMessage('Geçerli bir indirim değeri giriniz'),
  body('description').optional().trim(),
  body('productCondition').optional().isIn(['New', 'Used', 'Refurbished']).withMessage('Geçersiz ürün durumu'),
  productController.createProduct
);

// Update product (Admin)
router.put('/:id',
  param('id').isInt().withMessage('Geçersiz ürün ID'),
  body('name').optional().trim().notEmpty().withMessage('Ürün adı boş olamaz'),
  body('brand').optional().trim().notEmpty().withMessage('Marka boş olamaz'),
  body('ram').optional().isInt({ min: 1, max: 128 }).withMessage('Geçerli bir RAM değeri giriniz'),
  body('storage').optional().isInt({ min: 1 }).withMessage('Geçerli bir depolama değeri giriniz'),
  body('price').optional().isFloat({ min: 0 }).withMessage('Geçerli bir fiyat giriniz'),
  body('stock').optional().isInt({ min: 0 }).withMessage('Geçerli bir stok miktarı giriniz'),
  body('categoryId').optional().isInt().withMessage('Geçerli bir kategori ID giriniz'),
  body('imageUrl').optional().trim().isURL().withMessage('Geçerli bir URL giriniz'),
  body('isActive').optional().isBoolean().withMessage('Geçerli bir aktiflik durumu giriniz'),
  productController.updateProduct
);

// Update product stock (Admin)
router.patch('/:id/stock',
  param('id').isInt().withMessage('Geçersiz ürün ID'),
  body('quantity').isInt().withMessage('Geçerli bir miktar giriniz'),
  productController.updateProductStock
);

// Delete product (Admin)
router.delete('/:id',
  param('id').isInt().withMessage('Geçersiz ürün ID'),
  productController.deleteProduct
);

module.exports = router;
