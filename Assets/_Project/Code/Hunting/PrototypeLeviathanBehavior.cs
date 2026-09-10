using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeSeaCreature))]
    public sealed class PrototypeLeviathanBehavior :
        MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float chargeSpeed = 2.65f;

        [SerializeField, Min(0f)]
        private float turnSpeed = 95f;

        [SerializeField, Min(0f)]
        private float ramDamage = 22f;

        [SerializeField, Min(0.1f)]
        private float ramCooldown = 2.4f;

        [SerializeField, Min(0.1f)]
        private float ramRange = 2.4f;

        public bool IsAggressive =>
            hunter != null &&
            creature != null &&
            !creature.IsHarvested;

        private PrototypeSeaCreature creature;
        private GameObject hunter;
        private float nextRamTime;
        private bool hasAwakened;

        private void Awake()
        {
            creature =
                GetComponent<PrototypeSeaCreature>();
        }

        private void OnEnable()
        {
            if (creature == null)
            {
                creature =
                    GetComponent<PrototypeSeaCreature>();
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
            creature.IsMovementExternallyControlled =
                false;
        }

        private void Update()
        {
            if (!IsAggressive)
            {
                return;
            }

            Vector3 toHunter =
                hunter.transform.position -
                transform.position;
            toHunter.y = 0f;

            if (toHunter.sqrMagnitude < 0.0001f)
            {
                return;
            }

            float distance = toHunter.magnitude;
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    toHunter / distance,
                    Vector3.up
                );
            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    turnSpeed * Time.deltaTime
                );

            if (distance > ramRange)
            {
                transform.position +=
                    transform.forward *
                    chargeSpeed *
                    Time.deltaTime;
                KeepAtWaterline();
                return;
            }

            TryRam(toHunter / distance);
        }

        private void HandleHarpooned(
            GameObject newHunter)
        {
            if (newHunter == null ||
                creature.IsHarvested)
            {
                return;
            }

            hunter = newHunter;
            creature.IsMovementExternallyControlled =
                true;

            if (hasAwakened)
            {
                return;
            }

            hasAwakened = true;
            Debug.Log(
                "Stormjaw öfkelendi ve avcı gemisine yöneldi.",
                this
            );
        }

        private void TryRam(Vector3 direction)
        {
            if (Time.time < nextRamTime)
            {
                return;
            }

            IDamageable damageable =
                hunter.GetComponentInChildren<
                    IDamageable>();

            if (damageable == null ||
                damageable.IsSunk)
            {
                return;
            }

            damageable.ApplyDamage(
                new DamageInfo(
                    ramDamage,
                    hunter.transform.position,
                    direction,
                    gameObject
                )
            );
            nextRamTime =
                Time.time + ramCooldown;

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
