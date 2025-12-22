const db = require('./db');

async function checkSchema() {
  try {
    const [columns] = await db.execute('DESCRIBE Products');
    console.log('Products table columns:');
    console.table(columns);
    
    await db.end();
  } catch (error) {
    console.error('Error:', error);
  }
}

checkSchema();
