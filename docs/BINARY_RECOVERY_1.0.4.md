# DMTPRO 1.0.4 ikili derleme (binary) denetimi — 2026-10-28

Korumaya alınan **DMTPRO.exe** için SHA-256:

`E578BB3D5D6E7D1C69E291AC1DECA68DB1C2E3D587C6164C66231AE6548563E8`

.NET 8 tek-dosya paketinin içindeki yönetilen modüller, paket manifestindeki ofset ve uzunluklar doğrulanarak çıkarıldı. **Bunlar kaynak kod değil, derlenmiş modüllerdir**:

| Managed assembly | Bytes | SHA-256 |
| --- | ---: | --- |
| `DMTPRO.dll` | 106496 | `045F15F12D108D7E89B6EEB8DF7A501317C46509DE615127535253BF9DFCECA8` |
| `DmtPro.Core.dll` | 94720 | `D054D6499F3BC5292E873C523C9386A8D1578A2D956714F06311E2A6DED58576` |
| `DmtPro.Cad.dll` | 53760 | `845FA6EDA21823C9D0D0017B234971EB2E94EDD259EAC3717CBF55F2887BEFE3` |

Özel uygulama dosyaları, C# kaynakları, kullanıcı proje verileri ve imza özel anahtarları bu **herkese açık depoya** yüklenmemiştir.

Kritik açık iş: gerçek 1.0.4 kaynak eşliği, Windows/.NET derleme, izole AutoCAD etkileşimli test, tek-EXE Setup kurulumu, V2 yayıncı anahtarı ve uçtan uca online rollback kanıtları. **Üretim kabulü NO-GO.**

Önceden sohbetten paylaşılmış V1 yayıncı imza anahtarı güvenli değildir. V2 anahtarı yalnız sahibinin yerel cihazında üretilmeden yeni imzalı sürüm yayımlanmamalıdır.
