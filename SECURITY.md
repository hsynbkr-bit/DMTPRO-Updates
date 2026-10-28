# DMTPRO Güvenlik

Güncelleme sistemi yalnız güvenilir GitHub deposundan HTTPS ile sürüm metadatası okumalı; indirilen arşivin SHA-256'sını ve gömülü yayıncı açık anahtarına karşı RSA imzasını doğrulamalıdır.

Yayıncı özel anahtarı veya parolası sohbetlere, genel depolara, üçüncü kişilere ya da dağıtım ZIP'lerine yüklenmemelidir. Daha önce paylaşılan V1 PFX **üretim için güvenli kabul edilmez**. Yeni imzalama anahtarı yalnız yayıncıya ait yerel Windows bilgisayarında oluşturulmalı ve dağıtım istemcileri yeni sabitlenmiş açık sertifikayla yeniden kurulmalıdır.

Güvenlik bildirimi: huseyinozdenbakir@gmail.com (açıklarda kullanıcıya ait DWG veya proje içeriği paylaşmayın).
