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

## 2026-09-11 — Enkaz alanında isteğe bağlı tehdit

**Karar:** Ana gemi enkazının çevresinde mevcut düşman borda AI’sını kullanan bir Enkaz Yağmacısı bulunur. Yağmacıyı batırmak ana gemiyi kurtarmak için zorunlu değildir; zafer 30 güvencesiz silver, 12 standart gülle ve 3 zıpkın verir.

**Neden:** Yedek geminin çeviklik avantajını savaşmak veya kaçınmak için anlamlı hale getirmek; kurtarma görevini tek bir etkileşim çubuğundan çıkarıp oyuncunun mevcut riskini büyütmeyi seçebildiği kısa bir karşılaşmaya dönüştürmek.

## 2026-09-11 — Düşman gemisi enkaz ganimeti

**Karar:** Normal düşman gemileri batırıldığında iki dakika boyunca suda kalan bir enkaz sandığı bırakır. Oyuncu yaklaşıp etkileşime geçtiğinde 35 güvencesiz silver ve 6 standart gülle alır. Enkaz Yağmacısı bu genel tablonun dışında kalır ve özel kurtarma ödülünü kullanır.

**Neden:** Deniz savaşını yalnızca tehdit temizlemeye değil sefer ekonomisine bağlamak; zaferden sonra oyuncuya ganimeti toplama ve limana sağ salim taşıma sorumluluğu vermek.

## 2026-09-11 — Oynanış HUD bilgi hiyerarşisi

**Karar:** Sürekli oynanış arayüzü gemi ve gövde durumunu sol üstte, sefer ritmini üst merkezde, güvenli ve güvencesiz ekonomiyi sağ üstte, borda mühimmatı, yeniden doldurma ve zıpkını alt merkezde gösterir. Liman hazırlık paneli bu HUD'ın üzerinde açılır; HUD sahne kurulumu gerektirmeden çalışma anında oluşturulur.

**Neden:** Aksiyon sırasında göz hareketini azaltmak, batışta kaybedilecek değeri sürekli görünür tutmak ve prototip sistemlerini geçici debug yazılarından ortak bir görsel dile taşımak.

## 2026-09-11 — Oynanış bildirimlerinin kapsamı

**Karar:** Anlık bildirim katmanı yalnızca oyuncu kararını etkileyen sonuçları gösterir: av/ganimet kazanımı, yükün güvenceye alınması veya kaybı, kritik gövde hasarı ve batış. Sürekli hasar sayıları ve her mühimmat değişimi bildirim akışına eklenmez.

**Neden:** Konsol loglarına ihtiyaç duymadan önemli sonuçları görünür kılmak; uzak izometrik savaşta ekranı tekrar eden mesajlarla kaplamamak.

## 2026-09-12 — Prototip ilerleme kalıcılığı

**Karar:** Yerel prototip kaydı yalnızca güvenceye alınmış silver, top mühimmatı, seçili mühimmat ve zıpkın stoğunu saklar. Güvencesiz yük, aktif sefer, gemi hasarı ve batış/kurtarma durumu kaydedilmez. Kayıt JSON olarak uygulamanın kalıcı veri dizinine yazılır; çevrim içi sürümde sunucu otoritesi bunun yerini alacaktır.

**Neden:** Oyuncuya oturumlar arasında ilerleme hissi verirken oyunu kapatmanın riskli yükü güvenceye alma yoluna dönüşmesini engellemek ve geçici istemci kaydını nihai backend mimarisiyle karıştırmamak.

## 2026-09-12 — İlk kalıcı gemi ekipmanları

**Karar:** İlk tersane ilerlemesi üç bağımsız ve üç seviyeli hattan oluşur: güçlendirilmiş gövde azami canı, top takımı borda hasarı ile yeniden doldurma hızını, zıpkın donanımı av hasarı ile yeniden doldurma hızını geliştirir. Yükseltmeler yalnızca güvenli limanda silver ile alınır ve yerel prototip kaydında korunur.

**Neden:** Ekipman gücünü hissedilir ve uzun vadeli kılarken tek bir doğrusal güç puanı yerine oyuncunun savaş, dayanıklılık veya av odağını seçmesine izin vermek. Yüzdesel artışları sınırlı tutarak rota, menzil ve atış zamanlamasının güç farkını aşabilmesini korumak.

