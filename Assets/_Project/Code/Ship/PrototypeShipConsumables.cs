using System;
using System.Collections.Generic;
using Seaborn.Combat;
using Seaborn.Hunting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Seaborn.Ship
{
    [DisallowMultipleComponent]
    public sealed class PrototypeShipConsumables : MonoBehaviour
    {
        private const float TonicCooldown = 15f;
        private const float ConcealDuration = 7f;

        [SerializeField, Min(0)]
        private int tortugaTonics = 3;
        [SerializeField, Min(0)]
        private int lightsOfTortuga = 1;

        public event Action ConsumablesChanged;

        public int TortugaTonics => tortugaTonics;
        public int LightsOfTortuga => lightsOfTortuga;
        public bool IsConcealed =>
            Time.time < concealedUntil;
        public float ConcealmentRemaining =>
            Mathf.Max(0f, concealedUntil - Time.time);
        public float TonicCooldownRemaining =>
            Mathf.Max(0f, tonicReadyAt - Time.time);
        public string LastStatus { get; private set; }
        public float LastStatusExpiresAt { get; private set; }

        private ShipHealth health;
        private BroadsideController broadside;
        private HarpoonHuntingController harpoons;
        private float tonicReadyAt;
        private float concealedUntil;
        private readonly Dictionary<Renderer, bool>
            rendererStates = new();

        public static PrototypeShipConsumables EnsureAttached(
            Transform player)
        {
            if (player == null) return null;

            PrototypeShipConsumables consumables =
                player.GetComponent<
                    PrototypeShipConsumables>();
            if (consumables == null)
            {
                consumables = player.gameObject.AddComponent<
                    PrototypeShipConsumables>();
            }
            consumables.Bind(player);
            return consumables;
        }

        private void Bind(Transform player)
        {
            Unsubscribe();
            health = player.GetComponentInChildren<ShipHealth>();
            broadside = player.GetComponentInChildren<
                BroadsideController>();
            harpoons = player.GetComponentInChildren<
                HarpoonHuntingController>();

            if (health != null)
                health.Damaged += HandleDamaged;
            if (broadside != null)
                broadside.BroadsideFired += HandleBroadsideFired;
            if (harpoons != null)
                harpoons.HarpoonFired += HandleHarpoonFired;
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.digit4Key
                    .wasPressedThisFrame)
                    UseTortugaTonic();
                if (Keyboard.current.digit5Key
                    .wasPressedThisFrame)
                    UseLightOfTortuga();
            }

            if (concealedUntil > 0f &&
                Time.time >= concealedUntil)
            {
                EndConcealment("Görünmezlik sona erdi.");
            }
        }

        public bool UseTortugaTonic()
        {
            if (health == null || health.IsSunk)
                return Fail("Gemi kullanılamaz durumda.");
            if (tortugaTonics <= 0)
                return Fail("Tortuga Tonic kalmadı.");
            if (Time.time < tonicReadyAt)
                return Fail(
                    $"Tonic {Mathf.CeilToInt(TonicCooldownRemaining)} sn sonra hazır."
                );
            if (health.CurrentHealth >=
                health.MaximumHealth - 0.1f)
                return Fail("Gövde zaten tam durumda.");

            float restored = health.RestoreHealth(250f);
            if (restored <= 0f) return false;

            tortugaTonics--;
            tonicReadyAt = Time.time + TonicCooldown;
            SetStatus(
                $"Tortuga Tonic: +{Mathf.RoundToInt(restored)} gövde"
            );
            ConsumablesChanged?.Invoke();
            return true;
        }

        public bool UseLightOfTortuga()
        {
            if (health == null || health.IsSunk)
                return Fail("Gemi kullanılamaz durumda.");
            if (lightsOfTortuga <= 0)
                return Fail("Light of Tortuga kalmadı.");
            if (IsConcealed)
                return Fail("Light of Tortuga zaten etkin.");

            lightsOfTortuga--;
            concealedUntil = Time.time + ConcealDuration;
            CaptureAndHideRenderers();
            SetStatus("Light of Tortuga: 7 sn görünmezlik");
            ConsumablesChanged?.Invoke();
            return true;
        }

        public void RestoreStocks(int tonics, int lights)
        {
            tortugaTonics = Mathf.Max(0, tonics);
            lightsOfTortuga = Mathf.Max(0, lights);
            ConsumablesChanged?.Invoke();
        }

        public void AddTortugaTonics(int amount)
        {
            if (amount <= 0) return;
            tortugaTonics += amount;
            ConsumablesChanged?.Invoke();
        }

        public void AddLightsOfTortuga(int amount)
        {
            if (amount <= 0) return;
            lightsOfTortuga += amount;
            ConsumablesChanged?.Invoke();
        }

        private void HandleDamaged(
            Seaborn.Combat.Damage.DamageInfo info)
        {
            if (IsConcealed)
                EndConcealment(
                    "Hasar görünmezliği bozdu."
                );
        }

        private void HandleBroadsideFired(
            BroadsideSide side)
        {
            if (IsConcealed)
                EndConcealment(
                    "Top ateşi görünmezliği bozdu."
                );
        }

        private void HandleHarpoonFired()
        {
            if (IsConcealed)
                EndConcealment(
                    "Zıpkın atışı görünmezliği bozdu."
                );
        }

        private void CaptureAndHideRenderers()
        {
            rendererStates.Clear();
            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                rendererStates[renderer] = renderer.enabled;
                renderer.enabled = false;
            }
        }

        private void EndConcealment(string reason)
        {
            concealedUntil = 0f;
            foreach (KeyValuePair<Renderer, bool> item
                     in rendererStates)
            {
                if (item.Key != null)
                    item.Key.enabled = item.Value;
            }
            rendererStates.Clear();
            SetStatus(reason);
            ConsumablesChanged?.Invoke();
        }

        private bool Fail(string message)
        {
            SetStatus(message);
            return false;
        }

        private void SetStatus(string message)
        {
            LastStatus = message;
            LastStatusExpiresAt =
                Time.unscaledTime + 3f;
            Debug.Log(message, this);
        }

        private void Unsubscribe()
        {
            if (health != null)
                health.Damaged -= HandleDamaged;
            if (broadside != null)
                broadside.BroadsideFired -=
                    HandleBroadsideFired;
            if (harpoons != null)
                harpoons.HarpoonFired -=
                    HandleHarpoonFired;
        }

        private void OnDisable()
        {
            if (IsConcealed)
                EndConcealment(
                    "Görünmezlik sona erdi."
                );
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }
    }
}
