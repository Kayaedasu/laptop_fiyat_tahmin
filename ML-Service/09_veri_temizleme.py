import pandas as pd
print("\n🧹 ADIM 2: Veri Temizleme Başlıyor...")

df = pd.read_csv("veriler/birlesik/laptops_ozellik_cikarilmis.csv")
baslangic_sayisi = len(df)

print(f"📊 Başlangıç satır sayısı: {baslangic_sayisi}")

# 1. Kritik alanları '-' olanları temizle
# (RAM, İşlemci veya Depolama bilgisi çekilemediyse o veri çöp olabilir)
kritik_kolonlar = ['RAM_Ham', 'Islemci', 'Depolama_Ham']

for col in kritik_kolonlar:
    df = df[df[col] != '-']

# 2. Fiyat temizliği
df = df.dropna(subset=['Fiyat'])
df = df[df['Fiyat'] > 0] # 0 TL olanları at

# 3. İndeks sıfırlama
df = df.reset_index(drop=True)

bitis_sayisi = len(df)
silinen = baslangic_sayisi - bitis_sayisi

print(f"📉 Temizlik Sonrası: {bitis_sayisi} satır (Silinen: {silinen})")

df.to_csv(
    "veriler/birlesik/laptops_veri_temizleme.csv",
    index=False,
    encoding="utf-8-sig"
)