@echo off
chcp 65001 >nul
cls

echo ╔════════════════════════════════════════════════════════════════════╗
echo ║         LAPTOP FİYAT TAHMİN MODELİ - EĞİTİM                       ║
echo ╚════════════════════════════════════════════════════════════════════╝
echo.

echo 📦 Gerekli paketler kontrol ediliyor...
pip install -q scikit-learn pandas numpy joblib

echo.
echo 🎯 Model eğitimi başlatılıyor...
echo.

python train_model.py

echo.
echo ✅ İşlem tamamlandı!
echo.
pause
