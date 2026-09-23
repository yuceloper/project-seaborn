# Meshy balıkçı modeli

Seaborn_FishingBoat_2K.zip içindeki Assets klasörünü proje köküne birleştir.
Editor import tamamlandığında FishingBoatModelSetup URP materyali ve
Resources/SeabornFishingBoatVisual.prefab oluşturur. Manuel tekrar kurulum:
Seaborn > Art > Build Fishing Boat. Bu işlem prefab ince ayarlarını sıfırlar.

Model: 30.154 üçgenli Meshy sürümü, BaseColor/Normal/MetallicSmoothness 2K.
Metallic R, smoothness A=255-roughness; renk sRGB, veri haritaları lineer.
Gövde uzunluğu 5.5, Y yönünde 90 derece düzeltme, su hizası yükseklikten %12.
Pruva yönü ve su hizası Unity'de kontrol edilmeli; prefab model çocuğu üzerinden ayarlanabilir.

Runtime yalnız FishingBoat rolünde yeni prefabı seçer; kaynak materyale renk
çarpanı uygulamaz ve yeni fizik collider eklemez. Eksik kaynakta mevcut görsel
kullanılır. Devriye, kaçış ve yeniden doğuş akışları değişmedi.

Kod Git'te; FBX/dokular bu oturumda LFS erişimi olmadığı için indirilebilir
paketle teslim edildi. Import sonrası sanat klasörü, .meta dosyaları ve
üretilen prefab yerel Git LFS akışıyla commit/push edilmeli.

Doğrulama: 2K boyutları, metallic/smoothness kanal eşitliği ve ZIP bütünlüğü
kontrol edildi. Unity derlemesi, materyal render'ı, heading/waterline, batış ve
respawn görsel kontrolü henüz yapılmadı.
