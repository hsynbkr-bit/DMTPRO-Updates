# DMTPRO Üretim Yayın Kabul Kapısı

Yeni GitHub Release oluşturulmadan önce:
1. Motor EXE SHA-256 kimliğini ve önceki sürümle farkını kaydet.
2. C# regresyonları çalıştır, gerçek Windows x64 açılış ve temiz Setup kurulum/kaldırma testlerini tamamla.
3. İzole AutoCAD test çiziminde TEXT/MTEXT/blok attribute, metraj ve çizim işaretlemelerini doğrula.
4. `DMTPRO_app_win_x64.zip` içinde yalnız `DMTPRO.exe`, `version.json` ve izinli ikonlar bulunmalı; `version.json.releaseVersion`, GitHub tag ile eşleşmeli; `exeSha256` gerçek EXE hash'i olmalı.
5. GitHub Release API'sinin ZIP için `sha256:<64 hex>` digest sağladığını doğrula.
6. Kurulu sürümden daha yeni tag, kullanıcı onayı, indirme, hash doğrulaması, ilk açılış ve rollback senaryolarını test et.
7. Test raporları yeni EXE ve yayın ZIP SHA-256 değerlerine bağlanmalı.
8. Kritik test veya derleme doğrulanmadıysa NO-GO. RSA .sig mevcut kanalda zorunlu değildir.

28 Ekim 2026: 1.0.4 motoru korunmuş; yeni motorun Windows/AutoCAD üretim kabulü yok. Kararlı GitHub Release v1.0.6; bu yayın mevcut istemciler için güncel kabul edilir.