## 2026-09-12 — Mühimmatın alt sistem rolleri

**Karar:** Standart gülle mevcut yüksek gövde hasarı rolünü korur. Zincirli gülle yelken bütünlüğünü düşürerek hız ve dönüşü, saçma mühimmatı mürettebat hazırlığını düşürerek borda yeniden doldurmasını zayıflatır. Alt sistem hasarı hem oyuncu hem düşman gemilerinde işler ve liman onarımıyla tamamen giderilir.

**Neden:** Üç mühimmatı yalnızca farklı hasar katsayıları olmaktan çıkarıp hedefi batırmak, kaçışını kesmek veya ateş temposunu bozmak arasında taktik seçim üretmek. Cezaları taban gücü tamamen yok etmeyecek şekilde sınırlamak, hasarlı gemiye hâlâ manevra ve karşı oyun şansı bırakır.

## 2026-09-12 — İlk düşman gemisi arketipleri

**Karar:** İlk PvE filosu üç okunabilir rolden oluşur. Razorwind Skirmisher hızlı ve çeviktir, zincirli gülleyle kaçışı keser; Ironwake Gunship yavaş ve dayanıklıdır, uzun menzilli standart borda kullanır; Saltfang Marauder yakın mesafeye girip saçmayla mürettebat hazırlığını bozar. Prototip sahnesinde mevcut düşman şablonundan yalnızca bir kez üçlü filo kurulur. Düşmanlar pasif başlar; yalnızca oyuncu tarafından hasar verilen gemi karşılık verir ve yakındaki diğer gemiler otomatik olarak savaşa katılmaz.

**Neden:** Aynı yapay zekânın yalnızca canı artan kopyaları yerine mühimmat ve manevra kararlarını değiştiren hedef öncelikleri üretmek; ilk gerçek sefer haritasına geçmeden önce küçük bir savaş ekolojisini doğrulamak.

## 2026-09-12 — Aktif düşman hedef kartı

**Karar:** Her düşman gemisinin yalnızca adı, ekran uzayında gemi silüetinin altında gösterilir. HUD aynı anda yalnızca bir düşmanın ayrıntılı bilgi panelini gösterir. Saldırgan gemi 30 metreye kadar önceliklidir; pasif gemi yalnızca 14 metre içinde aday olur. Kartta arketip, ad, çatışma durumu, gövde, yelken ve mürettebat bulunur.

**Neden:** Alt sistem mühimmatlarının sonucunu oyuncuya görünür kılmak, hedef önceliğini desteklemek ve üç gemilik karşılaşmayı ekranı çok sayıda dünya can çubuğuyla kaplamadan okunabilir tutmak.


## 2026-09-12 — Liman ve sefer denizleri ayrı haritalardır

- Seaborn Limanı savaşsız ve bağımsız bir merkez sahnesidir.
- Ticaret, tersane, görev panosu ve ileride sosyal özellikler limanda büyütülür.
- Sefer denizleri yönsel bir ağ oluşturur: merkez denizin batı ve doğu kenarları farklı haritalara açılır.
- Harita geçişi mevcut oyuncu gemisini korur; hasar ve güvencesiz yük sahne değiştirerek sıfırlanmaz.
- Harita isimleri ve tehlike seviyeleri HUD'da gösterilir.
- İlk sahneler aynı prototip deniz temelini paylaşabilir; sanat ve içerik kimlikleri ayrı iterasyonlarda farklılaştırılır.


## 2026-09-12 — Limanın ilk görsel yerleşimi işlevlere göre ayrılır

- Oyuncu gemisi korunaklı koyun merkezinde doğar ve kuzeye, açık deniz çıkışına yönelir.
- Tersane batı rıhtımında, ticaret doğu rıhtımında, liman idaresi güney kıyısında konumlanır.
- Mendirekler ve kuzeydoğu deniz feneri güvenli merkezin siluetini oluşturur.
- Prototip geometri yerleşim doğrulaması içindir; ileride aynı kök nesneler gerçek liman assetleriyle değiştirilecektir.


## 2026-09-12 — Liman servisleri konuma bağlı yanaşma etkileşimidir

