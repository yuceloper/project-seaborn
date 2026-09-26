"""Pack the three supplied Meshy FBX exports for the Unity coastal-harbor importer.

Usage: python Tools/prepare_coastal_harbor.py INPUT_DIRECTORY OUTPUT_DIRECTORY
Requires Pillow. Originals are read-only; only the named staging/output paths are written.
"""
from pathlib import Path
import hashlib
import io
import json
import sys
import uuid
import zipfile
from PIL import Image, ImageOps

source, output = map(Path, sys.argv[1:3])
stage = output / 'coastal-harbor-staging'
root = stage / 'Assets/_Project/Art/Environment/SeabornCoast'
records = []
for name, filename in [
    ('CoastalRock', 'Meshy_AI_Weathered_Stone_Outcr_0923231715_texture_fbx.zip'),
    ('DockModule', 'Meshy_AI_Weathered_Wooden_Dock_0923231920_texture_fbx.zip'),
    ('ShipwrightWorkshop', 'Meshy_AI_Medieval_Barn_0923232024_texture_fbx.zip'),
]:
    original = source / filename
    with zipfile.ZipFile(original) as archive:
        assert archive.testzip() is None, filename
        models = [p for p in archive.namelist() if p.lower().endswith('.fbx')]
        assert len(models) == 1, filename
        model = models[0]
        stem = model[:-4]
        target = root / name
        (target / 'Models').mkdir(parents=True, exist_ok=True)
        (target / 'Textures').mkdir(exist_ok=True)
        (target / 'Models' / f'SM_{name}.fbx').write_bytes(archive.read(model))

        def texture(suffix, mode):
            with Image.open(io.BytesIO(archive.read(stem + suffix + '.png'))) as image:
                return image.convert(mode).resize((2048, 2048), Image.Resampling.LANCZOS)

        texture('', 'RGB').save(target / 'Textures' / f'T_{name}_BaseColor.png')
        texture('_normal', 'RGB').save(target / 'Textures' / f'T_{name}_Normal.png')
        metal = texture('_metallic', 'L')
        smooth = ImageOps.invert(texture('_roughness', 'L'))
        zero = Image.new('L', metal.size, 0)
        Image.merge('RGBA', (metal, zero, zero, smooth)).save(
            target / 'Textures' / f'T_{name}_MetallicSmoothness.png')
        records.append({'asset': name, 'source': filename,
                        'source_sha256': hashlib.sha256(original.read_bytes()).hexdigest()})

for path in [root, *root.rglob('*')]:
    if path.suffix == '.meta':
        continue
    key = path.relative_to(stage).as_posix()
    meta = 'fileFormatVersion: 2\nguid: ' + uuid.uuid5(uuid.NAMESPACE_URL, 'seaborn-coast-v1/' + key).hex + '\n'
    if path.is_dir():
        meta += 'folderAsset: yes\n'
    path.with_name(path.name + '.meta').write_text(meta)

(stage / 'asset-manifest.json').write_text(json.dumps(records, indent=2))
(stage / 'README_COASTAL_HARBOR.txt').write_text(
    'SEABORN COASTAL HARBOR - 2K\n\n'
    '1. Pull feature/expedition-economy-report and leave Unity Play Mode.\n'
    '2. Extract the Assets folder into C:\\Dev\\project-seaborn (beside the existing Assets folder).\n'
    '   Do not create Assets/Assets. This archive contains models and textures, not replacement scenes.\n'
    '3. Wait for import. Choose Seaborn > Art > Build Coastal Harbor Models.\n'
    '4. Enter PrototypeHarbor Play Mode. No manual model rotation or positioning is required.\n\n'
    'BaseColor: sRGB. Normal: normal map. MetallicSmoothness: linear, R=metallic, A=1-roughness.\n'
    'All external textures are 2048x2048. FBX geometry is unchanged from supplied exports.\n'
    'Save generated Resources prefabs, materials, textures, models and meta files in your project.\n'
    'The original FBX may contain embedded source textures; importer disables automatic material import.\n'
    'This is a visual replacement; docking and navigation blockers are unchanged.\n'
    'Unity compilation and Play Mode validation must be performed locally.\n', encoding='utf-8')

archive_path = output / 'Seaborn_Coastal_Harbor_2K.zip'
with zipfile.ZipFile(archive_path, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(stage.rglob('*')):
        if path.is_file():
            archive.write(path, path.relative_to(stage).as_posix())
with zipfile.ZipFile(archive_path) as archive:
    assert archive.testzip() is None
    assert sum(p.endswith('.fbx') for p in archive.namelist()) == 3
    for path in archive.namelist():
        if path.endswith('.png'):
            with Image.open(io.BytesIO(archive.read(path))) as image:
                assert image.size == (2048, 2048), path
print(json.dumps({'path': str(archive_path.resolve()), 'size_bytes': archive_path.stat().st_size,
                  'sha256': hashlib.sha256(archive_path.read_bytes()).hexdigest(), 'source_assets': records}))
