using System;
using UnityEngine;

namespace Seaborn.Expeditions
{
    [DisallowMultipleComponent]
    public sealed class PrototypeSafeHarborProtection :
        MonoBehaviour
    {
        public event Action<bool> ProtectionChanged;

        public bool IsProtected =>
            director != null &&
            director.IsPlayerAtHarbor;

        private PrototypeExpeditionDirector director;
        private bool lastProtectionState;
        private bool hasProtectionState;

        public static void EnsureAttached(
            Transform player,
            PrototypeExpeditionDirector expeditionDirector)
        {
            if (player == null || expeditionDirector == null)
            {
                return;
            }

            PrototypeSafeHarborProtection protection =
                player.GetComponent<
                    PrototypeSafeHarborProtection>();

            if (protection == null)
            {
                protection = player.gameObject.AddComponent<
                    PrototypeSafeHarborProtection>();
            }

            protection.Bind(expeditionDirector);
        }

        public static bool AllowsWeapons(GameObject actor)
        {
            PrototypeSafeHarborProtection protection =
                FindFor(actor != null ? actor.transform : null);

            return protection == null ||
                !protection.IsProtected;
        }

        public static bool IsDamageProtected(
            Component target)
        {
            PrototypeSafeHarborProtection protection =
                FindFor(
                    target != null ? target.transform : null
                );

            return protection != null &&
                protection.IsProtected;
        }

        private static PrototypeSafeHarborProtection FindFor(
            Transform target)
        {
            if (target == null)
            {
                return null;
            }

            PrototypeSafeHarborProtection protection =
                target.GetComponentInParent<
                    PrototypeSafeHarborProtection>();

            if (protection != null)
            {
                return protection;
            }

            return target.GetComponentInChildren<
                PrototypeSafeHarborProtection>();
        }

        private void Bind(
            PrototypeExpeditionDirector expeditionDirector)
        {
            director = expeditionDirector;
            PublishState(true);
        }

        private void Update()
        {
            PublishState(false);
        }

        private void PublishState(bool force)
        {
            bool isProtected = IsProtected;
            if (!force &&
                hasProtectionState &&
                lastProtectionState == isProtected)
            {
                return;
            }

            hasProtectionState = true;
            lastProtectionState = isProtected;
            ProtectionChanged?.Invoke(isProtected);

            Debug.Log(
                isProtected
                    ? "Güvenli liman: silahlar kilitli, gemi koruma altında."
                    : "Liman sınırı aşıldı: silahlar etkin, sefer riski başladı.",
                this
            );
        }
    }
}
