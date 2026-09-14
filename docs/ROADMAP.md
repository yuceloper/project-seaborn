# Project Seaborn — Yol Haritası

Bu dosya yaşayan backlog’dur. Tamamlanan her feature merge’ünde güncellenir.

## Durum anahtarı

- [x] Tamamlandı
- [ ] Planlandı
- [~] Devam ediyor / prototip

## Milestone 0 — Prototip temeli

- [x] Unity 6000.6.0f1 URP proje kurulumu
- [x] Git ve Git LFS yapılandırması
- [x] Proje klasör yapısı
- [x] Windows Build Profile
- [x] Input System
- [x] Taktik izometrik takip kamerası
- [x] Gemi ileri/geri hareketi ve dümen
- [x] Kamera yakınlaştırma
- [x] Başlangıç GDD
- [x] Repo içi yaşayan dokümantasyon
- [x] Gemi, sefer, ekonomi ve savaş için bütünleşik oynanış HUD'ı
- [x] Ganimet, teslim, kayıp ve kritik hasar bildirimleri

## Milestone 1 — Savaş hissi

- [x] İskele/sancak borda bataryaları
- [x] Süre kontrollü parabolik gülle
- [x] Hasar arayüzü ve gemi canı
- [x] Batış akışı
- [x] Prototip namlu/isabet/su VFX
- [x] Kamera impulsu
- [x] Taktik düşman gemisi AI
- [x] Sağ tuşla manuel nişan
- [x] Mouse dünya hedef noktası
- [x] Otomatik borda seçimi
- [x] Menzil ve ateş yayı kontrolü
- [x] Zamanla hedefe yetişen top nişanı
- [x] Hazırlığa bağlı saçılma
- [x] Cooldown ve borda dünya göstergesi
- [x] Düşman ateş hazırlığı telegraph’ı
- [x] Aktif hedef gövde, yelken ve mürettebat HUD'ı

## Milestone 2 — Atmosfer dikey dilimi

Hedef sahne: **fırtına öncesi altın saat kıyı suları**.

### Deniz ve görüntü

- [x] URP uyumlu prototip okyanus shader’ı
- [x] İki ölçekli dalga hareketi
- [ ] İnce, seamless dokulu yüzey köpüğü
- [x] Derinlik/ufuk renk geçişi
- [x] Fresnel ve güneş yansıması
- [ ] Gemilerin çevresinde okunabilir su kontrastı
- [x] Atmosferik sis ve uzaklık solması
- [x] Altın saat yönlü ışık ayarı
- [x] Renk düzenleme ve kontrollü bloom
- [x] Uzak ada/kaya silüetleri
- [ ] Gökyüzü ve bulut yönü

### Gemi görsel kimliği

- [x] Oyuncu gemisi görsel blockout
- [x] Düşman gemisi renk ve bayrak ayrımı
- [x] Gövde, güverte, direk, yelken ve top silüeti
- [ ] Gerçek gemi modeli/üretim asset’i
- [ ] Hasar durumuna bağlı görsel değişimler

### Hareket ve çevresel geri bildirim

- [ ] Gemi hızına ve dönüşe bağlı dümen suyu
- [ ] Pruva köpüğü
- [x] Görsel gövdede hafif yalpa ve yükselme
- [x] Gülle su sıçramasının deniz paletine uyarlanması
- [ ] Rüzgâr yönü için görsel ipucu

### Ses

- [x] Dalga ve açık deniz ambience
- [x] Ahşap gıcırtısı
- [x] Rüzgâr katmanı
- [ ] Uzak gök gürültüsü
- [x] Top ateşi, isabet ve su sesi
- [ ] Ses yoğunluğunun savaş durumuna göre değişmesi

### Kabul kriterleri

- [x] Ekran görüntüsü gemi modelleri primitive olsa bile oyunun atmosfer yönünü anlatıyor.
- [x] Gemi ve nişangâh mevcut atmosfer ışığında okunuyor.
- [ ] Sabit 60 FPS hedefi prototip Windows build’inde korunuyor.
- [x] Deniz hareketi nişan noktasının okunmasını bozmuyor.
- [ ] Efektler ve sesler 15 dakikalık oturumda yorucu hale gelmiyor.

## Milestone 3 — Savaş sistemleri

