"""
Laptop Fiyat Tahmin Modeli Eğitimi
Gerçek CSV verisi ile eğitim
"""

import pandas as pd
import numpy as np
from sklearn.model_selection import train_test_split
from sklearn.ensemble import GradientBoostingRegressor
from sklearn.preprocessing import LabelEncoder
from sklearn.metrics import mean_absolute_error, mean_squared_error, r2_score
import joblib
import os
import re

print("=" * 70)
print("🤖 LAPTOP FİYAT TAHMİN MODELİ - EĞİTİM (GERÇEK VERİ)")
print("=" * 70)
print()

# CSV dosyasını oku
csv_path = os.path.join(os.path.dirname(__file__), "laptops_int_values.csv")
print(f"📂 CSV dosyası okunuyor: {csv_path}")

data = pd.read_csv(csv_path, sep=';', encoding='utf-8')
print(f"✅ {len(data)} adet veri okundu")
print(f"\n📊 İlk 5 satır:")
print(data.head())

# Sütun isimlerini kontrol et
print(f"\n📊 Sütunlar: {list(data.columns)}")

# Fiyat sütununu temizle ve numeric yap
def clean_price(price_str):
    """Fiyat stringini temizle ve float'a çevir"""
    if pd.isna(price_str):
        return None
    # Nokta ve virgül karakterlerini temizle, sadece sayıları al
    price_str = str(price_str).replace('.', '').replace(',', '.')
    try:
        return float(price_str)
    except:
        return None

data['Fiyat_Clean'] = data['Fiyat'].apply(clean_price)

# RAM ve Depolama değerlerini temizle
def clean_numeric(value):
    """Sayısal değerleri temizle"""
    if pd.isna(value):
        return None
    try:
        return float(value)
    except:
        return None

data['RAM_Clean'] = data['RAM'].apply(clean_numeric)
data['Depolama_Clean'] = data['Depolama'].apply(clean_numeric)

# Null değerleri temizle
print(f"\n🧹 Eksik veriler temizleniyor...")
initial_count = len(data)
data = data.dropna(subset=['Fiyat_Clean', 'RAM_Clean', 'Depolama_Clean', 'İşlemci', 'Ekran Kartı'])
print(f"   {initial_count - len(data)} satır silindi")
print(f"   Kalan veri: {len(data)} satır")

# CPU Tier çıkarma fonksiyonu
def get_cpu_tier(cpu_str):
    """CPU string'inden tier çıkar"""
    if pd.isna(cpu_str):
        return 5  # Default
    
    cpu_lower = str(cpu_str).lower()
    
    if 'i9' in cpu_lower or 'ryzen 9' in cpu_lower or 'ultra 9' in cpu_lower:
        return 9
    elif 'i7' in cpu_lower or 'ryzen 7' in cpu_lower or 'ultra 7' in cpu_lower:
        return 7
    elif 'i5' in cpu_lower or 'ryzen 5' in cpu_lower or 'ultra 5' in cpu_lower or 'm2' in cpu_lower or 'm3' in cpu_lower or 'm4' in cpu_lower:
        return 5
    elif 'i3' in cpu_lower or 'ryzen 3' in cpu_lower or 'm1' in cpu_lower:
        return 3
    elif 'celeron' in cpu_lower or 'pentium' in cpu_lower or 'n4020' in cpu_lower or 'n4120' in cpu_lower or 'n100' in cpu_lower or 'n150' in cpu_lower:
        return 1
    return 2  # Default

