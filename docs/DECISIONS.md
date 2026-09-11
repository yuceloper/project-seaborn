# Project Seaborn — Karar Günlüğü

Bu belge alınmış ürün ve teknik kararlarını korur. Değişen kararlar silinmez; yeni tarihli kayıtla geçersiz kılınır.

## 2026-09-09 — Motor ve ilk platform

**Karar:** Unity 6000.6.0f1, URP ve ilk prototip için Windows.

**Neden:** Küçük ekip için hızlı iterasyon, C# üretkenliği, cross-platform yolu ve izometrik aksiyon için yeterli araç zinciri.

## 2026-09-09 — Dünya yapısı

**Karar:** Güvenli/sosyal liman merkezi ve 15–25 dakikalık sefer haritaları.

**Neden:** Tek parça MMO dünyasına göre daha kontrollü üretim kapsamı; riskli bölgeler, oturum ritmi ve ölçeklenebilir sunucu instance’ları için uygun temel.

## 2026-09-09 — Kamera ve oyuncu kimliği

**Karar:** Uzak izometrik taktik kamera; bir oyuncu bir gemi. İlk aşamada yürünebilir kaptan yok.

**Neden:** Savaş okunabilirliğini ve gemi kimliğini merkeze almak, animasyon/iç mekân kapsamını sınırlamak.

## 2026-09-09 — Sanat yönü

**Karar:** Stilize gerçekçilik; gerçekçi temel üzerinde bölge değerine göre kontrollü fantastik yoğunluk.

**Neden:** Hem atmosfer hem performans/okunabilirlik; farklı bölgelerin mekanik ve görsel olarak yükselmesine izin vermek.

## 2026-09-09 — PvP ve kayıp

**Karar:** Suç/ödül sonuçlarıyla açık saldırı. Batışta yük ve taşınan mühimmat riskte; kurulu kalıcı ekipman korunur.

**Neden:** Gerilim üretmek fakat tek yenilgiyi ilerlemeyi silen cezaya çevirmemek.

## 2026-09-09 — Batış sonrası özgün döngü

**Karar:** Hasarlı ana gemi onarılırken yedek gemiyle kurtarma/enkaz seferi yapılabilir.

**Neden:** Batışı yalnızca bekleme veya para ödeme ekranı olmaktan çıkarıp yeni oynanış üretmek.

## 2026-09-09 — Güç ve beceri dengesi

**Karar:** Ekipman gücü ön planda; makul güç farkı taktikle aşılabilir.

**Neden:** Uzun vadeli ilerlemeyi değerli tutarken savaşın otomatik istatistik karşılaştırmasına dönüşmesini engellemek.

## 2026-09-09 — Manuel borda nişanı

**Karar:** Sağ tuşla nişan modu, mouse ile dünya hedefi, sol tuşla ateş. Toplar hedefe zamanla yetişir; erken atış daha fazla saçılır.

**Neden:** Hareketli hedefin rotasına ateş etme, borda açısı ve riskli zamanlama üzerinden oyuncu becerisi yaratmak.

## 2026-09-09 — İş modeli

**Karar:** Tek seferlik satın alma ve isteğe bağlı kozmetik/premium hizmetler.

**Neden:** Rekabetçi ekipman dengesini ödeme baskısından korumak.

## 2026-09-09 — Sıradaki ürün önceliği

**Karar:** Yeni mühimmat sistemlerinden önce atmosfer dikey dilimi.

**Neden:** Çekirdek savaş döngüsü artık eğlenceli. Bir sonraki en büyük belirsizlik sistem genişliği değil, dünyanın ayırt edilebilir görsel ve işitsel kimliği.

## 2026-09-09 — İlk atmosfer sahnesi

**Karar:** İlk atmosfer dikey dilimi “fırtına öncesi altın saat kıyı suları” paletini kullanır. Deniz dalgası ilk aşamada yalnızca görsel shader deformasyonudur; fizik ve nişan düzlemi sabit kalır.

