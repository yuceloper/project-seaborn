using UnityEngine;

namespace Seaborn.World
{
    public enum PrototypeRegionKind
    {
        SafeHarbor,
        CoastalHunt,
        PiratePassage,
        StormjawDepths,
        OpenSea
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeExpeditionRegionDirector :
        MonoBehaviour
    {
        private sealed class Region
        {
            public PrototypeRegionKind Kind;
            public string Name;
            public string Danger;
            public Vector3 Center;
            public float Radius;
            public Color Color;
        }

        public static PrototypeExpeditionRegionDirector
            Instance { get; private set; }

        public string CurrentRegionName =>
            current != null ? current.Name : "AÇIK DENİZ";

        public string CurrentDangerLabel =>
            current != null ? current.Danger : "ORTA TEHLİKE";

        public Color CurrentDangerColor =>
            current != null
                ? current.Color
                : new Color(0.86f, 0.68f, 0.3f, 1f);

        public PrototypeRegionKind CurrentRegionKind =>
            current != null
                ? current.Kind
                : PrototypeRegionKind.OpenSea;

        private Transform player;
        private Region[] regions;
        private Region openSea;
        private Region current;
        private Material ringMaterial;
        private bool initialized;

        public static void EnsureCreated(Transform player)
        {
            if (player == null)
            {
                return;
            }

            PrototypeExpeditionRegionDirector director =
                FindFirstObjectByType<
                    PrototypeExpeditionRegionDirector>();

            if (director == null)
            {
                GameObject root = new(
                    "Prototype Expedition Regions");
                director = root.AddComponent<
                    PrototypeExpeditionRegionDirector>();
            }

            director.Initialize(player);
        }

        private void Awake()
        {
            Instance = this;
        }

        private void Initialize(Transform playerTransform)
        {
            if (initialized)
            {
                player = playerTransform;
                return;
            }

            initialized = true;
            player = playerTransform;
            Vector3 harbor = player.position;
            harbor.y = 0.12f;

            regions = new[]
            {
                CreateRegion(
                    PrototypeRegionKind.SafeHarbor,
                    "GÜVENLİ LİMAN",
                    "GÜVENLİ BÖLGE",
                    harbor,
                    6.5f,
                    new Color(0.28f, 0.78f, 0.62f, 0.9f),
                    false
                ),
                CreateRegion(
                    PrototypeRegionKind.CoastalHunt,
                    "KIYI AV SAHASI",
                    "DÜŞÜK TEHLİKE",
                    harbor + new Vector3(17f, 0f, 11f),
                    11f,
                    new Color(0.36f, 0.72f, 0.82f, 0.68f),
                    true
                ),
                CreateRegion(
                    PrototypeRegionKind.PiratePassage,
                    "KORSAN GEÇİDİ",
                    "YÜKSEK TEHLİKE",
                    harbor + new Vector3(-18f, 0f, 22f),
                    11.5f,
                    new Color(0.9f, 0.48f, 0.2f, 0.72f),
                    true
                ),
                CreateRegion(
                    PrototypeRegionKind.StormjawDepths,
                    "STORMJAW DERİNLİĞİ",
                    "AŞIRI TEHLİKE",
                    harbor + new Vector3(12f, 0f, 34f),
                    12.5f,
                    new Color(0.72f, 0.24f, 0.3f, 0.76f),
                    true
                )
            };

            openSea = new Region
            {
                Kind = PrototypeRegionKind.OpenSea,
                Name = "AÇIK DENİZ",
                Danger = "ORTA TEHLİKE",
                Color = new Color(0.86f, 0.68f, 0.3f, 1f)
            };

            RefreshRegion(true);
        }

        private Region CreateRegion(
            PrototypeRegionKind kind,
            string regionName,
            string danger,
            Vector3 center,
            float radius,
            Color color,
            bool drawBoundary)
        {
            Region region = new()
            {
                Kind = kind,
                Name = regionName,
                Danger = danger,
                Center = center,
                Radius = radius,
                Color = color
            };

            if (drawBoundary)
            {
                CreateBoundary(region);
            }

            return region;
        }

        private void Update()
        {
            RefreshRegion(false);
        }

        private void RefreshRegion(bool silent)
        {
            if (player == null || regions == null)
            {
                return;
            }

            Region next = openSea;
            Vector2 playerPoint = new(
                player.position.x,
                player.position.z
            );

            for (int i = 0; i < regions.Length; i++)
            {
                Region candidate = regions[i];
                Vector2 center = new(
                    candidate.Center.x,
                    candidate.Center.z
                );

                if (Vector2.Distance(playerPoint, center) <=
                    candidate.Radius)
                {
                    next = candidate;
                    break;
                }
            }

            if (current == next)
            {
                return;
            }

            current = next;
            if (!silent)
            {
                Debug.Log(
                    $"Bölgeye girildi: {current.Name} " +
                    $"({current.Danger}).",
                    this
                );
            }
        }

        private void CreateBoundary(Region region)
        {
            GameObject boundary = new(
                $"{region.Name} Sınırı");
            boundary.transform.SetParent(transform, false);

            LineRenderer line =
                boundary.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.loop = true;
            line.positionCount = 72;
            line.widthMultiplier = 0.055f;
            line.numCornerVertices = 2;
            line.startColor = region.Color;
            line.endColor = region.Color;
            line.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.textureMode =
                LineTextureMode.Stretch;

            if (ringMaterial == null)
            {
                Shader shader = Shader.Find(
                    "Sprites/Default");
                if (shader != null)
                {
                    ringMaterial = new Material(shader)
                    {
                        name =
                            "Prototype Region Boundary Material"
                    };
                }
            }

            if (ringMaterial != null)
            {
                line.sharedMaterial = ringMaterial;
            }

            for (int i = 0; i < line.positionCount; i++)
            {
                float angle =
                    (float)i / line.positionCount *
                    Mathf.PI * 2f;
                line.SetPosition(
                    i,
                    region.Center +
                    new Vector3(
                        Mathf.Cos(angle) * region.Radius,
                        0f,
                        Mathf.Sin(angle) * region.Radius
                    )
                );
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (ringMaterial != null)
            {
                Destroy(ringMaterial);
            }
        }
    }
}