- Liman panelleri güvenli bölgede otomatik olarak her yerde açılmaz.
- Oyuncu ilgili rıhtımın yaklaşma alanına girip E ile yanaşır.
- Tersane batı istasyonunda; ikmal ve hazırlık doğu ticaret istasyonunda; kontratlar ayrıca liman idaresinde kullanılabilir.
- Bu durum ileride gemiden inme ve yürünebilir kaptan akışına dönüştürülebilir.


## 2026-09-13 — Sefer haritaları farklı risk ve faaliyet profillerine sahiptir

- Merkez Sular iki korsan ve üç Tideback ile dengeli başlangıç alanıdır; Leviathan içermez.
- Doğu Avları altı Tideback, seyrek korsan tehdidi ve Stormjaw ile av ekonomisi rotasıdır.
- Batı Sınırı beş korsan, tek Tideback ve daha yüksek çatışma yoğunluğuyla savaş rotasıdır.
- Karşılaşmalar oyuncunun giriş noktasına göre değil haritanın sabit merkezine göre üretilir.


## 2026-09-13 — Bölgesel atmosfer oynanış rotasını anlatır

- Seaborn Limanı sıcak, sakin ve açık görüşlü güvenli merkezdir.
- Merkez Sular mevcut fırtına öncesi altın saat dengesini korur.
- Doğu Avları daha berrak, turkuaz ve aydınlık bir av rotasıdır.
- Batı Sınırı daha soğuk, karanlık ve puslu bir savaş rotasıdır.
- Farklılıklar yeni köpük veya hız çizgileriyle değil; su paleti, dalga şiddeti, sis, güneş ve kontrollü post-processing ile kurulur.
- Okyanus asseti sahneler arasında değiştirilmez; her sahne kendi çalışma zamanı materyal örneğini kullanır.


## 2026-09-13 — Liman servisleri istasyona özgü arayüzlerdir

- Tersane geliştirmeleri yalnızca Tersane'ye yanaşıldığında açılır.
- Günlük görev seçimi ve aylık seyir defteri Liman İdaresi'ne yanaşıldığında görünür.
- Aynı gün içinde yalnızca seçili günlük görev ilerler; tamamlanıp limana dönüldükten sonra başka bir günlük görev seçilebilir.
- Günlük ana ödül için iki farklı günlük görevi tamamlama hedefi korunur.
- Yanaşma noktaları dünya koordinatında sabittir; oyuncunun haritaya giriş konumu istasyonları taşımaz.


## 2026-09-13 — Bölgeler farklı geliştirme kaynakları üretir

- Doğu Avları zıpkın ve av ilerlemesini besleyen Tide Yağı ile Stormjaw Pulu üretir.
- Batı Sınırı gövde ve top ilerlemesini besleyen Korsan Demiri üretir.
- Kayıp Harita Parçası iki risk rotasının nadir üst seviye kaynağıdır.
- Tersane geliştirmeleri silver ile birlikte bölgesel malzeme tüketir; böylece rotalar yalnızca farklı renkte aynı silver çiftliği olmaz.
- Malzemeler batışta kaybolmaz ve yerel prototip kaydında korunur. Güvencesiz silver yükü mevcut risk kuralını sürdürür.


## 2026-09-13 — Gemi, ekonomi ve ilerleme ürün omurgası

**Karar:** Oyuncunun ana karakteri gemidir. Gemiler limanda satın alınır, donatılır ve ileride oyuncular arasında Silver ile ticarete konu olabilir. Her gemi; top yuvası, menzil, hız, azami can, yelken yuvası, periyodik tamir, savunma, manevra, ambar, özel yuva ve güverte genişletme sınırı taşıyan veri tabanlı bir profile sahip olur.

**Karar:** Bir borda salvosunun taban gücü, geminin top yuvası sınırını aşmayan kurulu top sayısı ile seçilen top/mühimmat profilinin hasarından hesaplanır. Ekipman farkı önemlidir; menzil, açı, isabet, rota tahmini ve salvo zamanlaması karşı oyun üretmeye devam eder.

**Karar:** Silver tüm oynanış ekonomisinin parasıdır. Gold yalnızca kozmetik görünüm ve güç vermeyen premium hizmetlerde kullanılır; can, hasar, savunma, top yuvası veya zorunlu ilerleme Gold ile satılmaz.

