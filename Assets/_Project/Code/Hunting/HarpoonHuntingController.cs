using System;
using Seaborn.Combat;
using Seaborn.Expeditions;
using Seaborn.Harbor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class HarpoonHuntingController :
        MonoBehaviour
    {
        [SerializeField, Min(1f)]
        private float maximumRange = 12f;

        [SerializeField, Min(0.1f)]
        private float reloadDuration = 1.8f;

        [SerializeField, Min(0)]
        private int harpoonStock = 30;

        [SerializeField, Min(0f)]
        private float harpoonDamage = 34f;

        [SerializeField, Min(0.1f)]
        private float flightDuration = 0.72f;

        [SerializeField, Min(0f)]
        private float arcHeight = 0.65f;

        [SerializeField]
        private float aimPlaneHeight = 0.75f;

        public event Action HuntingStateChanged;

        public bool IsAiming { get; private set; }
        public Vector3 AimPoint { get; private set; }
        public int HarpoonStock => harpoonStock;
        public float ReloadRemaining =>
            Mathf.Max(0f, nextFireTime - Time.time);
        public float ReloadProgress =>
            reloadDuration <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(
                    ReloadRemaining / reloadDuration
                );
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
            aimCamera = UnityEngine.Camera.main;
            CreateAimLine();
        }

        private void Update()
        {
            if (aimCamera == null)
            {
                aimCamera = UnityEngine.Camera.main;
            }

            bool wantsToAim =
                !IsBlockedBySafeHarbor &&
                Keyboard.current != null &&
                Keyboard.current.leftShiftKey.isPressed;

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

        public void AddHarpoons(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            harpoonStock += amount;
            HuntingStateChanged?.Invoke();
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
                harpoonDamage
            );

            harpoonStock--;
            nextFireTime =
                Time.time + reloadDuration;
            HuntingStateChanged?.Invoke();
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
                    HuntCargoLossOnSinking>() == null)
            {
                player.gameObject.AddComponent<
                    HuntCargoLossOnSinking>();
            }

            PrototypeSeaCreatureSpawner.EnsureSpawned(
                player.transform.position
            );
            PrototypeHarborDeliveryZone.EnsureCreated(
                player.transform
            );
            PrototypeExpeditionDirector.EnsureCreated(
                player.transform
            );
            PrototypeHarborServices.EnsureAttached(
                player.transform
            );
            PrototypeContractBoard.EnsureCreated(
                player.transform
            );
        }
    }
}
