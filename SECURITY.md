# DMTPRO Güncelleme Güvenliği

Güncelleyici GitHub Releases API'sini HTTPS üzerinden kullanır, yalnız `hsynbkr-bit/DMTPRO-Updates` adresini kabul eder, yayın ZIP'inin API SHA-256 digest değerini ve ZIP içindeki EXE SHA-256 değerini denetler. Yeni paket kullanıcı onayıyla kurulur. İlk açılış başarısızlığında yedek EXE geri yüklenir.

**Not:** Bu dağıtım kanalında bağımsız RSA imzası zorunlu değildir. Bu seçim, GitHub hesabının ve Release metadatasının korunmasına daha fazla bağımlılık yaratır. GitHub hesabında iki faktörlü doğrulama kullanın. Özel anahtar veya parolaları, müşteri projelerini ve DWG dosyalarını genel depoya koymayın.

Güvenlik bildirimi: huseyinozdenbakir@gmail.com
