"""Prepare the approved Meshy Seaborn Sloop source inside Blender.

Run from Blender:
  blender --background --python Tools/Blender/prepare_seaborn_sloop.py -- \
    --fbx "path/to/Meshy_AI_The_Weathered_Corsair_texture.fbx" \
    --textures "path/to/texture_folder" \
    --output "path/to/SeabornSloop_Master.blend"

The script deliberately does not cut the AI mesh automatically. Semantic separation of
hull, sails, masts and rigging must be reviewed before the Unity export.
"""

from __future__ import annotations

import argparse
from pathlib import Path
import sys

import bpy


SOURCE_NAME = "Sloop_Source_Mesh"
MATERIAL_NAME = "MAT_SeabornSloop_MeshyPreview"
HELPER_COLLECTION = "_SEABORN_HELPERS"
SECTION_GUIDES = ("Guide_Fore_To_Mid", "Guide_Mid_To_Stern")
SOCKET_NAMES = (
    "Socket_Fore_To_Mid", "Socket_Mid_To_Fore", "Socket_Mid_To_Mid",
    "Socket_Mid_To_Stern", "Socket_Stern_To_Mid", "Socket_Mast_Main",
    "Socket_Mast_Fore", "Socket_Harpoon", "Socket_Wake_Bow", "Socket_Wake_Stern",
)
HARDPOINT_NAMES = tuple(
    f"HP_Cannon_{side}_{index:02d}"
    for side in ("Port", "Starboard")
    for index in range(1, 4)
)


def parse_args() -> argparse.Namespace:
    values = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
    parser = argparse.ArgumentParser()
    parser.add_argument("--fbx", type=Path)
    parser.add_argument("--textures", type=Path)
    parser.add_argument("--output", type=Path)
    parser.add_argument("--keep-scene", action="store_true")
    return parser.parse_args(values)


def reset_scene(keep_scene: bool) -> None:
    if not keep_scene:
        bpy.ops.wm.read_factory_settings(use_empty=True)


def import_fbx(path: Path | None) -> list[bpy.types.Object]:
    if path:
        if not path.is_file():
            raise FileNotFoundError(path)
        bpy.ops.import_scene.fbx(filepath=str(path.resolve()))

    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError("No mesh found. Supply --fbx or import the FBX first.")
    return meshes


def triangle_count(obj: bpy.types.Object) -> int:
    return sum(max(0, len(poly.vertices) - 2) for poly in obj.data.polygons)


def configure_scene() -> None:
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.length_unit = "METERS"
    scene.unit_settings.scale_length = 1.0


def choose_source(meshes: list[bpy.types.Object]) -> bpy.types.Object:
    source = max(meshes, key=triangle_count)
    source.name = SOURCE_NAME
    source.data.name = f"{SOURCE_NAME}_Mesh"
    return source


def find_texture(folder: Path | None, suffix: str) -> Path | None:
    if not folder or not folder.is_dir():
        return None
    matches = sorted(folder.glob(f"*{suffix}"))
    return matches[0] if matches else None


def image_node(nodes, path: Path, name: str, color_space: str):
    node = nodes.new("ShaderNodeTexImage")
    node.name = name
    node.label = name
    node.image = bpy.data.images.load(str(path.resolve()), check_existing=True)
    node.image.colorspace_settings.name = color_space
    return node


def build_preview_material(texture_folder: Path | None) -> bpy.types.Material:
    material = bpy.data.materials.get(MATERIAL_NAME) or bpy.data.materials.new(MATERIAL_NAME)
    material.use_nodes = True

    nodes = material.node_tree.nodes
    links = material.node_tree.links
    nodes.clear()

    output = nodes.new("ShaderNodeOutputMaterial")
    output.location = (720, 0)
    shader = nodes.new("ShaderNodeBsdfPrincipled")
    shader.location = (420, 0)
    links.new(shader.outputs["BSDF"], output.inputs["Surface"])

    texture_specs = (
        ("_texture.png", "Base Color", "sRGB", "Base Color", ( -480, 240)),
        ("_texture_metallic.png", "Metallic", "Non-Color", "Metallic", (-480, 40)),
        ("_texture_roughness.png", "Roughness", "Non-Color", "Roughness", (-480, -140)),
    )
    for suffix, label, color_space, target, location in texture_specs:
        path = find_texture(texture_folder, suffix)
        if path:
            node = image_node(nodes, path, label, color_space)
            node.location = location
            links.new(node.outputs["Color"], shader.inputs[target])

    normal = find_texture(texture_folder, "_texture_normal.png")
    if normal:
        node = image_node(nodes, normal, "Normal", "Non-Color")
        node.location = (-480, -340)
        normal_map = nodes.new("ShaderNodeNormalMap")
        normal_map.location = (140, -300)
        links.new(node.outputs["Color"], normal_map.inputs["Color"])
        links.new(normal_map.outputs["Normal"], shader.inputs["Normal"])

    return material


def assign_material(meshes: list[bpy.types.Object], material: bpy.types.Material) -> None:
    for obj in meshes:
        obj.data.materials.clear()
        obj.data.materials.append(material)


def get_collection(name: str) -> bpy.types.Collection:
    found = bpy.data.collections.get(name)
    if found:
        return found
    found = bpy.data.collections.new(name)
    bpy.context.scene.collection.children.link(found)
    return found


def add_empty(name: str, target: bpy.types.Collection, location=(0.0, 0.0, 0.0)):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = "PLAIN_AXES"
    obj.empty_display_size = 0.12
    obj.location = location
    target.objects.link(obj)
    return obj


def create_helpers(source: bpy.types.Object) -> None:
    helpers = get_collection(HELPER_COLLECTION)
    for obj in list(helpers.objects):
        bpy.data.objects.remove(obj, do_unlink=True)

    dimensions = source.dimensions
    center = source.location
    longitudinal_axis = max(range(3), key=lambda axis: dimensions[axis])
    length = dimensions[longitudinal_axis]

    for name, fraction in zip(SECTION_GUIDES, (-0.18, 0.20)):
        location = list(center)
        location[longitudinal_axis] += length * fraction
        guide = add_empty(name, helpers, location)
        guide.empty_display_type = "CUBE"
        guide.empty_display_size = max(dimensions) * 0.025

    for name in SOCKET_NAMES + HARDPOINT_NAMES:
        add_empty(name, helpers, center)

    helpers.hide_render = True


def validate(source: bpy.types.Object) -> None:
    triangles = triangle_count(source)
    uv_layers = len(source.data.uv_layers)
    print("[Seaborn] source:", source.name)
    print("[Seaborn] triangles:", triangles)
    print("[Seaborn] vertices:", len(source.data.vertices))
    print("[Seaborn] UV layers:", uv_layers)
    print("[Seaborn] dimensions:", tuple(round(value, 4) for value in source.dimensions))
    if not 80000 <= triangles <= 120000:
        print("[Seaborn][WARN] Source is outside the approved 80k-120k triangle window.")
    if uv_layers == 0:
        raise RuntimeError("The source mesh has no UV layer.")


def save(path: Path | None) -> None:
    if not path:
        print("[Seaborn] No --output supplied; scene was prepared but not saved.")
        return
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(path.resolve()))
    print("[Seaborn] Saved:", path.resolve())


def main() -> None:
    args = parse_args()
    reset_scene(args.keep_scene)
    configure_scene()
    meshes = import_fbx(args.fbx)
    source = choose_source(meshes)
    material = build_preview_material(args.textures)
    assign_material(meshes, material)
    create_helpers(source)
    validate(source)
    save(args.output)


if __name__ == "__main__":
    main()
