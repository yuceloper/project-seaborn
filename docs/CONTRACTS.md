# Project Seaborn — Kontrat ve Görev İlkeleri

Bu belge sefer sözleşmelerini, günlük kontratları ve aylık kaptan hedeflerini tanımlar.

## Tasarım amacı

Görev sistemi oyuncuya denize çıkmak için anlamlı hedefler sunar; oyunu yapılacaklar listesine dönüştürmez. Ödüller oyuncunun seçtiği savaş, avcılık, ticaret veya keşif faaliyetini destekler.

## Görev katmanları

### Sefer sözleşmeleri

Limandan ayrılmadan seçilir ve tek sefer içinde tamamlanır.

- **Kıyı Avı:** 2 Tideback avla ve yükü güvenceye al.
- **Tehlikeli Av:** Stormjaw Leviathan avla ve yükü güvenceye al.
- **Serbest Sefer:** Hedef sınırı olmadan istediğin yükle limana dön.

Bir sefer sözleşmesi oyuncunun asıl kısa vadeli hedefidir. Erken dönüş her zaman mümkündür.

### Günlük kontratlar

Her gün üç isteğe bağlı kontrat sunulur. Oyuncunun hepsini yapması beklenmez; iki tamamlamadan sonra günlük ana ödül alınır.

İlk prototip havuzu:

- Bir seferde en az 90 silver güvenceye al.
- Toplam 2 Tideback avla.
- Batmadan 2 sefer tamamla.
- Bir seferde riskli yükü 180 silver üzerine çıkar.

Kurallar:

- Giriş serisi ve kaçırılan gün cezası yoktur.
- Günlük görevler özel savaş gücü vermez.
- Ödüller silver, tamir/ikmal kolaylığı ve kozmetik ilerleme ağırlıklıdır.
- Aynı faaliyet sürekli zorunlu tutulmaz.

### Aylık kaptan seyir defteri

Tek gün giriş istemeyen, ay boyunca doğal oynanışla ilerleyen uzun hedeflerdir. Oyuncu farklı rotalardan ilerleyebilmelidir.

İlk hedef örnekleri:

- Ay boyunca toplam 1.500 silver güvenceye al.
- 12 başarılı sefer tamamla.
- 3 nadir av karşılaşmasını tamamla.
- 4 farklı kontrat hedefi bitir.
- En az 180 silver yükle başarılı bir riskli dönüş yap.

Aylık ana ödül için tüm hedefler değil, hedef havuzunun çoğunluğu yeterlidir. Kaçırılan günler ilerlemeyi bozmaz ve kalıcı ekipman gücü aylık sisteme kilitlenmez.

## Güvenli liman şehri

Ürün sürümünde güvenli liman, sefer haritalarından ayrı bir sosyal hub instance’ıdır.

- Limanda PvP ve silah kullanımı kapalıdır.
- Oyuncu kimliği gemidir; yürüyen kaptan ve kara savaşı yoktur.
- Tersane, onarım, ikmal, pazar ve kontrat panosu kıyı yapıları ile UI üzerinden kullanılır.
- Oyuncular gemileriyle liman sularında birbirini görebilir.
- Sefer bölgesine çıkış, limandaki rota/kontrat seçimiyle ayrı instance’a geçer.

İlk prototipte ayrı sahne üretmek yerine mevcut başlangıç halkası limanı temsil etmeye devam eder. Ayrı hub sahnesi, sefer sözleşmeleri doğrulandıktan sonra yapılır.

## Sunucu ve zaman kuralları

Günlük ve aylık yenilemeler ileride sunucu otoriteli UTC zamanına dayanır. İstemci yalnızca kalan süreyi gösterir. Prototip aşamasında görev tanımları ve ilerleme event’leri hazırlanır; gerçek reset ve kalıcılık backend aşamasına bırakılır.
