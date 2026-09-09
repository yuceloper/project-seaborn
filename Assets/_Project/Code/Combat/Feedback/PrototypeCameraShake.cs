using UnityEngine;

namespace Seaborn.Combat
{
    [DefaultExecutionOrder(1000)]
    public sealed class PrototypeCameraShake : MonoBehaviour
    {
        private static PrototypeCameraShake instance;

        private float remainingDuration;
        private float currentMagnitude;

        private void Awake()
        {
            instance = this;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        public static void Request(
            float magnitude,
            float duration)
        {
            if (instance == null)
            {
                return;
            }

            instance.currentMagnitude = Mathf.Max(
                instance.currentMagnitude,
                magnitude
            );
            instance.remainingDuration = Mathf.Max(
                instance.remainingDuration,
                duration
            );
        }

        private void LateUpdate()
        {
            if (remainingDuration <= 0f)
            {
                return;
            }

            remainingDuration -= Time.deltaTime;

            float fade = Mathf.Clamp01(
                remainingDuration / 0.12f
            );

            transform.position +=
                Random.insideUnitSphere *
                currentMagnitude *
                fade;

            if (remainingDuration <= 0f)
            {
                currentMagnitude = 0f;
            }
        }
    }
}
