using System;
using System.Collections;
using Seaborn.Expeditions;
using UnityEngine;

namespace Seaborn.Combat
{
    public sealed class BroadsideController : MonoBehaviour
    {
        [Header("Projectile")]
        [SerializeField] private CannonballProjectile cannonballPrefab;
        [SerializeField, Min(1f)] private float projectileRange = 9f;
        [SerializeField, Min(0.1f)] private float projectileFlightDuration = 1.25f;
        [SerializeField, Min(0f)] private float projectileArcHeight = 2f;

        [Header("Muzzles")]
        [SerializeField] private Transform[] portMuzzles;
        [SerializeField] private Transform[] starboardMuzzles;

        [Header("Timing")]
        [SerializeField, Min(0f)] private float delayBetweenCannons = 0.18f;
        [SerializeField, Min(0f)] private float broadsideCooldown = 2.5f;

        [Header("Ammunition")]
        [SerializeField] private AmmunitionType selectedAmmunition = AmmunitionType.Standard;
        [SerializeField, Min(0)] private int standardStock = 120;
        [SerializeField, Min(0)] private int chainStock = 36;
        [SerializeField, Min(0)] private int grapeshotStock = 48;

        private float nextPortFireTime;
        private float nextStarboardFireTime;
        private float portReloadDuration;
        private float starboardReloadDuration;
        private float equipmentDamageMultiplier = 1f;
        private float equipmentReloadMultiplier = 1f;

        public event Action AmmunitionStateChanged;
        public event Action<BroadsideSide> BroadsideFired;

        public AmmunitionType SelectedAmmunition => selectedAmmunition;
        public float MaximumRange => projectileRange * AmmunitionProfile.Get(selectedAmmunition).RangeMultiplier;
        public float CooldownDuration =>
            broadsideCooldown *
            AmmunitionProfile.Get(selectedAmmunition).ReloadMultiplier *
            equipmentReloadMultiplier;
        public bool IsBlockedBySafeHarbor =>
            !PrototypeSafeHarborProtection.AllowsWeapons(gameObject);

        public bool TrySelectAmmunition(AmmunitionType ammunitionType)
        {
            if (selectedAmmunition == ammunitionType) return false;
            selectedAmmunition = ammunitionType;
            AmmunitionStateChanged?.Invoke();
            return true;
        }

        public int GetAmmunitionStock(AmmunitionType ammunitionType)
        {
            switch (ammunitionType)
            {
                case AmmunitionType.Chain: return chainStock;
                case AmmunitionType.Grapeshot: return grapeshotStock;
                default: return standardStock;
            }
        }

        public void AddAmmunition(AmmunitionType ammunitionType, int amount)
        {
            if (amount <= 0) return;
            SetAmmunitionStock(ammunitionType, GetAmmunitionStock(ammunitionType) + amount);
            AmmunitionStateChanged?.Invoke();
        }

        public void RestorePersistentState(
            AmmunitionType ammunitionType,
            int standard,
            int chain,
            int grapeshot)
        {
            selectedAmmunition = ammunitionType;
            standardStock = Mathf.Max(0, standard);
            chainStock = Mathf.Max(0, chain);
            grapeshotStock = Mathf.Max(0, grapeshot);
            AmmunitionStateChanged?.Invoke();
        }

        public void SetEquipmentModifiers(
            float damageMultiplier,
            float reloadMultiplier)
        {
            equipmentDamageMultiplier =
                Mathf.Max(0.1f, damageMultiplier);
            equipmentReloadMultiplier =
                Mathf.Clamp(reloadMultiplier, 0.35f, 2f);
            AmmunitionStateChanged?.Invoke();
        }

        public bool TryFire(BroadsideSide side)
        {
            Vector3 direction = side == BroadsideSide.Port ? -transform.right : transform.right;
            return TryFireAt(side, transform.position + direction * MaximumRange, 1f);
        }

        public bool TryFireAt(BroadsideSide side, Vector3 targetPoint, float accuracy)
        {
            if (!CanFire(side)) return false;

            Transform[] muzzles = GetMuzzles(side);
            if (cannonballPrefab == null || muzzles == null || muzzles.Length == 0) return false;

            int loadedCannons = Mathf.Min(CountValidMuzzles(muzzles), GetAmmunitionStock(selectedAmmunition));
            if (loadedCannons <= 0) return false;

            AmmunitionType firedType = selectedAmmunition;
            AmmunitionProfile profile = AmmunitionProfile.Get(firedType);
            ConsumeAmmunition(firedType, loadedCannons);
            SetReload(side, profile.ReloadMultiplier);
            StartCoroutine(FireBroadsideAt(
                muzzles,
                targetPoint,
                Mathf.Clamp01(accuracy),
                firedType,
                profile,
                loadedCannons
            ));

            BroadsideFired?.Invoke(side);
            AmmunitionStateChanged?.Invoke();
            return true;
        }

