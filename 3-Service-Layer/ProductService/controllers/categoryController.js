const db = require('../db');
const { validationResult } = require('express-validator');

// ==========================================
// CATEGORY CONTROLLER
// ==========================================

/**
 * Get all categories
 * GET /api/v1/products/categories
 */
exports.getAllCategories = async (req, res) => {
  try {
    const query = `
      SELECT 
        c.CategoryID,
        c.Name,
        c.Description,
        c.CreatedAt,
        COUNT(p.ProductID) as ProductCount
      FROM Categories c
      LEFT JOIN Products p ON c.CategoryID = p.CategoryID AND p.IsActive = 1
      GROUP BY c.CategoryID, c.Name, c.Description, c.CreatedAt
      ORDER BY c.Name ASC
    `;

    const [categories] = await db.execute(query);

    res.json({
      success: true,
      data: categories,
      count: categories.length
    });
  } catch (error) {
    console.error('Get all categories error:', error);
    res.status(500).json({ success: false, message: 'Kategoriler getirilemedi', error: error.message });
  }
};

/**
 * Get category by ID
 * GET /api/v1/products/categories/:id
 */
exports.getCategoryById = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;

    const query = `
      SELECT 
        c.CategoryID,
        c.Name,
        c.Description,
        c.CreatedAt,
        COUNT(p.ProductID) as ProductCount
      FROM Categories c
      LEFT JOIN Products p ON c.CategoryID = p.CategoryID AND p.IsActive = 1
      WHERE c.CategoryID = ?
      GROUP BY c.CategoryID, c.Name, c.Description, c.CreatedAt
    `;

    const [categories] = await db.execute(query, [id]);

    if (categories.length === 0) {
      return res.status(404).json({ success: false, message: 'Kategori bulunamadı' });
    }

    res.json({
      success: true,
      data: categories[0]
    });
  } catch (error) {
    console.error('Get category by ID error:', error);
    res.status(500).json({ success: false, message: 'Kategori getirilemedi', error: error.message });
  }
};

/**
 * Create new category (Admin)
 * POST /api/v1/products/categories
 */
exports.createCategory = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { name, description } = req.body;

    // Check if category name already exists
    const [existing] = await db.execute('SELECT CategoryID FROM Categories WHERE Name = ?', [name]);
    if (existing.length > 0) {
      return res.status(400).json({ success: false, message: 'Bu kategori adı zaten kullanılıyor' });
    }

    const query = 'INSERT INTO Categories (Name, Description) VALUES (?, ?)';
    const [result] = await db.execute(query, [name, description || null]);

    // Get created category
    const [categories] = await db.execute('SELECT * FROM Categories WHERE CategoryID = ?', [result.insertId]);

    res.status(201).json({
      success: true,
      message: 'Kategori başarıyla oluşturuldu',
      data: categories[0]
    });
  } catch (error) {
    console.error('Create category error:', error);
    res.status(500).json({ success: false, message: 'Kategori oluşturulamadı', error: error.message });
  }
};

/**
 * Update category (Admin)
 * PUT /api/v1/products/categories/:id
 */
exports.updateCategory = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;
    const { name, description } = req.body;

    // Check if category exists
    const [categories] = await db.execute('SELECT CategoryID FROM Categories WHERE CategoryID = ?', [id]);
    if (categories.length === 0) {
      return res.status(404).json({ success: false, message: 'Kategori bulunamadı' });
    }

    // Check if new name already exists (excluding current category)
    if (name) {
      const [existing] = await db.execute(
        'SELECT CategoryID FROM Categories WHERE Name = ? AND CategoryID != ?',
        [name, id]
      );
      if (existing.length > 0) {
        return res.status(400).json({ success: false, message: 'Bu kategori adı zaten kullanılıyor' });
      }
    }

    // Build update query dynamically
    const updates = [];
    const values = [];

    if (name !== undefined) { updates.push('Name = ?'); values.push(name); }
    if (description !== undefined) { updates.push('Description = ?'); values.push(description); }

    if (updates.length === 0) {
      return res.status(400).json({ success: false, message: 'Güncellenecek alan bulunamadı' });
    }

    values.push(id);

    const query = `UPDATE Categories SET ${updates.join(', ')} WHERE CategoryID = ?`;
    await db.execute(query, values);

    // Get updated category
    const [updatedCategories] = await db.execute('SELECT * FROM Categories WHERE CategoryID = ?', [id]);

    res.json({
      success: true,
      message: 'Kategori başarıyla güncellendi',
      data: updatedCategories[0]
    });
  } catch (error) {
    console.error('Update category error:', error);
    res.status(500).json({ success: false, message: 'Kategori güncellenemedi', error: error.message });
  }
};

/**
 * Delete category (Admin)
 * DELETE /api/v1/products/categories/:id
 */
exports.deleteCategory = async (req, res) => {
  try {
    const errors = validationResult(req);
    if (!errors.isEmpty()) {
      return res.status(400).json({ success: false, errors: errors.array() });
    }

    const { id } = req.params;

    // Check if category exists
    const [categories] = await db.execute('SELECT CategoryID FROM Categories WHERE CategoryID = ?', [id]);
    if (categories.length === 0) {
      return res.status(404).json({ success: false, message: 'Kategori bulunamadı' });
    }

    // Check if category has products
    const [products] = await db.execute('SELECT COUNT(*) as count FROM Products WHERE CategoryID = ? AND IsActive = 1', [id]);
    if (products[0].count > 0) {
      return res.status(400).json({
        success: false,
        message: 'Bu kategoriye ait aktif ürünler bulunmaktadır. Önce ürünleri siliniz veya başka kategoriye taşıyınız.'
      });
    }

    await db.execute('DELETE FROM Categories WHERE CategoryID = ?', [id]);

    res.json({
      success: true,
      message: 'Kategori başarıyla silindi'
    });
  } catch (error) {
    console.error('Delete category error:', error);
    res.status(500).json({ success: false, message: 'Kategori silinemedi', error: error.message });
  }
};
