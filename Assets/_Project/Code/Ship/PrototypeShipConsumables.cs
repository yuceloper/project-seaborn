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
        private const float TimedBuffDuration = 90f;

        [SerializeField, Min(0)]
        private int tortugaTonics = 3;
        [SerializeField, Min(0)]
        private int lightsOfTortuga = 1;
        [SerializeField, Min(0)]
        private int corsairRum = 2;
        [SerializeField, Min(0)]
        private int galeElixirs = 2;
        [SerializeField, Min(0)]
        private int ironbarkBrews = 2;

        public event Action ConsumablesChanged;

        public int TortugaTonics => tortugaTonics;
        public int LightsOfTortuga => lightsOfTortuga;
        public int CorsairRum => corsairRum;
        public int GaleElixirs => galeElixirs;
        public int IronbarkBrews => ironbarkBrews;
        public float CorsairRumRemaining =>
            Mathf.Max(0f, corsairRumUntil - Time.time);
        public float GaleElixirRemaining =>
            Mathf.Max(0f, galeElixirUntil - Time.time);
        public float IronbarkBrewRemaining =>
            Mathf.Max(0f, ironbarkBrewUntil - Time.time);
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
        private ShipMotor motor;
        private float tonicReadyAt;
        private float concealedUntil;
        private float corsairRumUntil;
        private float galeElixirUntil;
        private float ironbarkBrewUntil;
        private readonly Dictionary<Renderer, Material[]>
            rendererMaterials = new();
        private readonly List<Material>
            concealmentMaterials = new();

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
            motor = player.GetComponentInChildren<ShipMotor>();

            if (health != null)
                health.Damaged += HandleDamaged;
            if (broadside != null)
                broadside.BroadsideFired += HandleBroadsideFired;
            if (harpoons != null)
                harpoons.HarpoonFired += HandleHarpoonFired;
            ApplyTimedModifiers();
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
                if (Keyboard.current.digit6Key
                    .wasPressedThisFrame)
                    UseCorsairRum();
                if (Keyboard.current.digit7Key
                    .wasPressedThisFrame)
                    UseGaleElixir();
                if (Keyboard.current.digit8Key
                    .wasPressedThisFrame)
                    UseIronbarkBrew();
            }

            if (concealedUntil > 0f &&
                Time.time >= concealedUntil)
            {
                EndConcealment("Görünmezlik sona erdi.");
            }

            bool expired =
                (corsairRumUntil > 0f &&
                 Time.time >= corsairRumUntil) ||
                (galeElixirUntil > 0f &&
                 Time.time >= galeElixirUntil) ||
                (ironbarkBrewUntil > 0f &&
                 Time.time >= ironbarkBrewUntil);
            if (expired)
            {
                if (Time.time >= corsairRumUntil)
                    corsairRumUntil = 0f;
                if (Time.time >= galeElixirUntil)
                    galeElixirUntil = 0f;
                if (Time.time >= ironbarkBrewUntil)
                    ironbarkBrewUntil = 0f;
                ApplyTimedModifiers();
                ConsumablesChanged?.Invoke();
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
            ApplyLocalConcealmentVisual();
            SetStatus("Light of Tortuga: 7 sn görünmezlik");
            ConsumablesChanged?.Invoke();
            return true;
        }

        public void RestoreStocks(
            int tonics,
            int lights,
            int rum,
            int gale,
            int ironbark)
        {
            tortugaTonics = Mathf.Max(0, tonics);
            lightsOfTortuga = Mathf.Max(0, lights);
            corsairRum = Mathf.Max(0, rum);
            galeElixirs = Mathf.Max(0, gale);
            ironbarkBrews = Mathf.Max(0, ironbark);
            ConsumablesChanged?.Invoke();
        }

        public bool UseCorsairRum()
        {
            if (corsairRum <= 0)
                return Fail("Corsair Rum kalmadı.");
            corsairRum--;
            corsairRumUntil =
                Time.time + TimedBuffDuration;
            ApplyTimedModifiers();
            SetStatus(
                "Corsair Rum: +%10 top hasarı ve doldurma"
            );
            ConsumablesChanged?.Invoke();
            return true;
        }

        public bool UseGaleElixir()
        {
            if (galeElixirs <= 0)
                return Fail("Gale Elixir kalmadı.");
            galeElixirs--;
            galeElixirUntil =
                Time.time + TimedBuffDuration;
            ApplyTimedModifiers();
            SetStatus(
                "Gale Elixir: +%12 hız ve manevra"
            );
            ConsumablesChanged?.Invoke();
            return true;
        }

        public bool UseIronbarkBrew()
        {
            if (ironbarkBrews <= 0)
                return Fail("Ironbark Brew kalmadı.");
            ironbarkBrews--;
            ironbarkBrewUntil =
                Time.time + TimedBuffDuration;
            ApplyTimedModifiers();
            SetStatus(
                "Ironbark Brew: -%15 alınan hasar"
            );
            ConsumablesChanged?.Invoke();
            return true;
        }

        private void ApplyTimedModifiers()
        {
            broadside?.SetConsumableModifiers(
                CorsairRumRemaining > 0f ? 1.1f : 1f,
                CorsairRumRemaining > 0f ? 0.9f : 1f
            );
            motor?.SetConsumablePerformance(
                GaleElixirRemaining > 0f ? 1.12f : 1f,
                GaleElixirRemaining > 0f ? 1.12f : 1f
            );
            health?.SetConsumableDamageTakenMultiplier(
                IronbarkBrewRemaining > 0f ? 0.85f : 1f
            );
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

        public void AddCorsairRum(int amount)
        {
            if (amount <= 0) return;
            corsairRum += amount;
            ConsumablesChanged?.Invoke();
        }

        public void AddGaleElixirs(int amount)
        {
            if (amount <= 0) return;
            galeElixirs += amount;
            ConsumablesChanged?.Invoke();
        }

        public void AddIronbarkBrews(int amount)
        {
            if (amount <= 0) return;
            ironbarkBrews += amount;
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

        private void ApplyLocalConcealmentVisual()
        {
            RestoreLocalConcealmentVisual();

            Renderer[] renderers =
                GetComponentsInChildren<Renderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null ||
                    renderer is ParticleSystemRenderer ||
                    renderer is LineRenderer)
                {
                    continue;
                }

                Material[] originals = renderer.sharedMaterials;
                rendererMaterials[renderer] = originals;
                Material[] ghosts =
                    new Material[originals.Length];

                for (int j = 0; j < originals.Length; j++)
                {
                    Material source = originals[j];
                    if (source == null)
                    {
                        ghosts[j] = null;
                        continue;
                    }

                    Material ghost = new(source)
                    {
                        name = source.name +
                            " (Local Concealment)",
                        renderQueue = 3000
                    };
                    Color color = source.HasProperty(
                            "_BaseColor")
                        ? source.GetColor("_BaseColor")
                        : source.color;
                    color = Color.Lerp(
                        color,
                        new Color(0.35f, 0.9f, 0.92f, 1f),
                        0.28f
                    );
                    color.a = 0.3f;

                    if (ghost.HasProperty("_BaseColor"))
                        ghost.SetColor("_BaseColor", color);
                    if (ghost.HasProperty("_Color"))
                        ghost.SetColor("_Color", color);
                    if (ghost.HasProperty("_Surface"))
                        ghost.SetFloat("_Surface", 1f);
                    if (ghost.HasProperty("_Blend"))
                        ghost.SetFloat("_Blend", 0f);
                    if (ghost.HasProperty("_SrcBlend"))
                        ghost.SetFloat("_SrcBlend", 5f);
                    if (ghost.HasProperty("_DstBlend"))
                        ghost.SetFloat("_DstBlend", 10f);
                    if (ghost.HasProperty("_ZWrite"))
                        ghost.SetFloat("_ZWrite", 0f);

                    ghost.EnableKeyword(
                        "_SURFACE_TYPE_TRANSPARENT");
                    ghost.DisableKeyword(
                        "_SURFACE_TYPE_OPAQUE");
                    concealmentMaterials.Add(ghost);
                    ghosts[j] = ghost;
                }

                renderer.sharedMaterials = ghosts;
            }
        }

        private void RestoreLocalConcealmentVisual()
        {
            foreach (
                KeyValuePair<Renderer, Material[]> item
                in rendererMaterials)
            {
                if (item.Key != null)
                    item.Key.sharedMaterials = item.Value;
            }
            rendererMaterials.Clear();

            for (int i = 0;
                 i < concealmentMaterials.Count;
                 i++)
            {
                if (concealmentMaterials[i] != null)
                    Destroy(concealmentMaterials[i]);
            }
            concealmentMaterials.Clear();
        }

        private void EndConcealment(string reason)
        {
            concealedUntil = 0f;
            RestoreLocalConcealmentVisual();
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

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(LastStatus) ||
                Time.unscaledTime >= LastStatusExpiresAt)
                return;

            const float width = 410f;
            Rect rect = new(
                (Screen.width - width) * 0.5f,
                Screen.height - 224f,
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
                new Color(0.91f, 0.78f, 0.46f);
            GUI.Label(rect, LastStatus, style);
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
            RestoreLocalConcealmentVisual();
            Unsubscribe();
        }
    }
}
