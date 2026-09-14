using System;
using Seaborn.Harbor;
using Seaborn.Ship;
using Seaborn.Ship.Data;
using UnityEngine;

namespace Seaborn.Progression
{
    public enum DeckExtensionInstallResult
    {
        Completed,
        NotAtShipyard,
        NoExtension,
        ProfileUnavailable,
        MaximumInstalled
    }

    [DisallowMultipleComponent]
    public sealed class PrototypeDeckExtensionInventory :
        MonoBehaviour
    {
        [SerializeField, Min(0)]
        private int availableExtensions = 1;
        [SerializeField, Min(0)]
        private int starterSloopExtensions;
        [SerializeField, Min(0)]
        private int ratSailsExtensions;
        [SerializeField, Min(0)]
        private int dreadwakeExtensions;

        public event Action DeckExtensionsChanged;

        public int AvailableExtensions =>
            availableExtensions;
        public int StarterSloopExtensions =>
            starterSloopExtensions;
        public int RatSailsExtensions =>
            ratSailsExtensions;
        public int DreadwakeExtensions =>
            dreadwakeExtensions;
        public int ActiveInstalled =>
            profile != null
                ? GetInstalled(profile.ShipId)
                : 0;
        public int ActiveLimit =>
            profile != null &&
            profile.Definition != null
                ? profile.Definition.deckExtensionLimit
                : 0;

        private ShipProfileController profile;

        public static PrototypeDeckExtensionInventory
            EnsureAttached(Transform player)
        {
            if (player == null) return null;

            PrototypeDeckExtensionInventory inventory =
                player.GetComponent<
                    PrototypeDeckExtensionInventory>();
            if (inventory == null)
            {
                inventory = player.gameObject.AddComponent<
                    PrototypeDeckExtensionInventory>();
            }

            inventory.Bind(player);
            return inventory;
        }

        public DeckExtensionInstallResult
            TryInstallOnActiveShip()
        {
            PrototypeHarborDockingDirector docking =
                PrototypeHarborDockingDirector.Instance;
            if (docking == null ||
                !docking.IsDockedAt(
                    PrototypeHarborStation.Shipyard))
            {
                return DeckExtensionInstallResult
                    .NotAtShipyard;
            }
            if (availableExtensions <= 0)
                return DeckExtensionInstallResult.NoExtension;
            if (profile == null ||
                profile.Definition == null)
            {
                return DeckExtensionInstallResult
                    .ProfileUnavailable;
            }

            int installed = GetInstalled(profile.ShipId);
            int limit = Mathf.Max(
                0,
                profile.Definition.deckExtensionLimit);
            if (installed >= limit)
            {
                return DeckExtensionInstallResult
                    .MaximumInstalled;
            }

            availableExtensions--;
            SetInstalled(profile.ShipId, installed + 1);
            ApplyActive(false);
            DeckExtensionsChanged?.Invoke();
            Debug.Log(
                $"Extended Deck kuruldu: " +
                $"{profile.Definition.displayName} " +
                $"{installed + 1}/{limit}.",
                this
            );
            return DeckExtensionInstallResult.Completed;
        }

        public void AwardLeviathan(string creatureName)
        {
            if (string.IsNullOrEmpty(creatureName) ||
                creatureName.IndexOf(
                    "Stormjaw",
                    StringComparison.OrdinalIgnoreCase) < 0 ||
                UnityEngine.Random.value > 0.25f)
            {
                return;
            }

            availableExtensions++;
            DeckExtensionsChanged?.Invoke();
            Debug.Log(
                "Nadir ganimet: +1 Extended Deck.",
                this
            );
        }

        public int GetInstalled(string shipId)
        {
            if (string.Equals(
                    shipId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
                return ratSailsExtensions;
            if (string.Equals(
                    shipId,
                    "dreadwake",
                    StringComparison.OrdinalIgnoreCase))
                return dreadwakeExtensions;
            return starterSloopExtensions;
        }

        public void Restore(
            int available,
            int starter,
            int rat,
            int dreadwake)
        {
            availableExtensions = Mathf.Max(0, available);
            starterSloopExtensions = Mathf.Max(0, starter);
            ratSailsExtensions = Mathf.Max(0, rat);
            dreadwakeExtensions = Mathf.Max(0, dreadwake);
            ApplyActive(false);
            DeckExtensionsChanged?.Invoke();
        }

        private void Bind(Transform player)
        {
            if (profile != null)
                profile.ProfileApplied -=
                    HandleProfileApplied;

            profile = player.GetComponentInChildren<
                ShipProfileController>();
            if (profile != null)
            {
                profile.ProfileApplied +=
                    HandleProfileApplied;
                ApplyActive(false);
            }
        }

        private void HandleProfileApplied(
            ShipDefinition definition)
        {
            ApplyActive(false);
        }

        private void ApplyActive(bool restoreHealth)
        {
            if (profile == null ||
                profile.Definition == null)
                return;

            int installed = Mathf.Clamp(
                GetInstalled(profile.ShipId),
                0,
                profile.Definition.deckExtensionLimit
            );
            SetInstalled(profile.ShipId, installed);
            profile.SetDeckExtensions(
                installed,
                restoreHealth
            );
        }

        private void SetInstalled(
            string shipId,
            int value)
        {
            if (string.Equals(
                    shipId,
                    "rat_sails",
                    StringComparison.OrdinalIgnoreCase))
            {
                ratSailsExtensions = value;
            }
            else if (string.Equals(
                         shipId,
                         "dreadwake",
                         StringComparison.OrdinalIgnoreCase))
            {
                dreadwakeExtensions = value;
            }
            else
            {
                starterSloopExtensions = value;
            }
        }

        private void OnDestroy()
        {
            if (profile != null)
                profile.ProfileApplied -=
                    HandleProfileApplied;
        }
    }
}
