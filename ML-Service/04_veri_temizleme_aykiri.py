import pandas as pd
from pathlib import Path

GIRIS = Path("veriler/birlesik/laptops_birlesik_dedup.csv")
CIKTI = Path("veriler/birlesik/laptops_birlesik_temiz.csv")

df = pd.read_csv(GIRIS)

print("📊 Başlangıç satır:", len(df))

# -----------------------
# 1️⃣ Sayısal kolonlar
# -----------------------
for col in ["fiyat", "ram_gb", "ssd_gb"]:
    if col in df.columns:
        df[col] = pd.to_numeric(df[col], errors="coerce")

# -----------------------
# 2️⃣ Mantıksız değerleri at
# -----------------------
df = df[df["fiyat"] > 1000]        # aşırı ucuz (hatalı)
df = df[df["fiyat"] < 300000]      # aşırı pahalı (uç değer)

if "ram_gb" in df.columns:
    df = df[df["ram_gb"].between(2, 128)]

if "ssd_gb" in df.columns:
    df = df[df["ssd_gb"].between(64, 8192)]

print("🧹 Mantıksız değer temizliği sonrası:", len(df))

# -----------------------
# 3️⃣ IQR ile aykırı fiyat temizliği
# -----------------------
Q1 = df["fiyat"].quantile(0.25)
Q3 = df["fiyat"].quantile(0.75)
IQR = Q3 - Q1

alt_sinir = Q1 - 1.5 * IQR
ust_sinir = Q3 + 1.5 * IQR

before = len(df)
df = df[(df["fiyat"] >= alt_sinir) & (df["fiyat"] <= ust_sinir)]
print("📉 IQR aykırı temizliği sonrası:", len(df), "(silinen:", before - len(df), ")")

# -----------------------
# 4️⃣ Boş kritik alanları at
# -----------------------
kritik = ["urun_adi", "fiyat", "islemci"]
df = df.dropna(subset=[c for c in kritik if c in df.columns])

print("✅ Son satır sayısı:", len(df))

df.to_csv(CIKTI, index=False, encoding="utf-8-sig")
print("💾 Temiz veri kaydedildi:", CIKTI)
