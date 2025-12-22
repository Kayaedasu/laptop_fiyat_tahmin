import pandas as pd
import re

print("\n🔢 ADIM 3: Sayısal Dönüşüm (ML Hazırlık) Başlıyor...")

df = pd.read_csv("veriler/birlesik/laptops_veri_temizleme.csv")

# ---------------------------------------------------------
# DÖNÜŞÜM FONKSİYONLARI
# ---------------------------------------------------------

def ram_to_int(val):
    if pd.isna(val): return None
    m = re.search(r'(\d+)', str(val))
    return int(m.group(1)) if m else None

def storage_to_int(val):
    if pd.isna(val): return None
    val_str = str(val)
    
    # TB kontrolü (TB -> GB çevrimi)
    m = re.search(r'(\d+)\s*TB', val_str, re.I)
    if m: return int(m.group(1)) * 1024
    
    # GB kontrolü
    m = re.search(r'(\d+)\s*GB', val_str, re.I)
    if m: return int(m.group(1))
    
    return None

# Uygulama
df['RAM_GB'] = df['RAM_Ham'].apply(ram_to_int)
df['SSD_GB'] = df['Depolama_Ham'].apply(storage_to_int)

# Gereksiz ham kolonları istersen atabilirsin, şimdilik tutuyoruz.
print("💾 RAM Dağılımı (İlk 5):")
print(df['RAM_GB'].value_counts().head())

print("\n🗄️ Depolama Dağılımı (İlk 5):")
print(df['SSD_GB'].value_counts().head())

df.to_csv("veriler/birlesik/laptops_sayisal_donusum.csv", index=False, sep=';')
print(f"\n✅ Sayısal dönüşümler tamamlandı ve kaydedildi: veriler/birlesik/laptops_sayisal_donusum.csv")