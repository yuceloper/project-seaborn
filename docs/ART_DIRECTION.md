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
- Sis yakın savaş alanını kapatmaz; öncelikle uzak silüetleri katmanlar.
- Dalga yüksekliği top hedef noktasını görsel olarak saklamaz.
- Fantastik renkler ilk kıyı bölgesinde baskın değildir.

## Gemi silüeti

- Uzak kamerada önce pruva yönü ve borda genişliği okunmalıdır.
- Oyuncu ve düşman ayrımı yalnızca isim/UI ile değil yelken ve bayrak rengiyle yapılır.
- İlk blockout gövde, güverte, kıç kamarası, direk, iki yelken ve altı top içerir.
- Görsel kök fizik gövdesinden ayrıdır; collider ve namlu noktaları korunur.
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