**Neden:** Atmosferi hızlı doğrularken mevcut savaş okunabilirliğini ve deterministik mermi hedeflemesini korumak.

## 2026-09-10 — Gemi-su temas efektlerini erteleme

**Karar:** Prototip kutu gemiler üzerindeki pruva köpüğü ve dümen izi üretimden kaldırıldı; gerçek gemi modeli, gövde ölçüsü ve su çizgisi belirlendikten sonra yeniden ele alınacak. Gülle su sıçraması korunur.

**Neden:** Parçacık ve şerit denemeleri teknik olarak çalışsa da uzak izometrik kamerada doğal görünmedi. Geçici geometriye göre yapılan görsel ayarın tekrar iş üretmesini önlemek.

## 2026-09-10 — Build güvenli çalışma zamanı materyalleri

**Karar:** Kodla üretilen gemi ve combat VFX görselleri shader adına yalnızca `Shader.Find` ile bağlanmaz. Kaynak materyaller `Resources` altında asset olarak saklanır ve çalışma anında örneklenir.

**Neden:** Editörde bulunan fakat player build sırasında strip edilen veya farklı fallback kullanan shader’lar, namlu dumanının beyaz ekran lekelerine dönüşmesine ve editör/build görüntüsünün ayrışmasına yol açtı.

## 2026-09-10 — Atmosferden savaş derinliğine geçiş

**Karar:** Okyanus, kıyı silüetleri, gemi blockout’u, görsel yalpa, temel ses ve kontrollü post-processing ile atmosfer dikey diliminin ana yönü doğrulandı. Gökyüzü/bulut, yüzey köpüğü ve ayrıntılı gemi-su teması polish backlog’unda kalır; aktif öncelik mühimmat ve yeniden doldurma sistemidir.

**Neden:** Prototip artık görsel ve işitsel kimliğini anlatıyor. Bir sonraki en büyük ürün riski atmosfer değil, savaş kararlarının standart borda ateşinin ötesine geçip geçememesi.

## 2026-09-10 — Mühimmat ve yeniden doldurma temeli

**Karar:** Standart, zincirli ve saçma mühimmatı ortak bir veri profili üzerinden çalışır. Her tür hasar, menzil, saçılma, yeniden doldurma, salvo sayısı ve mermi ölçeğiyle ayrışır. İskele ve sancak bataryaları bağımsız yeniden dolar; her ateşlenen top taşınan stoktan bir mühimmat tüketir.

**Neden:** Donanım gücünü korurken hedef mesafesi, düşman yönü ve salvo zamanlaması üzerinden taktik karar üretmek. Stok ve durum olayları UI ile ağ katmanını combat koduna bağlamadan ilerletir.

## 2026-09-10 — Av zıpkını ve deniz avı döngüsü

**Karar:** İlk zıpkın sistemi gemi bağlayan PvP silahı değil, deniz canlılarını avlayan PvE ekipmanıdır. Avdan silver ve malzeme elde edilir; yük limana ulaşmadan güvenceye alınmaz. Gerçekçi kıyı canlılarından kontrollü fantastik leviathan karşılaşmalarına yükselen bölgesel bir av hattı kurulacaktır.

**Neden:** Savaşa alternatif fakat aynı risk ekonomisini besleyen bir sefer etkinliği yaratmak; avcı gemilerine, zıpkın ekipmanına ve PvP’de değerli yük taşıyan doğal hedeflere anlam kazandırmak.

## 2026-09-10 — İlk deniz avı kontrolü

**Karar:** Prototip av zıpkını top nişanından ayrı olarak Sol Shift basılıyken mouse ile hedeflenir ve sol tıkla atılır. İlk av canlısı Tideback gezinir, zıpkın isabetinden sonra avcıdan kaçar ve av tamamlandığında prototip silver kazandırır.

