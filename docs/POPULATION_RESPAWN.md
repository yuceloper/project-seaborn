# Harita nüfusu ve yeniden doğuş

- Düşman gemisi: batıştan 90 oyun saniyesi sonra, aynı filo yuvası.
- Tideback: hasat tamamlanmasından 60 oyun saniyesi sonra, aynı tür.
- Stormjaw: hasattan 300 oyun saniyesi sonra.
- Süreler Time.time ölçeğindedir; oyun duraklatılınca ilerlemez.
- Harita başına başlangıç filo/av sayıları korunur. Tüm avlar öldüğünde bootstrap
  bütün sürüyü hemen yeniden üretmez. Her hasat yalnız bir yeni av üretir.
- Yeniden doğuş noktaları yuva çevresinde 30 birim içinde aranır. Oyuncudan
  en az 30 birim uzaklık, X/Z ±70 ve collider engel kontrolü uygulanır.
  Güvenli nokta yoksa 5 saniye sonra tekrar denenir.
- Gemi nesnesi yeniden kullanılır: can, yelken, mürettebat, mühimmat, dolum,
  fizik, devriye ve enkaz hakkı yenilenir. Eski enkaz kendi 120 saniyelik
  ömrünü tamamlar; yeni hayat ayrıca bir kez enkaz bırakabilir.
- Yönetici sahneye aittir. Harita değişince bekleyen işler iptal olur;
  yeniden giriş mevcut başlangıç nüfusunu kurar. Haritalar arası/offline
  kalıcı ölüm zamanları bu sürüme dahil değildir.
- Bu mesafe tabanlı doğuş korumasıdır; kamera görüş alanı hesabı değildir.

## Unity kabul denemesi

1. Merkezde bir gemi batır. 90 saniyeden önce doğmamalı; sonra uygun noktada
   tam can ve alt sistemlerle devriye yapmalı. Tekrar batır; tekrar enkaz ve
   yalnız bir yeniden doğuş olmalı. En az üç hayat boyunca sayıyı kontrol et.
2. Gemiyi yelken ve mürettebatı hasarlı halde batır. Yeni hayatta hareket,
   mühimmat, iki borda ve çarpışmalar çalışmalı; eski çatışmayı sürdürmemeli.
3. Üç merkez avını bitir. Anında toplu üretim olmamalı. Her biri kendi
   60 saniyesinden sonra dönmeli. Doğuda Stormjaw için 300 saniyeyi doğrula.
4. Doğuş çevresinde bekle/engel bırak. Güvenli yer yoksa ertelemeli;
   uzaklaşınca bir adet üretmeli. Harita sınırları aşılmamalı.
5. Bekleyen süre varken limana geç; limanda düşman/av oluşmamalı. Geri dön,
   başlangıç nüfusu kurulmalı; eski zamanlayıcı ek düşman üretmemeli.
6. Duraklat ve devam et; süreler duraklamada ilerlememeli. Console'da
   eksik referans, kinematic velocity veya coroutine hatası olmamalı.

Kaynak bağlantıları kontrol edildi. Unity/derleyici burada bulunmadığı için
derleme ve yukarıdaki Play Mode denemeleri henüz çalıştırılmadı.
