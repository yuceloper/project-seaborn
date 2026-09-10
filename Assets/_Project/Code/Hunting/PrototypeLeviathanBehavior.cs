using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeSeaCreature))]
    public sealed class PrototypeLeviathanBehavior : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float chargeSpeed = 3.15f;
        [SerializeField, Min(0f)] private float turnSpeed = 135f;
        [SerializeField, Min(0f)] private float chargeWindup = 0.7f;
        [SerializeField, Min(0f)] private float ramDamage = 22f;
        [SerializeField, Min(0.1f)] private float ramCooldown = 2.4f;
        [SerializeField, Min(0.1f)] private float ramRange = 6.4f;
        [SerializeField, Min(0f)] private float retreatSpeed = 2.45f;
        [SerializeField, Min(0f)] private float retreatDuration = 1.2f;
        [SerializeField, Min(0.05f)] private float wakeInterval = 0.2f;

        public bool IsAggressive =>
            hunter != null &&
            creature != null &&
            !creature.IsHarvested;

        private PrototypeSeaCreature creature;
        private GameObject hunter;
        private float chargeBeginsAt;
        private float nextRamTime;
        private float nextWakeTime;
        private float retreatUntil;
        private bool needsChargeWarning;
        private bool hasAwakened;

        private void Awake()
        {
            creature = GetComponent<PrototypeSeaCreature>();
        }

        private void OnEnable()
        {
            if (creature == null)
            {
                creature = GetComponent<PrototypeSeaCreature>();
            }

            creature.Harpooned += HandleHarpooned;
        }

        private void OnDisable()
        {
            if (creature == null)
            {
                return;
            }

            creature.Harpooned -= HandleHarpooned;
            creature.IsMovementExternallyControlled = false;
        }

        private void Update()
        {
            if (!IsAggressive)
            {
                return;
            }

            if (Time.time < retreatUntil)
            {
                RetreatFromHunter();
                return;
            }

            if (needsChargeWarning)
            {
                BeginCharge();
            }

            Vector3 toHunter =
                hunter.transform.position - transform.position;
            toHunter.y = 0f;

            if (toHunter.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float distance = toHunter.magnitude;
            RotateTowards(toHunter / distance);

            if (Time.time < chargeBeginsAt)
            {
                return;
            }

            if (distance > ramRange)
            {
                transform.position +=
                    transform.forward *
                    chargeSpeed *
                    Time.deltaTime;
                KeepAtWaterline();
                PlayWake();
                return;
            }

            TryRam(toHunter / distance);
        }

        private void HandleHarpooned(GameObject newHunter)
        {
            if (newHunter == null || creature.IsHarvested)
            {
                return;
            }

            hunter = newHunter;
            creature.IsMovementExternallyControlled = true;

            if (hasAwakened)
            {
                return;
            }

            hasAwakened = true;
            BeginCharge();

            Debug.Log(
                "Stormjaw öfkelendi ve hücuma hazırlanıyor.",
                this
            );
        }

        private void BeginCharge()
        {
            needsChargeWarning = false;
            chargeBeginsAt = Time.time + chargeWindup;
            nextRamTime = Mathf.Max(nextRamTime, chargeBeginsAt);

            PrototypeCombatVfx.PlayLeviathanWarning(
                transform.position
            );
        }

        private void RetreatFromHunter()
        {
            Vector3 away =
                transform.position - hunter.transform.position;
            away.y = 0f;

            if (away.sqrMagnitude < 0.0001f)
            {
                away = -transform.forward;
            }

            RotateTowards(away.normalized);
            transform.position +=
                transform.forward *
                retreatSpeed *
                Time.deltaTime;
            KeepAtWaterline();
            PlayWake();
        }

        private void RotateTowards(Vector3 direction)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );
        }

        private void PlayWake()
        {
            if (Time.time < nextWakeTime)
            {
                return;
            }

            nextWakeTime = Time.time + wakeInterval;

            PrototypeCombatVfx.PlayLeviathanWake(
                transform.position -
                transform.forward * 1.35f,
                -transform.forward
            );
        }

        private void TryRam(Vector3 direction)
        {
            if (Time.time < nextRamTime)
            {
                return;
            }

            IDamageable damageable =
                hunter.GetComponentInChildren<IDamageable>();

            if (damageable == null || damageable.IsSunk)
            {
                return;
            }

            Vector3 impactPoint =
                Vector3.Lerp(
                    transform.position,
                    hunter.transform.position,
                    0.62f
                );

            damageable.ApplyDamage(
                new DamageInfo(
                    ramDamage,
                    impactPoint,
                    direction,
                    gameObject
                )
            );

            nextRamTime = Time.time + ramCooldown;
            retreatUntil = Time.time + retreatDuration;
            needsChargeWarning = true;

            PrototypeCombatVfx.PlayLeviathanRam(
                impactPoint,
                direction
            );
            PrototypeCameraShake.Request(0.18f, 0.22f);

            Debug.Log(
                $"Stormjaw koçbaşı: {ramDamage:0} hasar.",
                this
            );
        }

        private void KeepAtWaterline()
        {
            Vector3 position = transform.position;
            position.y = 0.74f;
            transform.position = position;
        }
    }
}
