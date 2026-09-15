using System.Collections.Generic;
using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Ship
{
    public enum PrototypeShipSectionKind
    {
        Forecastle,
        Midship,
        Stern
    }

    public enum PrototypeHardpointSide
    {
        Port,
        Starboard
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeCannonHardpoint :
        MonoBehaviour
    {
        public PrototypeHardpointSide Side { get; private set; }
        public int Index { get; private set; }
        public Transform Muzzle { get; private set; }

        public void Configure(
            PrototypeHardpointSide side,
            int index,
            Transform muzzle)
        {
            Side = side;
            Index = index;
            Muzzle = muzzle;
        }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeModularShipAssembler :
        MonoBehaviour
    {
        private const float SectionLength = 1.25f;
        private const int MinimumMidships = 1;
        private const int MaximumMidships = 3;

        [SerializeField, Range(
            MinimumMidships,
            MaximumMidships)]
        private int midshipSectionCount = 1;

        private readonly List<Transform>
            portHardpoints = new();
        private readonly List<Transform>
            starboardHardpoints = new();

        private Transform assemblyRoot;

        public int MidshipSectionCount =>
            midshipSectionCount;
        public IReadOnlyList<Transform> PortHardpoints =>
            portHardpoints;
        public IReadOnlyList<Transform> StarboardHardpoints =>
            starboardHardpoints;

        public void Build(
            Transform visualParent,
            Material hullMaterial,
            Material deckMaterial,
            Material cannonMaterial)
        {
            if (visualParent == null) return;

            ClearAssembly();
            portHardpoints.Clear();
            starboardHardpoints.Clear();
            midshipSectionCount = Mathf.Clamp(
                midshipSectionCount,
                MinimumMidships,
                MaximumMidships
            );

            GameObject root =
                new("Modular Sloop Assembly");
            assemblyRoot = root.transform;
            assemblyRoot.SetParent(visualParent, false);

            float midLength =
                midshipSectionCount * SectionLength;
            float foreZ = midLength * 0.5f + 0.82f;
            float sternZ = -midLength * 0.5f - 0.72f;

            CreateSectionRoot(
                "Forecastle Section",
                PrototypeShipSectionKind.Forecastle,
                foreZ
            );
            BuildForecastle(
                assemblyRoot.Find("Forecastle Section"),
                hullMaterial,
                deckMaterial
            );

            for (int i = 0;
                 i < midshipSectionCount;
                 i++)
            {
                float z =
                    (i - (midshipSectionCount - 1) * 0.5f) *
                    SectionLength;
                Transform section = CreateSectionRoot(
                    $"Midship Section {i + 1}",
                    PrototypeShipSectionKind.Midship,
                    z
                );
                BuildMidship(
                    section,
                    i,
                    hullMaterial,
                    deckMaterial,
                    cannonMaterial
                );
            }

            Transform stern = CreateSectionRoot(
                "Stern Section",
                PrototypeShipSectionKind.Stern,
                sternZ
            );
            BuildStern(
                stern,
                hullMaterial,
                deckMaterial
            );

            BroadsideController broadside =
                GetComponent<BroadsideController>();
            broadside?.SetRuntimeMuzzles(
                portHardpoints.ToArray(),
                starboardHardpoints.ToArray()
            );
        }


        public void BuildHardpointsOnly(
            Transform visualParent,
            Material cannonMaterial)
        {
            if (visualParent == null) return;

            ClearAssembly();
            portHardpoints.Clear();
            starboardHardpoints.Clear();

            GameObject root =
                new("Production Sloop Hardpoints");
            assemblyRoot = root.transform;
            assemblyRoot.SetParent(visualParent, false);

            float[] longitudinalPositions =
                { -0.82f, 0f, 0.82f };

            for (int index = 0;
                 index < longitudinalPositions.Length;
                 index++)
            {
                float z = longitudinalPositions[index];

                CreateCannonHardpoint(
                    assemblyRoot,
                    PrototypeHardpointSide.Port,
                    index,
                    new Vector3(-0.72f, 0.3f, z),
                    cannonMaterial,
                    false
                );
                CreateCannonHardpoint(
                    assemblyRoot,
                    PrototypeHardpointSide.Starboard,
                    index,
                    new Vector3(0.72f, 0.3f, z),
                    cannonMaterial,
                    false
                );
            }

            BroadsideController broadside =
                GetComponent<BroadsideController>();
            broadside?.SetRuntimeMuzzles(
                portHardpoints.ToArray(),
                starboardHardpoints.ToArray()
            );
        }

        public void SetMidshipSectionCount(
            int count,
            Transform visualParent,
            Material hullMaterial,
            Material deckMaterial,
            Material cannonMaterial)
        {
            midshipSectionCount = Mathf.Clamp(
                count,
                MinimumMidships,
                MaximumMidships
            );
            Build(
                visualParent,
                hullMaterial,
                deckMaterial,
                cannonMaterial
            );
        }

        private Transform CreateSectionRoot(
            string sectionName,
            PrototypeShipSectionKind kind,
            float z)
        {
            GameObject section = new(sectionName);
            section.transform.SetParent(
                assemblyRoot,
                false
            );
            section.transform.localPosition =
                new Vector3(0f, 0f, z);

            PrototypeShipSection marker =
                section.AddComponent<
                    PrototypeShipSection>();
            marker.Configure(kind);
            return section.transform;
        }

        private void BuildForecastle(
            Transform section,
            Material hull,
            Material deck)
        {
            CreatePart(
                section,
                "Pruva Gövdesi",
                PrimitiveType.Cube,
                new Vector3(0f, -0.06f, 0f),
                new Vector3(1.45f, 0.72f, 1.55f),
                Quaternion.identity,
                hull
            );
            CreatePart(
                section,
                "Pruva Güvertesi",
                PrimitiveType.Cube,
                new Vector3(0f, 0.34f, -0.08f),
                new Vector3(1.34f, 0.14f, 1.35f),
                Quaternion.identity,
                deck
            );
            CreatePart(
                section,
                "Pruva Ucu",
                PrimitiveType.Cube,
                new Vector3(0f, -0.02f, 0.92f),
                new Vector3(0.18f, 0.62f, 0.55f),
                Quaternion.identity,
                hull
            );
        }

        private void BuildMidship(
            Transform section,
            int sectionIndex,
            Material hull,
            Material deck,
            Material cannon)
        {
            CreatePart(
                section,
                "Orta Gövde",
                PrimitiveType.Cube,
                new Vector3(0f, -0.06f, 0f),
                new Vector3(1.86f, 0.78f, SectionLength),
                Quaternion.identity,
                hull
            );
            CreatePart(
                section,
                "Top Güvertesi",
                PrimitiveType.Cube,
                new Vector3(0f, 0.37f, 0f),
                new Vector3(1.68f, 0.15f, SectionLength),
                Quaternion.identity,
                deck
            );

            float[] offsets = { -0.38f, 0f, 0.38f };
            for (int i = 0; i < offsets.Length; i++)
            {
                CreateCannonHardpoint(
                    section,
                    PrototypeHardpointSide.Port,
                    sectionIndex * offsets.Length + i,
                    new Vector3(
                        -1.02f,
                        0.39f,
                        offsets[i]
                    ),
                    cannon
                );
                CreateCannonHardpoint(
                    section,
                    PrototypeHardpointSide.Starboard,
                    sectionIndex * offsets.Length + i,
                    new Vector3(
                        1.02f,
                        0.39f,
                        offsets[i]
                    ),
                    cannon
                );
            }
        }

        private void BuildStern(
            Transform section,
            Material hull,
            Material deck)
        {
            CreatePart(
                section,
                "Kıç Gövdesi",
                PrimitiveType.Cube,
                new Vector3(0f, -0.03f, 0f),
                new Vector3(1.62f, 0.76f, 1.4f),
                Quaternion.identity,
                hull
            );
            CreatePart(
                section,
                "Kıç Güvertesi",
                PrimitiveType.Cube,
                new Vector3(0f, 0.38f, 0.02f),
                new Vector3(1.48f, 0.15f, 1.24f),
                Quaternion.identity,
                deck
            );
            CreatePart(
                section,
                "Kıç Kamara",
                PrimitiveType.Cube,
                new Vector3(0f, 0.69f, -0.12f),
                new Vector3(1.02f, 0.5f, 0.72f),
                Quaternion.identity,
                hull
            );
        }

        private void CreateCannonHardpoint(
            Transform parent,
            PrototypeHardpointSide side,
            int index,
            Vector3 position,
            Material material,
            bool showPreview = true)
        {
            GameObject hardpoint = new(
                $"{side} Cannon Hardpoint {index + 1}"
            );
            hardpoint.transform.SetParent(parent, false);
            hardpoint.transform.localPosition = position;

            float direction =
                side == PrototypeHardpointSide.Port
                    ? -1f
                    : 1f;
            if (showPreview)
            {
                CreatePart(
                    hardpoint.transform,
                    "Installed Cannon Preview",
                    PrimitiveType.Cylinder,
                    Vector3.zero,
                    new Vector3(0.09f, 0.3f, 0.09f),
                    Quaternion.Euler(0f, 0f, 90f),
                    material
                );
            }

            GameObject muzzle = new("Muzzle");
            muzzle.transform.SetParent(
                hardpoint.transform,
                false
            );
            muzzle.transform.localPosition =
                new Vector3(direction * 0.38f, 0f, 0f);

            PrototypeCannonHardpoint marker =
                hardpoint.AddComponent<
                    PrototypeCannonHardpoint>();
            marker.Configure(side, index, muzzle.transform);

            if (side == PrototypeHardpointSide.Port)
                portHardpoints.Add(muzzle.transform);
            else
                starboardHardpoints.Add(muzzle.transform);
        }

        private static GameObject CreatePart(
            Transform parent,
            string partName,
            PrimitiveType primitive,
            Vector3 position,
            Vector3 scale,
            Quaternion rotation,
            Material material)
        {
            GameObject part =
                GameObject.CreatePrimitive(primitive);
            part.name = partName;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation;
            part.transform.localScale = scale;

            Collider collider =
                part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            Renderer renderer =
                part.GetComponent<Renderer>();
            if (renderer != null)
                renderer.sharedMaterial = material;
            return part;
        }

        private void ClearAssembly()
        {
            if (assemblyRoot == null) return;

            if (Application.isPlaying)
                Destroy(assemblyRoot.gameObject);
            else
                DestroyImmediate(assemblyRoot.gameObject);
            assemblyRoot = null;
        }

        private void OnDestroy()
        {
            portHardpoints.Clear();
            starboardHardpoints.Clear();
        }
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeShipSection :
        MonoBehaviour
    {
        public PrototypeShipSectionKind Kind
        {
            get;
            private set;
        }

        public void Configure(
            PrototypeShipSectionKind kind)
        {
            Kind = kind;
        }
    }
}
