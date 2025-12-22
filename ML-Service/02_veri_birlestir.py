import pandas as pd
from pathlib import Path

ISLENMIS_KLASOR = Path("veriler/islenmis")
CIKTI_KLASOR = Path("veriler/birlesik")

CIKTI_KLASOR.mkdir(parents=True, exist_ok=True)

csv_dosyalari = list(ISLENMIS_KLASOR.glob("turkce_*.csv"))

print(f"📂 Bulunan işlenmiş CSV sayısı: {len(csv_dosyalari)}")

df_listesi = []

for csv in csv_dosyalari:
    df = pd.read_csv(csv)
    print(f"➡️ Okundu: {csv.name} | Satır: {len(df)}")
    df_listesi.append(df)

if not df_listesi:
    raise ValueError("❌ Birleştirilecek veri bulunamadı!")

birlesik_df = pd.concat(df_listesi, ignore_index=True)

print(f"\n📊 Birleştirme sonrası toplam satır: {len(birlesik_df)}")

cikti_dosya = CIKTI_KLASOR / "laptops_birlesik.csv"
birlesik_df.to_csv(cikti_dosya, index=False, encoding="utf-8-sig")

print(f"✅ Birleşik dataset kaydedildi: {cikti_dosya}")
