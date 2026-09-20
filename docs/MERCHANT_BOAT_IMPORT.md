# Meshy yük tüccarı

Seaborn_MerchantBoat_2K.zip içindeki Assets klasörünü proje köküne birleştir.
Import sonrasında SeabornMerchantBoatVisual prefabı otomatik oluşur.
Manuel kurulum: Seaborn > Art > Build Merchant Boat.

Model: 30.144 üçgen. BaseColor ve normal kaynakları 2K; metallic/roughness
4K kaynaklardan 2K'ya indirildi. URP metallic R, smoothness A kanalına paketlendi.
İçe aktarılan FBX dönüşü korunarak parent-space Y90 yön düzeltmesi uygulanır.
Uzunluk 6.5, su hizası bounds yüksekliğinin %12'sidir; Unity'de doğrulanmalı.
Yalnız Merchant rolü yeni prefabı kullanır; collider/devriye/kaçış/respawn değişmedi.
Eksik kaynakta geçici görsel korunur. Yeniden kurulum prefab ince ayarlarını sıfırlar.

Kod repoda; binary model/dokular LFS erişimi olmadığı için ZIP olarak teslim edildi.
Yerel import sonrası sanat klasörü, metalar ve prefab Git LFS ile commit/push edilmeli.
2K boyutları, kanal dönüşümü ve ZIP bütünlüğü doğrulandı. Unity derlemesi ve
oynanış/render testi burada çalıştırılamadı.
