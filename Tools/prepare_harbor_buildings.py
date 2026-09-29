"""Pack the two supplied harbor building FBX exports for the Unity coastal-harbor importer.

Usage: python Tools/prepare_harbor_buildings.py INPUT_DIRECTORY OUTPUT_DIRECTORY
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
stage = output / 'harbor-buildings-staging'
root = stage / 'Assets/_Project/Art/Environment/SeabornCoast'
records = []

def save_png(image, path):
    # Encode in memory to avoid partial low-level encoder writes on hosted filesystems.
    buffer = io.BytesIO()
    image.save(buffer, format='PNG')
    data = buffer.getvalue()
    with Image.open(io.BytesIO(data)) as check:
        check.verify()
    with Image.open(io.BytesIO(data)) as check:
        check.load()
    temporary = path.with_suffix('.png.tmp')
    temporary.write_bytes(data)
    temporary.replace(path)

for name, filename in [
    ('HarborOffice', 'Meshy_AI_Stonewatch_Manor_0927010320_texture_fbx.zip'),
    ('TradeWarehouse', 'Meshy_AI_Medieval_Stable_0927010403_texture_fbx.zip'),
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

        save_png(texture('', 'RGB'), target / 'Textures' / f'T_{name}_BaseColor.png')
        save_png(texture('_normal', 'RGB'), target / 'Textures' / f'T_{name}_Normal.png')
        metal = texture('_metallic', 'L')
        smooth = ImageOps.invert(texture('_roughness', 'L'))
        zero = Image.new('L', metal.size, 0)
        save_png(Image.merge('RGBA', (metal, zero, zero, smooth)),
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
    '3. Wait for import. Choose Seaborn > Art > Build Harbor Buildings.\n'
    '4. Enter PrototypeHarbor Play Mode. No manual model rotation or positioning is required.\n\n'
    'BaseColor: sRGB. Normal: normal map. MetallicSmoothness: linear, R=metallic, A=1-roughness.\n'
    'All external textures are 2048x2048. FBX geometry is unchanged from supplied exports.\n'
    'Save generated Resources prefabs, materials, textures, models and meta files in your project.\n'
    'The original FBX may contain embedded source textures; importer disables automatic material import.\n'
    'This is a visual replacement; docking and navigation blockers are unchanged.\n'
    'Unity compilation and Play Mode validation must be performed locally.\n', encoding='utf-8')

archive_path = output / 'Seaborn_Harbor_Buildings_2K.zip'
with zipfile.ZipFile(archive_path, 'w', zipfile.ZIP_DEFLATED, compresslevel=6) as archive:
    for path in sorted(stage.rglob('*')):
        if path.is_file():
            archive.write(path, path.relative_to(stage).as_posix())
with zipfile.ZipFile(archive_path) as archive:
    assert archive.testzip() is None
    assert sum(p.endswith('.fbx') for p in archive.namelist()) == 2
    for path in archive.namelist():
        if path.endswith('.png'):
            with Image.open(io.BytesIO(archive.read(path))) as image:
                image.verify()  # Validate every PNG chunk and CRC, not just its header.
            with Image.open(io.BytesIO(archive.read(path))) as image:
                image.load()  # Decode the full pixel stream; rejects truncated IDAT data.
                assert image.size == (2048, 2048), path
print(json.dumps({'path': str(archive_path.resolve()), 'size_bytes': archive_path.stat().st_size,
                  'sha256': hashlib.sha256(archive_path.read_bytes()).hexdigest(), 'source_assets': records}))