# CPU Nesil çıkarma fonksiyonu
def get_cpu_generation(cpu_str):
    """CPU nesli çıkar (Intel için 10, 11, 12, 13, 14, 15 gibi)"""
    if pd.isna(cpu_str):
        return 10  # Default
    
    cpu_str = str(cpu_str)
    
    # Intel nesilleri (i7-12700H, i5-1335U gibi)
    import re
    
    # Ultra serisi (155H, 258V gibi)
    if 'ultra' in cpu_str.lower():
        # Ultra 5, 7, 9 -> 15. nesil sayılır
        return 15
    
    # Intel Core i3/i5/i7/i9 nesilleri
    intel_match = re.search(r'[i]\d-(\d{1,2})\d{2,3}', cpu_str.lower())
    if intel_match:
        gen = int(intel_match.group(1))
        return min(gen, 15)  # Max 15. nesil
    
    # AMD Ryzen nesilleri (Ryzen 5 5600H -> 5, Ryzen 7 7730U -> 7)
    ryzen_match = re.search(r'ryzen\s+\d\s+(\d)', cpu_str.lower())
    if ryzen_match:
        gen = int(ryzen_match.group(1))
        return gen
    
    # Apple M serisi (M1, M2, M3, M4)
    if 'm1' in cpu_str.lower():
        return 11
    elif 'm2' in cpu_str.lower():
        return 12
    elif 'm3' in cpu_str.lower():
        return 13
    elif 'm4' in cpu_str.lower():
        return 14
    
    # Celeron, Pentium -> eski nesil
    if 'celeron' in cpu_str.lower() or 'pentium' in cpu_str.lower():
        return 8
    
    return 10  # Default orta nesil

# CPU Brand çıkarma fonksiyonu
def get_cpu_brand(cpu_str):
    """CPU markasını belirle"""
    if pd.isna(cpu_str):
        return 'Intel'
    
    cpu_lower = str(cpu_str).lower()
    
    if 'intel' in cpu_lower or 'core i' in cpu_lower or 'celeron' in cpu_lower or 'pentium' in cpu_lower or 'ultra' in cpu_lower:
        return 'Intel'
    elif 'amd' in cpu_lower or 'ryzen' in cpu_lower:
        return 'AMD'
    elif 'apple' in cpu_lower or 'm1' in cpu_lower or 'm2' in cpu_lower or 'm3' in cpu_lower or 'm4' in cpu_lower:
        return 'Apple'
    return 'Intel'  # Default

# GPU Type çıkarma fonksiyonu
def get_gpu_type(gpu_str):
    """GPU tipini belirle"""
    if pd.isna(gpu_str):
        return 'Entegre'
    
    gpu_lower = str(gpu_str).lower()
    
    if 'rtx 50' in gpu_lower or 'rtx50' in gpu_lower or 'rtx 40' in gpu_lower or 'rtx40' in gpu_lower:
        return 'RTX_Yeni'
    elif 'rtx 30' in gpu_lower or 'rtx30' in gpu_lower:
        return 'RTX_30'
    elif 'rtx' in gpu_lower:
        return 'RTX'
    elif 'gtx' in gpu_lower:
        return 'GTX'
    elif 'nvidia' in gpu_lower or 'geforce' in gpu_lower:
        return 'NVIDIA_Diger'
    elif 'radeon rx' in gpu_lower:
        return 'Radeon_RX'
    elif 'radeon' in gpu_lower or 'amd radeon' in gpu_lower:
        return 'Radeon'
    elif 'apple' in gpu_lower:
        return 'Apple_GPU'
    elif 'iris' in gpu_lower or 'arc' in gpu_lower:
        return 'Intel_Iris'
    elif 'intel uhd' in gpu_lower or 'uhd' in gpu_lower:
        return 'Intel_UHD'
    elif 'entegre' in gpu_lower or 'integrated' in gpu_lower:
        return 'Entegre'
    
    return 'Entegre'  # Default

# Feature'ları çıkar
print(f"\n🔧 Feature'lar çıkarılıyor...")
data['CPU_Seviye'] = data['İşlemci'].apply(get_cpu_tier)
data['CPU_Nesil'] = data['İşlemci'].apply(get_cpu_generation)
data['CPU_Marka'] = data['İşlemci'].apply(get_cpu_brand)
data['GPU_Tipi'] = data['Ekran Kartı'].apply(get_gpu_type)
data['Laptop_Marka'] = data['Marka'].fillna('Diğer')  # Marka bilgisi

