const db = require('../db');
const { validationResult } = require('express-validator');

// ==========================================
// PRODUCT CONTROLLER (Updated for actual DB schema)
// ==========================================

/**
 * Get all products with filters, search, pagination
 */
exports.getAllProducts = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const page = parseInt(req.query.page) || 1;
    const limit = parseInt(req.query.limit) || 10;
    const offset = (page - 1) * limit;

    const categoryId = req.query.categoryId;
    const minPrice = req.query.minPrice;
    const maxPrice = req.query.maxPrice;
    const inStock = req.query.inStock;
    const search = req.query.search;
    const sortBy = req.query.sortBy || 'createdAt';
    const order = req.query.order || 'DESC';

    // Build WHERE clause
    let whereConditions = ['p.IsActive = 1'];
    let queryParams = [];

    if (categoryId) {
      whereConditions.push('p.CategoryId = ?');
      queryParams.push(categoryId);
    }

    if (minPrice) {
      whereConditions.push('p.Price >= ?');
      queryParams.push(minPrice);
    }

    if (maxPrice) {
      whereConditions.push('p.Price <= ?');
      queryParams.push(maxPrice);
    }

    if (inStock === 'true') {
      whereConditions.push('p.Stock > 0');
    }

    if (search) {
      whereConditions.push('(p.Name LIKE ? OR p.Description LIKE ? OR p.Brand LIKE ?)');
      queryParams.push(`%${search}%`, `%${search}%`, `%${search}%`);
    }

    const whereClause = whereConditions.join(' AND ');

    // Count total
    const countQuery = `SELECT COUNT(*) as total FROM Products p WHERE ${whereClause}`;
    const [[{ total }]] = await db.query(countQuery, queryParams);

    // Build ORDER BY clause
    const sortColumns = {
      name: 'p.Name',
      price: 'p.Price',
      brand: 'p.Brand',
      createdAt: 'p.CreatedAt'
    };
    const sortColumn = sortColumns[sortBy] || 'p.CreatedAt';
    const orderDirection = order.toUpperCase() === 'ASC' ? 'ASC' : 'DESC';

    // Get products
    const query = `
      SELECT 
        p.ProductId,
        p.Name,
        p.Brand,
        p.Model,
        p.Processor,
        p.RAM,
        p.Storage,
        p.StorageType,
        p.GPU,
        p.ScreenSize,
        p.Resolution,
        p.Price,
        p.Stock,
        p.Discount,
        p.CategoryId,
        c.Name as CategoryName,
        p.Description,
        p.ImageUrl,
        p.ProductCondition,
        p.IsActive,
        p.ViewCount,
        p.CreatedAt,
        p.UpdatedAt
      FROM Products p
      LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
      WHERE ${whereClause}
      ORDER BY ${sortColumn} ${orderDirection}
      LIMIT ? OFFSET ?
    `;

    const productParams = [...queryParams, limit, offset];
    const [products] = await db.query(query, productParams);

    res.json({
      success: true,
      data: products,
      pagination: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit)
      }
    });
  } catch (error) {
    console.error('Get all products error:', error);
    res.status(500).json({ success: false, message: 'Ürünler getirilemedi', error: error.message });
  }
};

/**
 * Get product by ID with details
 */
exports.getProductById = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;

    const query = `
      SELECT 
        p.ProductId,
        p.Name,
        p.Brand,
        p.Model,
        p.Processor,
        p.RAM,
        p.Storage,
        p.StorageType,
        p.GPU,
        p.ScreenSize,
        p.Resolution,
        p.Price,
        p.Stock,
        p.Discount,
        p.CategoryId,
        c.Name as CategoryName,
        c.Description as CategoryDescription,
        p.Description,
        p.ImageUrl,
        p.ProductCondition,
        p.IsActive,
        p.ViewCount,
        p.CreatedAt,
        p.UpdatedAt,
        (SELECT COUNT(*) FROM Reviews WHERE ProductId = p.ProductId) as ReviewCount,
        (SELECT AVG(Rating) FROM Reviews WHERE ProductId = p.ProductId) as AvgRating
      FROM Products p
      LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
      WHERE p.ProductId = ? AND p.IsActive = 1
    `;

    const [products] = await db.execute(query, [id]);

    if (products.length === 0) {
      return res.status(404).json({ success: false, message: 'Ürün bulunamadı' });
    }

    // Increment view count
    await db.execute('UPDATE Products SET ViewCount = ViewCount + 1 WHERE ProductId = ?', [id]);

    res.json({
      success: true,
      data: products[0]
    });
  } catch (error) {
    console.error('Get product by ID error:', error);
    res.status(500).json({ success: false, message: 'Ürün getirilemedi', error: error.message });
  }
};

