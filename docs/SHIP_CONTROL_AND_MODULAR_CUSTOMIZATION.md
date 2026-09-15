# Project Seaborn — Dümen Hissi ve Modüler Gemi Tasarımı

## Tasarım amacı

Project Seaborn’da gemi, oyuncunun yalnızca ekipmanı değil karakteridir. Kontrol; araba gibi anlık gaz/fren tepkisi vermemeli, komut alan ağır bir deniz aracı hissi taşımalıdır. Tersane ise sayı yükselten bir menü değil, oyuncunun gemisini fiziksel olarak kurduğu ve sonucunu doğrudan gördüğü bir mekân olmalıdır.

Bu belge iki sistemi birlikte tanımlar:

1. Kademeli seyir emri ve dümen tabanlı gemi kontrolü
2. Ön, orta ve kıç bölümlerden oluşan modüler gemi ve görsel tersane

---

## 1. Kademeli seyir emri

### W/S davranışı

W ve S doğrudan kuvvet uygulayan “gaz/fren” tuşları değildir. Kaptanın seyir emrini bir kademe değiştirir.

| Kademe | HUD adı | Hedef hareket |
|---:|---|---|
| -1 | Tornistan | Kontrollü düşük geri hız |
| 0 | Dur / Yelkenleri indir | İleri itki yok, mevcut momentum sürer |
| 1 | Ağır yol | Hassas yanaşma ve av yaklaşımı |
| 2 | Yarım yol | Ekonomik normal seyir |
| 3 | Tam yol | Azami hız, daha geniş dönüş ve daha zor duruş |

- W her basışta bir üst kademeye çıkar.
- S her basışta bir alt kademeye iner.
- W/S basılı tutulduğunda kontrollü tekrar olabilir; tek karede birden fazla kademe atlanmaz.
- Son kademe HUD’da sürekli ve açık biçimde görünür.
- İstasyon yanaşması veya batış, seyir emrini sıfıra alır.
- Menü açıkken kademe değişmez.

### Gerçekçilik sınırı

Yelkenli gemilerin gerçek davranışı rüzgâr, yelken açısı ve akıntıyla çok daha karmaşıktır. Prototipte üç ileri kademe, bu süreçlerin okunabilir oyun soyutlamasıdır. Tornistan da tarihsel yelken simülasyonu değil; limanda sıkışmayı önleyen düşük güçlü “backwater/çekici yardımı” soyutlamasıdır.

İleride rüzgâr sistemi geldiğinde kademeler doğrudan motor gücü değil, **istenen yelken alanı** olarak yorumlanacaktır.

---

## 2. İvmelenme, momentum ve duruş

Kademe değişikliği hızı anında değiştirmez. Her gemi aşağıdaki değerlere sahiptir:

- İleri ivmelenme
- Tornistan ivmelenmesi
- Doğal su direnci
- Yelken indirme yavaşlaması
- Azami ileri/geri hız
- Gövde kütlesi
- Dümen tepkisi
- Dönüşte hız kaybı

Hız, mevcut değerden seçili kademenin hedef hızına eğriyle yaklaşır.

### Hedef his

- Sloop tam yola daha hızlı çıkar ve daha kısa sürede durur.
- Dreadwake komuta gecikmeli cevap verir fakat momentumunu korur.
- Tam yoldan dur emrine geçince gemi bir süre süzülür.
- Tornistana geçmek önce ileri momentumu tüketir; gemi anında ters yöne fırlamaz.
- Sert dümen hızın bir bölümünü yer; sonsuz hızla kusursuz daire çizilemez.
- Çarpışmadan sonra kontrol girdisi gövdeyi yeniden kararlı hale getirir; fizik sistemi gemiyi sürekli spin’e sokmaz.

### Önerilen hız oranları

Kesin değerler gerçek modelle test edilir:

| Kademe | Azami hız oranı |
|---|---:|
| Ağır yol | %30 |
| Yarım yol | %65 |
| Tam yol | %100 |
| Tornistan | -%20 |

İvmelenme doğrusal olmak zorunda değildir. İlk tepki okunabilir, son hızlanma daha yavaş olmalıdır. Böylece komut hissedilir fakat gemi oyuncuyu bekletmez.

---

## 3. Dümen ve dönüş geometrisi

### A/D davranışı

A ve D doğrudan gövdeyi döndürmez; dümen açısını değiştirir.

Önerilen prototip:

- A basılıyken dümen kademeli olarak iskeleye gider.
- D basılıyken dümen kademeli olarak sancağa gider.
- Tuş bırakılınca dümen ayarlanabilir bir hızla merkeze döner.
- HUD veya gemi üzerinde dümen açısı okunabilir.
- Gemi neredeyse duruyorsa dümen etkisi çok düşüktür; yerinde tank gibi dönmez.
- Orta seyir hızında en verimli dönüş alınır.
- Çok yüksek hızda dönüş yarıçapı büyür ve gövde yana doğru kontrollü sürüklenir.
- Tornistanda dümen etkisi terslenir veya özel olarak ayarlanır; testte anlaşılır kalması önceliklidir.

