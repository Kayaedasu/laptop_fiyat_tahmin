import pandas as pd
import re

df = pd.read_csv("veriler/birlesik/laptops_birlesik_temiz.csv")

print("📊 Başlangıç satır:", len(df))

# -----------------------
# RAM çıkarımı
# -----------------------
def extract_ram(text):
    m = re.search(r'(\d+)\s?gb\s?ram', text.lower())
    if m:
        return int(m.group(1))
    return None

# -----------------------
# SSD çıkarımı
# -----------------------
def extract_ssd(text):
    m = re.search(r'(\d+)\s?gb\s?(ssd|nvme)', text.lower())
    if m:
        return int(m.group(1))
    return None

# -----------------------
# CPU çıkarımı (basit)
# -----------------------
def extract_cpu(text):
    text = text.lower()
    if "i3" in text:
        return "i3"
    if "i5" in text:
        return "i5"
    if "i7" in text:
        return "i7"
    if "ryzen 3" in text:
        return "ryzen 3"
    if "ryzen 5" in text:
        return "ryzen 5"
    if "ryzen 7" in text:
        return "ryzen 7"
    return "unknown"

# -----------------------
# GPU seviyesi çıkarımı
# -----------------------
def extract_gpu_tier(text):
    text = text.lower()

    if "rtx 40" in text or "rtx 4070" in text or "rtx 4060" in text:
        return "high"
    if "rtx" in text or "gtx" in text:
        return "mid"
    if "iris" in text or "uhd" in text or "radeon" in text:
        return "integrated"
    return "integrated"

df["ram_gb"] = df["urun_adi"].apply(extract_ram)
df["ssd_gb"] = df["urun_adi"].apply(extract_ssd)
df["islemci"] = df["urun_adi"].apply(extract_cpu)
df["ekran_karti_seviyesi"] = df["urun_adi"].apply(extract_gpu_tier)

print("\n💾 RAM dağılımı:")
print(df["ram_gb"].value_counts(dropna=False).head(10))

print("\n🗄️ SSD dağılımı:")
print(df["ssd_gb"].value_counts(dropna=False).head(10))

print("\n🧠 CPU dağılımı:")
print(df["islemci"].value_counts())

print("\n🎮 GPU seviyesi dağılımı:")
print(df["ekran_karti_seviyesi"].value_counts())

df.to_csv(
    "veriler/birlesik/laptops_feature_cikarilmis.csv",
    index=False,
    encoding="utf-8-sig"
)

print("\n✅ FEATURE ÇIKARMA TAMAMLANDI")