/**
 * Get product reviews
 */
exports.getProductReviews = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;
    const page = parseInt(req.query.page) || 1;
    const limit = parseInt(req.query.limit) || 10;
    const offset = (page - 1) * limit;

    // Check if product exists
    const [products] = await db.execute('SELECT ProductId FROM Products WHERE ProductId = ?', [id]);
    if (products.length === 0) {
      return res.status(404).json({ success: false, message: 'Ürün bulunamadı' });
    }

    // Count total
    const [[{ total }]] = await db.execute(
      'SELECT COUNT(*) as total FROM Reviews WHERE ProductId = ?',
      [id]
    );

    // Get reviews
    const query = `
      SELECT 
        r.ReviewId,
        r.UserId,
        CONCAT(u.FirstName, ' ', u.LastName) as UserName,
        r.Rating,
        r.Comment,
        r.CreatedAt
      FROM Reviews r
      JOIN Users u ON r.UserId = u.UserId
      WHERE r.ProductId = ?
      ORDER BY r.CreatedAt DESC
      LIMIT ? OFFSET ?
    `;

    const [reviews] = await db.query(query, [id, limit, offset]);

    res.json({
      success: true,
      data: reviews,
      pagination: {
        page,
        limit,
        total,
        totalPages: Math.ceil(total / limit)
      }
    });
  } catch (error) {
    console.error('Get product reviews error:', error);
    res.status(500).json({ success: false, message: 'Yorumlar getirilemedi', error: error.message });
  }
};

/**
 * Get top rated products (based on reviews)
 */
exports.getTopRatedProducts = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const limit = parseInt(req.query.limit) || 10;

    const query = `
      SELECT 
        p.ProductId,
        p.Name,
        p.Brand,
        p.Model,
        p.Price,
        p.Stock,
        p.Discount,
        p.CategoryId,
        c.Name as CategoryName,
        p.ImageUrl,
        (SELECT COUNT(*) FROM Reviews WHERE ProductId = p.ProductId) as ReviewCount,
        (SELECT AVG(Rating) FROM Reviews WHERE ProductId = p.ProductId) as AvgRating
      FROM Products p
      LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
      WHERE p.IsActive = 1
      HAVING ReviewCount > 0
      ORDER BY AvgRating DESC, ReviewCount DESC
      LIMIT ?
    `;

    const [products] = await db.query(query, [limit]);

    res.json({
      success: true,
      data: products
    });
  } catch (error) {
    console.error('Get top rated products error:', error);
    res.status(500).json({ success: false, message: 'En iyi ürünler getirilemedi', error: error.message });
  }
};

/**
 * Get low stock products (Admin)
 */
exports.getLowStockProducts = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const threshold = parseInt(req.query.threshold) || 10;

    const query = `
      SELECT 
        p.ProductId,
        p.Name,
        p.Brand,
        p.Model,
        p.Stock,
        p.CategoryId,
        c.Name as CategoryName,
        p.Price
      FROM Products p
      LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
      WHERE p.IsActive = 1 AND p.Stock <= ?
      ORDER BY p.Stock ASC
    `;

    const [products] = await db.execute(query, [threshold]);

    res.json({
      success: true,
      data: products,
      count: products.length
    });
  } catch (error) {
    console.error('Get low stock products error:', error);
    res.status(500).json({ success: false, message: 'Düşük stoklu ürünler getirilemedi', error: error.message });
  }
};

/**
 * Create new product (Admin)
 */
exports.createProduct = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const {
      name, brand, model, processor, ram, storage, storageType, gpu,
      screenSize, resolution, price, stock, discount, categoryId,
      description, imageUrl, productCondition
    } = req.body;

    // Check if category exists
    const [categories] = await db.execute('SELECT CategoryId FROM Categories WHERE CategoryId = ?', [categoryId]);
    if (categories.length === 0) {
      return res.status(404).json({ success: false, message: 'Kategori bulunamadı' });
    }

    const query = `
      INSERT INTO Products (
        Name, Brand, Model, Processor, RAM, Storage, StorageType, GPU,
        ScreenSize, Resolution, Price, Stock, Discount, CategoryId,
        Description, ImageUrl, ProductCondition
      )
      VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
    `;

    const [result] = await db.execute(query, [
      name,
      brand,
      model || null,
      processor || null,
      ram,
      storage,
      storageType || 'SSD',
      gpu || null,
      screenSize || null,
      resolution || null,
      price,
      stock || 0,
      discount || 0,
      categoryId,
      description || null,
      imageUrl || null,
      productCondition || 'New'
    ]);

    // Get created product
    const [products] = await db.execute('SELECT * FROM Products WHERE ProductId = ?', [result.insertId]);

    res.status(201).json({
      success: true,
      message: 'Ürün başarıyla oluşturuldu',
      data: products[0]
    });
  } catch (error) {
    console.error('Create product error:', error);
    res.status(500).json({ success: false, message: 'Ürün oluşturulamadı', error: error.message });
  }
};

