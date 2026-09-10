using Seaborn.Ship;
using UnityEngine;

namespace Seaborn.Hunting
{
    [DisallowMultipleComponent]
    public sealed class HuntCargoLossOnSinking :
        MonoBehaviour
    {
        private ShipHealth shipHealth;
        private PrototypeHuntCargo cargo;

        private void Awake()
        {
            shipHealth =
                GetComponentInParent<ShipHealth>();
            cargo =
                GetComponentInChildren<
                    PrototypeHuntCargo>();
        }

        private void OnEnable()
        {
            if (shipHealth == null)
            {
                shipHealth =
                    GetComponentInParent<ShipHealth>();
            }

            if (shipHealth != null)
            {
                shipHealth.Sunk += HandleShipSunk;
            }
        }

        private void OnDisable()
        {
            if (shipHealth != null)
            {
                shipHealth.Sunk -= HandleShipSunk;
            }
        }

        private void HandleShipSunk()
        {
            if (cargo == null)
            {
                cargo =
                    GetComponentInChildren<
                        PrototypeHuntCargo>();
            }

            if (cargo == null || !cargo.HasCargo)
            {
                return;
            }

            int lostValue = cargo.LoseAllCargo();

            if (lostValue > 0)
            {
                Debug.Log(
                    $"Gemi battı: {lostValue} silver " +
                    "değerindeki güvencesiz av yükü " +
                    "kaybedildi.",
                    this
                );
            }
        }
    }
}
