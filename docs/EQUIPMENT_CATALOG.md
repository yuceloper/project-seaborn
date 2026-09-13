# Project Seaborn — Ekipman Kataloğu

Ekipmanların prototip veri kaynağı `Assets/_Project/Resources/Definitions/equipment.json` dosyasıdır. Sahne ve prefab değerleri yeni içerik eklemek için kopyalanmamalıdır.

## İlk katalog

| Kategori | Kimlik | Rol |
|---|---|---|
| Top | `iron_6lb` | Dengeli başlangıç topu, 100 temel hasar |
| Top | `iron_12lb` | Ağır, yavaş ve 200 temel hasarlı top |
| Mühimmat | `standard` | Gövde hasarı |
| Mühimmat | `chain` | Kısa menzil, yelken baskısı |
| Mühimmat | `grapeshot` | Çoklu saçma, mürettebat baskısı |
| Zıpkın | `light_2kg` | 50 hasar, hızlı dolum ve uzun menzil |
| Zıpkın | `heavy_4kg` | 100 hasar, yavaş dolum ve kısa menzil |
| Yelken | `patched_canvas` | Başlangıç profili |
| Yelken | `rat_sails` | Hız ve manevra yükseltmesi |
| Tüketilebilir | `tortuga_tonic` | Anlık 250 can |
| Tüketilebilir | `gun_crews_grog` | Süreli doldurma hızı |
| Özel | `light_of_tortuga` | Yedi saniyelik görünmezlik taslağı |

## Geçici kontrol

- Sol Shift: zıpkın nişanı
- Sol Shift + 1: 2 kg hafif zıpkın
- Sol Shift + 2: 4 kg ağır zıpkın
- Sol tık: seçili zıpkını fırlat

Bu kontrol üretim hotbar'ı değildir. Loadout ve UI tamamlandığında ortak aksiyon sistemine taşınacaktır.

## Veri sahipliği

- Katalog tanımları değişmez tasarım verisidir.
- Oyuncunun sahip olduğu adetler, kurulu ekipman ve dayanıklılık ayrı kalıcı durumdur.
- Sunucu sürümünde satın alma, kurma ve tüketim işlemleri sunucu otoriteli olur.
- Gold fiyatı sıfır olan oynanış eşyalarına sonradan güç satan Gold fiyatı eklenmez.

## Geçiş durumu

- Top mühimmatı katsayıları katalogdan okunuyor.
- Hafif/ağır zıpkın uçuş ve hasar değerleri katalogdan okunuyor.
- Topların temel hasarı gerçek loadout/salvo aşamasında bağlanacak.
- Yelkenler gemi hareket profiline loadout aşamasında bağlanacak.
- İksir ve özel eşyalar cooldown/etki sistemi tamamlandığında etkinleşecek.
