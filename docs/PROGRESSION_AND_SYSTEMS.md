# Project Seaborn — İlerleme ve Sistem Omurgası

## Para birimleri

### Silver

Oynanıştan kazanılır ve oynanış sistemlerinde harcanır:

- Gemi satın alma
- Top, zıpkın ve yelken
- Mühimmat ve sarf malzemeleri
- Tamir ve ikmal
- Tersane geliştirmeleri
- Oyuncu pazarı ve işlem vergileri

### Gold

Premium para birimidir ve güç sağlamaz:

- Gemi kostümü
- Yelken görünümü
- Top ateşi ve batış efekti
- İsim plakası ve profil kozmetiği
- Liman/sosyal dekorasyon
- Güç vermeyen kolaylıklar

## Kaptan seviyesi ve EXP

Kaptan seviyesi doğrudan büyük HP veya hasar katsayısı vermez. Seviyenin görevi içerik erişimi ve uzmanlaşmadır.

Önerilen kullanım:

- Harita tier erişimi
- Gemi lisansları
- Zor kontrat ve boss erişimi
- Ekipman kullanım seviyesi
- Pazar slotları
- Skill puanları

İlk hedef:

- 30 kaptan seviyesi
- Her 2 seviyede 1 skill puanı
- Görev, av, savaş, keşif ve başarılı seferden EXP
- PvP'de aynı oyuncuyu tekrar tekrar yenerek EXP kasmayı engelleyen azalan getiri

## Gemi veri modeli

Her gemi tanımı en az şu alanlara sahip olur:

| Alan | Açıklama |
|---|---|
| Kimlik ve ad | Kalıcı katalog kimliği ve görünen ad |
| Tier | Güç ve harita erişim katmanı |
| Temel fiyat | Silver satın alma bedeli |
| Top yuvası | Takılabilecek azami top |
| Temel menzil | Borda saldırı erişimi |
| Hız katsayısı | Gövdenin hareket tabanı |
| Manevra | Dönüş ve hızlanma |
| Azami HP | Gövde dayanıklılığı |
| Savunma | Gelen hasar azaltımı |
| Yelken yuvası | Takılabilecek yelken sayısı |
| Tamir gücü | Tamir tikinde yenilenen HP |
| Tamir aralığı | Tikler arasındaki süre |
| Ambar | Güvencesiz yük kapasitesi |
| Mürettebat | Reload ve tamir verimi |
| Özel yuva | Light of Tortuga benzeri ekipman |
| Güverte limiti | Extended Deck kullanım sınırı |

### Örnek gemi basamakları

Bu değerler yön göstergesidir, nihai denge değildir.

| Rol | Top | Menzil | Hız | HP | Savunma | Yelken | Tamir |
|---|---:|---:|---:|---:|---:|---:|---|
| Başlangıç gemisi | 6 | 10 | 1,2 | 2.000 | 10 | 1 | 5 sn'de 80 |
| Rat Sails sınıfı | 9 | 12 | 1,3 | 4.000 | 15 | 2 | 5 sn'de 160 |
| Üst tier ağır gemi | 32 | 19 | 2,0 | 75.000 | 50 | 10 | 5 sn'de 1.000 |

Büyük sayı farkları nedeniyle tier koruması, harita erişimi ve PvP teşvikleri birlikte dengelenmelidir.

## Top ve salvo modeli

Salvo taban hasarı:

```text
aktif top = min(top yuvası, takılı top sayısı)
salvo hasarı = aktif top × top birim hasarı × mühimmat katsayısı
```

Ardından şu değişkenler uygulanır:

- Top seviyesi/kalitesi
- Skill bonusu
- İsabet eden mermi sayısı
- Kritik vuruş
- Hedef savunması
- Geçici buff/debuff
- Mesafe veya isabet cezası

Top rolleri:

- 6 lb: düşük hasar, hızlı reload, ekonomik
- 12 lb: yüksek hasar, daha yavaş reload
- Standart: gövde hasarı
- Zincirli: yelken, hız ve dönüş hasarı
- Saçma: mürettebat ve reload hasarı
- Zırh delici: yüksek savunmalı hedef

Toplar ilk sürümde tek tek fiziksel envanter nesnesi yerine aynı tür batarya grubu olarak yönetilebilir.

## Zıpkın modeli

Başlangıç örneği:

| Zıpkın | Hasar | Rol |
|---|---:|---|
| 2 kg | 50 | Hızlı, ekonomik tamamlama atışı |
| 4 kg | 100 | Ağır, yavaş ve pahalı atış |

