# Seaborn Sloop — İlk Gerçek Model Üretim Paketi

## Durum

- Ana üç çeyrek referans: oluşturuldu, ilk yön onayı bekliyor
- 3B üretim yöntemi: Meshy 7 Image-to-3D Multi-View
- Nihai temizlik ve modüler ayırma: Blender
- Oyun entegrasyonu: Unity URP, mevcut `PrototypeModularShipAssembler`
- Bu gemi onaylanmadan Rat Sails veya Dreadwake üretimine geçilmez

## Model kimliği

**Ad:** Seaborn Sloop  
**Rol:** Çevik başlangıç savaş/av gemisi  
**Görsel dil:** Orta gerçekçilik, uzaktan okunabilir büyük formlar, koyu meşe, sıcak güverte, kırık beyaz yamalı yelken, sınırlı turkuaz oyuncu vurgusu  
**Fantastik yoğunluk:** Çok düşük; doğaüstü parıltı veya büyü mekanizması yok

## Ana referans değerlendirmesi

Güçlü taraflar:

- Pruva, orta top güvertesi ve yüksek kıç okunuyor.
- İzometrik kamerada ayırt edilecek güçlü gövde silüeti var.
- Koyu meşe, keten ve turkuaz paleti Seaborn sanat yönüne uyuyor.
- Güverte, yelken ve top bölgeleri ayrı okunuyor.
- Avcı gemisi kimliğini destekleyen pruva donanımı bulunuyor.

Temizlenecek noktalar:

- Nihai Sloop toplam altı top kapasitesine göre üç iskele ve üç sancak hardpoint taşımalı.
- Referanstaki görünür top dağılımı doğrudan kopyalanmamalı.
- Halat/arma yoğunluğu oyun kamerası için azaltılmalı.
- İnce korkuluk ve merdivenler optimize edilmeli.
- Pruva zıpkın yuvası silahın kendisinden ayrılmalı.
- Turkuaz kaplama boya maskesi olarak ayrılmalı.
- Tek parça AI mesh’i nihai modüler sözleşme değildir.

## Meshy üretim akışı

1. Meshy’de **Image to 3D** aç.
2. Model olarak Meshy 7 seç.
3. Oluşturulan ana üç çeyrek referansı yükle.
4. **Generate Multi-View** ile sol, arka ve sağ tamamlayıcı görünümleri üret.
5. Görünümlerde top sayısı, direk, bowsprit ve kıç kamarası tutarlı değilse 3B üretime geçmeden yeniden üret.
6. İlk üretimi dokusuz/preview kaliteyle değerlendir.
7. Silüet onaylanırsa dokulu sürümü üret.
8. Mümkünse GLB ve FBX dışa aktar; Blender ana çalışma dosyası oluştur.
9. AI-generated LOD veya remesh sonucu otomatik olarak kabul edilmez.

## Meshy ana promptu

```text
A game-ready small age-of-sail combat and hunting vessel named Seaborn Sloop, believable historical construction with restrained fantasy character, medium-realistic proportions with simplified large forms readable from a distant isometric camera. Dark weathered oak hull, warm timber deck, patched warm-ivory canvas sails, blackened iron cannons, subtle oxidized brass, restrained teal paint accents as a separate material region. Strong pointed forecastle and bow, straight-sided modular midship gun-deck zone with clean section boundaries, raised quarterdeck and stern cabin. One main mast and one smaller fore rig, practical simplified rigging, bowsprit, a separate reinforced harpoon hardpoint near the bow. Exactly three cannon hardpoint openings per side, six total. Symmetrical where appropriate. Full ship, coherent construction, clean silhouette, game asset, Unity URP.

Avoid: crew, ocean, waves, dock, base, text, logo, skull motifs, neon magic, steampunk machinery, modern engine, oversized fantasy sails, tangled dense ropes, tiny ornaments, toy proportions, plastic low-poly look, damaged wreck, baked background, permanently attached cannon projectiles.
```

## Blender temizlik ve modüler ayırma

### Dosya ve yön

- Ana dosya: `SeabornSloop_Master.blend`
- Unity ileri yönü: +Z
- Yukarı yön: +Y
- Pivot: su çizgisinde, geminin boyuna merkezine yakın
- Uygulanmış scale ve rotation
- Gerçek ölçek referansı: ilk entegrasyonda mevcut collider ile eşleştirilecek

### Mesh bölümleri

Zorunlu ayrı nesneler:

