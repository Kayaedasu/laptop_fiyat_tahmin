from flask import Flask, request, jsonify
import joblib
import numpy as np
import pandas as pd
import os

app = Flask(__name__)

# -----------------------
# Modeli ve Encoderları Yükle
# -----------------------
try:
    # Model ve encoders yolu (ML-Service/model/ klasöründen yükle)
    model_path = os.path.join(os.path.dirname(__file__), "..", "model", "laptop_fiyat_model.pkl")
    encoders_path = os.path.join(os.path.dirname(__file__), "..", "model", "label_encoders.pkl")
    
    # Modeli yükle
    model = joblib.load(model_path)
    label_encoders = joblib.load(encoders_path)

    print("✅ Model yüklendi: laptop_fiyat_model.pkl")
    print(f"✅ Encoders yüklendi: label_encoders.pkl")

    # Encoder keylerini kontrol et
    print(f"📊 Kullanılabilir encoders: {list(label_encoders.keys())}")
    
    # Modelin beklediği feature sayısını kontrol et
    try:
        n_features = model.n_features_in_
        print(f"📊 Model beklenen feature sayısı: {n_features}")
        if hasattr(model, 'feature_names_in_'):
            print(f"📊 Feature isimleri: {list(model.feature_names_in_)}")
    except:
        print("⚠️ Feature bilgisi modelde bulunamadı")
    
except FileNotFoundError as e:
    print(f"❌ HATA: Model dosyaları bulunamadı!")
    print(f"   Lütfen 'laptop_fiyat_model.pkl' ve 'label_encoders.pkl' dosyalarının")
    print(f"   ML-Service klasöründe olduğundan emin olun.")
    raise e
except Exception as e:
    print(f"❌ HATA: Model yüklenirken hata oluştu: {str(e)}")
    raise e

# -----------------------
# Helper Functions
# -----------------------
def get_cpu_tier(cpu_str):
    """CPU string'inden tier çıkar"""
    cpu_lower = str(cpu_str).lower()
    
    if 'i9' in cpu_lower or 'ryzen 9' in cpu_lower or 'ultra 9' in cpu_lower:
        return 9
    elif 'i7' in cpu_lower or 'ryzen 7' in cpu_lower or 'ultra 7' in cpu_lower:
        return 7
    elif 'i5' in cpu_lower or 'ryzen 5' in cpu_lower or 'ultra 5' in cpu_lower or 'm2' in cpu_lower or 'm3' in cpu_lower or 'm4' in cpu_lower:
        return 5
    elif 'i3' in cpu_lower or 'ryzen 3' in cpu_lower or 'm1' in cpu_lower:
        return 3
    elif 'celeron' in cpu_lower or 'pentium' in cpu_lower:
        return 1
    return 2  # Default

def get_cpu_generation(cpu_str):
    """CPU nesli çıkar (Intel için 10, 11, 12, 13, 14, 15 gibi)"""
    if not cpu_str:
        return 10  # Default
    
    cpu_str = str(cpu_str)
    
    # Intel nesilleri (i7-12700H, i5-1335U gibi)
    import re
    
    # Ultra serisi (155H, 258V gibi)
    if 'ultra' in cpu_str.lower():
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

def get_cpu_brand(cpu_str):
    """CPU markasını belirle"""
    cpu_lower = str(cpu_str).lower()
    
    if 'intel' in cpu_lower or 'core i' in cpu_lower or 'celeron' in cpu_lower or 'pentium' in cpu_lower or 'ultra' in cpu_lower:
        return 'Intel'
    elif 'amd' in cpu_lower or 'ryzen' in cpu_lower:
        return 'AMD'
    elif 'apple' in cpu_lower or 'm1' in cpu_lower or 'm2' in cpu_lower or 'm3' in cpu_lower or 'm4' in cpu_lower:
        return 'Apple'
    return 'Intel'  # Default

def get_gpu_type(gpu_str):
    """GPU tipini belirle"""
    if not gpu_str or gpu_str.lower() == 'integrated':
        return 'Entegre'
    
    gpu_lower = str(gpu_str).lower()
    
    if 'rtx 40' in gpu_lower or 'rtx40' in gpu_lower or 'rtx 50' in gpu_lower:
        return 'RTX_Yeni'
    elif 'rtx 30' in gpu_lower or 'rtx30' in gpu_lower:
        return 'RTX_30'
    elif 'rtx' in gpu_lower:
        return 'RTX'
    elif 'gtx' in gpu_lower:
        return 'GTX'
    elif 'nvidia' in gpu_lower or 'geforce' in gpu_lower or 'mx' in gpu_lower:
        return 'NVIDIA_Diger'
    elif 'radeon rx' in gpu_lower:
        return 'Radeon_RX'
    elif 'radeon' in gpu_lower or 'amd' in gpu_lower:
        return 'Radeon'
    elif 'apple' in gpu_lower:
        return 'Apple_GPU'
    elif 'iris' in gpu_lower or 'arc' in gpu_lower:
        return 'Intel_Iris'
    
    if gpu_lower == 'mid':
        return 'GTX'
    elif gpu_lower == 'high':
        return 'RTX_Yeni'
    
    return 'Entegre'

