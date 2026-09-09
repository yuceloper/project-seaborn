using System.Collections;
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

        private void Awake()
        {
            shipHealth = GetComponent<ShipHealth>();
            shipColliders = GetComponentsInChildren<Collider>();
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
            foreach (Collider shipCollider in shipColliders)
            {
                shipCollider.enabled = false;
            }

            StartCoroutine(Sink());
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
