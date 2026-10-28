# DMTPRO — Resmi Güncelleme Kanalı

DMTPRO donatı metraj Windows uygulamasının herkese açık **güncelleme dağıtım deposudur**.

## İndirme ve sürüm politikası
- Kullanıcı paketleri **GitHub Releases** üzerinden dağıtılır; kaynak dosyalar ve taslak paketler bir kurulum sürümü değildir.
- Yayın paketinde `DMTPRO_app_win_x64.zip` ve `DMTPRO_app_win_x64.zip.sig` bulunmalıdır.
- Online güncelleyici SHA-256 ve uygulamaya gömülmüş **yayıncı açık anahtarıyla RSA imzasını** doğrulamadan yeni EXE kurmaz.
- Uygulama motoru ve dağıtım/güncelleyici sürümleri farklı olabilir. Mevcut korunan motor sürümü **1.0.4**'tür.
- Bir güncelleme ancak C# regresyonları, gerçek Windows kurulum/kaldırma, online geri alma ve izole AutoCAD etkileşim testleri geçtikten sonra üretime alınır.
- Özel PFX anahtarı, parolalar, müşteri projeleri ve DWG dosyaları **asla** bu depoya yüklenmez.

## Güncel durum (28 Ekim 2026)
**Yeni motor ve kurulum için üretim onayı verilmedi.** 1.0.4 EXE korunmaktadır. Yayıncı özel anahtarı daha önce sohbet dosyalarında paylaşıldığından üretim için **anahtar yenileme zorunludur**. Eski anahtarla imzalanmış aday paketler dağıtılmamalıdır.

## Güvenlik
Yeni sürüm gelmeden önce .dmtpro proje dosyalarınızı yedekleyin. Test raporu olmayan paketleri mevcut kurulu EXE'nin yerine koymayın.

İletişim: huseyinozdenbakir@gmail.com
