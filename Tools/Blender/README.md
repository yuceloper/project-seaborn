# Seaborn Sloop — Blender Hazırlama Aracı

Bu araç, onaylanan Meshy FBX ve 2K PBR texture setini kontrollü bir Blender master sahnesine hazırlar.

## Girdi

Meshy FBX ZIP'ini bir klasöre çıkar:

- `*_texture.fbx`
- `*_texture.png`
- `*_texture_metallic.png`
- `*_texture_normal.png`
- `*_texture_roughness.png`

## Çalıştırma

Blender kurulum yolunu kendi makinen için değiştir:

```powershell
& "C:\\Program Files\\Blender Foundation\\Blender 4.5\\blender.exe" `
  --background `
  --python Tools/Blender/prepare_seaborn_sloop.py `
  -- `
  --fbx "C:\\Models\\SeabornSloop\\Meshy_AI_The_Weathered_Corsair_0915155558_texture.fbx" `
  --textures "C:\\Models\\SeabornSloop" `
  --output "C:\\Models\\SeabornSloop\\SeabornSloop_Master.blend"
```

Araç:

- sahneyi metre birimine geçirir;
- ana mesh'i `Sloop_Source_Mesh` olarak adlandırır;
- Base Color, Metallic, Roughness ve Normal haritalarını preview materyaline bağlar;
- bölüm kılavuzlarını oluşturur;
- gerekli soket ve altı top hardpoint adını hazırlar;
- vertex, triangle, UV ve boyut raporu basar;
- belirtilen `.blend` dosyasını kaydeder.

## Bilinçli olarak otomatik yapılmayanlar

AI mesh'i anlamına bakmadan otomatik kesmek üretim kalitesini bozar. Aşağıdakiler Blender'da görsel denetimle yapılır:

1. Pruvanın oyun ileri yönüne çevrilmesi.
2. Su çizgisi ve fizik pivotunun ayarlanması.
3. Gövde, direk, yelken ve halatların semantik ayrılması.
4. Pruva/orta/kıç kesim düzlemlerinin elle düzeltilmesi.
5. Hardpoint ve socket boş nesnelerinin gerçek yüzeye taşınması.
6. Görünmeyen yüzeylerin temizlenmesi ve LOD üretimi.
7. Unity export öncesi rotation/scale uygulanması.

## Unity export

- Format: FBX
- Forward: `-Z Forward`
- Up: `Y Up`
- Apply Transform: açık
- Add Leaf Bones: kapalı
- Yalnızca seçili üretim nesneleri
- Texture'lar FBX içine gömülmez

Unity tarafında roughness doğrudan kullanılmaz; `Smoothness = 1 - Roughness` dönüşümü uygulanır.
