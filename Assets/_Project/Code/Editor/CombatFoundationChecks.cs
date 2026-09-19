using System;
using Seaborn.Combat;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class CombatFoundationChecks
    {
        [MenuItem("Seaborn/Validation/Check Broadside Allocation")]
        public static void Run()
        {
            GameObject ship = new GameObject("Broadside Check");
            try
            {
                BroadsideController battery = ship.AddComponent<BroadsideController>();
                Transform[] port = new Transform[3];
                Transform[] starboard = new Transform[3];
                for (int i = 0; i < 3; i++)
                {
                    port[i] = Muzzle(ship.transform, -1f, i);
                    starboard[i] = Muzzle(ship.transform, 1f, i);
                }
                battery.SetRuntimeMuzzles(port, starboard);
                battery.SetShipConfiguration(12, 6, 10f);
                Check(battery, 3, 3, "six total cannons");
                battery.SetShipConfiguration(12, 5, 10f);
                Check(battery, 3, 2, "odd loadout assigns spare to port");
                battery.SetShipConfiguration(12, 12, 10f);
                Check(battery, 3, 3, "physical muzzle count caps the salvo");
                battery.SetRuntimeMuzzles(new[] { port[0], port[0], starboard[0], null }, starboard);
                Check(battery, 1, 3, "duplicate, wrong-side and null muzzles excluded");
                port[0].gameObject.SetActive(false);
                Check(battery, 0, 3, "disabled mount excluded");
                ship.transform.rotation = Quaternion.Euler(0f, 137f, 0f);
                Check(battery, 0, 3, "allocation follows rotated ship space");
                Debug.Log("Seaborn broadside allocation checks passed.");
            }
            finally { UnityEngine.Object.DestroyImmediate(ship); }
        }

        private static Transform Muzzle(Transform parent, float x, int index)
        {
            var muzzle = new GameObject("Muzzle").transform;
            muzzle.SetParent(parent, false);
            muzzle.localPosition = new Vector3(x, 0f, index);
            return muzzle;
        }

        private static void Check(BroadsideController battery, int port, int starboard, string scenario)
        {
            if (battery.GetBroadsideCannonCount(BroadsideSide.Port) != port ||
                battery.GetBroadsideCannonCount(BroadsideSide.Starboard) != starboard)
                throw new Exception("Broadside allocation failed: " + scenario);
        }
    }
}