- [x] Standart gülle
- [x] Zincirli gülle temel profili
- [x] Saçma mühimmatı temel profili
- [x] Gövde/yelken/mürettebat alt sistemleri
- [x] İskele/sancak bağımsız yeniden doldurma
- [x] Sonlu mühimmat stoğu ve UI olayları
- [x] Üç seviyeli gövde, top ve zıpkın ekipman istatistikleri
- [x] Razorwind, Ironwake ve Saltfang düşman arketipleri
- [x] Batırma ödülü ve süreli enkaz sandığı

## Milestone 3.5 — Deniz avcılığı dikey dilimi

- [x] Av zıpkını ve nişan sistemi
- [x] Zıpkın stoğu, menzil ve yeniden doldurma
- [x] İlk avlanabilir deniz canlısı
- [x] Canlı davranışı: gezinme, kaçış ve tehdit tepkisi
- [~] Silver ödülü; av malzemesi planlandı
- [x] Av yükünü gemiye alma
- [x] Limana dönmeden güvenceye alınmayan av yükü
- [x] Prototip liman teslim alanı ve silver güvenceye alma
- [x] Oyuncu batışında güvencesiz av yükü kaybı
- [x] Nadir fantastik leviathan karşılaşması
- [x] Yaralanınca gemiye saldıran Stormjaw davranışı
- [x] Stormjaw hücum hazırlığı, su izi ve koçbaşı geri bildirimi
- [x] Stormjaw vur-kaç saldırı döngüsü ve güvenli temas mesafesi
- [x] Batışta av yükü kaybı için hiyerarşi güvenli abonelik
- [ ] PvP bağlama zıpkını — sonraki genişleme

## Milestone 4 — Sefer prototipi

- [~] Liman → sefer → liman akışı
- [~] 15–25 dakikalık sefer zamanlaması; hızlandırılmış prototip saat hazır
- [x] PvE hedefleri ve mini boss
- [x] Kargo toplama
- [x] Çıkış/ganimeti güvenceye alma
- [x] Batış sonrası yedek gemi ve kurtarma görevi prototipi
- [x] Batış konumunda kalıcı enkaz hedefi
- [x] Ana gemiyi limanda tekrar hizmete alma
- [x] Yedek gemi hız, dönüş, can ve yük profili
- [x] Enkaz Yağmacısı isteğe bağlı savaş karşılaşması
- [x] Yağmacı için riskli silver ve mühimmat ödülü
- [x] Prototip güvenli ilerleme kalıcılığı

### Kontratlar ve liman

- [x] Sefer öncesi kontrat seçimi
- [x] Kıyı Avı, Tehlikeli Av ve Serbest Sefer
- [x] İsteğe bağlı günlük kontrat havuzu
- [x] Aylık kaptan seyir defteri
- [ ] Sunucu otoriteli günlük/aylık yenileme
- [x] Prototip liman halkasında silah kilidi ve hasar koruması
- [x] Stormjaw’ın güvenli liman sınırında takibi bırakması
- [x] Silver karşılığı gemi onarımı
- [x] Top mühimmatı ve zıpkın ikmali
- [x] Liman servisleri için UI-bağımsız API ve olaylar
- [x] Kontrat, onarım ve ikmali birleştiren liman hazırlık paneli
- [x] Silver harcatan kalıcı tersane geliştirme paneli
- [ ] Ayrı güvenli liman hub sahnesi
- [ ] Limanda gemi temelli sosyal görünürlük

## Milestone 5 — Çevrim içi temel

- [ ] Sunucu otoriteli hareket ve savaş spike’ı
- [ ] Oturum ve kimlik doğrulama
- [ ] PostgreSQL kalıcılık taslağı
- [ ] Liman sosyal alanı
- [ ] Sefer instance yaşam döngüsü
- [ ] Temel hile tehdidi modeli

## Kapsam dışı — şimdilik

- Yürünebilir kaptan
- Büyük açık kesintisiz dünya
- Mobil build
- Lonca savaşı
- Sezon sistemi
- Üretim ekonomisinin tamamı
- Gerçek para mağazası


### Multi-map expedition foundation (2026-09-12)

