# NPC gemi rolleri ve görsel geçiş

Her deniz haritasında toplam 5 gemi: 1 balıkçı, 1 tüccar, 3 korsan.
Normal av 20 ve Stormjaw 1; yeniden doğuş süreleri değişmedi.

| Tür | Görünüş | Davranış | Merkez enkaz Silver |
|---|---|---|---|
| Balıkçı | Mavi gövde, alçak kabin, tente, ağ bomları ve kasalar | Devriye, hasarda kaçış; silahsız | 20 |
| Tüccar | Krem çift yelken, yük kasaları, kıç kabini | Devriye, hasarda kaçış; silahsız | 100 |
| Avcı korsan | Tek direk, turkuaz yelken | Mevcut yaklaş/pozisyon al/ateş düzeni | 80 |
| Yağmacı | Tek direk, kırmızı yelken | Mevcut savaş düzeni | 110 |
| Topçu | Çift direk, gri yelken, yükseltilmiş güverte | Mevcut savaş düzeni | 180 |

Batıda korsan dağılımı avcı + iki topçu; diğer haritalarda avcı + yağmacı + topçu.
Siviller ateş etmez, borda sayısı sıfırdır ve enkazları mühimmat içermez.
Bölge ödül çarpanları sürer; suç/itibar yaptırımları henüz bu değişikliğe dahil değil.

NPC gövdesi sivrilen mesh ile oluşturulur; güverte, arma, ağ ve yükler prosedüreldir.
Yeni dokulu FBX eklenmedi. Oyuncunun üretim modeli korunur.
NPC seçimi artık nesne adındaki Enemy metnine değil EnemyShipController bileşenine
bakar. Awake sonrasında rol atanırsa görsel bir kez yeniden kurulur. Görsel
parçaların collider'ları kapalıdır; fizik kökü yeniden şekillendirilmez.

## Unity kontrolü
- Her deniz haritasında 5 gemi, 20 normal av ve 1 Stormjaw olduğunu doğrula.
- Uzak kamera açısında balıkçı/tüccar/korsan siluetlerini kontrol et.
- Oyuncu modelinin aynı kaldığını, yeniden adlandırılmış NPC'nin oyuncu
  modeline dönüşmediğini kontrol et.
- Sivile ateş et: kaçmalı, top veya telegraph üretmemeli; sınırda kalmalı.
- Her korsanın bordasında 3 namlu olduğunu ve atışların görünür namlularla
  hizalı olduğunu kontrol et.
- Sivil ve korsanı batırıp 90 saniye bekle: aynı rol ve görselle dönmeli;
  ikinci batış tekrar tek enkaz vermeli.
- Liman-deniz geçişlerini, Console'u ve artan görsel parça sayısının FPS
  etkisini kontrol et.

Kaynak bağlantıları incelendi. Unity ve C# derleyicisi bulunmadığından görsel,
derleme ve Play Mode doğrulaması yapılmadı.
