# Project Seaborn

Project Seaborn, izometrik kamera ile oynanan modern bir çevrim içi deniz savaşı ve sefer oyunudur.

Oyuncu tek başına bir gemiyi yönetir; gemisini konumlandırır, doğru bordayı açar, toplarını hazırlar ve hareketli hedeflerin rotasını tahmin ederek ateş eder. Uzun vadeli yapı sosyal liman merkezleri ile 15–25 dakikalık tehlikeli sefer haritalarını birleştirir.

## Tasarım sütunları

1. **Okunabilir taktik aksiyon** — ekipman önemlidir ancak konum, borda açısı, nişan ve zamanlama sonucu değiştirebilir.
2. **Değerli seferler** — oyuncu limandan hazırlık yaparak çıkar; yük ve mühimmat riske girer.
3. **Anlamlı batış** — batmak yalnızca yeniden doğmak değildir; hasarlı gemi, yedek filo ve kurtarma seferleri üretir.
4. **Yaşayan deniz dünyası** — gerçekçi temel, kontrollü fantastik bölgeler, suç/ödül sistemi ve bölgesel tehditler.
5. **Tek gemi, net kimlik** — ilk sürümde kaptanı yürütmek yerine oyuncunun kimliği gemisidir.

## Teknoloji

- Unity 6000.6.0f1
- Universal Render Pipeline
- Input System
- Cinemachine (gelecek kamera ihtiyaçları için)
- İlk prototip hedefi: Windows
- Uzun vadeli hedef: cross-platform istemciler ve sunucu otoriteli çevrim içi mimari

## Belgeler

- [Oyun vizyonu](docs/GAME_VISION.md)
- [Yol haritası ve yapılacaklar](docs/ROADMAP.md)
- [Karar günlüğü](docs/DECISIONS.md)
- [Sanat yönü](docs/ART_DIRECTION.md)
- [UI görsel standardı](docs/UI_STYLE_GUIDE.md)
- [Dümen ve modüler gemi tasarımı](docs/SHIP_CONTROL_AND_MODULAR_CUSTOMIZATION.md)

## Mevcut oynanabilir döngü

Gemi hareketi → manuel borda nişanı → hazırlık ve saçılma → salvo → hasar → batış.

Prototip kodu ve görseller geçicidir. Amaç önce doğru hissi ve sistem sınırlarını kanıtlamak, ardından üretim varlıklarına geçmektir.


## Prototip seyir kontrolleri

- `W`: seyir emrini bir kademe yükseltir — Ağır Yol, Yarım Yol, Tam Yol
- `S`: seyir emrini bir kademe düşürür — Dur, ardından Tornistan
- `A / D`: dümeni kademeli olarak iskele/sancağa çevirir
- Tuş bırakıldığında dümen merkeze döner; gemi hedef hıza ivmeyle ulaşır.