- [x] Güvenli limanı bağımsız merkez sahnesine ayırma
- [x] Merkez deniz, batı sınırı ve doğu avları için ayrı sahne omurgası
- [x] Harita kenarı geçitleri ve yönsel bağlantılar
- [x] Harita geçişinde oyuncu gemisi, hasar ve güvencesiz yükü koruma
- [x] Mevcut harita ve tehlike seviyesini HUD'da gösterme
- [x] Yönleri, çıkışları ve gemi konumunu gösteren taktik mini harita
- [x] Düşman AI, loot ve liman teslim sistemlerini haritalar arasında yeniden kurma
- [ ] Her haritaya özgü çevre sanatı, ses katmanı ve içerik tabloları
- [ ] Limanı ticaret, tersane, görev ve sosyal merkez olarak görsel üretime alma


### Harbor identity pass (2026-09-12)

- [x] Güvenli koy ve yönlü açık deniz koridoru
- [x] Batı tersane rıhtımı ve prototip vinç
- [x] Doğu ticaret rıhtımı ve depo alanı
- [x] Liman idaresi, mendirekler ve deniz feneri
- [x] Tersane, ticaret ve açık deniz dünya etiketleri
- [ ] Prototip geometrileri üretim kalitesinde liman assetleriyle değiştirme
- [ ] Yürünebilir liman ve görünür kaptan katmanı


### Harbor docking interactions (2026-09-12)

- [x] Tersane, ticaret/ikmal ve liman idaresi yanaşma alanları
- [x] Yakınlık tabanlı dünya işareti ve E etkileşimi
- [x] Tersane panelini batı rıhtımına bağlama
- [x] Hazırlık ve kontrat panelini doğu/güney istasyonlarına bağlama
- [x] Kısa yanaşma hizalama animasyonu ve motor kilidi
- [x] Dinamik baş/kıç halat görselleri
- [ ] Rıhtım sesleri ve suya temas geri bildirimi


### Expedition map content profiles (2026-09-13)

- [x] Merkez Sular için dengeli başlangıç karşılaşmaları
- [x] Doğu Avları için yoğun Tideback sürüleri ve Stormjaw
- [x] Batı Sınırı için beş gemilik yüksek riskli korsan filosu
- [x] Spawn noktalarını harita merkezine sabitleme
- [x] Bölgesel ödül tabloları ve nadir ganimetler
- [x] Haritaya özgü sis, ışık, su rengi ve post-process kimliği
- [ ] Haritaya özgü çevresel ses kimliği


### Regional atmosphere profiles (2026-09-13)

- [x] Seaborn Limanı için sıcak, güvenli ve sakin kıyı tonu
- [x] Merkez Sular için dengeli altın saat profili
- [x] Doğu Avları için berrak, turkuaz ve daha sakin deniz
- [x] Batı Sınırı için soğuk, karanlık ve daha yoğun uzaklık sisi
- [x] Sahne geçişlerinde atmosfer profilini yeniden uygulama
- [x] Paylaşılan okyanus assetini değiştirmeden çalışma zamanı materyal kopyası
- [ ] Bölgesel ambiyans döngüleri ve uzamsal çevre sesleri


### Harbor station UI repair (2026-09-13)

- [x] Tersane panelini ekipman sistemi kurulduktan sonra bağlama
- [x] Eksik UI bağlarını çalışma anında yeniden çözme
- [x] Yanaşma noktalarını oyuncu girişinden bağımsız sabit harita koordinatlarına alma
- [x] Liman İdaresi için günlük görev seçim paneli
- [x] Aylık seyir defteri ilerlemesini Liman İdaresi'nde gösterme


### Regional loot economy (2026-09-13)

- [x] Doğu avları için Tide Yağı ve Stormjaw Pulu
- [x] Batı korsanları için Korsan Demiri
- [x] İki risk rotasında nadir Kayıp Harita Parçası
- [x] Bölgeye göre enkaz silver ve mühimmat miktarı
- [x] Malzemeleri tersane geliştirme maliyetlerine bağlama
- [x] Bölgesel malzemeleri yerel prototip kaydında koruma
- [ ] Malzemeler için üretim kalitesinde ikon ve envanter ekranı
- [ ] Sunucu otoriteli düşüş tablosu ve ekonomi telemetrisi


### Product backbone plan (2026-09-13)

