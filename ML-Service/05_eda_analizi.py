import pandas as pd

df = pd.read_csv("veriler/birlesik/laptops_birlesik_temiz.csv")

print("\n📊 GENEL BİLGİ")
print(df.info())

print("\n📈 FİYAT TANIMSAL İSTATİSTİK")
print(df["fiyat"].describe())

# -----------------------
# RAM – Fiyat
# -----------------------
print("\n💾 RAM - Ortalama Fiyat")
ram_fiyat = (
    df.groupby("ram_gb")["fiyat"]
    .mean()
    .sort_index()
)
print(ram_fiyat)

# -----------------------
# SSD – Fiyat
# -----------------------
print("\n🗄️ SSD - Ortalama Fiyat (ilk 15)")
ssd_fiyat = (
    df.groupby("ssd_gb")["fiyat"]
    .mean()
    .sort_index()
)
print(ssd_fiyat.head(15))

# -----------------------
# Marka – Fiyat
# -----------------------
print("\n🏷️ Marka - Ortalama Fiyat (ilk 10)")
marka_fiyat = (
    df.groupby("marka")["fiyat"]
    .mean()
    .sort_values(ascending=False)
    .head(10)
)
print(marka_fiyat)

# -----------------------
# Ekran Kartı Seviyesi – Fiyat
# -----------------------
if "ekran_karti_seviyesi" in df.columns:
    print("\n🎮 Ekran Kartı Seviyesi - Ortalama Fiyat")
    gpu_fiyat = df.groupby("ekran_karti_seviyesi")["fiyat"].mean()
    print(gpu_fiyat)

print("\n✅ GRAFİKSİZ EDA TAMAMLANDI")