Gerçek gemiler genel olarak bir yay/dönüş çemberi izler. Dönüş yarıçapı hız, gövde uzunluğu, dümen alanı ve su akışıyla değişir. Oyundaki hedef tam denizcilik simülasyonu değil; bu geometriyi taktik olarak hissettirmektir.

### Taktik sonuçlar

- Borda atışı için doğru yayı hazırlamak zaman ister.
- Küçük gemi ağır geminin kıçına yerleşebilir.
- Dar liman alanında tam yol risklidir.
- Zincirli gülleyle yelken hasarı dönüş ve kaçış planını etkiler.
- Oyuncu yalnızca nişan değil, saldırı hattını birkaç saniye önceden kurar.

---

## 4. Fizik güvenliği

Unity Rigidbody serbestçe bütün eksenlerde dönmeye bırakılmayacaktır.

- Simülasyon deniz düzleminde tutulur.
- Pitch ve roll yalnızca görsel kökte uygulanır.
- Fizik gövdesi yaw dışında kararlı kalır.
- Çarpışma açısal hızına üst sınır konur.
- Çarpışma sonrası kısa bir açısal sönümleme uygulanır.
- İki geminin collider’ı birbirine kilitlenmeyecek kadar sade ve dışbükey olmalıdır.
- Gemi kontrolü transform rotasyonu ile Rigidbody kuvvetlerini birbiriyle kavga ettirmez.
- Dümen modeli tek otorite üzerinden yaw üretir.

Bu sınırlar gerçekçilikten vazgeçmek değil, oyuncunun kontrolünü korumaktır.

---

## 5. Modüler gemi anatomisi

Kullanıcının tarif ettiği üç ana bölüm için üretim adları:

1. **Pruva bölümü / Forecastle (ön)**
2. **Orta gövde / Midship hull section (orta)**
3. **Kıç bölümü / Quarterdeck–stern (arka)**

Temel kurulum:

```text
Pruva Soketi → Orta Gövde Soketleri → Kıç Soketi
```

Bir gemi sınıfı belirli sayıda orta gövde bölümü kabul eder. Oyuncu sınırsız gövde ekleyemez; her sınıfın uzunluk, deplasman ve güverte sınırı bulunur.

### Neden yalnızca üç rastgele mesh yetmez?

Bölümler birleştiğinde şunların da sürekliliği gerekir:

- Güverte yüksekliği ve genişliği
- Bordo eğrisi
- Su çizgisi
- Küpeşte
- Direk ve arma bağlantıları
- Top hardpoint aralıkları
- Collider ve hasar bölgeleri
- Pruva/kıç dekor çizgisi
- Pruva köpüğü ve dümen suyu VFX noktaları

Bu nedenle modeller ortak ölçü ve soket standardıyla üretilmelidir.

---

## 6. Orta gövde modülleri

Orta bölüm yalnızca “gemiyi uzatan boş parça” değildir. Her modül oynanış ve görünüş taşır.

Örnek modüller:

| Modül | Görsel | Ana katkı | Bedel |
|---|---|---|---|
| Top Güvertesi | Açık top kapakları | Top yuvası | Kütle ve dönüş yarıçapı |
| Kargo Ambarı | Vinç ve ambar kapağı | Yük kapasitesi | Daha düşük ivmelenme |
| Takviyeli Omurga | Kalın kaplama | HP/defans | Hız |
| Avcı Güvertesi | Zıpkın tertibatı | Zıpkın performansı | Top kapasitesi |
| Mürettebat Güvertesi | Yaşam alanı | Tamir/mürettebat | Kargo alanı |
| Nadir Extended Deck | Geniş üst güverte | Esnek hardpoint | Yüksek değer ve ağırlık |

Mevcut Extended Deck sistemi ileride bu fiziksel orta bölüm/hardpoint mimarisine bağlanacaktır. “+1 top yuvası ve +250 HP” değeri yalnızca veri değil, gemide görünen bir dönüşüm olacaktır.

### Denge kuralı

Daha uzun gemi her yönden daha iyi olmaz.

- Daha fazla HP ve hardpoint
- Daha yüksek kütle
- Daha yavaş ivmelenme
- Daha geniş dönüş çemberi
- Daha büyük hedef profili
- Daha yüksek mürettebat/tamir maliyeti

Böylece küçük çevik gemiler yüksek seviyede de taktik rolünü korur.

---

## 7. Hardpoint ve donanım mimarisi

Her modül tanımlı soketler taşır:

- Port Cannon Slot
- Starboard Cannon Slot
- Harpoon Slot
- Mast/Sail Slot
- Utility Slot
- Cargo Slot
- Armor Slot
- Figurehead/Cosmetic Slot

Hardpoint yalnızca kapasite sayısı değildir; dünyada fiziksel bir Transform ve görsel montaj noktasıdır.

Her takılan top:

- Seçilen hardpoint’e yerleşir.
- Dünya modelinde görünür.
- Ateş çıkış noktası o topun namlusundan gelir.
- Gerekirse ağırlık ve reload hesabına katılır.
- Söküldüğünde envantere geri döner.
- Hasar durumunda görsel olarak devre dışı kalabilir.

