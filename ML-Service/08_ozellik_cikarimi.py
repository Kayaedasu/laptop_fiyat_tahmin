import pandas as pd
import re

print("🚀 ADIM 1: Özellik Çıkarımı Başlıyor...")

try:
    df = pd.read_csv("veriler/birlesik/laptops_feature_doldurulmus.csv")
except FileNotFoundError:
    print("❌ HATA: Giriş dosyası bulunamadı!")
    exit()

print(f"📊 Ham veri sayısı: {len(df)}")

# ---------------------------------------------------------
# FONKSİYONLAR (Senin gelişmiş regex motorun)
# ---------------------------------------------------------

def marka_bul(text):
    text_lower = text.lower()
    brands = [
        ('HP', ['hp ', 'hp-', 'elitebook', 'probook', 'pavilion', 'omen', 'envy']),
        ('Dell', ['dell ']),
        ('Acer', ['acer ']),
        ('Lenovo', ['lenovo ', 'thinkpad', 'ideapad', 'thinkbook', 'legion']),
        ('Asus', ['asus ', 'vivobook', 'zenbook', 'rog ', 'tuf ']),
        ('Apple', ['macbook', 'apple ']),
        ('MSI', ['msi ']),
        ('Samsung', ['samsung ', 'galaxy book']),
        ('Toshiba', ['toshiba ', 'dynabook']),
        ('Huawei', ['huawei ', 'matebook']),
        ('Casper', ['casper ', 'nirvana', 'excalibur']),
        ('Monster', ['monster ']),
        ('Microsoft', ['surface']),
    ]
    for brand, patterns in brands:
        for pattern in patterns:
            if pattern in text_lower:
                return brand
    return 'Diğer'

def model_bul(text, brand):
    patterns = {
        'HP': [r'HP\s+(\d{2}[\-\w]*)', r'(EliteBook|ProBook|Pavilion|Omen|Envy|Spectre)[\s\-]?([A-Za-z0-9\-\s]*)'],
        'Dell': [r'Dell\s+(Latitude|Inspiron|XPS|Vostro|Precision|G\d+)[\s\-]?([A-Za-z0-9\-]*)'],
        'Lenovo': [r'(ThinkPad|IdeaPad|ThinkBook|Legion|Yoga)[\s\-]?([A-Za-z0-9\-\s]*)'],
        'Asus': [r'(Vivobook|Zenbook|ROG|TUF)[\s\-]?([A-Za-z0-9\-\s]*)'],
        'Apple': [r'MacBook\s+(Air|Pro)(?:\s+(M\d+))?'],
        'MSI': [r'MSI\s+([A-Za-z]+[\s\-]?[A-Za-z0-9\-]*)'],
    }
    if brand in patterns:
        for pattern in patterns[brand]:
            m = re.search(pattern, text, re.I)
            if m:
                return ' '.join([g for g in m.groups() if g]).strip()[:50]
    return '-'

def gpu_bul(text):
    patterns = [
        (r'(RTX\s*5\d{3}(?:\s*Ti)?)', lambda m: 'NVIDIA RTX' + m.group(1).replace('RTX','').replace(' ','')),
        (r'(RTX\s*4\d{3}(?:\s*Ti)?)', lambda m: 'NVIDIA RTX' + m.group(1).replace('RTX','').replace(' ','')),
        (r'(RTX\s*3\d{3}(?:\s*Ti)?)', lambda m: 'NVIDIA RTX' + m.group(1).replace('RTX','').replace(' ','')),
        (r'(GTX\s*\d{3,4}(?:\s*Ti)?)', lambda m: 'NVIDIA GTX' + m.group(1).replace('GTX','').replace(' ','')),
        (r'(Arc\s+\d{3})', lambda m: 'Intel ' + m.group(1)),
        (r'Iris\s+Xe', lambda m: 'Intel Iris Xe'),
    ]
    for pattern, formatter in patterns:
        m = re.search(pattern, text, re.I)
        if m: return formatter(m)
    
    if 'macbook' in text.lower() or 'apple' in text.lower():
        m = re.search(r'\b(M\d+)(?:\s+(Pro|Max|Ultra))?\b', text, re.I)
        if m: return f"Apple {m.group(1)} GPU"
        
    return 'Entegre' # Varsayılan değer

def ram_text_bul(text):
    # Sadece metni bulur ("16 GB" gibi), sayıya çevirme işlemi Adım 3'te
    m = re.search(r'(\d{1,3})\s*GB\s*(?:RAM|DDR|LPDDR)?(?!\s*(?:SSD|HDD))', text, re.I)
    if m and int(m.group(1)) in [4,8,12,16,24,32,48,64,96,128]:
        return f"{m.group(1)} GB"
    return '-'

def islemci_bul(text):
    patterns = [
        (r'(M\d+)(?:\s+(Pro|Max|Ultra))?', lambda m: f"Apple {m.group(1)}"),
        (r'Ultra\s*(\d+)', lambda m: f"Intel Core Ultra {m.group(1)}"),
        (r'Core\s*(5|7|9)\s*(\d{3})', lambda m: f"Intel Core {m.group(1)} {m.group(2)}"),
        (r'(i[3579])[\-\s]?(\d{4,5})', lambda m: f"Intel Core {m.group(1)}-{m.group(2)}"),
        (r'Ryzen\s*(\d+)', lambda m: f"AMD Ryzen {m.group(1)}"),
    ]
    for pattern, formatter in patterns:
        m = re.search(pattern, text, re.I)
        if m: return formatter(m)
    return '-'

def depolama_text_bul(text):
    m = re.search(r'(\d+)\s*TB', text, re.I)
    if m: return f"{m.group(1)} TB SSD"
    m = re.search(r'(\d{3,4})\s*GB', text, re.I)
    if m: return f"{m.group(1)} GB SSD"
    return '-'

# ---------------------------------------------------------
# UYGULAMA
# ---------------------------------------------------------

results = []
for _, row in df.iterrows():
    text = str(row['urun_adi'])
    brand = marka_bul(text)
    
    results.append({
        'Marka': brand,
        'Model': model_bul(text, brand),
        'Islemci': islemci_bul(text),
        'RAM_Ham': ram_text_bul(text),
        'Depolama_Ham': depolama_text_bul(text),
        'Ekran_Karti': gpu_bul(text),
        'Fiyat': row['fiyat']
    })

df_new = pd.DataFrame(results)
df_new.to_csv(
    "veriler/birlesik/laptops_ozellik_cikarilmis.csv",
    index=False,
    encoding="utf-8-sig"
)