        public float GetCooldownRemaining(BroadsideSide side)
        {
            float nextFireTime = side == BroadsideSide.Port ? nextPortFireTime : nextStarboardFireTime;
            return Mathf.Max(0f, nextFireTime - Time.time);
        }

        public float GetReloadDuration(BroadsideSide side)
        {
            return side == BroadsideSide.Port ? portReloadDuration : starboardReloadDuration;
        }

        public float GetReloadProgress(BroadsideSide side)
        {
            float duration = GetReloadDuration(side);
            return duration <= 0f
                ? 1f
                : 1f - Mathf.Clamp01(GetCooldownRemaining(side) / duration);
        }

        private bool CanFire(BroadsideSide side)
        {
            return !IsBlockedBySafeHarbor &&
                   GetCooldownRemaining(side) <= 0f &&
                   GetAmmunitionStock(selectedAmmunition) > 0;
        }

        private Transform[] GetMuzzles(BroadsideSide side)
        {
            return side == BroadsideSide.Port ? portMuzzles : starboardMuzzles;
        }

        private void SetReload(BroadsideSide side, float reloadMultiplier)
        {
            float duration =
                broadsideCooldown *
                reloadMultiplier *
                equipmentReloadMultiplier;
            if (side == BroadsideSide.Port)
            {
                portReloadDuration = duration;
                nextPortFireTime = Time.time + duration;
            }
            else
            {
                starboardReloadDuration = duration;
                nextStarboardFireTime = Time.time + duration;
            }
        }

        private IEnumerator FireBroadsideAt(
            Transform[] muzzles,
            Vector3 targetPoint,
            float accuracy,
            AmmunitionType ammunitionType,
            AmmunitionProfile profile,
            int loadedCannons)
        {
            float spreadRadius = Mathf.Lerp(1.8f, 0.12f, accuracy) * profile.SpreadMultiplier;
            int firedCannons = 0;

            foreach (Transform muzzle in muzzles)
            {
                if (muzzle == null || firedCannons >= loadedCannons) continue;

                Vector3 baseDirection = targetPoint - muzzle.position;
                baseDirection.y = 0f;
                if (baseDirection.sqrMagnitude <= Mathf.Epsilon) continue;

                PrototypeCombatVfx.PlayMuzzleBurst(muzzle.position, baseDirection.normalized);
                PrototypeCameraShake.Request(0.06f, 0.08f);

                for (int index = 0; index < profile.ProjectilesPerCannon; index++)
                {
                    SpawnProjectile(muzzle, targetPoint, spreadRadius, ammunitionType, profile);
                }

                firedCannons++;
                if (delayBetweenCannons > 0f && firedCannons < loadedCannons)
                    yield return new WaitForSeconds(delayBetweenCannons);
            }
        }

        private void SpawnProjectile(
            Transform muzzle,
            Vector3 targetPoint,
            float spreadRadius,
            AmmunitionType ammunitionType,
            AmmunitionProfile profile)
        {
            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spreadRadius;
            Vector3 scatteredTarget = targetPoint + new Vector3(randomOffset.x, 0f, randomOffset.y);
            Vector3 toTarget = scatteredTarget - muzzle.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= Mathf.Epsilon) return;

            float maximumRange = projectileRange * profile.RangeMultiplier;
            float distance = Mathf.Clamp(toTarget.magnitude, 0.1f, maximumRange);
            Vector3 clampedTarget = muzzle.position + toTarget.normalized * distance;
            clampedTarget.y = targetPoint.y;

            float distanceRatio = Mathf.Clamp01(distance / maximumRange);
            float duration = projectileFlightDuration * Mathf.Lerp(0.4f, 1f, distanceRatio);
            float height = projectileArcHeight * Mathf.Lerp(0.35f, 1f, distanceRatio);

            CannonballProjectile projectile = Instantiate(
                cannonballPrefab,
                muzzle.position,
                Quaternion.LookRotation(toTarget.normalized, Vector3.up)
            );
            projectile.Configure(
                ammunitionType,
                profile.DamageMultiplier *
                    equipmentDamageMultiplier,
                profile.ProjectileScale
            );
            projectile.LaunchAt(transform, clampedTarget, duration, height);
        }

        private static int CountValidMuzzles(Transform[] muzzles)
        {
            int count = 0;
            foreach (Transform muzzle in muzzles)
                if (muzzle != null) count++;
            return count;
        }

        private void ConsumeAmmunition(AmmunitionType ammunitionType, int amount)
        {
            SetAmmunitionStock(
                ammunitionType,
                Mathf.Max(0, GetAmmunitionStock(ammunitionType) - amount)
            );
        }

        private void SetAmmunitionStock(AmmunitionType ammunitionType, int value)
        {
            value = Mathf.Max(0, value);
            switch (ammunitionType)
            {
                case AmmunitionType.Chain: chainStock = value; break;
                case AmmunitionType.Grapeshot: grapeshotStock = value; break;
                default: standardStock = value; break;
            }
        }
    }
}
