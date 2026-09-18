using UnityEngine;

namespace Seaborn.Harbor
{
    [DisallowMultipleComponent]
    public sealed class HarborMaterialLightingPolish : MonoBehaviour
    {
        private static readonly Color OfficePlaster = new(0.63f, 0.59f, 0.47f, 1f);
        private static readonly Color TradePlaster = new(0.56f, 0.46f, 0.34f, 1f);
        private static readonly Color ShipyardWood = new(0.34f, 0.24f, 0.16f, 1f);
        private static readonly Color RoofSlate = new(0.10f, 0.15f, 0.16f, 1f);
        private static readonly Color StoneWarm = new(0.34f, 0.36f, 0.33f, 1f);
        private static readonly Color WindowBlue = new(0.08f, 0.20f, 0.23f, 1f);
        private static readonly Color Brass = new(0.72f, 0.49f, 0.18f, 1f);
        private static readonly Color Lantern = new(1f, 0.55f, 0.18f, 1f);

        private bool applied;

        private void Update()
        {
            if (applied) return;
            if (FindFirstObjectByType<PrototypeHarborVisualDirector>() == null) return;
            if (FindFirstObjectByType<HarborEnvironmentPolish>() == null) return;
            Apply();
            applied = true;
        }

        private void Apply()
        {
            Renderer[] renderers = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
            foreach (Renderer renderer in renderers)
            {
                if (renderer == null) continue;
                string n = renderer.gameObject.name;
                if (!IsHarborPart(n)) continue;

                MaterialPropertyBlock block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);
                Color tint = ResolveColor(n);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                block.SetFloat("_Smoothness", ResolveSmoothness(n));
                block.SetFloat("_Metallic", n.Contains("Sign") || n.Contains("Beacon") ? 0.12f : 0f);
                renderer.SetPropertyBlock(block);
            }

            AddWarmPool("Office Courtyard Light", new Vector3(0f, 4.2f, -16.4f), 10f, 4.5f);
            AddWarmPool("Trade Loading Light", new Vector3(16f, 4.2f, -5.9f), 9f, 4.0f);
            AddWarmPool("Shipyard Work Light", new Vector3(-16f, 4.0f, -6.2f), 8f, 3.5f);
        }

        private static bool IsHarborPart(string n)
        {
            return n.Contains("Office") || n.Contains("Trade") || n.Contains("Shipyard") ||
                   n.Contains("Workshop") || n.Contains("Warehouse") || n.Contains("Quay") ||
                   n.Contains("Harbor Wall") || n.Contains("Southern Stone Shore") ||
                   n.Contains("Breakwater") || n.Contains("Lighthouse");
        }

        private static Color ResolveColor(string n)
        {
            if (n.Contains("Window")) return WindowBlue;
            if (n.Contains("Roof") || n.Contains("Cupola") || n.Contains("Awning")) return RoofSlate;
            if (n.Contains("Office") && !n.Contains("Stone") && !n.Contains("Post")) return OfficePlaster;
            if ((n.Contains("Trade") || n.Contains("Warehouse")) && !n.Contains("Door") && !n.Contains("Crate")) return TradePlaster;
            if (n.Contains("Shipyard") || n.Contains("Workshop") || n.Contains("Lumber") || n.Contains("Barrel")) return ShipyardWood;
            if (n.Contains("Sign")) return Brass;
            if (n.Contains("Beacon") || n.Contains("Lantern")) return Lantern;
            if (n.Contains("Stone") || n.Contains("Quay") || n.Contains("Breakwater") || n.Contains("Harbor Wall") || n.Contains("Shore")) return StoneWarm;
            return new Color(0.30f, 0.26f, 0.21f, 1f);
        }

        private static float ResolveSmoothness(string n)
        {
            if (n.Contains("Window")) return 0.42f;
            if (n.Contains("Sign") || n.Contains("Beacon")) return 0.28f;
            if (n.Contains("Roof")) return 0.12f;
            return 0.08f;
        }

        private static void AddWarmPool(string name, Vector3 position, float range, float intensity)
        {
            GameObject lightObject = new GameObject(name);
            lightObject.transform.position = position;
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.63f, 0.34f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
        }
    }

    internal sealed class HarborMaterialLightingPolishBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<HarborMaterialLightingPolishBootstrap>() != null) return;
            GameObject root = new GameObject("Harbor Material Lighting Polish Bootstrap");
            root.AddComponent<HarborMaterialLightingPolishBootstrap>();
        }

        private void Start()
        {
            if (FindFirstObjectByType<HarborMaterialLightingPolish>() == null)
            {
                gameObject.AddComponent<HarborMaterialLightingPolish>();
            }
        }
    }
}