**Neden:** Mevcut sağ tuş borda nişanını bozmadan aynı fare kas hafızasını kullanmak ve av döngüsünü sahne/prefab kurulumu gerektirmeden hızlıca doğrulamak. Üretim kontrol şeması UI ve gamepad çalışmasıyla yeniden ele alınacaktır.

## 2026-09-10 — Av ödülünün riskli yük olması

**Karar:** Deniz canlısı avı silver'ı doğrudan cüzdana eklemez. Değer, geminin güvencesiz av yükünde birikir; yalnızca ileride liman teslimiyle kalıcı silver'a çevrilir. Yük bileşeni teslim ve tüm yükü kaybetme işlemlerini ayrı API olarak sunar.

**Neden:** Av faaliyetini risksiz para üretiminden çıkarıp limana dönüş, kargo kapasitesi ve PvP tehdidiyle bağlamak. Böylece başarılı av, seferin sonuna kadar korunması gereken bir değere dönüşür.

## 2026-09-10 — Prototip liman teslimi

**Karar:** Oyuncunun başlangıç konumu ilk geçici liman teslim alanıdır. Av yüküyle işaretli halkaya dönüldüğünde güvencesiz değer sıfırlanır ve aynı miktar kalıcı prototip silver cüzdanına aktarılır.

**Neden:** Ayrı bir liman sahnesi üretmeden seferin çıkış, risk alma, dönüş ve kazancı güvenceye alma ritmini uçtan uca test etmek.

## 2026-09-10 — Batışta av yükü kaybı

**Karar:** Oyuncu gemisinin batış olayı güvencesiz av yükünü tamamen siler. Limanda güvenceye alınmış silver etkilenmez.

**Neden:** Oyuncuya risk miktarını anlaşılır tutmak ve ilk prototipte kısmi kayıp hesabı yerine net bir “dön veya riske et” kararı üretmek.

## 2026-09-10 — İlk leviathan karşılaşması

**Karar:** İlk nadir av Stormjaw Leviathan’dır. Normal Tideback’ten büyük, dayanıklı ve yüksek değerli olur; ilk zıpkın isabetinden sonra kaçmak yerine avcı gemisine yönelip aralıklı koçbaşı hasarı verir.

**Neden:** Nadir avı yalnızca daha uzun bir can çubuğuna çevirmemek; oyuncunun kaçış, yön verme ve riskli yük kararını savaş tehdidiyle birleştirmek.

## 2026-09-10 — Hızlandırılmış sefer prototipi

**Karar:** İlk sefer durum makinesi limandan ayrılınca otomatik başlar. Oyuncu istediği anda yükle dönebilir; 90 güvencesiz silver yalnızca “dönüş tavsiye edilir” eşiğidir. Limanda güvenceye alınan herhangi bir pozitif yük başarılı sefer, batış başarısız sefer sayılır. Prototipte baskı 3,5 dakikada başlar ve 6 dakikada tepeye çıkar; üretim hedefi sert süre sonu olmadan 15–25 dakikadır.

**Neden:** Hazırlık, av/savaş, risk büyütme ve dönüş ritmini kısa testlerde doğrulamak; oyuncunun erken çıkış özgürlüğünü korurken ileride hava, görünürlük ve çıkış baskısını aynı durum modeline bağlamak.

## 2026-09-10 — Günlük ve aylık kontratlar

**Karar:** Günlük kontratlar üç seçenek sunar ve iki tamamlamayla ana ödül verir; aylık kaptan seyir defteri farklı faaliyetlerle ilerleyen uzun hedeflerden oluşur. Giriş serisi, kaçırılan gün cezası ve zorunlu güç ödülü kullanılmaz. Gerçek yenileme ve kalıcılık sunucu otoriteli olacaktır.

**Neden:** Düzenli dönüş motivasyonu üretirken Project Seaborn’u oyuncunun kendi sefer hedefini seçtiği yapıdan zorunlu günlük görev listesine çevirmemek ve premium/pay-to-win baskısından korumak.

## 2026-09-10 — Güvenli limanın ayrı hub olması

