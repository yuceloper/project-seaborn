using UnityEngine;

namespace Seaborn.Harbor
{
    [DisallowMultipleComponent]
    public sealed class HarborEnvironmentPolish : MonoBehaviour
    {
        private static readonly Color Stone = new(0.24f, 0.27f, 0.26f, 1f);
        private static readonly Color StoneLight = new(0.39f, 0.40f, 0.36f, 1f);
        private static readonly Color Timber = new(0.16f, 0.13f, 0.10f, 1f);
        private static readonly Color TimberLight = new(0.34f, 0.27f, 0.19f, 1f);
        private static readonly Color Plaster = new(0.51f, 0.49f, 0.42f, 1f);
        private static readonly Color Roof = new(0.16f, 0.20f, 0.21f, 1f);
        private static readonly Color Glass = new(0.08f, 0.16f, 0.17f, 1f);
        private static readonly Color SignInk = new(0.83f, 0.75f, 0.57f, 1f);
        private static readonly Color Lantern = new(1f, 0.60f, 0.22f, 1f);

        private Material sharedMaterial;
        private bool applied;

        private void Update()
        {
            if (!applied)
            {
                TryApply();
            }
        }

        private void TryApply()
        {
            if (transform.Find("Harbor Office") == null ||
                transform.Find("Trade Warehouse") == null ||
                transform.Find("Shipwright Workshop") == null)
            {
                return;
            }

            applied = true;
            BuildHarborOfficeSilhouette();
            BuildTradeWarehouseDetail();
            BuildShipyardDetail();
            BuildQuayClutter();
            BuildAnnexRoofs();
            TuneWorldLabels();
        }

        private void BuildAnnexRoofs()
        {
            foreach (string partName in new[] { "Office West Wing Roof", "Office East Wing Roof",
                "Trade East Annex Roof", "Shipyard West Shed Roof" })
            {
                Transform flat = transform.Find(partName);
                if (flat == null) continue;
                Vector3 center = flat.localPosition;
                Vector3 size = flat.localScale;
                flat.gameObject.SetActive(false);
                for (int side = -1; side <= 1; side += 2)
                    CreatePart(partName + " Slope " + side, PrimitiveType.Cube,
                        center + new Vector3(side * size.x * 0.25f, size.x * 0.08f, 0f),
                        new Vector3(size.x * 0.55f, 0.16f, size.z + 0.15f), Roof,
                        Quaternion.Euler(0f, 0f, -side * 18f));
                CreatePart(partName + " Ridge", PrimitiveType.Cube,
                    center + Vector3.up * size.x * 0.16f,
                    new Vector3(0.18f, 0.16f, size.z + 0.2f), TimberLight);
            }
        }

        private void BuildHarborOfficeSilhouette()
        {
            // A small central watch tower and lower side wings stop the office
            // reading as one large rectangular block from the tactical camera.
            CreatePart("Office West Wing", PrimitiveType.Cube,
                new Vector3(-6.15f, 1.95f, -20.25f),
                new Vector3(3.5f, 2.8f, 4.4f), Plaster);
            CreatePart("Office East Wing", PrimitiveType.Cube,
                new Vector3(6.15f, 1.95f, -20.25f),
                new Vector3(3.5f, 2.8f, 4.4f), Plaster);
            CreatePart("Office West Wing Roof", PrimitiveType.Cube,
                new Vector3(-6.15f, 3.46f, -20.25f),
                new Vector3(3.9f, 0.18f, 4.8f), Roof);
            CreatePart("Office East Wing Roof", PrimitiveType.Cube,
                new Vector3(6.15f, 3.46f, -20.25f),
                new Vector3(3.9f, 0.18f, 4.8f), Roof);

            CreatePart("Office Watch Tower", PrimitiveType.Cube,
                new Vector3(0f, 5.55f, -20.15f),
                new Vector3(3.1f, 2.5f, 2.8f),
                new Color(0.55f, 0.52f, 0.43f, 1f));
            CreatePart("Office Watch Tower Cornice", PrimitiveType.Cube,
                new Vector3(0f, 6.82f, -20.15f),
                new Vector3(3.55f, 0.22f, 3.2f), StoneLight);
            CreatePart("Office Cupola", PrimitiveType.Cylinder,
                new Vector3(0f, 7.38f, -20.15f),
                new Vector3(1.25f, 0.55f, 1.25f), Roof);
            CreatePart("Office Beacon", PrimitiveType.Sphere,
                new Vector3(0f, 8.05f, -20.15f),
                Vector3.one * 0.28f, Lantern);

            // Front rhythm: posts, inset windows and shallow entrance steps.
            for (int i = -2; i <= 2; i++)
            {
                float x = i * 1.85f;
                if (i != 0)
                {
                    CreatePart($"Office Front Post {i}", PrimitiveType.Cube,
                        new Vector3(x, 2.25f, -17.33f),
                        new Vector3(0.24f, 2.75f, 0.24f), StoneLight);
                }
            }

            for (int side = -1; side <= 1; side += 2)
            {
                CreatePart($"Office Tall Window {side}", PrimitiveType.Cube,
                    new Vector3(side * 2.35f, 2.55f, -17.26f),
                    new Vector3(1.05f, 1.45f, 0.10f), Glass);
                CreatePart($"Office Wing Window {side}", PrimitiveType.Cube,
                    new Vector3(side * 6.15f, 2.05f, -17.99f),
                    new Vector3(1.15f, 1.0f, 0.10f), Glass);
            }

            CreatePart("Office Step Lower", PrimitiveType.Cube,
                new Vector3(0f, 0.62f, -16.72f),
                new Vector3(4.0f, 0.22f, 1.4f), StoneLight);
            CreatePart("Office Step Upper", PrimitiveType.Cube,
                new Vector3(0f, 0.79f, -17.10f),
                new Vector3(3.2f, 0.22f, 0.95f), StoneLight);

            CreateSignBoard("Harbor Office Sign Board",
                new Vector3(0f, 4.48f, -17.18f), new Vector3(5.7f, 0.78f, 0.16f));
        }