#### Decisions locked

- [x] Ship is the player's primary character and progression object
- [x] Silver is the gameplay economy; Gold is cosmetic/premium only
- [x] Captain EXP unlocks access and skill points instead of granting large direct stat bonuses
- [x] Ship data model includes cannon, sail, range, speed, durability, repair, defense, cargo and extension limits
- [x] Broadside damage scales with installed cannons up to the ship's cannon-slot limit
- [x] Cannon and harpoon ammunition have distinct tactical weight classes
- [x] Extended Deck is rare, tradeable and limited by ship class
- [x] Directional map network, map tiers and variable map quality define world progression
- [x] Hotbar, contextual actions and collectible sea sparkles have a documented interaction direction

#### Implementation sequence

- [x] Create data-driven ship catalogue and starter ship definitions
- [x] Create cannon, ammunition, harpoon, sail and consumable catalogues
- [x] Replace prototype broadside constants with equipped-cannon salvo calculation
- [x] Add equipment inventory, ship loadout validation and shipyard equipment market
- [x] Add Silver-based harbor ship market and ship switching
- [x] Add captain EXP, levels, unlock gates and skill-point earning
- [x] Add first cannon, harpoon and ship skill branches
- [x] Add timed repair, tonic and special-item cooldown categories
- [x] Add interruptible sea-sparkle collection and weighted rewards
- [ ] Add map-tier and map-quality modifiers
- [ ] Add player-to-player Silver market after server authority exists


### Shipyard equipment market (2026-09-13)

- [x] Silver ile adet bazlı top satın alma
- [x] Sahip olunan top sayısını gemi yuvasıyla doğrulama
- [x] Başlangıç ve Rat Sails yelken envanteri
- [x] Tersaneye yanaşma şartlı satın alma ve ekipman takma
- [x] Top, yelken ve loadout'un yerel kayıtta korunması
- [x] Loadout için bağımsız tersane paneli
- [x] Kurulu top hasarı/dolumu ve yelken hareket çarpanlarını uygulama
- [ ] Gemi satın alma, gemi envanteri ve aktif gemi değiştirme
- [ ] Karma top bataryası ve tekil ekipman dayanıklılığı


### Harbor fleet market (2026-09-13)

- [x] Başlangıç gemisini kalıcı filoya ekleme
- [x] Rat Sails ve Dreadwake için Silver satın alma
- [x] Yalnızca tersanede aktif gemi değiştirme
- [x] Aktif gemide can, top yuvası, menzil, hız ve manevra profilini yeniden uygulama
- [x] Gemi sahipliği ve aktif gemiyi yerel kayıtta koruma
- [x] Gemi pazarı paneli ve HUD'da aktif gemi adı
- [ ] Her gemi profiline özgü görsel prefab
- [ ] Gemiler arasında ayrı hasar ve onarım durumu


### Captain progression foundation (2026-09-13)

- [x] 30 seviyeli kaptan EXP modeli
- [x] Güvenceye alınan sefer değerinden EXP kazanımı
- [x] Batışta kaybedilen yük için EXP vermeme
- [x] Her iki seviyede bir skill puanı kazanımı
- [x] Dört seviyede bir harita tier erişimi
- [x] Rat Sails için seviye 5 gemi lisansı
- [x] Dreadwake için seviye 25 gemi lisansı
- [x] HUD seviye bilgisi ve seviye atlama bildirimi
- [x] Kaptan EXP'sini yerel kayıtta koruma
- [x] Harita geçitlerinde tier erişimini uygulama
- [x] Skill puanı harcama ve ilk skill ağacı


### Map tier access gates (2026-09-13)

- [x] Liman ve Merkez Sular tier 1
- [x] Doğu Avları tier 2 / Kaptan seviye 5
- [x] Batı Sınırı tier 3 / Kaptan seviye 9
- [x] Kilitli sınırda gemiyi güvenli biçimde harita içine geri alma
- [x] Kilitli geçitte hareket hızını sıfırlama
- [x] HUD kilit bildirimi ve mini harita seviye etiketleri
- [ ] Tier 4–8 harita sahneleri ve bağlantı ağı


### İlk kaptan yetenek ağacı

