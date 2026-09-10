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
