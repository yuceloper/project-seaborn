using System.Collections;
using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(ShipHealth))]
    public sealed class ShipCombatFeedback : MonoBehaviour
    {
        [SerializeField, Min(0f)]
        private float hitShakeMagnitude = 0.09f;

        [SerializeField, Min(0.01f)]
        private float hitShakeDuration = 0.14f;

        private ShipHealth shipHealth;
        private Coroutine shakeRoutine;

        private void Awake()
        {
            shipHealth = GetComponent<ShipHealth>();
        }

        private void OnEnable()
        {
            shipHealth.Damaged += HandleDamaged;
            shipHealth.Sunk += HandleSunk;
        }

        private void OnDisable()
        {
            shipHealth.Damaged -= HandleDamaged;
            shipHealth.Sunk -= HandleSunk;
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (shipHealth.CurrentHealth <= 0f)
            {
                return;
            }

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(Shake());
        }

        private void HandleSunk()
        {
            PrototypeCombatVfx.PlaySinkingSmoke(
                transform.position + Vector3.up * 0.4f
            );
        }

        private IEnumerator Shake()
        {
            Vector3 startPosition = transform.position;
            float elapsedTime = 0f;

            while (elapsedTime < hitShakeDuration)
            {
                elapsedTime += Time.deltaTime;

                float fade = 1f - Mathf.Clamp01(
                    elapsedTime / hitShakeDuration
                );

                transform.position =
                    startPosition +
                    Random.insideUnitSphere *
                    hitShakeMagnitude *
                    fade;

                yield return null;
            }

            transform.position = startPosition;
            shakeRoutine = null;
        }
    }
}
