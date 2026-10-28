# DMTPRO Üretim Kabul Kapısı

Üretim yayını için kanıtlar, ilgili sürümün SHA-256 kimliğiyle eşleştirilmelidir.

1. Kaynak sürüm kimliği ve derlenmiş EXE hash'i; önceki motor işlevlerinde kayıp olmaması.
2. Windows x64 .NET 8 solution build + otomatik C# regresyon testi (0 hata).
3. Gerçek AutoCAD üzerinde izole test çizimi: TEXT/MTEXT/blok attribute seçimi, poz/çap/adet/boy, metraj tablosu, hakedis numaraları ve çizim temizliği.
4. Temiz Windows x64 test makinesinde Setup.exe kurulum, kısayol, açılış, kapanış, kaldırma ve kullanıcı veri koruma kontrolleri.
5. Gerçek GitHub Release üzerinden indirme, yayıncı sertifikası RSA imza denetimi, SHA-256, yanlış paketin reddi ve bilinçli başarısız güncellemede eski EXE'ye dönüş.
6. Yeni ve yalnız yayıncı bilgisayarında üretilmiş PFX; özel anahtar depoya/görüşmeye/kullanıcılara koyulmaz.
7. Test raporlarının sürüm + EXE SHA-256 + tarih ve ortam kimliğiyle birlikte arşivlenmesi.
8. Herhangi bir zorunlu kontrol atlandıysa sonuç **NO-GO**: Release yayımlanmaz.

Durum (28.10.2026): Kaynak bire bir eşdeğerliği, etkileşimli AutoCAD kabulü, temiz Windows Setup ve gerçek online rollback henüz kanıtlanmadı. Kod adayı dağıtım için **onaylı değildir**.
