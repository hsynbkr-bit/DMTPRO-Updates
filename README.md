# DMTPRO — Resmi Güncelleme Kanalı

DMTPRO donatı metraj Windows uygulamasının güncelleme dağıtım deposu.

## Yayın ve indirme
- Uygulama başlangıçta GitHub Releases son kararlı sürümü kontrol eder.
- İndirme yalnız `hsynbkr-bit/DMTPRO-Updates` deposundan, HTTPS üzerinden yapılır.
- Yayın ZIP dosyası tam `DMTPRO_app_win_x64.zip` adıyla yüklenir.
- Güncelleyici GitHub Releases API'sinde yayınlanan SHA-256 digest değerini indirilen ZIP ile karşılaştırır; ZIP içindeki `version.json` sürüm ve EXE SHA-256 değerini doğrular.
- **RSA .sig dosyası mevcut güncelleyici için zorunlu değildir.** İndirilen yeni sürüm ancak kullanıcı onayıyla kurulur; sorunlu ilk açılışta eski EXE yedeği geri alınır.
- Uygulama motoru ve dağıtım sürümü ayrı tutulur. Korunan motor 1.0.4, son kararlı yayın v1.0.6 (28.10.2026).
- İleride yeni motor EXE üretilmeden yeni motor özellikleri yayımlanmış sayılmaz.
- Yeni Release yalnız test ve Windows/AutoCAD doğrulama kanıtları tamamlanınca oluşturulmalıdır.

## Mevcut müşteriler
28.10.2026 tarihinden önceki Setup ile kuranlarda güncelleyici devre dışı olabilir. Bu kurulumlara **GUNCELLEYICI AKTIF ONARIM** bir kez uygulanmalıdır. GitHub'a yeni Release yüklemek eski kopyalara doğrudan onarım göndermez.

## Gizlilik
Müşteri proje ve DWG dosyaları, yayıncı özel anahtarları veya parolalar depoya yüklenmez.

İletişim: huseyinozdenbakir@gmail.com
