import pandas as pd

df = pd.read_csv("veriler/birlesik/laptops_feature_cikarilmis.csv")

print("📊 Başlangıç satır:", len(df))

# -----------------------
# 1️⃣ RAM doldurma
# -----------------------
# Strateji:
# - CPU biliniyorsa CPU grubunun medyan RAM’i
# - CPU da unknown ise genel medyan

ram_median_by_cpu = df.groupby("islemci")["ram_gb"].median()
global_ram_median = df["ram_gb"].median()

def fill_ram(row):
    if pd.notna(row["ram_gb"]):
        return row["ram_gb"]
    cpu = row["islemci"]
    if cpu in ram_median_by_cpu and not pd.isna(ram_median_by_cpu[cpu]):
        return ram_median_by_cpu[cpu]
    return global_ram_median

df["ram_gb"] = df.apply(fill_ram, axis=1)

# -----------------------
# 2️⃣ SSD doldurma
# -----------------------
# Strateji:
# - RAM >= 16 → SSD medyan (yüksek)
# - RAM < 16 → SSD medyan (düşük)

ssd_high = df[df["ram_gb"] >= 16]["ssd_gb"].median()
ssd_low = df[df["ram_gb"] < 16]["ssd_gb"].median()

def fill_ssd(row):
    if pd.notna(row["ssd_gb"]):
        return row["ssd_gb"]
    if row["ram_gb"] >= 16:
        return ssd_high
    return ssd_low

df["ssd_gb"] = df.apply(fill_ssd, axis=1)

# -----------------------
# 3️⃣ CPU unknown doldurma
# -----------------------
# Basit ama savunulabilir:
# - RAM >= 16 → i5
# - RAM < 16 → i3

def fill_cpu(row):
    if row["islemci"] != "unknown":
        return row["islemci"]
    if row["ram_gb"] >= 16:
        return "i5"
    return "i3"

df["islemci"] = df.apply(fill_cpu, axis=1)

# -----------------------
# 4️⃣ GPU seviyesi düzeltme
# -----------------------
# Çok nadir high/mid var, integrated normal
# RAM >= 32 ve SSD >= 512 ise mid yapabiliriz

def fill_gpu(row):
    if row["ekran_karti_seviyesi"] != "integrated":
        return row["ekran_karti_seviyesi"]
    if row["ram_gb"] >= 32 and row["ssd_gb"] >= 512:
        return "mid"
    return "integrated"

df["ekran_karti_seviyesi"] = df.apply(fill_gpu, axis=1)

print("\n💾 RAM (son):")
print(df["ram_gb"].value_counts().head())

print("\n🗄️ SSD (son):")
print(df["ssd_gb"].value_counts().head())

print("\n🧠 CPU (son):")
print(df["islemci"].value_counts())

print("\n🎮 GPU (son):")
print(df["ekran_karti_seviyesi"].value_counts())

df.to_csv(
    "veriler/birlesik/laptops_feature_doldurulmus.csv",
    index=False,
    encoding="utf-8-sig"
)

print("\n✅ EKSİK VERİLER DOLDURULDU")
