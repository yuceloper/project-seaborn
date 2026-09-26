using UnityEngine;

namespace Seaborn.Harbor
{
    internal static class MeshyCoastAssets
    {
        internal static bool HasWorkshop => Resources.Load<GameObject>("SeabornShipwrightWorkshopVisual") != null;

        internal static bool Place(Transform parent, string asset, string name, Vector3 position,
            Vector3 scale, float yaw = 0f)
        {
            var prefab = Resources.Load<GameObject>("Seaborn" + asset + "Visual");
            if (prefab == null) return false;
            var instance = Object.Instantiate(prefab, parent, false);
            instance.name = name;
            instance.transform.localPosition = position;
            instance.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            instance.transform.localScale = scale;
            return true;
        }

        internal static bool Rock(Transform parent, string name, Vector3 position, float width, float yaw) =>
            Place(parent, "CoastalRock", name, position, Vector3.one * width / 12f, yaw);

        internal static bool Dock(Transform parent, string name, Vector3 deck, Vector2 footprint)
        {
            if (Resources.Load<GameObject>("SeabornDockModuleVisual") == null) return false;
            // Builder normalizes pier footprint to 3.48 x 8; Y=0 is the walking surface.
            // A single wide loading deck avoids rows of embedded piles through the work area.
            int columns = 1;
            int rows = Mathf.Max(1, Mathf.CeilToInt(footprint.y / 8f));
            Vector2 cell = new Vector2(footprint.x / columns, footprint.y / rows);
            Transform root = new GameObject(name).transform;
            root.SetParent(parent, false);
            root.localPosition = deck;
            for (int x = 0; x < columns; x++)
                for (int z = 0; z < rows; z++)
                    Place(root, "DockModule", $"Dock Module {x} {z}",
                        new Vector3(-footprint.x / 2 + cell.x * (x + 0.5f), 0,
                            -footprint.y / 2 + cell.y * (z + 0.5f)),
                        new Vector3(cell.x / 3.48f, 1, cell.y / 8f));
            return true;
        }
    }
}
