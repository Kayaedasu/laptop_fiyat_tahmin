const grpc = require('@grpc/grpc-js');
const protoLoader = require('@grpc/proto-loader');
const path = require('path');

// Load Proto
const PROTO_PATH = path.join(__dirname, 'user.proto');
const packageDefinition = protoLoader.loadSync(PROTO_PATH, {
  keepCase: true,
  longs: String,
  enums: String,
  defaults: true,
  oneofs: true
});

const userProto = grpc.loadPackageDefinition(packageDefinition).userservice;

// Create Client
const client = new userProto.UserService(
  'localhost:50051',
  grpc.credentials.createInsecure()
);

console.log('🧪 UserService Test Client\n');

// Test 1: RegisterUser
console.log('📝 Test 1: RegisterUser');
client.RegisterUser({
  email: 'test@smartshop.com',
  password: 'Test123!',
  firstName: 'Test',
  lastName: 'User',
  phone: '05551234599'
}, (error, response) => {
  if (error) {
    console.error('❌ RegisterUser Error:', error.message);
  } else {
    console.log('✅ RegisterUser Response:', response);
    
    if (response.success) {
      // Test 2: LoginUser
      console.log('\n🔐 Test 2: LoginUser');
      client.LoginUser({
        email: 'test@smartshop.com',
        password: 'Test123!'
      }, (error, loginResponse) => {
        if (error) {
          console.error('❌ LoginUser Error:', error.message);
        } else {
          console.log('✅ LoginUser Response:', loginResponse);
          
          if (loginResponse.success) {
            const userId = loginResponse.user.userId;
            const token = loginResponse.token;
            console.log('🎫 JWT Token:', token);
            
            // Test 3: GetUser
            console.log('\n👤 Test 3: GetUser');
            client.GetUser({ userId: userId }, (error, getUserResponse) => {
              if (error) {
                console.error('❌ GetUser Error:', error.message);
              } else {
                console.log('✅ GetUser Response:', getUserResponse);
                
                // Test 4: UpdateUser
                console.log('\n✏️ Test 4: UpdateUser');
                client.UpdateUser({
                  userId: userId,
                  firstName: 'Updated',
                  lastName: 'Name',
                  phone: '05559999999'
                }, (error, updateResponse) => {
                  if (error) {
                    console.error('❌ UpdateUser Error:', error.message);
                  } else {
                    console.log('✅ UpdateUser Response:', updateResponse);
                    
                    // Test 5: ListUsers
                    console.log('\n📋 Test 5: ListUsers');
                    client.ListUsers({ page: 1, pageSize: 5 }, (error, listResponse) => {
                      if (error) {
                        console.error('❌ ListUsers Error:', error.message);
                      } else {
                        console.log('✅ ListUsers Response:');
                        console.log('   Total Count:', listResponse.totalCount);
                        console.log('   Users:', listResponse.users.length);
                        
                        // Test 6: DeleteUser
                        console.log('\n🗑️ Test 6: DeleteUser');
                        client.DeleteUser({ userId: userId }, (error, deleteResponse) => {
                          if (error) {
                            console.error('❌ DeleteUser Error:', error.message);
                          } else {
                            console.log('✅ DeleteUser Response:', deleteResponse);
                            console.log('\n✅ TÜM TESTLER TAMAMLANDI!');
                            process.exit(0);
                          }
                        });
                      }
                    });
                  }
                });
              }
            });
          }
        }
      });
    }
  }
});

// Test existing user login
setTimeout(() => {
  console.log('\n🔐 Extra Test: Existing User Login (admin@smartshop.com)');
  client.LoginUser({
    email: 'admin@smartshop.com',
    password: 'hashed_password_123'  // Bu gerçek şifre değil, database'de hash'lenmiş
  }, (error, response) => {
    if (error) {
      console.error('❌ Error:', error.message);
    } else {
      console.log('Response:', response.message);
      // Not: Database'deki şifre bcrypt hash'lenmiş, bu yüzden plain text ile giriş yapamayız
      // Gerçek test için yeni kullanıcı oluşturmalıyız
    }
  });
}, 2000);