**Karar:** Kaptan EXP ve seviyeleri büyük doğrudan istatistik artışları vermez. Yeni harita tier'ları, gemi lisansları, kontratlar, boss erişimi, ekipman uygunluğu, pazar imkânları ve skill puanları açar. İlk öneri 30 seviye ve iki seviyede bir skill puanıdır; sayılar dengeleme sırasında değişebilir.

**Karar:** Extended Deck nadir ve ticarete açık bir genişletmedir; top yuvası ve dayanıklılık ekleyebilir fakat gemi sınıfının genişletme sınırını aşamaz ve gerektiğinde manevra/hız bedeli taşır. Light of Tortuga yaklaşık yedi saniyelik görünmezlik sağlayan özel eşyadır; ateş etmek, hasar almak veya yakın tespit etkisini bozar.

**Karar:** Başlangıç içerik dili iki zıpkın ağırlığı, iki temel top sınıfı, periyodik tamir, anlık can toniği ve süreli güçlendirmelerle öğretilir. Deniz pırıltıları yaklaşık üç saniyelik, hareket veya hasarla kesilebilen toplama etkileşimidir; Silver, iyileştirme, çok nadir Gold ve nadir eşya verebilir.

**Karar:** Dünya güneyde güvenli liman, kuzeyde sakin PvE, doğuda avcılık ve batıda yüksek risk/PvP anlamını korur. Kalıcı harita tier'ı zorluğu, değişken harita kalitesi ise loot, boss ve çevresel koşulları etkiler.

**Neden:** Tek tek özellik üretmeden önce gemi edinme, ekipman kurma, sefere çıkma, riskli değer toplama, limana dönme ve yeni erişimler açma döngüsünü ortak bir ekonomik ve veri temeline bağlamak. Ayrıntılı formüller ve örnek sayılar `PROGRESSION_AND_SYSTEMS.md` içinde ayarlanabilir tasarım değerleri olarak tutulur.


## 2026-09-13 — İlk filo ve gemi pazarı kuralı

**Karar:** Satın alınan gemiler hesabın kalıcı filosuna eklenir; aktif gemi yalnızca güvenli limandaki tersaneye yanaşınca değiştirilebilir. Gemi değişimi profilin can, top yuvası, menzil, hız ve manevra sınırlarını yeniden uygular. Mevcut ekipman yeni kapasiteye sığmıyorsa fazla adet silinmez, depoda kalır.

**Karar:** İlk prototipte farklı gemi profilleri aynı blockout görselini kullanabilir. HUD ve tersane aktif mekanik profili adıyla gösterir. Üretim gemi prefabları ayrı sanat geçişinde bağlanacaktır.

**Neden:** Gemi satın almayı yalnızca bir istatistik yükseltmesi değil, oyuncunun koruduğu ve roller arasında değiştirdiği filo ilerlemesine dönüştürmek; düşük kapasiteli gemiye dönüşte kazanılmış ekipmanı yok etmemek.


## 2026-09-13 — Kaptan EXP kaynak ve erişim kuralı

**Karar:** Kaptan seviyesi 1–30 arasındadır. İlk prototipte EXP yalnızca güvencesiz yük limanda güvenceye alındığında verilir; kazanım teslim edilen Silver değerinin yüzde 50'si ve en az 10 EXP'dir. Batışta kaybedilen yük EXP üretmez.

**Karar:** Kaptan seviyesi doğrudan can veya hasar vermez. Her iki seviyede bir skill puanı, her dört seviyede bir sonraki harita tier erişimi kazanılır. Rat Sails gemi lisansı seviye 5, Dreadwake lisansı seviye 25 ister.

**Neden:** Seferin başarılı dönüşünü kalıcı ilerlemenin merkezi yapmak, limana dönme kararını güçlendirmek ve hesap seviyesini ham istatistik şişirmesi yerine yeni içerik/tercih erişimi olarak kullanmak.


## 2026-09-13 — İlk kaptan yetenek ağacı

**Karar:** İlk ağaç Topçuluk, Zıpkıncılık ve Gemi olmak üzere üç daldır. Her dalın ilk yeteneği doğrudan açılabilir; ikinci yetenek için aynı dalın ilk yeteneğinde en az bir rütbe gerekir. Puanlar yalnızca güvenli limandaki Liman İdaresi'ne yanaşıldığında harcanır ve yerel prototip kaydında korunur.