**Karar:** Ürün sürümünde güvenli liman, sefer haritalarından ayrı bir sosyal şehir/hub instance’ıdır. Oyuncu limanda da gemisiyle temsil edilir; yürüyen kaptan ve kara savaşı yapılmaz. İlk prototipte mevcut başlangıç halkası liman yerine geçmeye devam eder.

**Neden:** PvP’siz hazırlık, ticaret ve sosyalleşme alanını riskli seferlerden net ayırmak; sunucu instance yaşam döngüsünü sadeleştirirken kara karakteri kapsamını açmamak.

## 2026-09-11 — Güvenli limanın merkezi kuralı

**Karar:** Prototip liman halkası içindeyken oyuncunun top ve zıpkınları kilitlenir, oyuncu gemisi hasar almaz ve Stormjaw liman sınırında takibi bırakır. Bu durum tek bir güvenli liman koruma bileşeninden okunur ve UI için olay yayınlar.

**Neden:** Güvenli alanı yalnızca görsel bir halka olmaktan çıkarıp oynanış sözleşmesine dönüştürmek; ileride ayrı hub sahnesine geçerken savaş, av, AI ve UI kodlarının aynı otoriteyi kullanmasını sağlamak.

## 2026-09-11 — Liman servisleri ve silver gideri

**Karar:** Onarım ile standart, zincirli, saçma ve zıpkın ikmali yalnızca güvenli limandayken kullanılabilir. Servisler kalıcı silver harcar; fiyat ve paket miktarları prototipte serileştirilebilir değerlerdir. UI doğrudan sistemleri değiştirmek yerine liman servisi API’sini çağırır.

**Neden:** Av ve kontrat kazançlarının karşısına anlamlı bir silver gideri koymak, sefere hazırlığı limanın temel işlevine dönüştürmek ve ileride sunucu otoritesine taşınabilecek tek bir işlem sınırı oluşturmak.

## 2026-09-11 — Liman hazırlık paneli

**Karar:** Prototip liman arayüzü tek bir kaptan hazırlık panelinde silver, sefer kontratı seçimi, gemi onarımı, mühimmat/zıpkın ikmali ve sefere hazır olma özetini birleştirir. Panel yalnızca güvenli limanda görünür ve oyun sistemlerinin public API’lerini kullanır.

**Neden:** Oyuncunun limandaki karar sırasını tek bakışta anlaşılır kılmak ve ayrı hub sahnesi üretilmeden önce hazırlık döngüsünün kullanılabilirliğini doğrulamak.

## 2026-09-11 — Batış sonrası kurtarma döngüsü

**Karar:** Ana gemi battığında oyuncu kısa gecikmeyle güvenli limanda yedek gemiye geçer. Ana geminin enkazı batış konumunda görev hedefi olarak kalır; yedek gemiyle bölgeye dönülüp yakın mesafede etkileşim tamamlandıktan sonra ana gemi limanda silver karşılığı yeniden hizmete alınır. Yedek geminin batması mevcut enkaz hedefini silmez.

**Neden:** Batışı yalnızca yeniden doğma ekranı olmaktan çıkarıp kayıp, geri dönüş ve toparlanma üzerine özgün bir sefer üretmek; kalıcı ekipmanı silmeden yenilgiye anlamlı bedel ve hikâye kazandırmak.

## 2026-09-11 — Yedek geminin oynanış kimliği

**Karar:** Prototip yedek gemi ana gemiye göre yüzde 18 daha hızlı, yüzde 30 daha çevik, 60 cana ve 90 silver av yükü kapasitesine sahiptir. Ana gemi yeniden hizmete alındığında hareket, can ve yük değerleri eksiksiz biçimde ana gemi profiline döner.

**Neden:** Kurtarma seferini normal kazanç seferinden farklılaştırmak; oyuncuya enkaza ulaşmak için güçlü kaçış araçları verirken yedek geminin düşük dayanıklılık ve ambar sınırıyla ana geminin yerine kalıcı olarak geçmesini engellemek.