- `Sloop_Forecastle_A`
- `Sloop_Midship_GunDeck_A`
- `Sloop_Stern_Quarterdeck_A`
- `Sloop_Mast_Main_A`
- `Sloop_Mast_Fore_A`
- `Sloop_Sails_PatchedCanvas_A`
- `Sloop_Rigging_Gameplay_A`
- `Sloop_HarpoonMount_A`
- `Sloop_Cannon_6lb_A`

Bölüm sınırlarında:

- Güverte yüksekliği eşleşmeli.
- Su çizgisi kesintisiz olmalı.
- Bordo eğrisi ani kırılmamalı.
- Küpeşte bağlantısı kapanmalı.
- Soket düzlemlerinde üst üste binen yüzey olmamalı.
- Normaller ve UV dikişleri kontrol edilmeli.

### Soketler

- `Socket_Fore_To_Mid`
- `Socket_Mid_To_Fore`
- `Socket_Mid_To_Mid`
- `Socket_Mid_To_Stern`
- `Socket_Stern_To_Mid`
- `Socket_Mast_Main`
- `Socket_Mast_Fore`
- `Socket_Harpoon`
- `Socket_Wake_Bow`
- `Socket_Wake_Stern`

### Top hardpoint’leri

- `HP_Cannon_Port_01..03`
- `HP_Cannon_Starboard_01..03`
- Her hardpoint’in +X veya yerel ileri ekseni namlu yönünü tutarlı göstermeli.
- Her top altında ayrı `Muzzle` noktası olmalı.
- Top mesh’i gövdeye birleştirilmemeli.
- Boş yuva kapağı ayrı mesh veya varyant olmalı.

## Materyal standardı

En fazla ilk sürüm hedefi:

1. Hull Wood
2. Deck Wood
3. Canvas
4. Metal
5. Teal Paint/Accent

- URP Lit uyumlu PBR haritaları
- 2K ana texture seti; ilk testte 4K zorunlu değil
- Base Color, Normal, Metallic/Smoothness ve AO
- Ahşap mikro detayı uzak kamerada titreşmeyecek ölçekte
- Turkuaz vurgu ayrı materyal veya maskeli renk parametresi
- Yelken çift taraflı görünmeli; gölge davranışı ayrıca test edilmeli

## Geometri bütçesi

İlk hedef, görünüş doğrulandıktan sonra Blender’da kesinleştirilir:

- LOD0: yaklaşık 35–70 bin üçgen
- LOD1: LOD0’ın %45–55’i
- LOD2: LOD0’ın %15–25’i
- Uzak impostor bu aşamada zorunlu değil
- İnce halatlar mesh yerine sınırlı eğri/kalın şerit veya optimize geometri olabilir
- Görünmeyen iç hacimler ve alt güverte ayrıntıları temizlenir

## Unity teslim yapısı

```text
Assets/_Project/Art/Ships/SeabornSloop/
  Models/
  Materials/
  Textures/
  Prefabs/
  Source/
```

Ana prefab:

`PF_SeabornSloop_Modular.prefab`

Alt prefablar:

- `PF_Sloop_Forecastle_A`
- `PF_Sloop_Midship_GunDeck_A`
- `PF_Sloop_Stern_A`
- `PF_Sloop_MastAndSails_A`
- `PF_Cannon_6lb_A`

## Unity kabul testi

- Gerçek oyun kamerasında baş/kıç ilk bakışta okunur.
- Tek orta gövdeyle Sloop oranı doğal görünür.
- İkinci orta gövde eklendiğinde bağlantı bozulmaz.
- Üç iskele ve üç sancak topu doğru hardpoint’e oturur.
- Gülle doğru Muzzle noktasından çıkar.
- Yelken/gövde hasar rengi yalnızca ilgili materyali etkiler.
- Light of Tortuga bütün modülleri birlikte saydamlaştırır.
- Görsel buoyancy yalnızca visual root’u hareket ettirir.
- Collider fizik kökünde kararlı kalır.
- 1080p oyun kamerasında halat ve korkuluklarda titreşim kabul edilebilir düzeydedir.

## İlk onay kapısı

Meshy’den çıkan ilk model doğrudan projeye kalıcı asset olarak eklenmez. Önce şu üç görüntü paylaşılır:

1. Meshy 3B önizlemesinde üç çeyrek görünüm
2. Tam yan görünüm
3. Üstten güverte görünümü

Silüet ve oran onayından sonra Blender temizliği ve modüler ayırma başlar.
