using System;
using Seaborn.Combat;
using Seaborn.Expeditions;
using Seaborn.Equipment;
using Seaborn.Harbor;
using Seaborn.Recovery;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class HarpoonHuntingController :
        MonoBehaviour
    {
        [Header("Harpoon profile")]
        [SerializeField]
        private string selectedHarpoonId = "light_2kg";

        [SerializeField, Min(1f)]
        private float maximumRange = 12f;

        [SerializeField, Min(0.1f)]
        private float reloadDuration = 1.8f;

        [SerializeField, Min(0)]
        private int lightHarpoonStock = 20;

        [SerializeField, Min(0)]
        private int heavyHarpoonStock = 10;

        [SerializeField, Min(0f)]
        private float harpoonDamage = 34f;

        [SerializeField, Min(0.1f)]
        private float flightDuration = 0.72f;

        [SerializeField, Min(0f)]
        private float arcHeight = 0.65f;

        [SerializeField]
        private float aimPlaneHeight = 0.75f;

        private float equipmentDamageMultiplier = 1f;
        private float equipmentReloadMultiplier = 1f;

        public event Action HuntingStateChanged;

        public string SelectedHarpoonId => selectedHarpoonId;
        public HarpoonDefinition SelectedHarpoon { get; private set; }

        public bool IsAiming { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public int HarpoonStock =>
            GetHarpoonStock(selectedHarpoonId);
        public int LightHarpoonStock => lightHarpoonStock;
        public int HeavyHarpoonStock => heavyHarpoonStock;
        public int TotalHarpoonStock =>
            lightHarpoonStock + heavyHarpoonStock;
        public float ReloadRemaining =>
            Mathf.Max(0f, nextFireTime - Time.time);
        public float ReloadProgress =>
            EffectiveReloadDuration <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(
                    ReloadRemaining / EffectiveReloadDuration
                );

        private float EffectiveReloadDuration =>
            reloadDuration * equipmentReloadMultiplier;
        public bool IsBlockedBySafeHarbor =>
            !PrototypeSafeHarborProtection.AllowsWeapons(
                gameObject
            );
        public bool CanFire =>
            !IsBlockedBySafeHarbor &&
            IsAiming &&
            hasAimPoint &&
            HarpoonStock > 0 &&
            ReloadRemaining <= 0f;

        private UnityEngine.Camera aimCamera;
        private LineRenderer aimLine;
        private bool hasAimPoint;
        private float nextFireTime;

        private void Awake()
        {
            ApplyHarpoonProfile();
            aimCamera = UnityEngine.Camera.main;
            CreateAimLine();
        }

        private void Update()
        {
            if (aimCamera == null)
            {
                aimCamera = UnityEngine.Camera.main;
            }

            bool shiftHeld =
                Keyboard.current != null &&
                Keyboard.current.leftShiftKey.isPressed;

            if (shiftHeld &&
                Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                TrySelectHarpoon("light_2kg");
            }
            else if (shiftHeld &&
                     Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                TrySelectHarpoon("heavy_4kg");
            }

            bool wantsToAim =
                !IsBlockedBySafeHarbor &&
                shiftHeld;

            if (IsAiming != wantsToAim)
            {
                IsAiming = wantsToAim;
                HuntingStateChanged?.Invoke();
            }

            hasAimPoint =
                IsAiming &&
                TryUpdateAimPoint();

            aimLine.enabled = hasAimPoint;

            if (hasAimPoint)
            {
                aimLine.SetPosition(
                    0,
                    transform.position +
                    Vector3.up * 0.7f
                );
                aimLine.SetPosition(1, AimPoint);
            }

            if (CanFire &&
                Mouse.current != null &&
                Mouse.current.leftButton
                    .wasPressedThisFrame)
            {
                Fire();
            }
        }

        public bool TrySelectHarpoon(string harpoonId)
        {
            if (!EquipmentCatalog.TryGetHarpoon(
                    harpoonId,
                    out HarpoonDefinition definition))
            {
                return false;
            }

            selectedHarpoonId = harpoonId;
            ApplyHarpoonProfile(definition);
            HuntingStateChanged?.Invoke();
            return true;
        }

        public void AddHarpoons(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (IsHeavyHarpoon(selectedHarpoonId))
            {
                heavyHarpoonStock += amount;
            }
            else
            {
                lightHarpoonStock += amount;
            }

            HuntingStateChanged?.Invoke();
        }

        public void SetEquipmentModifiers(
            float damageMultiplier,
            float reloadMultiplier)
        {
            equipmentDamageMultiplier =
                Mathf.Max(0.1f, damageMultiplier);
            equipmentReloadMultiplier =
                Mathf.Clamp(reloadMultiplier, 0.35f, 2f);
            HuntingStateChanged?.Invoke();
        }

        public void RestoreHarpoonStock(int amount)
        {
            lightHarpoonStock = Mathf.Max(0, amount);
            heavyHarpoonStock = 0;
            HuntingStateChanged?.Invoke();
        }

        public void RestoreHarpoonStocks(
            int light,
            int heavy,
            string selectedId)
        {
            lightHarpoonStock = Mathf.Max(0, light);
            heavyHarpoonStock = Mathf.Max(0, heavy);

            if (!string.IsNullOrWhiteSpace(selectedId))
            {
                TrySelectHarpoon(selectedId);
            }

            HuntingStateChanged?.Invoke();
        }

        public int GetHarpoonStock(string harpoonId)
        {
            return IsHeavyHarpoon(harpoonId)
                ? heavyHarpoonStock
                : lightHarpoonStock;
        }

        private bool TryUpdateAimPoint()
        {
            if (aimCamera == null ||
                Mouse.current == null)
            {
                return false;
            }

            Ray ray = aimCamera.ScreenPointToRay(
                Mouse.current.position.ReadValue()
            );
            Plane plane = new Plane(
                Vector3.up,
                new Vector3(0f, aimPlaneHeight, 0f)
            );

            if (!plane.Raycast(ray, out float enter))
            {
                return false;
            }

            Vector3 rawPoint = ray.GetPoint(enter);
            Vector3 fromShip =
                rawPoint - transform.position;
            fromShip.y = 0f;

            float distance = fromShip.magnitude;
            if (distance < 1.5f)
            {
                return false;
            }

            AimPoint =
                transform.position +
                fromShip.normalized *
                Mathf.Min(distance, maximumRange);
            AimPoint = new Vector3(
                AimPoint.x,
                aimPlaneHeight,
                AimPoint.z
            );
            return true;
        }

        private void Fire()
        {
            GameObject projectileObject =
                new GameObject("Harpoon Projectile");
            projectileObject.transform.position =
                transform.position +
                Vector3.up * 0.8f;

            HarpoonProjectile projectile =
                projectileObject.AddComponent<
                    HarpoonProjectile>();
            projectile.Launch(
                transform,
                AimPoint,
                flightDuration,
                arcHeight,
                harpoonDamage *
                    equipmentDamageMultiplier
            );

            if (IsHeavyHarpoon(selectedHarpoonId))
            {
                heavyHarpoonStock--;
            }
            else
            {
                lightHarpoonStock--;
            }
            nextFireTime =
                Time.time + EffectiveReloadDuration;
            HuntingStateChanged?.Invoke();
        }

        private static bool IsHeavyHarpoon(string harpoonId)
        {
            return string.Equals(
                harpoonId,
                "heavy_4kg",
                StringComparison.OrdinalIgnoreCase
            );
        }

        private void ApplyHarpoonProfile()
        {
            if (EquipmentCatalog.TryGetHarpoon(
                    selectedHarpoonId,
                    out HarpoonDefinition definition))
            {
                ApplyHarpoonProfile(definition);
                return;
            }

            Debug.LogWarning(
                $"Unknown harpoon profile: {selectedHarpoonId}",
                this
            );
        }

        private void ApplyHarpoonProfile(
            HarpoonDefinition definition)
        {
            SelectedHarpoon = definition;
            harpoonDamage = Mathf.Max(0f, definition.damage);
            maximumRange = Mathf.Max(1f, definition.range);
            reloadDuration = Mathf.Max(
                0.1f,
                definition.reloadDuration
            );
            flightDuration = Mathf.Max(
                0.1f,
                definition.flightDuration
            );
            arcHeight = Mathf.Max(0f, definition.arcHeight);
        }

        private void CreateAimLine()
        {
            GameObject lineObject =
                new GameObject("Harpoon Aim Line");
            lineObject.transform.SetParent(
                transform,
                false
            );

            aimLine =
                lineObject.AddComponent<LineRenderer>();
            aimLine.positionCount = 2;
            aimLine.useWorldSpace = true;
            aimLine.widthMultiplier = 0.035f;
            aimLine.numCapVertices = 2;
            aimLine.startColor =
                new Color(0.76f, 0.9f, 0.78f, 0.8f);
            aimLine.endColor =
                new Color(0.9f, 0.95f, 0.8f, 0.25f);
            aimLine.enabled = false;

            Material material =
                Resources.Load<Material>(
                    "PrototypeCombatParticle"
                );
            if (material != null)
            {
                aimLine.sharedMaterial = material;
            }
        }
    }

    internal sealed class HuntingBootstrap :
        MonoBehaviour
    {
        private const float ScanInterval = 0.5f;
        private float nextScanTime;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        [RuntimeInitializeOnLoadMethod(
            RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (FindFirstObjectByType<HuntingBootstrap>() !=
                null)
            {
                return;
            }

            GameObject bootstrap =
                new GameObject("Prototype Hunting Bootstrap");
            bootstrap.AddComponent<HuntingBootstrap>();
        }

        private void Update()
        {
            if (Time.unscaledTime < nextScanTime)
            {
                return;
            }

            nextScanTime =
                Time.unscaledTime + ScanInterval;

            ManualBroadsideAimController player =
                FindFirstObjectByType<
                    ManualBroadsideAimController>();

            if (player == null)
            {
                return;
            }

            Seaborn.World.PrototypeExpeditionRegionDirector
                .EnsureCreated(player.transform);
            bool isHarbor =
                Seaborn.World.PrototypeExpeditionRegionDirector
                    .IsHarborScene;

            if (player.GetComponent<
                    HarpoonHuntingController>() == null)
            {
                player.gameObject.AddComponent<
                    HarpoonHuntingController>();
            }

            if (player.GetComponent<
                    PrototypeSilverWallet>() == null)
            {
                player.gameObject.AddComponent<
                    PrototypeSilverWallet>();
            }

            if (player.GetComponent<
                    PrototypeHuntCargo>() == null)
            {
                player.gameObject.AddComponent<
                    PrototypeHuntCargo>();
            }

            if (player.GetComponent<
                    HuntCargoLossOnSinking>() == null)
            {
                player.gameObject.AddComponent<
                    HuntCargoLossOnSinking>();
            }

            Seaborn.Progression
                .PrototypeRegionalLootInventory
                .EnsureAttached(player.transform);
            Seaborn.Progression.PrototypeShipEquipment
                .EnsureAttached(player.transform);
            Seaborn.Ship.ShipLoadout
                .EnsureAttached(player.transform);
            Seaborn.Progression
                .PrototypeEquipmentInventory
                .EnsureAttached(player.transform);
            Seaborn.Progression
                .PrototypeFleetInventory
                .EnsureAttached(player.transform);

            if (isHarbor)
            {
                Seaborn.Harbor
                    .PrototypeHarborVisualDirector
                    .EnsureCreated(player.transform.position);
                Seaborn.Harbor
                    .PrototypeHarborDockingDirector
                    .EnsureCreated(player.transform);
                PrototypeHarborDeliveryZone.EnsureCreated(
                    player.transform
                );
                PrototypeHarborServices.EnsureAttached(
                    player.transform
                );
                PrototypeContractBoard.EnsureCreated(
                    player.transform
                );
                PrototypeHarborPreparationPanel.EnsureCreated(
                    player.transform
                );
                Seaborn.Harbor.UI
                    .PrototypeShipyardUpgradePanel
                    .EnsureCreated(player.transform);
                Seaborn.Harbor.UI
                    .PrototypeShipyardLoadoutPanel
                    .EnsureCreated(player.transform);
                Seaborn.Harbor.UI
                    .PrototypeShipMarketPanel
                    .EnsureCreated(player.transform);
                Seaborn.Harbor.UI
                    .PrototypeHarborContractPanel
                    .EnsureCreated(player.transform);
            }
            else
            {
                PrototypeSeaCreatureSpawner.EnsureSpawned(
                    player.transform.position
                );
                Seaborn.Ship.PrototypeEnemyFleetDirector
                    .EnsureCreated(player.transform);
                PrototypeExpeditionDirector.EnsureCreated(
                    player.transform
                );
            }
            PrototypeShipRecoveryDirector.EnsureCreated(
                player.transform
            );
            Seaborn.Persistence.PrototypeProgressPersistence
                .EnsureAttached(player.transform);
            Seaborn.UI.PrototypeGameplayHud.EnsureCreated(
                player.transform
            );
        }
    }
}
