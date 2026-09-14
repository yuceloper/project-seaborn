using System;
using Seaborn.Combat;
using Seaborn.Combat.Damage;
using Seaborn.Hunting;
using Seaborn.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class PrototypeFieldRepairController :
        MonoBehaviour
    {
        private const float PrototypeRepairScale = 0.05f;
        private const float DamageLockDuration = 8f;

        public event Action RepairStateChanged;

        public bool IsRepairing { get; private set; }
        public float LockRemaining =>
            Mathf.Max(0f, repairLockedUntil - Time.time);
        public float CycleProgress
        {
            get
            {
                float interval = RepairInterval;
                return IsRepairing && interval > 0f
                    ? 1f - Mathf.Clamp01(
                        (nextRepairTime - Time.time) /
                        interval)
                    : 0f;
            }
        }
        public float RepairAmount =>
            profile != null &&
            profile.Definition != null
                ? profile.Definition.repairAmount *
                  PrototypeRepairScale
                : 4f;
        public float RepairInterval =>
            profile != null &&
            profile.Definition != null
                ? Mathf.Max(
                    0.1f,
                    profile.Definition.repairInterval)
                : 5f;

        private ShipHealth health;
        private ShipMotor motor;
        private ShipProfileController profile;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private float nextRepairTime;
        private float repairLockedUntil;
        private string lastStatus;
        private float statusExpiresAt;

        public static PrototypeFieldRepairController
            EnsureAttached(Transform player)
        {
            if (player == null) return null;

            PrototypeFieldRepairController repairs =
                player.GetComponent<
                    PrototypeFieldRepairController>();
            if (repairs == null)
            {
                repairs = player.gameObject.AddComponent<
                    PrototypeFieldRepairController>();
            }

            repairs.Bind(player);
            return repairs;
        }

        private void Bind(Transform player)
        {
            Unsubscribe();
            health = player.GetComponentInChildren<ShipHealth>();
            motor = player.GetComponentInChildren<ShipMotor>();
            profile = player.GetComponentInChildren<
                ShipProfileController>();
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponentInChildren<
                HarpoonHuntingController>();

            if (health != null)
            {
                health.Damaged += HandleDamaged;
                health.Sunk += HandleSunk;
            }
            if (broadside != null)
                broadside.BroadsideFired += HandleBroadsideFired;
            if (harpoons != null)
                harpoons.HarpoonFired += HandleHarpoonFired;
        }

        private void Update()
        {
            if (Keyboard.current != null &&
                Keyboard.current.rKey.wasPressedThisFrame)
            {
                ToggleRepair();
            }

            if (!IsRepairing) return;

            if (health == null ||
                health.IsSunk ||
                health.CurrentHealth >=
                    health.MaximumHealth - 0.1f)
            {
                StopRepair(
                    health != null && !health.IsSunk
                        ? "Gövde tamir edildi."
                        : "Tamir sona erdi."
                );
                return;
            }

            if (Time.time < nextRepairTime) return;

            float restored =
                health.RestoreHealth(RepairAmount);
            nextRepairTime =
                Time.time + RepairInterval;
            SetStatus(
                $"+{Mathf.RoundToInt(restored)} gövde tamiri"
            );
            RepairStateChanged?.Invoke();
        }

        public bool ToggleRepair()
        {
            if (IsRepairing)
            {
                StopRepair("Saha tamiri durduruldu.");
                return false;
            }

            if (PrototypeExpeditionRegionDirector.IsHarborScene)
                return Fail(
                    "Limanda tersane onarımını kullan."
                );
            if (health == null || health.IsSunk)
                return Fail("Gemi tamir edilemez durumda.");
            if (health.CurrentHealth >=
                health.MaximumHealth - 0.1f)
                return Fail("Gövde zaten tam durumda.");
            if (Time.time < repairLockedUntil)
                return Fail(
                    $"Tamir {Mathf.CeilToInt(LockRemaining)} sn kilitli."
                );
            if (RepairAmount <= 0f)
                return Fail(
                    "Bu geminin saha tamir kapasitesi yok."
                );

            IsRepairing = true;
            nextRepairTime =
                Time.time + RepairInterval;
            motor?.SetRepairPerformance(0.45f, 0.7f);
            SetStatus(
                $"Saha tamiri başladı: " +
                $"+{Mathf.RoundToInt(RepairAmount)} / " +
                $"{RepairInterval:0.#} sn"
            );
            RepairStateChanged?.Invoke();
            return true;
        }

        private void HandleDamaged(DamageInfo info)
        {
            repairLockedUntil =
                Time.time + DamageLockDuration;
            if (IsRepairing)
            {
                StopRepair(
                    "Hasar tamiri kesti: 8 sn kilit."
                );
            }
        }

        private void HandleBroadsideFired(
            BroadsideSide side)
        {
            if (IsRepairing)
                StopRepair("Top ateşi tamiri kesti.");
        }

        private void HandleHarpoonFired()
        {
            if (IsRepairing)
                StopRepair("Zıpkın atışı tamiri kesti.");
        }

        private void HandleSunk()
        {
            if (IsRepairing)
                StopRepair("Gemi battı; tamir sona erdi.");
        }

        private void StopRepair(string reason)
        {
            if (!IsRepairing) return;

            IsRepairing = false;
            nextRepairTime = 0f;
            motor?.SetRepairPerformance(1f, 1f);
            SetStatus(reason);
            RepairStateChanged?.Invoke();
        }

        private bool Fail(string reason)
        {
            SetStatus(reason);
            return false;
        }

        private void SetStatus(string message)
        {
            lastStatus = message;
            statusExpiresAt =
                Time.unscaledTime + 3f;
            Debug.Log(message, this);
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(lastStatus) ||
                Time.unscaledTime >= statusExpiresAt)
                return;

            const float width = 420f;
            Rect rect = new(
                (Screen.width - width) * 0.5f,
                Screen.height - 270f,
                width,
                38f
            );
            GUI.Box(rect, GUIContent.none);
            GUIStyle style =
                new(GUI.skin.label)
                {
                    alignment = TextAnchor.MiddleCenter,
                    fontStyle = FontStyle.Bold,
                    fontSize = 14
                };
            style.normal.textColor =
                IsRepairing
                    ? new Color(0.3f, 0.82f, 0.58f)
                    : new Color(0.91f, 0.78f, 0.46f);
            GUI.Label(rect, lastStatus, style);
        }

        private void Unsubscribe()
        {
            if (health != null)
            {
                health.Damaged -= HandleDamaged;
                health.Sunk -= HandleSunk;
            }
            if (broadside != null)
                broadside.BroadsideFired -= HandleBroadsideFired;
            if (harpoons != null)
                harpoons.HarpoonFired -= HandleHarpoonFired;
        }

        private void OnDisable()
        {
            if (IsRepairing)
                StopRepair("Saha tamiri durduruldu.");
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
