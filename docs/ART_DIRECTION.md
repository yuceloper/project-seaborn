# Project Seaborn — Atmosfer Sanat Yönü

## Sahne hedefi

**Fırtına öncesi altın saat kıyı suları**

Sahne huzurlu bir kartpostal değil; yaklaşan tehlikeden hemen önceki sıcak ve ağır havayı taşımalıdır.

## Ana palet

| Rol | Yaklaşık renk | Kullanım |
|---|---|---|
| Derin deniz | #051721 | Ana su kütlesi |
| Sığ/tepe suyu | #093D47 | Dalga tepeleri |
| Uzak su/sis | #336B75 | Fresnel ve ufuk |
| Güneş | #FFB075 | Ana yönlü ışık |
| Güneş parlaması | #FF9442 | Deniz üzerindeki vurgu |
| Uzak sis | #3D525E | Silüet ayrımı |
| Tehlike vurgusu | #FF3326 | Düşman telegraph ve kritik bilgi |

## Okunabilirlik kuralları

- Deniz, gemi gövdelerinden daha düşük orta ton kontrastında kalır.
- Nişangâh rengi hiçbir zaman doğrudan deniz tepe rengiyle birleşmez.
- Bloom yalnızca güneş parlaması, ateş ve güçlü VFX üzerinde hissedilir.
- Bloom eşiği normal gemi/yelken renklerini yakalamaz; beyaz silüet taşmasına izin verilmez.
- Renk düzenleme denizin petrol mavisini korur, doygunluğu hafifçe bastırır.
- Vignette yalnızca kadrajı toplar; ekran kenarlarında belirgin siyah halka oluşturmaz.
- Sis yakın savaş alanını kapatmaz; öncelikle uzak silüetleri katmanlar.
- Dalga yüksekliği top hedef noktasını görsel olarak saklamaz.
- Fantastik renkler ilk kıyı bölgesinde baskın değildir.

## Gemi silüeti

- Uzak kamerada önce pruva yönü ve borda genişliği okunmalıdır.
- Oyuncu ve düşman ayrımı yalnızca isim/UI ile değil yelken ve bayrak rengiyle yapılır.
- İlk blockout gövde, güverte, kıç kamarası, direk, iki yelken ve altı top içerir.
- Görsel kök fizik gövdesinden ayrıdır; collider ve namlu noktaları korunur.
- Hafif yükselme, pitch ve roll yalnızca görsel köke uygulanır; savaş simülasyonu düzlemde kalır.
- Gemilerin salınım fazları farklıdır; filo aynı ritimde mekanik olarak sallanmaz.
- Nihai model gelene kadar gerçekçi küçük detay yerine güçlü ana şekiller tercih edilir.

## Teknik yaklaşım

- URP özel shader
- Dünya koordinatında iki sinüs dalga katmanı
- Shader tabanlı normal yaklaşımı
- Fresnel ile ufuk rengi
- Ana ışığa bağlı sıcak specular parlaması
- Unity lineer sis
- Düz fizik/nişan düzlemi
- Deniz paletine bağlı iki katmanlı gülle su sıçraması
- Collider içermeyen, sis içinde düşük kontrastta kalan kıyı/kaya silüetleri

Bu shader üretim okyanusu değildir. İlk hedef, renk ve hareket yönünü doğrulamaktır.


## Üretim kuzey yıldızı

Project Seaborn, gerçekçi temeli olan ve bölgesel kontrollü fantastik unsurlarla zenginleşen, uzaktan okunabilir taktik bir deniz oyunudur.

Ana üç kelime: **Denizci — Tehlikeli — Masalsı**

Hedef foto-gerçekçi korsan simülasyonu değildir. Büyük formlar hafif stilize edilir; malzemeler ve oranlar inandırıcı kalır. Her karar gerçek izometrik oynanış kamerasında değerlendirilir.

## Asset kabul standardı

Bir gemi veya liman asset’i projeye alınmadan önce aşağıdaki kriterlerde 0–2 puanlanır.

| Kriter | 0 | 1 | 2 |
|---|---|---|---|
| Kamera okunabilirliği | Kayboluyor | Düzenleme gerekir | Doğrudan okunuyor |
| Sanat dili | Uyumsuz | Dönüştürülebilir | Seaborn’a uyuyor |
| URP ve teknik yapı | Sorunlu | Düzeltilebilir | Hazır |
| Modülerlik | Tek parça | Kısmen ayrık | İyi ayrılmış |
| Performans | Ağır | Optimizasyon ister | Uygun |
| Lisans | Belirsiz | Kontrol gerekli | Açık ve uygun |

Minimum kabul **9/12**’dir. Kamera okunabilirliği veya lisans kriterinden 0 alan asset toplam puandan bağımsız reddedilir.

## Gemi teknik standardı

- İlk gerçek üretim gemisi Seaborn Sloop olacaktır.
- Unity ileri yönü +Z, pivot su çizgisine ve geometrik merkeze yakın olmalıdır.
- Gövde, yelken, direk ve mümkünse toplar ayrılmalıdır.
- Top yuvaları temiz hardpoint noktalarına sahip olmalıdır.
- Yelken ve gövde hasar varyantlarına uygun materyal/mesh düzeni gerekir.
- Renk varyantı için maske veya bağımsız materyal bölgeleri gerekir.
- LOD üretimine uygun topology veya hazır LOD zinciri beklenir.
- Baş/kıç yönü uzak oyun kamerasında ilk bakışta anlaşılmalıdır.

Sloop onaylanmadan Rat Sails ve Dreadwake için geniş üretime geçilmez. Ölçek, malzeme, hardpoint ve hasar standardı ilk gemide sabitlenir.

## Güvenli liman üretim sırası

1. Modüler iskele, kazık ve bağlama seti
2. Tersane ana yapısı, kızak ve vinç
3. Ticaret ambarı ve kumaş tenteler
4. Liman idaresi ve fener
5. Kasa, varil, halat, kereste ve yedek direk prop seti
6. Arka plan tekneleri ve yaşam animasyonları

Tersane, Ticaret ve Liman İdaresi renkli halka veya yazı olmadan da silüet, ışık ve çevresel prop’larla ayrılmalıdır.

## Kontrollü fantastik yoğunluk

| Bölge | Yoğunluk | Örnek |
|---|---:|---|
| Güvenli Liman | Çok düşük | Denizci folkloru ve semboller |
| Merkez Sular | Düşük | Tideback ve sıra dışı hava |
| Doğu/Batı | Orta | Biyolüminesans veya yozlaşmış enkaz |
| Yüksek tier | Yüksek ama odaklı | Leviathan ve bölgesel doğa olayı |

Her bölge tek bir ana olağanüstü fikre sahip olur; fantastik motifler aynı sahnede gelişigüzel yığılmaz.

## İlk görsel dikey dilim kabulü

Tek bir gerçek oynanış karesinde şunlar birlikte çalışmalıdır:

- Gerçek Sloop modeli
- Okunabilir koyu deniz ve kontrollü köpük
- Bir düşman gemisi
- Bir Tideback
- Tersane veya uzak kıyı silüeti
- Pruva köpüğü, top ateşi ve su isabeti
- Kompakt deniz HUD’ı

Bu kare onaylanmadan geniş asset alımına ve final UI üretimine geçilmez.