print(f"\n📊 CPU Tier dağılımı:")
print(data['CPU_Seviye'].value_counts().sort_index())
print(f"\n📊 CPU Nesil dağılımı:")
print(data['CPU_Nesil'].value_counts().sort_index())
print(f"\n📊 CPU Marka dağılımı:")
print(data['CPU_Marka'].value_counts())
print(f"\n📊 GPU Tipi dağılımı:")
print(data['GPU_Tipi'].value_counts())
print(f"\n📊 Laptop Marka dağılımı:")
print(data['Laptop_Marka'].value_counts())

# Final dataframe
df = pd.DataFrame({
    'RAM': data['RAM_Clean'],
    'Depolama': data['Depolama_Clean'],
    'CPU_Seviye': data['CPU_Seviye'],
    'CPU_Nesil': data['CPU_Nesil'],
    'CPU_Marka': data['CPU_Marka'],
    'GPU_Tipi': data['GPU_Tipi'],
    'Laptop_Marka': data['Laptop_Marka'],
    'Fiyat': data['Fiyat_Clean']
})

print(f"\n📊 Veri özeti:")
print(df.describe())
print(f"\n📊 Fiyat aralığı: ₺{df['Fiyat'].min():.0f} - ₺{df['Fiyat'].max():.0f}")
print(f"📊 Ortalama fiyat: ₺{df['Fiyat'].mean():.0f}")

# Label Encoding
print("\n🔧 Label Encoding yapılıyor...")
label_encoders = {}

categorical_cols = ['CPU_Marka', 'GPU_Tipi', 'Laptop_Marka']
for col in categorical_cols:
    le = LabelEncoder()
    df[f'{col}_encoded'] = le.fit_transform(df[col])
    label_encoders[col] = le
    print(f"   {col}: {len(le.classes_)} kategori")

# Feature ve target ayırma
feature_columns = ['RAM', 'Depolama', 'CPU_Seviye', 'CPU_Nesil', 'CPU_Marka_encoded', 'GPU_Tipi_encoded', 'Laptop_Marka_encoded']
X = df[feature_columns]
y = df['Fiyat']

print(f"\n📊 Feature'lar: {feature_columns}")
print(f"📊 Feature shape: {X.shape}")
print(f"📊 Target shape: {y.shape}")

# Train-test split
X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
print(f"\n✅ Train set: {X_train.shape[0]} samples")
print(f"✅ Test set: {X_test.shape[0]} samples")

# Model eğitimi
print("\n🎯 Model eğitiliyor (Gradient Boosting)...")
model = GradientBoostingRegressor(
    n_estimators=200,
    learning_rate=0.1,
    max_depth=5,
    min_samples_split=5,
    min_samples_leaf=2,
    random_state=42,
    verbose=0
)

model.fit(X_train, y_train)
print("✅ Model eğitimi tamamlandı!")

# Tahmin ve değerlendirme
print("\n📊 Model performansı değerlendiriliyor...")
y_pred_train = model.predict(X_train)
y_pred_test = model.predict(X_test)

# Train metrikleri
train_mae = mean_absolute_error(y_train, y_pred_train)
train_rmse = np.sqrt(mean_squared_error(y_train, y_pred_train))
train_r2 = r2_score(y_train, y_pred_train)

# Test metrikleri
test_mae = mean_absolute_error(y_test, y_pred_test)
test_rmse = np.sqrt(mean_squared_error(y_test, y_pred_test))
test_r2 = r2_score(y_test, y_pred_test)

print("\n" + "="*70)
print("📈 PERFORMANS METRİKLERİ")
print("="*70)
print(f"\n🎯 TRAIN SET:")
print(f"   MAE  : ₺{train_mae:.2f}")
print(f"   RMSE : ₺{train_rmse:.2f}")
print(f"   R²   : {train_r2:.4f}")

