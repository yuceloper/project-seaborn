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
- [ ] Gövde/yelken/mürettebat alt sistemleri
- [x] İskele/sancak bağımsız yeniden doldurma
- [x] Sonlu mühimmat stoğu ve UI olayları
- [ ] Ekipman istatistikleri
- [ ] AI zorluk profilleri
- [ ] Batırma ödülü ve enkaz

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
- [ ] Basit kalıcılık

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