**Karar:** Top Ustalığı rütbe başına yüzde 5 top hasarı, Menzil Hesabı yüzde 4 top menzili; Zıpkın Ustalığı yüzde 8 zıpkın hasarı, Hızlı Donanım yüzde 5 daha kısa zıpkın dolumu; Güçlendirilmiş Omurga yüzde 8 azami gövde, İnce Yelkenler yüzde 4 hız ve yüzde 3 manevra verir. İlk ağacın toplam maliyeti 15 puandır ve 30 seviyelik modelde kazanılabilen 15 puanla bütünü tamamlanabilir.

**Karar:** İlk prototipte puan dağıtımı kalıcıdır. Silver karşılığı yeniden dağıtım ayrı bir ekonomi ve kullanılabilirlik iterasyonunda eklenecektir.

**Neden:** Kaptan seviyesini doğrudan otomatik güç artışına çevirmeden oyuncuya uzmanlaşma kararı vermek; seferden başarılı dönüş, seviye, liman hazırlığı ve geminin sahadaki davranışı arasında görünür bir bağ kurmak.


## 2026-09-14 — Deniz pırıltıları riskli sefer ödülüdür

**Karar:** Deniz pırıltıları yalnızca sefer haritalarında oluşur. Oyuncu 3,2 metre içine girip E tuşuna üç saniye basılı tutarak toplar. Toplama başladıktan sonra 0,75 metreden fazla hareket etmek veya hasar almak ilerlemeyi sıfırlar.

**Karar:** Ağırlıklı ödül tablosu yüzde 58 güvencesiz Silver, yüzde 20 kısmi gövde onarımı, yüzde 20 bölgesel geliştirme malzemesi ve yüzde 2 oranında 1 Gold üretir. Tam canlı gemiye gelen onarım ödülü güvencesiz Silver'a dönüşür. Denizde bulunan Silver doğrudan kalıcı cüzdana değil batışta kaybedilen ambara girer.

**Karar:** Gold oynanış gücü satın almayan kalıcı kozmetik para birimidir. İlk prototipte yalnızca çok nadir pırıltı ödülü olarak elde edilir, HUD'da Silver yanında gösterilir ve yerel kayıtta korunur.

**Neden:** Harita boyunca küçük rota sapmaları ve kısa savunmasızlık anları üretmek; keşif ödülünü mevcut riskli yük ve limana dönüş döngüsüne bağlarken premium para biriminin güç ekonomisinden ayrılığını erken doğrulamak.


## 2026-09-14 — İlk taktik sarf malzemeleri

**Karar:** İlk iki özel eşya Tortuga Tonic ve Light of Tortuga'dır. Tortuga Tonic 4 tuşuyla kullanılır, 250 gövde yeniler, tam canda tüketilemez ve 15 saniye yeniden kullanım süresine sahiptir. Light of Tortuga 5 tuşuyla kullanılır ve yedi saniye boyunca gemiyi görünmez kılar.

**Karar:** Light of Tortuga etkin olduğunda korsan gemileri ateş hazırlığını durdurur ve Stormjaw avcıyı kovalamayı bırakır. Oyuncunun top veya zıpkın ateşlemesi ya da hasar alması görünmezliği erken bozar. Süre bittiğinde düşman önceden saldırgansa takibe devam edebilir; eşya savaşı silmez, kısa bir kaçış ve yeniden konumlanma penceresi üretir.

**Karar:** Prototip başlangıç stoğu üç Tortuga Tonic ve bir Light of Tortuga'dır. Stoklar kalıcı kayda yazılır ve HUD'da gösterilir. Yeniden ikmal, Ticaret rıhtımı ekonomisiyle ayrı iterasyonda bağlanacaktır.

**Neden:** Hotbar'ı yalnızca mühimmat seçimi olmaktan çıkarıp oyuncuya acil dayanıklılık ve taktik kaçış kararı vermek; görünmezliği ücretsiz çatışma sıfırlama yerine saldırıyla bozulan sınırlı bir konumlanma aracı olarak tutmak.
