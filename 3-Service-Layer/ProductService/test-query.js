const db = require('./db');

async function testQuery() {
  try {
    console.log('Testing simple query with LIMIT and OFFSET...');
    
    const limit = 5;
    const offset = 0;
    
    console.log('Params:', { limit, offset });
    console.log('Types:', { limit: typeof limit, offset: typeof offset });
    
    // Try with query instead of execute
    const [products] = await db.query(
      'SELECT * FROM Products WHERE IsActive = 1 LIMIT ? OFFSET ?',
      [limit, offset]
    );
    
    console.log('✅ Query succeeded! Found', products.length, 'products');
    
    await db.end();
  } catch (error) {
    console.error('❌ Query failed:', error.message);
    await db.end();
  }
}

testQuery();