# -----------------------
# API Endpoints
# -----------------------
@app.route("/predict", methods=["POST"])
def predict():
    """Laptop fiyat tahmini endpoint"""
    try:
        data = request.get_json()
        
        # Parametreleri al
        ram = float(data.get("ram_gb", 16))
        depolama = float(data.get("ssd_gb", 512))
        
        # CPU bilgisi
        cpu_input = data.get("islemci", "i5")
        cpu_tier = get_cpu_tier(cpu_input)
        cpu_nesil = get_cpu_generation(cpu_input)
        cpu_marka = get_cpu_brand(cpu_input)
        
        # GPU bilgisi
        gpu_input = data.get("ekran_karti", "integrated")
        gpu_tipi = get_gpu_type(gpu_input)
        
        # Laptop markası
        laptop_marka = data.get("marka", "Diğer")
        
        # Encode işlemleri
        try:
            cpu_marka_enc = label_encoders['CPU_Marka'].transform([cpu_marka])[0]
        except:
            cpu_marka_enc = 0
        
        try:
            gpu_tipi_enc = label_encoders['GPU_Tipi'].transform([gpu_tipi])[0]
        except:
            gpu_tipi_enc = 0
        
        try:
            laptop_marka_enc = label_encoders['Laptop_Marka'].transform([laptop_marka])[0]
        except:
            laptop_marka_enc = 0
        
        # Feature array oluştur (pandas DataFrame ile - feature isimleri korunur)
        # Sıralama: RAM, Depolama, CPU_Seviye, CPU_Nesil, CPU_Marka_encoded, GPU_Tipi_encoded, Laptop_Marka_encoded
        X = pd.DataFrame([[
            ram,
            depolama,
            cpu_tier,
            cpu_nesil,
            cpu_marka_enc,
            gpu_tipi_enc,
            laptop_marka_enc
        ]], columns=['RAM', 'Depolama', 'CPU_Seviye', 'CPU_Nesil', 'CPU_Marka_encoded', 'GPU_Tipi_encoded', 'Laptop_Marka_encoded'])
        
        # Tahmin yap
        tahmin = model.predict(X)[0]
        
        # Response
        return jsonify({
            "tahmini_fiyat": round(float(tahmin), 2),
            "model_version": "v3_full_features",
            "input_features": {
                "ram_gb": ram,
                "ssd_gb": depolama,
                "cpu_tier": cpu_tier,
                "cpu_generation": cpu_nesil,
                "cpu_brand": cpu_marka,
                "gpu_type": gpu_tipi,
                "laptop_brand": laptop_marka
            }
        })
    
    except Exception as e:
        return jsonify({
            "error": str(e),
            "message": "Tahmin sırasında hata oluştu"
        }), 400

@app.route("/health", methods=["GET"])
def health():
    """API sağlık kontrolü"""
    return jsonify({
        "status": "healthy",
        "model": "laptop_fiyat_model.pkl",
        "features": [
            "RAM",
            "Depolama", 
            "CPU_Seviye",
            "CPU_Nesil",
            "CPU_Marka_encoded",
            "GPU_Tipi_encoded",
            "Laptop_Marka_encoded"
        ],
        "available_encoders": list(label_encoders.keys()) if label_encoders else []
    })

@app.route("/", methods=["GET"])
def index():
    """Ana sayfa - API bilgisi"""
    return jsonify({
        "service": "Laptop Fiyat Tahmin API",
        "version": "3.0",
        "model": "laptop_fiyat_model.pkl",
        "endpoints": {
            "POST /predict": "Fiyat tahmini yap",
            "GET /health": "Sistem durumu",
            "GET /": "Bu sayfa"
        },
        "example_request": {
            "ram_gb": 16,
            "ssd_gb": 512,
            "islemci": "Intel Core i7-12700H",
            "ekran_karti": "NVIDIA RTX 3060",
            "marka": "Asus"
        }
    })

# -----------------------
# Uygulama Başlat
# -----------------------
if __name__ == "__main__":
    print("\n" + "="*70)
    print("🚀 LAPTOP FİYAT TAHMİN API - GELİŞMİŞ MODEL")
    print("="*70)
    print(f"📦 Model: laptop_fiyat_model.pkl")
    print(f"🔧 Encoders: label_encoders.pkl")
    print(f"🌐 URL: http://127.0.0.1:5000")
    print(f"📝 Endpoints:")
    print(f"   POST /predict  - Fiyat tahmini")
    print(f"   GET  /health   - Sistem durumu")
    print(f"   GET  /         - API bilgisi")
    print("="*70)
    print("💡 Örnek request:")
    print('   {"ram_gb": 16, "ssd_gb": 512,')
    print('    "islemci": "Intel Core i7-12700H",')
    print('    "ekran_karti": "NVIDIA RTX 3060"}')
    print("="*70 + "\n")
    
    app.run(host="127.0.0.1", port=5000, debug=True)
