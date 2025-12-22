import pandas as pd
from pathlib import Path

HAM_KLASOR = Path("veriler/ham")
CIKTI_KLASOR = Path("veriler/islenmis")

CIKTI_KLASOR.mkdir(parents=True, exist_ok=True)

# Kolon eşleme
KOLON_ESLEME = {
    "title": "urun_adi",
    "price": "fiyat",
    "brand": "marka",
    "cpu": "islemci",
    "ram_gb": "ram_gb",
    "storage_gb": "ssd_gb",
    "gpu": "ekran_karti",
    "gpu_tier": "ekran_karti_seviyesi",
    "gpu_tier_filled": "ekran_karti_seviyesi"
}

def temizle_ve_turkcelestir(csv_yolu):
    df = pd.read_csv(csv_yolu)

    # Kolonları Türkçeleştir
    df = df.rename(columns=KOLON_ESLEME)

    gerekli_kolonlar = [
        "urun_adi",
        "fiyat",
        "marka",
        "islemci",
        "ram_gb",
        "ssd_gb",
        "ekran_karti_seviyesi"
    ]

    mevcut = [k for k in gerekli_kolonlar if k in df.columns]
    df = df[mevcut]

    # Temel temizlik
    if "fiyat" in df.columns:
        df = df[df["fiyat"].notna()]
        df = df[df["fiyat"] > 0]

    return df

tum_csvler = list(HAM_KLASOR.glob("*.csv"))

print(f"📂 Bulunan ham CSV sayısı: {len(tum_csvler)}")

for csv in tum_csvler:
    print(f"➡️ İşleniyor: {csv.name}")
    temiz_df = temizle_ve_turkcelestir(csv)

    cikti_dosya = CIKTI_KLASOR / f"turkce_{csv.name}"
    temiz_df.to_csv(cikti_dosya, index=False, encoding="utf-8-sig")

    print(f"✅ Kaydedildi: {cikti_dosya.name} | Satır: {len(temiz_df)}")

print("\n🎉 ADIM 1 TAMAMLANDI: TÜRKÇELEŞTİRME BİTTİ")