İskele ve sancak donanımı ayrı düzenlenebilir. Bu, oyuncuya simetrik güvenli kurulum veya tek bordaya yüklenen riskli saldırı gemisi seçenekleri açar. Denge ve PvP okunabilirliği test edilmeden bu asimetri üretime açılmaz.

---

## 8. Tersane deneyimi

### Giriş

Oyuncu tersaneye yanaşır. Normal deniz HUD’ı geri çekilir. Kamera aktif gemiye kontrollü biçimde yaklaşır ve tersane çalışma açısına döner. Gemi denizde veya kızakta canlı bir 3B nesne olarak kalır.

### Ekran yapısı

- Sol: Gemi bölümleri ve kategori seçimi
- Orta: Döndürülebilen/yakınlaştırılabilen gerçek gemi
- Sağ: Oyuncu envanteri ve seçili parçanın istatistikleri
- Alt: Uygula, sök, karşılaştır ve değişiklik özeti

### Top takma akışı

1. Oyuncu gemi üzerindeki bir top hardpoint’ini seçer.
2. Seçili yuva ışık/çerçeve ile vurgulanır.
3. Sağ envanter yalnızca uyumlu topları filtreler.
4. Top seçildiğinde gemi üzerinde hayalet önizleme görünür.
5. Hasar, reload, menzil, ağırlık ve toplam borda hasarı karşılaştırılır.
6. Oyuncu Uygula der.
7. Top gerçek modelde yerine oturur; envanter ve loadout tek işlem olarak güncellenir.
8. İşlem kaydedilir ve geri al/iptal sınırı açıkça gösterilir.

Aynı model yelken, zırh, zıpkın, utility ve kozmetik parçalar için kullanılır.

### Kamera kuralları

- Tersane kamerası oynanış kamerasından ayrıdır.
- Geçiş 0,5–0,9 saniyelik kontrollü bir blend kullanır.
- Seçilen bölüme göre pruva, borda veya kıç kadrajına yaklaşır.
- Kamera geminin içine girmez ve UI tarafından kapatılan bölgeyi dikkate alır.
- Parça takılırken kısa fiziksel animasyon ve tok ses geri bildirimi kullanılır.

---

## 9. Veri ve ağ otoritesi

Gemi görünüşü kayıtlı loadout verisinden üretilir; sahnedeki mesh tek başına gerçek veri değildir.

Önerilen kayıt yapısı:

- Ship Definition ID
- Fore section ID
- Mid section ID listesi
- Stern section ID
- Her hardpoint için Equipment Instance ID
- Yelken ve direk kurulumları
- Kozmetik boya/sancak
- Kalıcı hasar/onarım durumu

Çok oyunculu sürümde satın alma ve takma işlemini sunucu doğrular. İstemci yalnızca önizleme yapar; onay sonrası kalıcı loadout yayınlanır.

---

## 10. Uygulama sırası

### Aşama A — Kontrol prototipi

- Kademeli seyir durumu
- İvmelenme/yavaşlama eğrisi
- Dümen açısı ve merkeze dönüş
- Hıza bağlı dönüş çemberi
- Tornistan
- Çarpışma sonrası spin koruması
- HUD’da seyir ve dümen göstergesi

### Aşama B — Tek gemi modüler iskeleti

- Sloop’u pruva/orta/kıç olarak böl
- Ortak soket standardı
- Bir tekrar edilebilir orta gövde
- Fizik collider bileşimi
- Top hardpoint’leri
- Runtime gemi assembler

### Aşama C — Görsel tersane

- Tersane kamera rig’i
- Gemi bölümü seçimi
- Hardpoint vurgusu
- Filtrelenmiş envanter
- Hayalet parça önizlemesi
- Uygula/sök/kaydet akışı

### Aşama D — Denge ve ağ hazırlığı

- Modül ağırlığı ve manevra bedeli
- Hasar bölgeleri
- İstemci önizleme / sunucu onayı ayrımı
- Diğer oyunculara loadout replikasyonu
- PvP silüet ve okunabilirlik testi

---

## Kabul kriteri

Bu tasarım ilk kez tamamlanmış sayılırken:

- W/S tek basışla seyir kademesi değiştirir.
- Gemi hedef hıza anında sıçramaz.
- A/D dümen açısını yönetir ve gemi yerinde dönmez.
- Sloop ve ağır gemi belirgin farklı ivmelenme/dönüş hissi verir.
- Çarpışma sonrası gemi kontrolsüz daire çizmez.
- Sloop en az pruva, orta gövde ve kıç bölümünden runtime kurulur.
- Bir orta bölüm eklemek geminin görünüşünü ve istatistiğini değiştirir.
- Oyuncu tersanede fiziksel bir top yuvasını seçip envanterden top takabilir.
- Takılan top gemi modelinde görünür ve doğru namludan ateş eder.
- Kurulum kaydedilir ve harita geçişinden sonra korunur.
