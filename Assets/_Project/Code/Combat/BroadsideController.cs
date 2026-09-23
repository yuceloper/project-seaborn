using System;
using System.Collections;
using System.Collections.Generic;
using Seaborn.Expeditions;
using Seaborn.Equipment;
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

        [Header("Ship configuration")]
        [SerializeField, Min(1)] private int cannonSlotCapacity = 6;
        [SerializeField, Min(1)] private int installedCannons = 6;

        [SerializeField, Min(0f)] private float cannonHitDamage = 25f;

        private CannonDefinition[] modularCannons;
        private int[] modularLevels;
        private float modularRangeCap;

        public void SetModularLoadout(string[] slots, float shipRange, int[] enhancements = null)
        {
            modularCannons = new CannonDefinition[slots.Length];
            modularLevels = new int[slots.Length];
            if (enhancements != null)
                Array.Copy(enhancements, modularLevels, Mathf.Min(enhancements.Length, slots.Length));
            modularRangeCap = Mathf.Max(0.1f, shipRange);
            cannonSlotCapacity = Mathf.Max(1, slots.Length);
            installedCannons = 0;
            projectileRange = 0.1f;
            broadsideCooldown = 0.1f;
            for (int i = 0; i < slots.Length; i++)
                if (EquipmentCatalog.TryGetCannon(slots[i], out var cannon))
                {
                    modularCannons[i] = cannon;
                    installedCannons++;
                    projectileRange = Mathf.Max(projectileRange, Mathf.Min(modularRangeCap, cannon.range));
                    broadsideCooldown = Mathf.Max(broadsideCooldown, cannon.reloadDuration);
                }
            AmmunitionStateChanged?.Invoke();
        }

        private int SlotAtMuzzle(Transform muzzle)
        {
            if (modularCannons == null) return -1;
            int index = portMuzzles == null ? -1 : Array.IndexOf(portMuzzles, muzzle);
            int slot = index >= 0 ? index * 2 : -1;
            if (slot < 0 && starboardMuzzles != null)
            {
                index = Array.IndexOf(starboardMuzzles, muzzle);
                if (index >= 0) slot = index * 2 + 1;
            }
            return slot >= 0 && slot < modularCannons.Length ? slot : -1;
        }

        private CannonDefinition CannonAtMuzzle(Transform muzzle)
        {
            int slot = SlotAtMuzzle(muzzle);
            return slot >= 0 ? modularCannons[slot] : null;
        }

        private float CannonDamageAtMuzzle(Transform muzzle)
        {
            int slot = SlotAtMuzzle(muzzle);
            return slot >= 0 && modularCannons[slot] != null
                ? modularCannons[slot].damage * Seaborn.Progression.CannonItem.DamageAt(modularLevels[slot])
                : cannonHitDamage;
        }

        public float GetBaseBroadsideReload(BroadsideSide side)
        {
            if (modularCannons == null) return broadsideCooldown;
            float duration = 0f;
            foreach (var muzzle in GetFiringMuzzles(side))
                duration = Mathf.Max(duration, CannonAtMuzzle(muzzle)?.reloadDuration ?? 0f);
            return duration;
        }

        private float nextPortFireTime;
        private float nextStarboardFireTime;
        private float portReloadDuration;
        private float starboardReloadDuration;
        private float equipmentDamageMultiplier = 1f;
        private float equipmentReloadMultiplier = 1f;
        private float skillDamageMultiplier = 1f;
        private float skillRangeMultiplier = 1f;
        private float skillReloadMultiplier = 1f;
        private float consumableDamageMultiplier = 1f;
        private float consumableReloadMultiplier = 1f;
        private float crewReloadMultiplier = 1f;

        public event Action AmmunitionStateChanged;
        public event Action<AmmunitionType, int> AmmunitionConsumed;
        public event Action<BroadsideSide> BroadsideFired;

        public AmmunitionType SelectedAmmunition => selectedAmmunition;
        public int CannonSlotCapacity => cannonSlotCapacity;
        public int InstalledCannons => Mathf.Min(installedCannons, cannonSlotCapacity);
        public float MaximumRange =>
            projectileRange *
            AmmunitionProfile.Get(selectedAmmunition).RangeMultiplier *
            skillRangeMultiplier;
        public float CooldownDuration =>
            broadsideCooldown *
            AmmunitionProfile.Get(selectedAmmunition).ReloadMultiplier *
            equipmentReloadMultiplier *
            skillReloadMultiplier *
            consumableReloadMultiplier *
            crewReloadMultiplier;
        public bool IsBlockedBySafeHarbor =>
            !PrototypeSafeHarborProtection.AllowsWeapons(gameObject);

        public void RestoreEnemyLife(int standard, int chain, int grapeshot)
        {
            StopAllCoroutines();
            nextPortFireTime = nextStarboardFireTime = 0f;
            portReloadDuration = starboardReloadDuration = 0f;
            RestorePersistentState(selectedAmmunition, standard, chain, grapeshot);
        }

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

        public void SetShipConfiguration(
            int slotCapacity,
            int cannonCount,
            float range)
        {
            cannonSlotCapacity = Mathf.Max(1, slotCapacity);
            installedCannons = Mathf.Clamp(cannonCount, 1, cannonSlotCapacity);
            projectileRange = Mathf.Max(0.1f, range);
            AmmunitionStateChanged?.Invoke();
        }

        public void SetCannonLoadout(
            int slotCapacity,
            int cannonCount,
            float range,
            float reloadDuration,
            float hitDamage)
        {
            modularCannons = null; // NPC/uniform battery compatibility.
            SetShipConfiguration(
                slotCapacity,
                cannonCount,
                range
            );
            broadsideCooldown = Mathf.Max(
                0.1f,
                reloadDuration
            );
            cannonHitDamage = Mathf.Max(0f, hitDamage);
            AmmunitionStateChanged?.Invoke();
        }

        public void SetSkillModifiers(
            float damageMultiplier,
            float rangeMultiplier,
            float reloadMultiplier)
        {
            skillDamageMultiplier =
                Mathf.Max(0.1f, damageMultiplier);
            skillRangeMultiplier =
                Mathf.Max(0.1f, rangeMultiplier);
            skillReloadMultiplier =
                Mathf.Clamp(reloadMultiplier, 0.35f, 2f);
            AmmunitionStateChanged?.Invoke();
        }

        public void SetConsumableModifiers(
            float damageMultiplier,
            float reloadMultiplier)
        {
            consumableDamageMultiplier =
                Mathf.Max(0.1f, damageMultiplier);
            consumableReloadMultiplier =
                Mathf.Clamp(reloadMultiplier, 0.35f, 2f);
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

        public void SetRuntimeMuzzles(
            Transform[] port,
            Transform[] starboard)
        {
            if (port != null && port.Length > 0)
                portMuzzles = port;
            if (starboard != null &&
                starboard.Length > 0)
            {
                starboardMuzzles = starboard;
            }
        }

        public void SetCrewReloadMultiplier(
            float reloadMultiplier)
        {
            crewReloadMultiplier =
                Mathf.Clamp(reloadMultiplier, 1f, 2f);
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

            Vector3 toTarget = Vector3.ProjectOnPlane(targetPoint - transform.position, Vector3.up);
            Vector3 outward = side == BroadsideSide.Port ? -transform.right : transform.right;
            // Reject a stale tracked aim point on the opposite side of the hull.
            if (toTarget.sqrMagnitude < 0.01f || Vector3.Dot(outward, toTarget.normalized) < 0.5f)
                return false;
            Transform[] muzzles = GetFiringMuzzles(side);
            if (cannonballPrefab == null || muzzles == null || muzzles.Length == 0) return false;

            int loadedCannons = Mathf.Min(
                muzzles.Length,
                GetAmmunitionStock(selectedAmmunition)
            );
            if (loadedCannons <= 0) return false;

            AmmunitionType firedType = selectedAmmunition;
            AmmunitionProfile profile = AmmunitionProfile.Get(firedType);
            ConsumeAmmunition(firedType, loadedCannons);
            AmmunitionConsumed?.Invoke(firedType, loadedCannons);
            SetReload(side, profile.ReloadMultiplier);
            StartCoroutine(FireBroadsideAt(
                muzzles,
                targetPoint,
                Mathf.Clamp01(accuracy),
                firedType,
                profile,
                loadedCannons
            ));

            if (GetComponent<ManualBroadsideAimController>() != null)
                PrototypeCameraShake.Request(0.02f, 0.08f);
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
            var npc = GetComponent<Seaborn.Ship.EnemyShipController>();
            return (npc == null || !npc.IsCivilian) && !IsBlockedBySafeHarbor &&
                   GetCooldownRemaining(side) <= 0f &&
                   GetAmmunitionStock(selectedAmmunition) > 0;
        }

        private Transform[] GetMuzzles(BroadsideSide side)
        {
            return side == BroadsideSide.Port ? portMuzzles : starboardMuzzles;
        }

        public int GetBroadsideCannonCount(BroadsideSide side)
        {
            var npc = GetComponent<Seaborn.Ship.EnemyShipController>();
            return npc != null && npc.IsCivilian ? 0 : GetFiringMuzzles(side).Length;
        }

        private Transform[] GetFiringMuzzles(BroadsideSide side)
        {
            // InstalledCannons is the total ship loadout. Alternate assignments
            // port/starboard; an odd final cannon belongs to port.
            int budget = side == BroadsideSide.Port
                ? (InstalledCannons + 1) / 2 : InstalledCannons / 2;
            var result = new List<Transform>();
            Transform[] candidates = GetMuzzles(side);
            if (candidates == null) return result.ToArray();
            for (int i = 0; i < candidates.Length; i++)
            {
                Transform muzzle = candidates[i];
                if (modularCannons == null && result.Count >= budget) break;
                if (modularCannons != null)
                {
                    int slot = i * 2 + (side == BroadsideSide.Port ? 0 : 1);
                    if (slot >= modularCannons.Length || modularCannons[slot] == null) continue;
                }
                if (muzzle == null || !muzzle.gameObject.activeInHierarchy || result.Contains(muzzle)) continue;
                float localX = transform.InverseTransformPoint(muzzle.position).x;
                if (side == BroadsideSide.Port ? localX >= 0f : localX <= 0f) continue;
                result.Add(muzzle);
            }
            return result.ToArray();
        }

        private void SetReload(BroadsideSide side, float reloadMultiplier)
        {
            float duration =
                GetBaseBroadsideReload(side) *
                reloadMultiplier *
                equipmentReloadMultiplier *
                skillReloadMultiplier *
                consumableReloadMultiplier *
                crewReloadMultiplier;
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

            for (int cannonIndex = 0; cannonIndex < loadedCannons; cannonIndex++)
            {
                Transform muzzle = muzzles[cannonIndex];
                if (muzzle == null) continue;

                Vector3 baseDirection = targetPoint - muzzle.position;
                baseDirection.y = 0f;
                if (baseDirection.sqrMagnitude <= Mathf.Epsilon) continue;

                PrototypeCombatVfx.PlayMuzzleBurst(muzzle.position, baseDirection.normalized);


                for (int projectileIndex = 0;
                     projectileIndex < profile.ProjectilesPerCannon;
                     projectileIndex++)
                {
                    SpawnProjectile(
                        muzzle,
                        targetPoint,
                        spreadRadius,
                        ammunitionType,
                        profile
                    );
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

            CannonDefinition fitted = CannonAtMuzzle(muzzle);
            float range = fitted != null ? Mathf.Min(modularRangeCap, fitted.range) : projectileRange;
            float maximumRange = range * profile.RangeMultiplier * skillRangeMultiplier;
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
            projectile.ConfigureAbsoluteDamage(
                ammunitionType,
                CannonDamageAtMuzzle(muzzle) *
                    profile.DamageMultiplier *
                    equipmentDamageMultiplier *
                    skillDamageMultiplier *
                    consumableDamageMultiplier,
                profile.ProjectileScale
            );
            projectile.LaunchAt(transform, clampedTarget, duration, height);
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
