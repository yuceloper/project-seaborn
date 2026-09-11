using System.Collections;
using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(ShipHealth))]
    public sealed class ShipSinkController : MonoBehaviour
    {
        [SerializeField, Min(0.1f)]
        private float sinkDuration = 2.5f;

        [SerializeField, Min(0f)]
        private float sinkDepth = 3f;

        [SerializeField]
        private float rollAngle = 22f;

        [SerializeField]
        private bool disableAfterSinking = true;

        private ShipHealth shipHealth;
        private Collider[] shipColliders;
        private bool originalIsKinematic;

        private void Awake()
        {
            shipHealth = GetComponent<ShipHealth>();
            shipColliders = GetComponentsInChildren<Collider>();

            Rigidbody shipRigidbody = GetComponent<Rigidbody>();
            originalIsKinematic =
                shipRigidbody != null &&
                shipRigidbody.isKinematic;
        }

        private void OnEnable()
        {
            shipHealth.Sunk += BeginSinking;
        }

        private void OnDisable()
        {
            shipHealth.Sunk -= BeginSinking;
        }

        private void BeginSinking()
        {
            ShipMotor shipMotor = GetComponent<ShipMotor>();

            if (shipMotor != null)
            {
                shipMotor.enabled = false;
            }

            BroadsideController broadsideController =
                GetComponent<BroadsideController>();

            if (broadsideController != null)
            {
                broadsideController.enabled = false;
            }

            EnemyShipController enemyController =
                GetComponent<EnemyShipController>();

            if (enemyController != null)
            {
                enemyController.enabled = false;
            }

            Rigidbody shipRigidbody =
                GetComponent<Rigidbody>();

            if (shipRigidbody != null)
            {
                shipRigidbody.linearVelocity = Vector3.zero;
                shipRigidbody.angularVelocity = Vector3.zero;
                shipRigidbody.isKinematic = true;
            }

            foreach (Collider shipCollider in shipColliders)
            {
                shipCollider.enabled = false;
            }

            StartCoroutine(Sink());
        }

        public void RestoreAfterSinking(
            Vector3 position,
            Quaternion rotation)
        {
            StopAllCoroutines();

            gameObject.SetActive(true);
            transform.SetPositionAndRotation(
                position,
                rotation
            );

            ShipMotor shipMotor = GetComponent<ShipMotor>();
            if (shipMotor != null)
            {
                shipMotor.enabled = true;
            }

            BroadsideController broadsideController =
                GetComponent<BroadsideController>();
            if (broadsideController != null)
            {
                broadsideController.enabled = true;
            }

            Rigidbody shipRigidbody =
                GetComponent<Rigidbody>();
            if (shipRigidbody != null)
            {
                shipRigidbody.isKinematic =
                    originalIsKinematic;
                shipRigidbody.linearVelocity = Vector3.zero;
                shipRigidbody.angularVelocity = Vector3.zero;
            }

            foreach (Collider shipCollider in shipColliders)
            {
                if (shipCollider != null)
                {
                    shipCollider.enabled = true;
                }
            }

            shipHealth.RestoreToFullHealth();
        }

        private IEnumerator Sink()
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition =
                startPosition + Vector3.down * sinkDepth;

            Quaternion startRotation = transform.rotation;
            Quaternion endRotation =
                startRotation *
                Quaternion.Euler(0f, 0f, rollAngle);

            float elapsedTime = 0f;

            while (elapsedTime < sinkDuration)
            {
                elapsedTime += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsedTime / sinkDuration
                );
                float easedProgress =
                    progress * progress * (3f - 2f * progress);

                transform.SetPositionAndRotation(
                    Vector3.Lerp(
                        startPosition,
                        endPosition,
                        easedProgress
                    ),
                    Quaternion.Slerp(
                        startRotation,
                        endRotation,
                        easedProgress
                    )
                );

                yield return null;
            }

            if (disableAfterSinking)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