        private void BuildTradeWarehouseDetail()
        {
            // Loading canopy and bays establish function at a glance and break
            // the huge blank warehouse face seen in the accepted HUD capture.
            CreatePart("Trade Loading Awning", PrimitiveType.Cube,
                new Vector3(16f, 3.55f, -6.45f),
                new Vector3(9.4f, 0.18f, 1.9f), Roof,
                Quaternion.Euler(7f, 0f, 0f));

            for (int bay = -1; bay <= 1; bay++)
            {
                float x = 16f + bay * 2.75f;
                CreatePart($"Trade Loading Door {bay}", PrimitiveType.Cube,
                    new Vector3(x, 2.05f, -6.92f),
                    new Vector3(1.65f, 2.25f, 0.12f), Timber);
                CreatePart($"Trade Door Header {bay}", PrimitiveType.Cube,
                    new Vector3(x, 3.25f, -6.86f),
                    new Vector3(1.9f, 0.18f, 0.20f), TimberLight);
                CreatePart($"Trade Awning Post {bay}", PrimitiveType.Cube,
                    new Vector3(x, 2.25f, -5.78f),
                    new Vector3(0.16f, 2.4f, 0.16f), Timber);
            }

            CreatePart("Trade East Annex", PrimitiveType.Cube,
                new Vector3(21.25f, 1.95f, -10.3f),
                new Vector3(2.7f, 2.8f, 4.6f), Plaster);
            CreatePart("Trade East Annex Roof", PrimitiveType.Cube,
                new Vector3(21.25f, 3.46f, -10.3f),
                new Vector3(3.0f, 0.18f, 4.9f), Roof);

            for (int chimney = 0; chimney < 2; chimney++)
            {
                CreatePart($"Trade Roof Vent {chimney}", PrimitiveType.Cylinder,
                    new Vector3(14.0f + chimney * 4.0f, 5.75f, -10.2f),
                    new Vector3(0.34f, 0.75f, 0.34f), Timber);
                CreatePart($"Trade Roof Vent Cap {chimney}", PrimitiveType.Cylinder,
                    new Vector3(14.0f + chimney * 4.0f, 6.52f, -10.2f),
                    new Vector3(0.48f, 0.10f, 0.48f), Roof);
            }

            CreateSignBoard("Trade Sign Board",
                new Vector3(16f, 4.35f, -6.80f), new Vector3(4.4f, 0.72f, 0.16f));
        }

        private void BuildShipyardDetail()
        {
            CreatePart("Shipyard Work Awning", PrimitiveType.Cube,
                new Vector3(-16f, 3.50f, -7.25f),
                new Vector3(8.2f, 0.18f, 1.65f), Roof,
                Quaternion.Euler(7f, 0f, 0f));

            for (int post = -1; post <= 1; post++)
            {
                float x = -16f + post * 2.9f;
                CreatePart($"Shipyard Awning Post {post}", PrimitiveType.Cube,
                    new Vector3(x, 2.25f, -6.75f),
                    new Vector3(0.17f, 2.35f, 0.17f), Timber);
                CreatePart($"Shipyard Brace {post}", PrimitiveType.Cube,
                    new Vector3(x + 0.35f, 3.02f, -7.0f),
                    new Vector3(1.05f, 0.14f, 0.14f), TimberLight,
                    Quaternion.Euler(0f, 0f, 35f));
            }

            CreatePart("Shipyard West Shed", PrimitiveType.Cube,
                new Vector3(-20.9f, 1.75f, -10.45f),
                new Vector3(2.8f, 2.4f, 4.0f), TimberLight);
            CreatePart("Shipyard West Shed Roof", PrimitiveType.Cube,
                new Vector3(-20.9f, 3.05f, -10.45f),
                new Vector3(3.1f, 0.18f, 4.3f), Roof);

            // Barrel-like cylinders and lumber stacks add readable working-yard clutter.
            for (int i = 0; i < 3; i++)
            {
                CreatePart($"Shipyard Barrel {i}", PrimitiveType.Cylinder,
                    new Vector3(-18.7f + i * 0.9f, 1.42f, -6.0f),
                    new Vector3(0.42f, 0.55f, 0.42f), TimberLight,
                    Quaternion.Euler(90f, 0f, 0f));
            }
            for (int i = 0; i < 4; i++)
            {
                CreatePart($"Shipyard Lumber {i}", PrimitiveType.Cube,
                    new Vector3(-12.5f, 1.35f + i * 0.18f, -9.0f + i * 0.08f),
                    new Vector3(3.0f, 0.14f, 0.32f), TimberLight,
                    Quaternion.Euler(0f, 8f, 0f));
            }

            CreateSignBoard("Shipyard Sign Board",
                new Vector3(-16f, 4.22f, -7.58f), new Vector3(4.2f, 0.72f, 0.16f));
        }