/**
 * Update product (Admin)
 */
exports.updateProduct = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;
    const updates = [];
    const values = [];

    // Check if product exists
    const [products] = await db.execute('SELECT ProductId FROM Products WHERE ProductId = ?', [id]);
    if (products.length === 0) {
      return res.status(404).json({ success: false, message: 'Ürün bulunamadı' });
    }

    // Build dynamic update query
    const fields = [
      'name', 'brand', 'model', 'processor', 'ram', 'storage', 'storageType', 'gpu',
      'screenSize', 'resolution', 'price', 'stock', 'discount', 'categoryId',
      'description', 'imageUrl', 'productCondition', 'isActive'
    ];

    const dbFields = {
      name: 'Name', brand: 'Brand', model: 'Model', processor: 'Processor',
      ram: 'RAM', storage: 'Storage', storageType: 'StorageType', gpu: 'GPU',
      screenSize: 'ScreenSize', resolution: 'Resolution', price: 'Price',
      stock: 'Stock', discount: 'Discount', categoryId: 'CategoryId',
      description: 'Description', imageUrl: 'ImageUrl',
      productCondition: 'ProductCondition', isActive: 'IsActive'
    };

    fields.forEach(field => {
      if (req.body[field] !== undefined) {
        updates.push(`${dbFields[field]} = ?`);
        values.push(req.body[field]);
      }
    });

    if (updates.length === 0) {
      return res.status(400).json({ success: false, message: 'Güncellenecek alan bulunamadı' });
    }

    values.push(id);
    const query = `UPDATE Products SET ${updates.join(', ')} WHERE ProductId = ?`;
    await db.execute(query, values);

    // Get updated product
    const [updatedProducts] = await db.execute('SELECT * FROM Products WHERE ProductId = ?', [id]);

    res.json({
      success: true,
      message: 'Ürün başarıyla güncellendi',
      data: updatedProducts[0]
    });
  } catch (error) {
    console.error('Update product error:', error);
    res.status(500).json({ success: false, message: 'Ürün güncellenemedi', error: error.message });
  }
};

/**
 * Update product stock (Admin)
 */
exports.updateProductStock = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;
    const { quantity } = req.body;

    // Check if product exists
    const [products] = await db.execute('SELECT Stock FROM Products WHERE ProductId = ?', [id]);
    if (products.length === 0) {
      return res.status(404).json({ success: false, message: 'Ürün bulunamadı' });
    }

    const newStock = products[0].Stock + quantity;

    if (newStock < 0) {
      return res.status(400).json({ success: false, message: 'Stok negatif olamaz' });
    }

    await db.execute('UPDATE Products SET Stock = ? WHERE ProductId = ?', [newStock, id]);

    res.json({
      success: true,
      message: 'Stok başarıyla güncellendi',
      data: {
        productId: id,
        oldStock: products[0].Stock,
        change: quantity,
        newStock
      }
    });
  } catch (error) {
    console.error('Update product stock error:', error);
    res.status(500).json({ success: false, message: 'Stok güncellenemedi', error: error.message });
  }
};

/**
 * Delete product (Admin - Soft delete)
 */
exports.deleteProduct = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;

    // Check if product exists
    const [products] = await db.execute('SELECT ProductId FROM Products WHERE ProductId = ?', [id]);
    if (products.length === 0) {
      return res.status(404).json({ success: false, message: 'Ürün bulunamadı' });
    }

    // Soft delete
    await db.execute('UPDATE Products SET IsActive = 0 WHERE ProductId = ?', [id]);

    res.json({
      success: true,
      message: 'Ürün başarıyla silindi'
    });
  } catch (error) {
    console.error('Delete product error:', error);
    res.status(500).json({ success: false, message: 'Ürün silinemedi', error: error.message });
  }
};
