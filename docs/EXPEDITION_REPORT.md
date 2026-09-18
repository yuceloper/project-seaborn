# Sefer sonu ekonomi özeti

## Davranış

- Oyuncuya bağlı kayıt, limandan denize geçince başlar; deniz bölgeleri arasında sıfırlanmaz.
- Editor'de doğrudan deniz sahnesini açmak da bir kayıt başlatır.
- Limana dönüşte, teslim ve ödül olayları bittikten sonra sonuç dondurulur. Boş dönüş de rapor üretir.
- Batışta aynı karedeki yük kaybı olayları beklenir. Sonuç limana dönünce gösterilir; yedek gemi kurtarma akışı aynı sefere eklenmez.
- Son rapor oturum boyunca saklanır. Yeni sefer başlayınca panel gizlenir; sonuçta yenisiyle değiştirilir. Disk kaydı bu turun kapsamında değildir.
- Parşömen/pirinç panel liman servisleri açıkken gizlenir. Kapatılan rapor, limandaki Son Sefer düğmesinden tekrar açılır.

## Hesap tanımı

Cüzdana giren toplam, yalnızca `AddSilver` üzerinden gerçekten kazanılan Silver'dır. Kayıt yükleme ve stok iadesi kazanç sayılmaz. Teslim edilen yük bu toplamın bir alt kalemidir; tekrar eklenmez. Diğer Silver ödülleri toplamdan teslim tutarının çıkarılmasıyla gösterilir.

Mühimmat ikmal değeri = her gerçek tüketim için adet × paket fiyatı / paket adedi. Top mühimmatı mevcut liman servisi fiyatlarını, zıpkın atış anındaki seçili tanımı kullanır. Kısmi salvo sadece tüketilen adedi sayar; saçmanın pellet sayısı mühimmat giderini katlamaz. Denizde bulunan mühimmat tüketim sayacını düşürmez. Değer ondalıklı tutulur, yalnızca gösterimde yuvarlanır.

Seferin onarım payı = max(0, dönüş onarım teklifi − çıkış onarım teklifi). Bu, sefer öncesi hasarı tekrar gider saymayan bir tahmindir; mevcut liman onarım formülünü kullanır. Bedelsiz saha tamiri dönüş teklifini azaltabilir. Batışta normal onarım teklifi kullanılmaz; kurtarma gideri rapor dışında belirtilir.

Tahmini net = cüzdana giren toplam − mühimmat ikmal değeri − seferin onarım payı.

Bu değer banka bakiyesi değişimi veya satın alınacak tam paketlerin fiyatı değildir. Sarf malzemeleri, gemi/ekipman alımları ve kurtarma masrafları kapsam dışıdır. Malzemeler adet olarak gösterilir; Silver'a çevrilmez. Kaybedilen güvencesiz yük zaten kazanca eklenmediği için netten ikinci kez düşülmez. Özet hiçbir ödeme, onarım veya ödül işlemi yapmaz.

## Doğrulama

Unity menüsü: `Seaborn > Validation > Check Expedition Accounting`.

Saf hesap kontrolleri: boş sefer, teslimi iki kez saymama, kısmi salvo, saçma pellet/adet ayrımı, farklı zıpkın fiyatları, ondalık giderler, önceden hasarlı çıkış, ücretsiz tamir, batış ve geçersiz paket boyutu.

Oyun içi kabul turu:

1. Limandan çık, 6 standart gülle ateşle: varsayılan 20/40 paket fiyatıyla ikmal değeri 3 Silver olmalı.
2. Bir deniz bölgesinden diğerine geç; önceki tüketim korunmalı. Mühimmat topla; kullanılan adet azalmamalı.
3. Yükle dön; cüzdana giren Silver teslim tutarıyla ve varsa ödüllerle eşleşmeli. Özet cüzdandan para çekmemeli.
4. Hiç yük toplamadan dön; sıfır gelirli rapor açılmalı. Limanda beklemek veya raporu yeniden açmak yeni rapor/ödül üretmemeli.
5. Hasarlı çıkıp daha fazla hasarla dön; yalnızca teklif farkı onarım payına girmeli. Limanda sonradan onarım/alışveriş raporu değiştirmemeli.
6. Yüklü ve yüksüz bat; yük kaybı bir kez görünmeli. Kurtarma gemisiyle limana dönünce batış raporu korunmalı.
7. Yeni sefere çık; sayaçlar sıfırlanmalı. Son Sefer düğmesi ve liman panelleri üst üste gelmemeli.
8. 1280×720, 1920×1080 ve ultrawide çözünürlükte raporun tüm satırlarını, dört malzeme satırını ve Kapat düğmesini kontrol et.

Bu çalışma ortamında Unity Editor / C# derleyicisi bulunmadığından menü kontrolü ve Play Mode turu burada çalıştırılmadı. PR merge öncesinde Unity doğrulaması gerekir.
