import pandas as pd
from pathlib import Path
import re

GIRIS = Path("veriler/birlesik/laptops_birlesik.csv")
CIKTI_KLASOR = Path("veriler/birlesik")
CIKTI_KLASOR.mkdir(parents=True, exist_ok=True)

df = pd.read_csv(GIRIS)

print("📊 Başlangıç satır:", len(df))

# 1) Temel kolon kontrol (yoksa hata vermesin)
for col in ["urun_adi", "fiyat"]:
    if col not in df.columns:
        raise ValueError(f"❌ Gerekli kolon yok: {col}")

# 2) Ürün adı normalize (yumuşak duplicate için)
def normalize_name(s):
    s = str(s).lower().strip()
    s = re.sub(r"\s+", " ", s)            # fazla boşluk
    s = re.sub(r"[^\w\s\-\.]", "", s)     # noktalama temizle (hafif)
    # bazı gereksiz kelimeleri kırp (istersen genişletiriz)
    for junk in ["türkiye garantili", "free dos", "freedos", "windows 11", "windows 10"]:
        s = s.replace(junk, "")
    s = re.sub(r"\s+", " ", s).strip()
    return s

df["urun_adi_norm"] = df["urun_adi"].apply(normalize_name)

# 3) Fiyatı sayıya çevir (olası stringleri temizle)
df["fiyat"] = pd.to_numeric(df["fiyat"], errors="coerce")
df = df[df["fiyat"].notna()]
df = df[df["fiyat"] > 0]

# 4) Kesin duplicate: aynı normalized isim + aynı fiyat
before = len(df)
df = df.drop_duplicates(subset=["urun_adi_norm", "fiyat"], keep="first")
print("✅ Kesin duplicate sonrası:", len(df), " (silinen:", before - len(df), ")")

# 5) Yumuşak duplicate: aynı normalized isim
# Burada aynı üründen farklı fiyatlar kalabilir. Ne yapacağız?
# En mantıklısı: aynı üründe EN DÜŞÜK fiyatı tut (piyasadaki en ucuz gibi)
before2 = len(df)
df = df.sort_values("fiyat", ascending=True).drop_duplicates(subset=["urun_adi_norm"], keep="first")
print("✅ Yumuşak duplicate sonrası:", len(df), " (silinen:", before2 - len(df), ")")

# 6) Temizlik kolonunu kaldır
df = df.drop(columns=["urun_adi_norm"])

cikti = CIKTI_KLASOR / "laptops_birlesik_dedup.csv"
df.to_csv(cikti, index=False, encoding="utf-8-sig")

print("💾 Kaydedildi:", cikti)
