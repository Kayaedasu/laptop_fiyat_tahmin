const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');
const bcrypt = require('bcrypt');
const jwt = require('jsonwebtoken');
const db = require('./db');
require('dotenv').config();

// Load Proto File
const PROTO_PATH = path.join(__dirname, 'user.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true
});

const userProto = grpc.loadPackageDefinition(packageDefinition).userservice;

// ============================================
// gRPC Service Implementations
// ============================================

// 1. RegisterUser
async function RegisterUser(call, callback) {
  try {
    const { email, password, firstName, lastName, phone } = call.request;

    // Validate
    if (!email || !password || !firstName || !lastName) {
      return callback(null, {
        success: false,
        message: 'Tüm alanlar zorunludur',
        user: null
      });
    }

    // Check if user exists
    const [existing] = await db.query('SELECT * FROM Users WHERE Email = ?', [email]);
    if (existing.length > 0) {
      return callback(null, {
        success: false,
        message: 'Bu email adresi zaten kullanılıyor',
        user: null
      });
    }

    // Hash password
    const hashedPassword = await bcrypt.hash(password, 10);

    // Insert user
    const [result] = await db.query(
      'INSERT INTO Users (Email, Password, FirstName, LastName, Phone, Role) VALUES (?, ?, ?, ?, ?, ?)',
      [email, hashedPassword, firstName, lastName, phone || null, 'Customer']
    );

    // Get created user
    const [users] = await db.query('SELECT * FROM Users WHERE UserId = ?', [result.insertId]);
    const user = users[0];

    callback(null, {
      success: true,
      message: 'Kullanıcı başarıyla oluşturuldu',
      user: {
        userId: user.UserId,
        email: user.Email,
        firstName: user.FirstName,
        lastName: user.LastName,
        phone: user.Phone,
        role: user.Role,
        isActive: user.IsActive,
        createdAt: user.CreatedAt.toISOString()
      }
    });
  } catch (error) {
    console.error('RegisterUser error:', error);
    callback(null, {
      success: false,
      message: 'Kayıt sırasında hata oluştu: ' + error.message,
      user: null
    });
  }
}

// 2. LoginUser
async function LoginUser(call, callback) {
  try {
    const { email, password } = call.request;

    // Validate
    if (!email || !password) {
      return callback(null, {
        success: false,
        message: 'Email ve şifre gereklidir',
        token: '',
        user: null
      });
    }

    // Find user
    const [users] = await db.query('SELECT * FROM Users WHERE Email = ?', [email]);
    if (users.length === 0) {
      return callback(null, {
        success: false,
        message: 'Kullanıcı bulunamadı',
        token: '',
        user: null
      });
    }

    const user = users[0];

    // Check password
    const isPasswordValid = await bcrypt.compare(password, user.Password);
    if (!isPasswordValid) {
      return callback(null, {
        success: false,
        message: 'Hatalı şifre',
        token: '',
        user: null
      });
    }

    // Check if active
    if (!user.IsActive) {
      return callback(null, {
        success: false,
        message: 'Hesap aktif değil',
        token: '',
        user: null
      });
    }

    // Generate JWT token
    const token = jwt.sign(
      { userId: user.UserId, email: user.Email, role: user.Role },
      process.env.JWT_SECRET,
      { expiresIn: process.env.JWT_EXPIRES_IN }
    );

    callback(null, {
      success: true,
      message: 'Giriş başarılı',
      token: token,
      user: {
        userId: user.UserId,
        email: user.Email,
        firstName: user.FirstName,
        lastName: user.LastName,
        phone: user.Phone,
        role: user.Role,
        isActive: user.IsActive,
        createdAt: user.CreatedAt.toISOString()
      }
    });
  } catch (error) {
    console.error('LoginUser error:', error);
    callback(null, {
      success: false,
      message: 'Giriş sırasında hata oluştu: ' + error.message,
      token: '',
      user: null
    });
  }
}

// 3. GetUser
async function GetUser(call, callback) {
  try {
    const { userId } = call.request;

    const [users] = await db.query('SELECT * FROM Users WHERE UserId = ?', [userId]);
    if (users.length === 0) {
      return callback(null, {
        success: false,
        message: 'Kullanıcı bulunamadı',
        user: null
      });
    }

    const user = users[0];
    callback(null, {
      success: true,
      message: 'Kullanıcı bulundu',
      user: {
        userId: user.UserId,
        email: user.Email,
        firstName: user.FirstName,
        lastName: user.LastName,
        phone: user.Phone,
        role: user.Role,
        isActive: user.IsActive,
        createdAt: user.CreatedAt.toISOString()
      }
    });
  } catch (error) {
    console.error('GetUser error:', error);
    callback(null, {
      success: false,
      message: 'Kullanıcı getirilirken hata oluştu: ' + error.message,
      user: null
    });
  }
}