- [x] Yetenek puanlarını yalnızca Liman İdaresi'nde harcama
- [x] Top Ustalığı ve Menzil Hesabı dalı
- [x] Zıpkın Ustalığı ve Hızlı Donanım dalı
- [x] Güçlendirilmiş Omurga ve İnce Yelkenler dalı
- [x] Yeteneklerin gerçek savaş, av, gövde ve hareket değerlerine uygulanması
- [x] Yetenek seviyelerini yerel prototip kaydında koruma
- [ ] Silver karşılığı yetenek sıfırlama / yeniden dağıtma
- [ ] İkinci kademe dallanma, kritik isabet ve özel yetenekler


### Deniz pırıltısı ödül döngüsü

- [x] Sefer haritalarında bölgesel pırıltı üretimi
- [x] E tuşuna üç saniye basılı tutarak toplama
- [x] Hareket veya hasar alındığında toplamanın kesilmesi
- [x] Ağırlıklı güvencesiz Silver, gövde onarımı ve bölgesel malzeme ödülleri
- [x] Yüzde 2 olasılıkla 1 kalıcı Gold
- [x] Gold bakiyesinin HUD ve yerel kayda bağlanması
- [ ] Pırıltı görselini nihai okyanus shader/VFX diliyle yenileme
- [ ] Sunucu otoriteli üretim, ödül tohumu ve hile doğrulaması


### İlk taktik sarf malzemeleri

- [x] 4 tuşunda Tortuga Tonic ile anlık 250 gövde onarımı
- [x] Tonic için 15 saniyelik yeniden kullanım süresi
- [x] 5 tuşunda Light of Tortuga ile 7 saniyelik görünmezlik
- [x] Top, zıpkın veya alınan hasarın görünmezliği bozması
- [x] Korsan ve Stormjaw hedeflemesinin görünmezlik sırasında durması
- [x] Stokların HUD ve yerel kayda bağlanması
- [x] Ticaret rıhtımında sarf malzemesi satın alma ve ikmal
- [x] Süreli saldırı, savunma, hız ve doldurma güçlendirmeleri


### Tortuga tedarikçisi

- [x] Ticaret iskelesine özel sarf malzemesi mağazası
- [x] 90 Silver karşılığı tek Tortuga Tonic
- [x] 240 Silver karşılığı üçlü Tonic sandığı
- [x] 650 Silver karşılığı Light of Tortuga
- [x] Bakiye yetersizliğini ve satın alma sonucunu panelde gösterme
- [x] Satın alınan stokları mevcut kalıcı kayıt akışına bağlama
- [ ] Fiyatları 15–25 dakikalık gerçek sefer verisiyle dengeleme
- [ ] Oyuncular arası sarf malzemesi ticareti ve pazar vergisi


### Süreli sefer güçlendirmeleri

- [x] 6 tuşunda Corsair Rum: top hasarı ve doldurma
- [x] 7 tuşunda Gale Elixir: hız ve manevra
- [x] 8 tuşunda Ironbark Brew: alınan hasar azaltma
- [x] 90 saniyelik hızlandırılmış prototip süresi
- [x] Aynı etkinin birikmemesi, yeniden kullanımda sürenin yenilenmesi
- [x] Aktif sürelerin ve stokların HUD'da gösterilmesi
- [x] Yeni ürünlerin Tortuga tedarikçisine bağlanması
- [x] Stokların yerel kayıtta korunması; aktif sürelerin oturumluk kalması
- [ ] Nihai 10 dakikalık süre ve fiyatların sefer telemetrisiyle dengelenmesi


### Saha tamiri

- [x] R tuşuyla açılıp kapanan saha tamir modu
- [x] Aktif geminin repairAmount ve repairInterval değerlerini kullanma
- [x] Tamir sırasında hızın yüzde 45, dönüşün yüzde 70 olması
- [x] Top ve zıpkın ateşinin tamiri kesmesi
- [x] Alınan hasarın tamiri kesip sekiz saniye kilitlemesi
- [x] Tam can, batış ve liman durumlarının güvenli kontrolü
- [x] HUD'da tamir döngüsü, miktarı ve kilit süresi
- [ ] Tamir verimliliği ekipmanları ve kaptan yetenekleri
- [ ] Üretim ölçeğinde tahta, bez ve mürettebat kaynağı tüketimi