        private void BuildQuayClutter()
        {
            // Repeated low bollards tie the shore wall to the timber piers while
            // keeping the navigation lane and all docking volumes untouched.
            for (int x = -18; x <= 18; x += 6)
            {
                CreatePart($"Quay Bollard {x}", PrimitiveType.Cylinder,
                    new Vector3(x + 1.45f, 1.92f, -14.75f),
                    new Vector3(0.30f, 0.42f, 0.30f), Timber);
                CreatePart($"Quay Bollard Cap {x}", PrimitiveType.Cylinder,
                    new Vector3(x + 1.45f, 2.35f, -14.75f),
                    new Vector3(0.39f, 0.08f, 0.39f), TimberLight);
            }
        }

        private void TuneWorldLabels()
        {
            TuneLabel("TERSANE Label", "TERSANE",
                new Vector3(-16f, 4.27f, -7.35f), 0.052f);
            TuneLabel("TİCARET Label", "TİCARET",
                new Vector3(16f, 4.40f, -6.56f), 0.052f);
            TuneLabel("LİMAN İDARESİ Label", "LİMAN İDARESİ",
                new Vector3(0f, 4.52f, -16.94f), 0.046f);
            TuneLabel("AÇIK DENİZ Label", "AÇIK DENİZ",
                new Vector3(0f, 2.25f, 58f), 0.054f);
        }

        private void TuneLabel(string objectName, string value,
            Vector3 localPosition, float characterSize)
        {
            Transform label = transform.Find(objectName);
            if (label == null) return;
            label.localPosition = localPosition;
            TextMesh text = label.GetComponent<TextMesh>();
            if (text == null) return;
            text.text = value;
            text.fontSize = 44;
            text.characterSize = characterSize;
            text.color = SignInk;
        }

        private void CreateSignBoard(string objectName, Vector3 position, Vector3 scale)
        {
            CreatePart(objectName, PrimitiveType.Cube, position, scale, Timber);
            CreatePart(objectName + " Trim", PrimitiveType.Cube,
                position + Vector3.forward * 0.10f,
                new Vector3(scale.x + 0.18f, scale.y + 0.18f, 0.045f),
                StoneLight);
            CreatePart(objectName + " Face", PrimitiveType.Cube,
                position + Vector3.forward * 0.135f,
                new Vector3(scale.x, scale.y, 0.035f), Timber);
        }

        private GameObject CreatePart(string objectName, PrimitiveType primitive,
            Vector3 position, Vector3 scale, Color color,
            Quaternion? rotation = null)
        {
            GameObject part = GameObject.CreatePrimitive(primitive);
            part.name = objectName;
            part.transform.SetParent(transform, false);
            part.transform.localPosition = position;
            part.transform.localRotation = rotation ?? Quaternion.identity;
            part.transform.localScale = scale;

            Collider collider = part.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
                Destroy(collider);
            }

            Renderer renderer = part.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = ResolveMaterial();
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                block.SetColor("_BaseColor", color);
                block.SetColor("_Color", color);
                renderer.SetPropertyBlock(block);
            }
            return part;
        }

        private Material ResolveMaterial()
        {
            if (sharedMaterial != null) return sharedMaterial;
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) return null;
            sharedMaterial = new Material(shader)
            {
                name = "Harbor Environment Polish Material"
            };
            sharedMaterial.SetFloat("_Smoothness", 0.13f);
            sharedMaterial.SetFloat("_Metallic", 0f);
            return sharedMaterial;
        }

        private void OnDestroy()
        {
            if (sharedMaterial != null)
            {
                Destroy(sharedMaterial);
            }
        }
    }

    internal sealed class HarborEnvironmentPolishBootstrap : MonoBehaviour
    {
        private float nextScan;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<HarborEnvironmentPolishBootstrap>() != null)
            {
                return;
            }
            new GameObject("Harbor Environment Polish Bootstrap")
                .AddComponent<HarborEnvironmentPolishBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.35f;

            PrototypeHarborVisualDirector harbor =
                FindFirstObjectByType<PrototypeHarborVisualDirector>();
            if (harbor == null) return;

            if (harbor.GetComponent<HarborEnvironmentPolish>() == null)
            {
                harbor.gameObject.AddComponent<HarborEnvironmentPolish>();
            }
            Destroy(gameObject);
        }
    }
}