print(f"\n🎯 TEST SET:")
print(f"   MAE  : ₺{test_mae:.2f}")
print(f"   RMSE : ₺{test_rmse:.2f}")
print(f"   R²   : {test_r2:.4f}")

# Feature importance
print("\n" + "="*70)
print("🔍 FEATURE ÖNEM SIRASI")
print("="*70)
feature_importance = pd.DataFrame({
    'feature': feature_columns,
    'importance': model.feature_importances_
}).sort_values('importance', ascending=False)

for idx, row in feature_importance.iterrows():
    print(f"   {row['feature']:<25} : {row['importance']:.4f}")

# Model kaydetme
print("\n" + "="*70)
print("💾 MODEL KAYDEDILIYOR")
print("="*70)

model_dir = os.path.join(os.path.dirname(__file__), "model")
os.makedirs(model_dir, exist_ok=True)

model_path = os.path.join(model_dir, "laptop_fiyat_model.pkl")
encoders_path = os.path.join(model_dir, "label_encoders.pkl")

joblib.dump(model, model_path)
joblib.dump(label_encoders, encoders_path)

print(f"✅ Model kaydedildi: {model_path}")
print(f"✅ Encoders kaydedildi: {encoders_path}")

# Test tahminleri
print("\n" + "="*70)
print("🧪 ÖRNEK TAHMİNLER")
print("="*70)

test_cases = [
    {"RAM": 8, "Depolama": 256, "CPU_Seviye": 3, "CPU_Nesil": 11, "CPU_Marka": "Intel", "GPU_Tipi": "Entegre", "Laptop_Marka": "HP"},
    {"RAM": 16, "Depolama": 512, "CPU_Seviye": 5, "CPU_Nesil": 12, "CPU_Marka": "Intel", "GPU_Tipi": "GTX", "Laptop_Marka": "Lenovo"},
    {"RAM": 16, "Depolama": 512, "CPU_Seviye": 7, "CPU_Nesil": 13, "CPU_Marka": "Intel", "GPU_Tipi": "RTX_30", "Laptop_Marka": "Asus"},
    {"RAM": 32, "Depolama": 1024, "CPU_Seviye": 9, "CPU_Nesil": 14, "CPU_Marka": "Intel", "GPU_Tipi": "RTX_Yeni", "Laptop_Marka": "MSI"},
    {"RAM": 16, "Depolama": 512, "CPU_Seviye": 5, "CPU_Nesil": 12, "CPU_Marka": "Apple", "GPU_Tipi": "Apple_GPU", "Laptop_Marka": "Apple"}
]

for i, test in enumerate(test_cases, 1):
    # Encode
    cpu_marka_enc = label_encoders['CPU_Marka'].transform([test['CPU_Marka']])[0]
    gpu_tipi_enc = label_encoders['GPU_Tipi'].transform([test['GPU_Tipi']])[0]
    laptop_marka_enc = label_encoders['Laptop_Marka'].transform([test['Laptop_Marka']])[0]
    
    X_test_sample = np.array([[
        test['RAM'],
        test['Depolama'],
        test['CPU_Seviye'],
        test['CPU_Nesil'],
        cpu_marka_enc,
        gpu_tipi_enc,
        laptop_marka_enc
    ]])
    
    prediction = model.predict(X_test_sample)[0]
    
    print(f"\n{i}. {test['Laptop_Marka']}: RAM {test['RAM']}GB, SSD {test['Depolama']}GB")
    print(f"   CPU: {test['CPU_Marka']} Tier-{test['CPU_Seviye']} Gen-{test['CPU_Nesil']}, GPU: {test['GPU_Tipi']}")
    print(f"   💰 Tahmini Fiyat: ₺{prediction:.2f}")

print("\n" + "="*70)
print("✅ EĞİTİM TAMAMLANDI!")
print("="*70)
print("\n💡 Şimdi ML servisini başlatabilirsiniz:")
print("   cd ML-Service")
print("   .\\start-ml-service.bat")
print()
