# Project Seaborn — UI Görsel Standardı

## Amaç

Arayüz denizin üzerinde duran geliştirici panelleri gibi değil, kaptanın haritası, seyir defteri ve gemi donanımı gibi hissettirmelidir. Bilgi okunabilir kalır; dekor hiçbir zaman etkileşimin önüne geçmez.

## Görsel dil

Ana yüzeyler:

- Kömür siyahı ve çok koyu sıcak kahve
- Eskitilmiş ahşap çerçeve
- İnce pirinç kenar ve bağlantılar
- Kırık beyaz metin
- Deniz yeşili olumlu durum
- Kiremit kırmızısı tehlike

Lacivert yalnızca denizle bağlantılı ikincil yüzeylerde kullanılır. Altın rengi geniş panel dolgusu değildir; vurgu, seçili durum ve nadir ödül içindir.

## Liman ekranı

Liman arayüzü küçük popup koleksiyonu değildir.

- Ekranın büyük bölümünü kullanan tek çalışma alanı
- Sol veya üst kısımda hizmet sekmeleri
- Ortada ürün/gemi/kontratın görsel odağı
- Sağda istatistik, fiyat ve ana eylem
- Alt bölümde bağlama göre geri/ayrıl ve kısa yardım
- Arka planda liman görünür kalır ancak odak için karartılır

### Tersane

- Ortada aktif geminin büyük 3B önizlemesi
- Gemi listesindeki kartlarda silüet, ad, sınıf ve fiyat
- Mevcut ve aday gemi istatistikleri yan yana
- Top yuvaları görsel hardpoint veya doluluk göstergesiyle anlatılır
- Satın al/aktif et gibi tek ana eylem belirgin olur

### Liman İdaresi

- Seyir defteri veya kontrat masası görünümü
- Sol sayfada görev listesi
- Sağ sayfada seçilen görevin açıklaması, ilerlemesi ve ödülü
- Aktif, tamamlandı ve ödül alındı durumları yalnızca renk ile anlatılmaz; ikon ve metin birlikte kullanılır

## Deniz HUD’ı

HUD her an bütün sistemleri anlatmaz.

Sürekli görünenler:

- Gemi gövdesi ve kritik alt sistem durumu
- Seçili mühimmat/zıpkın
- İskele ve sancak dolum durumu
- Güvencesiz yük
- Minimap ve bölge yönleri

Bağlamsal görünenler:

- Aktif hedef
- Tamir ilerlemesi
- Tonic/buff süresi
- Pırıltı toplama
- Liman yaklaşma istemi

### Hiyerarşi

1. Hayatta kalma: can, kritik hasar, tehdit
2. Anlık eylem: ateş, dolum, hedef
3. Risk: güvencesiz yük ve sefer hedefi
4. Kaynak: silver/gold
5. İkincil ilerleme: günlük görev

## Etkileşim standardı

- Bütün tıklanabilir öğelerde normal, hover, basılı ve pasif durum bulunur.
- Pasif düğme neden pasif olduğunu kısa metinle açıklar.
- Ana eylem başına tek vurgu rengi kullanılır.
- Klavye/gamepad odağı görünür olmalıdır.
- Menü açılışı 160–220 ms, sekme geçişi 100–160 ms hedefler.
- Animasyon işlevi açıklamalı; sırf süs için uzun bekleme oluşturmaz.
- 16:9 1080p referanstır; 16:10 ve ultrawide güvenli alanları test edilir.

## Tipografi

Geçici LegacyRuntime font üretim standardı değildir.

- Başlık: denizcilik karakteri taşıyan ölçülü display serif
- Gövde: yüksek okunabilirlikte sans serif
- Sayılar: mümkünse tabular rakam desteği
- Tamamı büyük harf yalnızca kısa başlık ve durumlarda
- Uzun açıklamalar normal cümle düzeninde

## İkon ailesi

İlk ikon seti:

- Gövde
- Yelken
- Mürettebat
- Top ve top yuvası
- Menzil
- Hız
- Tamir
- Defans
- Silver ve Gold
- Güvencesiz yük
- Zıpkın ağırlıkları
- Tonic ve Light of Tortuga
- Kontrat durumları

İkonlar tek çizgi kalınlığı ve aynı perspektif ailesini kullanmalıdır. Emoji veya birbirinden kopuk hazır ikonlar karıştırılmaz.

## Geçici prototip ile final UI sınırı

Mevcut çalışma:

- İş mantığını ve panel yaşam döngüsünü doğrular.
- İstasyon ve sekme yapısını kanıtlar.
- Nihai renk, font, ikon ve kompozisyon olarak kabul edilmez.

Final UI çalışmasına başlama koşulu:

1. İlk gerçek gemi modeli oyunda
2. Güvenli limanın temel sanat dili onaylı
3. 1080p HUD bilgi öncelikleri test edilmiş
4. Gamepad/klavye navigasyon kararı verilmiş
