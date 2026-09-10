using Seaborn.Combat;
using UnityEngine;

namespace Seaborn.Hunting
{
    public sealed class HarpoonProjectile : MonoBehaviour
    {
        private const int MaximumHits = 8;

        private readonly RaycastHit[] hits =
            new RaycastHit[MaximumHits];

        private Transform ownerRoot;
        private Vector3 startPosition;
        private Vector3 targetPosition;
        private float duration;
        private float arcHeight;
        private float damage;
        private float elapsed;
        private bool isFlying;
        private LineRenderer rope;

        public void Launch(
            Transform owner,
            Vector3 target,
            float flightDuration,
            float flightArc,
            float hitDamage)
        {
            ownerRoot =
                owner != null ? owner.root : null;
            startPosition = transform.position;
            targetPosition = target;
            duration = Mathf.Max(
                0.1f,
                flightDuration
            );
            arcHeight = Mathf.Max(0f, flightArc);
            damage = Mathf.Max(0f, hitDamage);
            elapsed = 0f;
            isFlying = true;

            BuildVisual();
            CreateRope();
            UpdateRope();
        }

        private void FixedUpdate()
        {
            if (!isFlying)
            {
                return;
            }

            elapsed += Time.fixedDeltaTime;
            float progress = Mathf.Clamp01(
                elapsed / duration
            );
            Vector3 flatPosition = Vector3.Lerp(
                startPosition,
                targetPosition,
                progress
            );
            Vector3 nextPosition =
                flatPosition +
                Vector3.up *
                (4f *
                 arcHeight *
                 progress *
                 (1f - progress));

            Vector3 displacement =
                nextPosition - transform.position;
            float distance = displacement.magnitude;

            if (distance > 0.0001f &&
                TryHit(
                    transform.position,
                    displacement / distance,
                    distance))
            {
                return;
            }

            if (distance > 0.0001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        displacement.normalized,
                        Vector3.up
                    );
            }

            transform.position = nextPosition;

            if (progress >= 1f)
            {
                isFlying = false;
                PrototypeCombatVfx.PlayWaterSplash(
                    nextPosition
                );
                Destroy(gameObject);
            }
        }

        private void LateUpdate()
        {
            UpdateRope();
        }

        private bool TryHit(
            Vector3 origin,
            Vector3 direction,
            float distance)
        {
            int hitCount = Physics.SphereCastNonAlloc(
                origin,
                0.16f,
                direction,
                hits,
                distance,
                ~0,
                QueryTriggerInteraction.Collide
            );

            for (
                int index = 0;
                index < hitCount;
                index++)
            {
                RaycastHit hit = hits[index];

                if (ownerRoot != null &&
                    hit.collider.transform.root ==
                    ownerRoot)
                {
                    continue;
                }

                IHarpoonTarget target =
                    hit.collider.GetComponentInParent<
                        IHarpoonTarget>();

                if (target == null ||
                    target.IsHarvested)
                {
                    continue;
                }

                target.ApplyHarpoonHit(
                    damage,
                    hit.point,
                    ownerRoot != null
                        ? ownerRoot.gameObject
                        : null
                );

                transform.position = hit.point;
                isFlying = false;
                PrototypeCombatVfx.PlayWaterSplash(
                    hit.point
                );
                PrototypeCameraShake.Request(
                    0.055f,
                    0.08f
                );
                Destroy(gameObject);
                return true;
            }

            return false;
        }

        private void BuildVisual()
        {
            GameObject shaft =
                GameObject.CreatePrimitive(
                    PrimitiveType.Cylinder
                );
            shaft.name = "Harpoon Shaft";
            shaft.transform.SetParent(
                transform,
                false
            );
            shaft.transform.localPosition =
                new Vector3(0f, 0f, 0.34f);
            shaft.transform.localRotation =
                Quaternion.Euler(90f, 0f, 0f);
            shaft.transform.localScale =
                new Vector3(
                    0.045f,
                    0.38f,
                    0.045f
                );

            Collider shaftCollider =
                shaft.GetComponent<Collider>();
            if (shaftCollider != null)
            {
                Destroy(shaftCollider);
            }

            Material template =
                Resources.Load<Material>(
                    "PrototypeShipBlockout"
                );
            MeshRenderer renderer =
                shaft.GetComponent<MeshRenderer>();

            if (template != null)
            {
                renderer.sharedMaterial = template;
            }
        }

        private void CreateRope()
        {
            rope = gameObject.AddComponent<
                LineRenderer>();
            rope.positionCount = 2;
            rope.useWorldSpace = true;
            rope.widthMultiplier = 0.028f;
            rope.numCapVertices = 2;
            rope.startColor =
                new Color(
                    0.32f,
                    0.22f,
                    0.12f,
                    0.9f
                );
            rope.endColor =
                new Color(
                    0.48f,
                    0.36f,
                    0.2f,
                    0.8f
                );

            Material material =
                Resources.Load<Material>(
                    "PrototypeCombatParticle"
                );
            if (material != null)
            {
                rope.sharedMaterial = material;
            }
        }

        private void UpdateRope()
        {
            if (rope == null)
            {
                return;
            }

            Vector3 ropeOrigin =
                ownerRoot != null
                    ? ownerRoot.position +
                      Vector3.up * 0.75f
                    : startPosition;

            rope.SetPosition(0, ropeOrigin);
            rope.SetPosition(1, transform.position);
        }
    }
}
