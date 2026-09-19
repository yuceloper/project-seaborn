# Savaş temeli düzeltmesi — oynanış videosu sonrası

## Teşhis

71.88 saniyelik kayıttan örneklenen kareler yakın mesafeli karşılaşmayı ve sefer
sonucunu gösteriyor: 35 gelir, 13.5 mühimmat, 24 onarım, -2.5 tahmini net.
Titremenin tüm kaynakları videodan ölçülmüş değildir; aşağıdaki hatalar kaynak
kodunda doğrulanmıştır.

- ShipCombatFeedback.Shake geminin fizik kökünü rastgele taşıyor, ardından eski
  konumuna geri koyuyordu; hareket ve çarpışma sistemleriyle çakışıyordu.
- BroadsideController toplam kurulu top adedini seçili bordadaki namlular üzerinde
  modulo ile döndürüyordu: üç fiziksel namludan altı/dokuz atış çıkabiliyordu.
- EnemyShipController her fizik adımında oyuncuya teğet rotayı yeniden hesaplıyor,
  bu nedenle sürekli yörünge izliyordu. Hızı da doğrudan aniden değiştiriyordu.
- Enkaz ödülü düşman arketipini dikkate almıyordu.

## Yeni davranış

### Fizik ve geri bildirim

İsabet geri bildirimi artık geminin Transform/Rigidbody konumuna yazmaz.
Gülle collider'ları kapalıdır; hasar mevcut SphereCast sorgusuyla çözülür.
Kozmetik gülle çarpışması gemi motorunun çarpışmadan kurtarma akışını tetiklemez.
Su/uzak düşman isabetleri kamerayı sarsmaz. Oyuncu salvosuna tek hafif kamera
tepkisi, oyuncu isabetlerine en fazla 0.2 saniyede bir hafif tepki uygulanır.
Gerçek gemi/gemi ve gemi/çevre çarpışmaları korunur.

### Gerçek borda sayısı

Kurulu top sayısı gemi toplamıdır. Çiftler iskele/sancağa dağıtılır; tek kalan top
iskeleye atanır. Salvo seçili taraftaki benzersiz, aktif, fiziksel namlu sayısıyla
sınırlıdır. Namlu tekrar kullanılmaz; yanlış taraftaki bağlantı ateşlemez.
Altı kurulu top ve 3+3 fiziksel namlu ile solda üç, sağda üç top ateşler.
Beş kurulu top 3+2 olur. Aynı salvo için mühimmat tüketimi aktif borda sayısıyla
sınırlanır; daha az stok varsa kısmi salvo yapılır. Dolumlar bağımsız kalır.

Mevcut üretim Sloop modeli 3+3 namlu sağlar. Modelin üzerinde daha çok fiziksel
hardpoint yoksa daha fazla kurulu top ek atış üretmez. Sahiplik silinmez; HUD ve
tersane gerçek aktif borda adetlerini gösterir. Diğer gemi sınıfları için yeterli
model/hardpoint üretimi sonraki çalışmadır. Gizli sanal toplara geri dönülmemeli.

### Düşman manevrası

Yaklaşma → sabit doğrultulu borda geçişi → mesafe açma → yeniden yaklaşma.
Geçiş doğrultusu başlangıçta seçilir, her kare oyuncunun çevresine çevrilmez.
Bir geçişte en fazla bir salvo; başarısız yaklaşma altı saniyede sonlanır.
Salvodan bir saniye sonra mesafe açılır, kaçış rotası dört saniye korunur.
Fazla yakınlık erken ayrılma üretir. Hız değişimi ivmeyle uygulanır; dönme
sırasında hız azalır. Ateş için gerçek seçili mühimmat menzili de kontrol edilir.
Bu ilk davranış turudur; tam engelden kaçınma/filo koordinasyonu içermez.

### Can ve ekonomi başlangıç değerleri

Gerçek üç toplu bordayla eski süre hedefini korumak için düşman taban canı 1250:

| Arketip | Can | Üç 6 lb top ile kusursuz salvo | Merkez Silver |
|---|---:|---:|---:|
| Avcı | 900 | 4 | 80 |
| Yağmacı | 1125 | 5 | 110 |
| Topçu | 1750 | 7 | 180 |

Bu tablo önceki 2500 taban can/altı topu tek bordaya sayan hedeflerin yerine geçer.
Iskalar, yükseltmeler ve 12 lb toplar gerçek süreyi değiştirir. Batı ödülü 1.5x,
Doğu ödülü 0.9x; gülle ve malzeme ödülleri korunur. Önceki onarım tarifesi
(0.025 Silver/HP, 0.1 Silver/alt sistem puanı) değişmedi.
Ödül yalnızca enkaz toplanıp yük güvenceye alındığında Silver'a dönüşür.
Bunlar final ekonomi değildir; 15–25 dakikalık sefer ölçümü hâlâ gereklidir.

## Kabul kapısı

1. Unity derlemesini doğrula. `Seaborn > Validation > Check Broadside Allocation`
   çalıştır: 6 top, 5 top, fiziksel kapasite, null/tekrar/yanlış yön, kapalı namlu
   ve döndürülmüş gemi kontrolleri geçmeli.
2. Altı top takılıyken HUD 3/3 göstermeli. Sol hedefe üç namlu ateşlemeli, stok
   üç azalmalı, yalnız iskele dolumu başlamalı. Sağ için tersini doğrula.
3. İsabet alırken sabit gemi yerinden sıçramamalı; hareketli gemi eski konumuna
   geri çekilmemeli. Gerçek gövde çarpışmasının çalıştığını ayrıca kontrol et.
4. Tek düşmanla durağan ve hareketli hedef olarak savaş. En az üç yaklaşmada
   borda geçişi, salvo fırsatı ve ayrılma gözlenmeli; kesintisiz orbit olmamalı.
5. Avcı, Yağmacı ve Topçu'yu ayrı dene. Salvo/hasar/yelken/mürettebat kaybını kaydet.
6. Merkezde enkaz ödüllerini ve limana dönüş raporunu doğrula. Aynı karşılaşmadan
   önceki kayıtla süre, harcanan gülle ve onarım giderini karşılaştır.
7. Bu kapı geçmeden main'e merge veya yeni özellik turu yok.

Burada kaynak yapısı ve muhasebe/hedef salvo aritmetiği kontrol edildi. Unity
Editor bulunmadığından C# derlemesi, menü kontrolleri ve oynanış çalıştırılmadı.
