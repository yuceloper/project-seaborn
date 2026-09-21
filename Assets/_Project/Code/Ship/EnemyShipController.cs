using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;
using Seaborn.World;

namespace Seaborn.Ship
{
    public enum EnemyShipArchetype
    {
        Skirmisher,
        Gunship,
        Marauder,
        FishingBoat,
        Merchant
    }

    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(BroadsideController))]
    [RequireComponent(typeof(ShipHealth))]
    public sealed class EnemyShipController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField]
        private Transform target;

        [SerializeField, Min(0f)]
        private float detectionRange = 22f;

        [Header("Navigation")]
        [SerializeField, Min(0.1f)]
        private float preferredRange = 7f;

        [SerializeField, Min(0.1f)]
        private float retreatDistance = 4.5f;

        [SerializeField, Min(0f)]
        private float forwardSpeed = 3f;

        [SerializeField, Min(0f)]
        private float turnSpeed = 38f;

        [Header("Combat")]
        [SerializeField, Range(0f, 1f)]
        private float fireAlignment = 0.82f;

        [SerializeField, Min(0f)]
        private float fireRange = 10f;

        [SerializeField, Min(0f)]
        private float initialReactionDelay = 2f;

        [SerializeField, Min(0.1f)]
        private float aimPreparationTime = 1.1f;

        [SerializeField, Range(0f, 1f)]
        private float movementPrediction = 0.65f;

        public EnemyShipArchetype Archetype
        {
            get;
            private set;
        } = EnemyShipArchetype.Marauder;

        public bool IsCivilian => Archetype == EnemyShipArchetype.FishingBoat ||
            Archetype == EnemyShipArchetype.Merchant;

        public bool IsAggressive { get; private set; }

        public float AimPreparation =>
            Mathf.Clamp01(
                aimPreparation /
                Mathf.Max(0.1f, aimPreparationTime)
            );

        public bool IsPreparingShot =>
            aimPreparation > 0f;

        public Vector3 PredictedAimPoint
        {
            get;
            private set;
        }

        private Rigidbody shipRigidbody;
        private BroadsideController broadsideController;
        private ShipHealth shipHealth;
        private ShipSubsystemController subsystems;
        private float engagementStartTime;
        private float aimPreparation;
        private enum AttackPhase { Approach, Align, Hold }
        private AttackPhase attackPhase;
        private Vector3 runHeading;
        private float lostFiringArcAt = -1f;
        private Vector3 escapeHeading;
        private float nextEscapeDecision;
        private const float RealignDelay = 0.9f;
        private float targetLostAt = -1f;
        private RigidbodyConstraints navigationConstraints;
        private const float CombatExitDelay = 5f;
        private Vector3 patrolDestination;
        private Vector3 lastSeenPosition;
        private bool hasPatrolDestination;
        private Vector3 patrolHome;
        private bool merchantOutbound;
        private BroadsideSide preparingSide;
        private float patrolDeadline;
        private float outOfFireRangeAt = -1f;
        private const float BoundaryMargin = 8f;
        private static float NavigationLimit =>
            PrototypeExpeditionRegionDirector.MapEdge - BoundaryMargin;

        private static Vector3 ClampToMap(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, -NavigationLimit, NavigationLimit);
            position.z = Mathf.Clamp(position.z, -NavigationLimit, NavigationLimit);
            return position;
        }

        private void EnforceMapBounds()
        {
            Vector3 position = shipRigidbody.position;
            Vector3 bounded = ClampToMap(position);
            if ((position - bounded).sqrMagnitude > 0.0001f)
            {
                shipRigidbody.position = bounded;
                StopMoving();
            }
        }

        private void Patrol()
        {
            Vector3 delta = patrolDestination - shipRigidbody.position;
            delta.y = 0f;
            if (!hasPatrolDestination || delta.sqrMagnitude < 9f || Time.time >= patrolDeadline)
            {
                // Local waypoints keep each ship roaming rather than crossing the entire map.
                Vector2 offset = Random.insideUnitCircle * (IsCivilian ? 10f : 14f);
                if (Archetype == EnemyShipArchetype.Merchant)
                {
                    merchantOutbound = !merchantOutbound;
                    offset = merchantOutbound ? new Vector2(12f, 18f) : new Vector2(-12f, -18f);
                }
                patrolDestination = ClampToMap(patrolHome +
                    new Vector3(offset.x, 0f, offset.y));
                hasPatrolDestination = true;
                patrolDeadline = Time.time + 25f;
                delta = patrolDestination - shipRigidbody.position;
                delta.y = 0f;
            }
            SteerAndMove(delta.normalized, forwardSpeed * 0.55f);
        }

        private void ResumeApproach()
        {
            shipRigidbody.constraints = navigationConstraints;
            attackPhase = AttackPhase.Approach;
            aimPreparation = 0f;
            outOfFireRangeAt = -1f;
            lostFiringArcAt = -1f;
        }

        private void Awake()
        {
            patrolHome = transform.position;
            shipRigidbody = GetComponent<Rigidbody>();
            shipRigidbody.constraints |= RigidbodyConstraints.FreezeRotationX |
                RigidbodyConstraints.FreezeRotationZ;
            navigationConstraints = shipRigidbody.constraints;
            shipRigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            broadsideController =
                GetComponent<BroadsideController>();
            shipHealth = GetComponent<ShipHealth>();
            subsystems =
                ShipSubsystemController.EnsureAttached(
                    transform
                );
            shipHealth.Damaged += HandleDamaged;
            engagementStartTime = Time.time;
        }

        public void ConfigureArchetype(
            EnemyShipArchetype archetype)
        {
            Archetype = archetype;
            patrolHome = transform.position;

            switch (archetype)
            {
                case EnemyShipArchetype.FishingBoat:
                case EnemyShipArchetype.Merchant:
                    bool fishing = archetype == EnemyShipArchetype.FishingBoat;
                    name = fishing ? "Kıyı Balıkçısı" : "Yük Tüccarı";
                    forwardSpeed = fishing ? 2.5f : 1.8f;
                    turnSpeed = fishing ? 24f : 18f;
                    detectionRange = 25f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(fishing ? 0.48f : 0.88f, true);
                    break;

                case EnemyShipArchetype.Skirmisher:
                    name = "Razorwind Skirmisher";
                    detectionRange = 25f;
                    preferredRange = 6.2f;
                    retreatDistance = 3.6f;
                    forwardSpeed = 4.5f;
                    turnSpeed = 62f;
                    fireRange = 8.5f;
                    fireAlignment = 0.76f;
                    aimPreparationTime = 0.78f;
                    movementPrediction = 0.9f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        0.72f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Chain);
                    ApplyPalette(
                        new Color(0.12f, 0.26f, 0.3f),
                        new Color(0.28f, 0.58f, 0.62f),
                        new Color(0.18f, 0.72f, 0.78f));
                    break;

                case EnemyShipArchetype.Gunship:
                    name = "Ironwake Gunship";
                    detectionRange = 27f;
                    preferredRange = 9f;
                    retreatDistance = 6f;
                    forwardSpeed = 2.2f;
                    turnSpeed = 27f;
                    fireRange = 12f;
                    fireAlignment = 0.86f;
                    aimPreparationTime = 1.55f;
                    movementPrediction = 0.52f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        1.4f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Standard);
                    ApplyPalette(
                        new Color(0.19f, 0.12f, 0.1f),
                        new Color(0.52f, 0.38f, 0.25f),
                        new Color(0.82f, 0.5f, 0.16f));
                    break;

                default:
                    name = "Saltfang Marauder";
                    detectionRange = 22f;
                    preferredRange = 5.5f;
                    retreatDistance = 3.5f;
                    forwardSpeed = 3.5f;
                    turnSpeed = 44f;
                    fireRange = 7.2f;
                    fireAlignment = 0.74f;
                    aimPreparationTime = 0.62f;
                    movementPrediction = 0.72f;
                    shipHealth.SetRuntimeMaximumHealthMultiplier(
                        0.9f, true);
                    broadsideController.TrySelectAmmunition(
                        AmmunitionType.Grapeshot);
                    ApplyPalette(
                        new Color(0.28f, 0.08f, 0.07f),
                        new Color(0.52f, 0.15f, 0.12f),
                        new Color(0.78f, 0.12f, 0.08f));
                    break;
            }

            engagementStartTime = Time.time;
        }

        private void ApplyPalette(
            Color hull,
            Color sail,
            Color accent)
        {
            PrototypeShipVisual visual =
                GetComponent<PrototypeShipVisual>();
            visual?.ApplyEnemyPalette(
                hull,
                sail,
                accent
            );
        }

        private void FixedUpdate()
        {
            if (shipHealth.IsSunk)
            {
                if (IsAggressive) SetPassive();
                StopMoving();
                return;
            }
            EnforceMapBounds();
            if (IsAggressive && target == null) SetPassive();
            if (!IsAggressive)
            {
                Patrol();
                return;
            }

            ShipHealth targetHealth = target.GetComponentInChildren<ShipHealth>();
            if (targetHealth != null && targetHealth.IsSunk)
            {
                SetPassive();
                return;
            }

            PrototypeShipConsumables consumables =
                target.root.GetComponent<
                    PrototypeShipConsumables>();
            if (consumables != null &&
                consumables.IsConcealed)
            {
                WaitForCombatExit();
                return;
            }

            Vector3 toTarget =
                target.position - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;

            if (distance > detectionRange ||
                Mathf.Abs(target.position.x) > NavigationLimit ||
                Mathf.Abs(target.position.z) > NavigationLimit)
            {
                WaitForCombatExit();
                return;
            }
            targetLostAt = -1f;
            lastSeenPosition = ClampToMap(target.position);
            if (distance <= Mathf.Epsilon)
            {
                aimPreparation = 0f;
                StopMoving();
                return;
            }

            Vector3 targetDirection =
                toTarget / distance;

            float effectiveRange = Mathf.Min(fireRange, broadsideController.MaximumRange);
            if (attackPhase != AttackPhase.Approach && distance > effectiveRange)
            {
                if (outOfFireRangeAt < 0f) outOfFireRangeAt = Time.time;
                if (Time.time - outOfFireRangeAt >= 1.5f) ResumeApproach();
            }
            else outOfFireRangeAt = -1f;

            if (IsCivilian)
            {
                aimPreparation = 0f;
                // Commit to a course instead of perfectly mirroring every player turn.
                Vector3 projected = shipRigidbody.position + escapeHeading * 12f;
                bool headingOutside = (projected - ClampToMap(projected)).sqrMagnitude > 0.01f;
                if (Time.time >= nextEscapeDecision || escapeHeading.sqrMagnitude < 0.01f || headingOutside)
                {
                    Vector3 escape = ClampToMap(shipRigidbody.position - targetDirection * 18f);
                    Vector3 direction = escape - shipRigidbody.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude < 16f || headingOutside)
                        direction = Vector3.ProjectOnPlane(-shipRigidbody.position, Vector3.up);
                    escapeHeading = direction.normalized;
                    nextEscapeDecision = Time.time + 3f;
                }
                SteerAndMove(escapeHeading, forwardSpeed);
                return;
            }

            Navigate(
                targetDirection,
                distance
            );
            PrepareAndFire(
                targetDirection,
                distance
            );
        }

        private void Navigate(Vector3 targetDirection, float distance)
        {
            if (attackPhase == AttackPhase.Hold)
            {
                StopMoving();
                float alignment = Mathf.Abs(Vector3.Dot(transform.right, targetDirection));
                if (alignment >= fireAlignment)
                {
                    lostFiringArcAt = -1f;
                    return;
                }
                if (lostFiringArcAt < 0f) lostFiringArcAt = Time.time;
                if (Time.time - lostFiringArcAt < RealignDelay) return;
                // Keep the position fixed, but unlock yaw when the player leaves the broadside.
                attackPhase = AttackPhase.Align;
                shipRigidbody.constraints = navigationConstraints |
                    RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
                aimPreparation = 0f;
                lostFiringArcAt = -1f;
            }

            if (attackPhase == AttackPhase.Approach)
            {
                float effectiveRange = Mathf.Min(fireRange, broadsideController.MaximumRange);
                float positionRange = Mathf.Min(preferredRange, effectiveRange * 0.8f);
                if (distance > positionRange + 0.5f)
                {
                    SteerAndMove(targetDirection, forwardSpeed);
                    return;
                }
                if (distance < positionRange * 0.6f)
                {
                    SteerAndMove(-targetDirection, forwardSpeed);
                    return;
                }
                attackPhase = AttackPhase.Align;
                shipRigidbody.constraints = navigationConstraints |
                    RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ;
            }

            // Refresh the heading while aligning: a moving target invalidates a cached heading.
            // Re-enter Hold as soon as a usable broadside has a small angular safety margin.
            runHeading = Vector3.Cross(Vector3.up, targetDirection);
            if (Vector3.Dot(runHeading, transform.forward) < 0f) runHeading = -runHeading;
            StopMoving();
            Quaternion desired = Quaternion.LookRotation(runHeading, Vector3.up);
            Quaternion next = Quaternion.RotateTowards(shipRigidbody.rotation, desired,
                turnSpeed * (subsystems != null ? subsystems.TurnMultiplier : 1f) * Time.fixedDeltaTime);
            shipRigidbody.MoveRotation(next);
            if (Mathf.Abs(Vector3.Dot(transform.right, targetDirection)) >=
                Mathf.Min(0.98f, fireAlignment + 0.06f))
            {
                attackPhase = AttackPhase.Hold;
                shipRigidbody.constraints = navigationConstraints |
                    RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                    RigidbodyConstraints.FreezeRotationY;
                aimPreparation = 0f;
            }
        }

        private void WaitForCombatExit()
        {
            if (targetLostAt < 0f)
            {
                targetLostAt = Time.time;
                ResumeApproach();
            }
            aimPreparation = 0f;
            if (IsCivilian)
            {
                SetPassive();
                Patrol();
                return;
            }
            if (Time.time - targetLostAt >= CombatExitDelay)
            {
                SetPassive();
                Patrol();
                return;
            }
            // Search only the last visible location; never track a concealed target.
            Vector3 delta = lastSeenPosition - shipRigidbody.position;
            delta.y = 0f;
            if (delta.sqrMagnitude > 9f) SteerAndMove(delta.normalized, forwardSpeed);
            else StopMoving();
        }

        private void PrepareAndFire(
            Vector3 targetDirection,
            float distance)
        {
            if (IsCivilian || attackPhase != AttackPhase.Hold)
            {
                aimPreparation = 0f;
                return;
            }
            float sideAlignment = Vector3.Dot(
                transform.right,
                targetDirection
            );

            bool hasFiringSolution =
                distance <= Mathf.Min(fireRange, broadsideController.MaximumRange) &&
                Mathf.Abs(sideAlignment) >= fireAlignment &&
                Time.time - engagementStartTime >=
                initialReactionDelay;

            if (!hasFiringSolution)
            {
                aimPreparation = 0f;
                return;
            }

            BroadsideSide side = sideAlignment >= 0f
                ? BroadsideSide.Starboard : BroadsideSide.Port;
            if (broadsideController.GetCooldownRemaining(side) > 0f ||
                broadsideController.GetAmmunitionStock(broadsideController.SelectedAmmunition) <= 0)
            {
                aimPreparation = 0f;
                return;
            }
            if (side != preparingSide) aimPreparation = 0f;
            preparingSide = side;
            // Show an honest preparation window, then lock aim so movement can dodge it.
            if (aimPreparation <= 0f || aimPreparation < Mathf.Max(0f, aimPreparationTime - 0.4f))
            {
                Vector3 point = target.position;
                Rigidbody tracked = target.GetComponent<Rigidbody>();
                if (tracked != null) point += tracked.linearVelocity * movementPrediction;
                point.y = transform.position.y + 0.25f;
                PredictedAimPoint = point;
            }
            aimPreparation += Time.fixedDeltaTime;
            if (aimPreparation < aimPreparationTime) return;

            broadsideController.TryFireAt(side, PredictedAimPoint, 0.82f);
            // A rejected locked prediction must not stall every following attempt.
            aimPreparation = 0f;
        }

        private void SteerAndMove(
            Vector3 desiredForward,
            float speed)
        {
            if (desiredForward.sqrMagnitude <= Mathf.Epsilon)
            {
                StopMoving();
                return;
            }

            Quaternion desiredRotation =
                Quaternion.LookRotation(
                    desiredForward,
                    Vector3.up
                );

            float turnAuthority = 1f;
            if (IsCivilian)
            {
                float planarSpeed = Vector3.ProjectOnPlane(shipRigidbody.linearVelocity, Vector3.up).magnitude;
                // Civilian rudders need water flow; no full-speed pivot at rest.
                turnAuthority = Mathf.Lerp(0.12f, 0.65f,
                    Mathf.Clamp01(planarSpeed / Mathf.Max(0.1f, forwardSpeed)));
            }
            Quaternion nextRotation =
                Quaternion.RotateTowards(
                    shipRigidbody.rotation,
                    desiredRotation,
                    turnSpeed * turnAuthority *
                    (subsystems != null
                        ? subsystems.TurnMultiplier
                        : 1f) *
                    Time.fixedDeltaTime
                );

            shipRigidbody.MoveRotation(nextRotation);
            float turnAlignment = Mathf.Clamp01(Vector3.Dot(transform.forward, desiredForward.normalized));
            Vector3 desiredVelocity = nextRotation * Vector3.forward * speed *
                Mathf.Lerp(0.25f, 1f, turnAlignment) *
                (subsystems != null ? subsystems.MovementSpeedMultiplier : 1f);
            Vector3 planarVelocity = Vector3.ProjectOnPlane(shipRigidbody.linearVelocity, Vector3.up);
            Vector3 nextVelocity = Vector3.MoveTowards(planarVelocity, desiredVelocity,
                2f * Time.fixedDeltaTime);
            // Limit the predicted next step too, including acceleration and turning drift.
            Vector3 boundedNext = ClampToMap(shipRigidbody.position + nextVelocity * Time.fixedDeltaTime);
            nextVelocity = Vector3.ProjectOnPlane(
                (boundedNext - shipRigidbody.position) / Time.fixedDeltaTime, Vector3.up);
            shipRigidbody.linearVelocity = nextVelocity + Vector3.up * shipRigidbody.linearVelocity.y;
            shipRigidbody.angularVelocity = Vector3.zero;
        }

        private void StopMoving()
        {
            shipRigidbody.linearVelocity = Vector3.zero;
            shipRigidbody.angularVelocity = Vector3.zero;
        }

        public void SetPassive()
        {
            IsAggressive = false;
            escapeHeading = Vector3.zero;
            nextEscapeDecision = 0f;
            lostFiringArcAt = -1f;
            hasPatrolDestination = false;
            outOfFireRangeAt = -1f;
            attackPhase = AttackPhase.Approach;
            targetLostAt = -1f;
            aimPreparation = 0f;
            if (shipRigidbody != null)
            {
                shipRigidbody.constraints = navigationConstraints;
                StopMoving();
            }
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            if (damageInfo.Instigator == null)
            {
                return;
            }

            ManualBroadsideAimController attacker =
                damageInfo.Instigator.transform.root
                    .GetComponentInChildren<
                        ManualBroadsideAimController>();
            if (attacker == null)
            {
                return;
            }

            // Scene copies hold a serialized reference to
            // their local PlayerShip. Map transitions remove
            // that duplicate, so always bind the living ship
            // that actually caused the damage.
            target = attacker.transform;
            lastSeenPosition = ClampToMap(target.position);
            targetLostAt = -1f;

            if (IsAggressive)
            {
                return;
            }

            IsAggressive = true;
            nextEscapeDecision = 0f;
            ResumeApproach();
            engagementStartTime = Time.time;
            Debug.Log(
                $"{name} saldırıya karşılık veriyor.",
                this
            );
        }

        private void OnDestroy()
        {
            if (shipHealth != null)
            {
                shipHealth.Damaged -= HandleDamaged;
            }
        }

        private void OnValidate()
        {
            preferredRange = Mathf.Max(
                preferredRange,
                retreatDistance + 0.1f
            );
            fireRange = Mathf.Max(
                fireRange,
                preferredRange
            );
            detectionRange = Mathf.Max(
                detectionRange,
                fireRange
            );
        }
    }
}