147 HP başlangıç köpekbalığı bir adet 4 kg ve bir adet 2 kg zıpkınla avlanabilir. Bu kombinasyon oyuncuya fazla atış yapmama ve doğru mühimmat seçme kararını öğretir.

## Tamir ve sarf malzemeleri

Tamir:

- Belirli aralıkla HP yeniler
- Tamir malzemesi tüketir
- Ateş etmeyi engelleyebilir veya gemiyi yavaşlatabilir
- Hasar alınca kesilebilir
- Tamir gücü gemi ve skill ile artabilir

Sarf kategorileri ortak cooldown kullanır:

- Anlık gövde toniği
- Saldırı gücü
- Saldırı/reload hızı
- Hareket hızı
- Savunma
- Kaçış ve görünmezlik

Örnek: Tortuga Tonic anında 250 HP yeniler. Light of Tortuga yaklaşık 7 saniye görünmezlik sağlar; ateş etmek, hasar almak veya yakın algılama görünmezliği bozabilir.

## Extended Deck

Extended Deck nadir ve oyuncular arasında satılabilir gemi genişletmesidir.

- Top yuvası ve sınırlı HP ekler
- Etki gemi sınıfına göre ölçeklenir
- Her geminin güverte genişletme limiti vardır
- Büyük genişletmeler hız veya manevra bedeli yaratabilir
- Söküm ücretsiz olmak zorunda değildir
- Premium para ile doğrudan satılmaz

Önerilen sınırlar:

| Gemi sınıfı | Azami ek top yuvası |
|---|---:|
| Küçük | +1–2 |
| Orta | +3–4 |
| Büyük | +6–8 |

## Skill ağacı

### Cannons

- Top hasarı
- Menzil veya isabet seçimi
- Reload hızı
- Kritik ihtimali
- Salvo disiplini

### Harpoons

- Zıpkın hasarı
- Zıpkın menzili
- Reload hızı
- Nadir av bonusu
- Ağır zıpkın uzmanlığı

### Ship

- Azami HP
- Hız veya savunma seçimi
- Tamir verimi
- Kaçış
- Spawn integrity

Top yuvası skill'i sınırlı olmalıdır. Gemi sınıfı ve Extended Deck kimliğini geçersiz kılmamalıdır.

## Hotbar ve savaş kontrolü

Önerilen alt merkez düzeni:

- 1–4: mühimmat/zıpkın hızlı seçimi
- Saldır veya ateş
- Dur
- Yağmala/assault
- Top seçimi
- Zıpkın seçimi
- Special
- Tamir

Fareyle manuel borda nişanı korunur. Hotbar silah seçer; nişan ve ateş kararının yerine geçmez.

## Deniz pırıltıları

Pırıltılar rota üzerinde kısa risk/ödül kararı üretir:

- Silver
- Tamir
- Mühimmat
- Bölgesel malzeme
- Çok düşük ihtimalle Gold
- Nadir eşya veya harita parçası

Kurallar:

- Geminin üstüne gelmesi gerekir
- Toplama yaklaşık 3 saniye sürer
- Hareket veya hasar alınca kesilir
- PvP haritasında toplama oyuncuyu geçici olarak savunmasız bırakır
- Gold ve rare oranları sunucu otoriteli olur

## Harita ağı, tier ve kalite

Ana yön kimliği:

| Yön | Kimlik |
|---|---|
| Güney | Güvenli liman |
| Kuzey | Sakin PvE ve başlangıç görevleri |
| Doğu | Av, yaratık sürüsü ve leviathan |
| Batı | Korsan, yüksek PvP ve savaş ganimeti |

Uzun vadede sekiz tier harita olabilir. Tier kalıcı zorluk katmanıdır; kalite ise açılan harita instance'ının değişken özelliğidir.

Örnek kalite değerleri:

- Sakin
- Bereketli
- Tehlikeli
- Lanetli
- Efsanevi

Kalite şunları değiştirebilir:

- Av ve düşman yoğunluğu
- Nadir düşüş ihtimali
- Boss ihtimali
- Görüş ve hava
- PvP ödül katsayısı
- Pırıltı kalitesi

## Uygulama sırası

1. Veri tabanlı gemi kataloğu
2. Top ve zıpkın kataloğu
3. Ekipman yuvaları ve gerçek salvo hesabı
4. Silver liman marketi
5. Gemi satın alma/değiştirme
6. EXP ve kaptan seviyesi
7. İlk skill ağacı
8. Pırıltı toplama sistemi
9. Değişken harita kalitesi
10. Oyuncudan oyuncuya pazar

Backend otoritesi ekonomi, envanter, pazar, drop roll, EXP ve ekipman durumunun sahibidir. Unity istemcisi sunum ve oyuncu girdisinden sorumludur.
