using Seaborn.Combat;
using Seaborn.Combat.Damage;
using UnityEngine;

namespace Seaborn.Ship
{
    [RequireComponent(typeof(ShipHealth))]
    public sealed class ShipCombatFeedback : MonoBehaviour
    {
        private ShipHealth shipHealth;
        private bool isPlayer;
        private float nextHitFeedback;
        private float previousHealth;
        private float damageTotal;
        private float damageShownAt = -10f;
        private float nextSmoke;
        private GUIStyle damageStyle;

        private void Awake()
        {
            shipHealth = GetComponent<ShipHealth>();
            isPlayer = GetComponent<ManualBroadsideAimController>() != null;
        }

        private void Start()
        {
            previousHealth = shipHealth.CurrentHealth;
        }

        private void OnEnable()
        {
            previousHealth = shipHealth.CurrentHealth;
            shipHealth.HealthChanged += HandleHealthChanged;
            shipHealth.Damaged += HandleDamaged;
            shipHealth.Sunk += HandleSunk;
        }

        private void OnDisable()
        {
            shipHealth.HealthChanged -= HandleHealthChanged;
            shipHealth.Damaged -= HandleDamaged;
            shipHealth.Sunk -= HandleSunk;
        }

        private void HandleHealthChanged(float current, float maximum)
        {
            if (current > previousHealth) damageShownAt = -10f;
            previousHealth = current;
        }

        private void Update()
        {
            if (shipHealth.IsSunk || shipHealth.CurrentHealth > shipHealth.MaximumHealth * 0.35f ||
                Time.time < nextSmoke) return;
            nextSmoke = Time.time + 0.8f;
            PrototypeCombatVfx.PlayDamagedHullSmoke(transform.position + Vector3.up * 0.8f);
        }

        private void OnGUI()
        {
            float age = Time.time - damageShownAt;
            UnityEngine.Camera camera = UnityEngine.Camera.main;
            if (age > 1.1f || damageTotal <= 0f || camera == null) return;
            Vector3 point = camera.WorldToScreenPoint(transform.position + Vector3.up * 2f);
            if (point.z <= 0f) return;
            if (damageStyle == null) damageStyle = new GUIStyle(GUI.skin.label)
                { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 22 };
            Color color = isPlayer ? new Color(1f, 0.35f, 0.25f) : new Color(1f, 0.87f, 0.48f);
            color.a = Mathf.Clamp01((1.1f - age) / 0.35f);
            damageStyle.normal.textColor = new Color(0f, 0f, 0f, color.a);
            Rect rect = new Rect(point.x - 60f, Screen.height - point.y - age * 26f, 120f, 32f);
            GUI.Label(new Rect(rect.x + 1f, rect.y + 1f, rect.width, rect.height), $"−{damageTotal:0}", damageStyle);
            damageStyle.normal.textColor = color;
            GUI.Label(rect, $"−{damageTotal:0}", damageStyle);
        }

        private void HandleDamaged(DamageInfo damageInfo)
        {
            float actualDamage = Mathf.Max(0f, previousHealth - shipHealth.CurrentHealth);
            if (Time.time - damageShownAt > 0.25f) damageTotal = 0f;
            damageTotal += actualDamage;
            damageShownAt = Time.time;
            // Never move the physics root for cosmetic hit feedback.
            // Hull impact particles are emitted by the projectile; a pellet burst
            // produces at most one small player-camera pulse per 0.2 seconds.
            if (!isPlayer || Time.time < nextHitFeedback) return;
            nextHitFeedback = Time.time + 0.2f;
            PrototypeCameraShake.Request(0.025f, 0.08f);
        }

        private void HandleSunk()
        {
            PrototypeCombatVfx.PlaySinkingSmoke(transform.position + Vector3.up * 0.4f);
        }
    }
}