// 4. UpdateUser
async function UpdateUser(call, callback) {
  try {
    const { userId, firstName, lastName, phone } = call.request;

    // Check if user exists
    const [existing] = await db.query('SELECT * FROM Users WHERE UserId = ?', [userId]);
    if (existing.length === 0) {
      return callback(null, {
        success: false,
        message: 'Kullanıcı bulunamadı',
        user: null
      });
    }

    // Update user
    await db.query(
      'UPDATE Users SET FirstName = ?, LastName = ?, Phone = ? WHERE UserId = ?',
      [firstName, lastName, phone || null, userId]
    );

    // Get updated user
    const [users] = await db.query('SELECT * FROM Users WHERE UserId = ?', [userId]);
    const user = users[0];

    callback(null, {
      success: true,
      message: 'Kullanıcı güncellendi',
      user: {
        userId: user.UserId,
        email: user.Email,
        firstName: user.FirstName,
        lastName: user.LastName,
        phone: user.Phone,
        role: user.Role,
        isActive: user.IsActive,
        createdAt: user.CreatedAt.toISOString()
      }
    });
  } catch (error) {
    console.error('UpdateUser error:', error);
    callback(null, {
      success: false,
      message: 'Kullanıcı güncellenirken hata oluştu: ' + error.message,
      user: null
    });
  }
}

// 5. DeleteUser
async function DeleteUser(call, callback) {
  try {
    const { userId } = call.request;

    // Check if user exists
    const [existing] = await db.query('SELECT * FROM Users WHERE UserId = ?', [userId]);
    if (existing.length === 0) {
      return callback(null, {
        success: false,
        message: 'Kullanıcı bulunamadı'
      });
    }

    // Soft delete (set IsActive to false)
    await db.query('UPDATE Users SET IsActive = FALSE WHERE UserId = ?', [userId]);

    callback(null, {
      success: true,
      message: 'Kullanıcı silindi'
    });
  } catch (error) {
    console.error('DeleteUser error:', error);
    callback(null, {
      success: false,
      message: 'Kullanıcı silinirken hata oluştu: ' + error.message
    });
  }
}

// 6. ListUsers
async function ListUsers(call, callback) {
  try {
    const { page = 1, pageSize = 10 } = call.request;
    const offset = (page - 1) * pageSize;

    // Get total count
    const [countResult] = await db.query('SELECT COUNT(*) as total FROM Users WHERE IsActive = TRUE');
    const totalCount = countResult[0].total;

    // Get users
    const [users] = await db.query(
      'SELECT * FROM Users WHERE IsActive = TRUE ORDER BY CreatedAt DESC LIMIT ? OFFSET ?',
      [pageSize, offset]
    );

    const userList = users.map(user => ({
      userId: user.UserId,
      email: user.Email,
      firstName: user.FirstName,
      lastName: user.LastName,
      phone: user.Phone,
      role: user.Role,
      isActive: user.IsActive,
      createdAt: user.CreatedAt.toISOString()
    }));

    callback(null, {
      success: true,
      message: `${userList.length} kullanıcı bulundu`,
      users: userList,
      totalCount: totalCount
    });
  } catch (error) {
    console.error('ListUsers error:', error);
    callback(null, {
      success: false,
      message: 'Kullanıcılar listelenirken hata oluştu: ' + error.message,
      users: [],
      totalCount: 0
    });
  }
}

// ============================================
// Start gRPC Server
// ============================================
function main() {
  const server = new grpc.Server();

  server.addService(userProto.UserService.service, {
    RegisterUser,
    LoginUser,
    GetUser,
    UpdateUser,
    DeleteUser,
    ListUsers
  });

  const PORT = process.env.PORT || 50051;
  server.bindAsync(
    `0.0.0.0:${PORT}`,
    grpc.ServerCredentials.createInsecure(),
    (error, port) => {
      if (error) {
        console.error('❌ Server failed to bind:', error);
        return;
      }
      console.log(`🚀 UserService (gRPC) is running on port ${port}`);
      server.start();
    }
  );
}

main();
