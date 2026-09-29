using System;
using System.Reflection;
using Seaborn.Atmosphere;
using Seaborn.Hunting;
using Seaborn.Ship;
using UnityEditor;
using UnityEngine;

namespace Seaborn.Editor
{
    public static class SailingFeelChecks
    {
        [MenuItem("Seaborn/Validation/Check Sailing Feel")]
        public static void Check()
        {
            MethodInfo method = typeof(ShipContactResponse).GetMethod("SlideVelocity",
                BindingFlags.NonPublic | BindingFlags.Static);
            Require(method != null, "Contact response helper is missing.");
            Vector3 Slide(Vector3 velocity, Vector3 normal, float separation) =>
                (Vector3)method.Invoke(null, new object[] { velocity, normal, separation });

            Vector3 glancing = Slide(new Vector3(-7f, 2f, 4f), Vector3.right, 0.65f);
            Near(glancing, new Vector3(0.65f, 2f, 4f), "Glancing contact must preserve vertical/tangential velocity.");
            Near(Slide(glancing, Vector3.right, 0.65f), glancing, "Repeated contact must not accumulate speed.");
            Vector3 escaping = new(3f, 0f, -2f);
            Near(Slide(escaping, Vector3.right, 0.65f), escaping, "Leaving a hull must not be slowed.");
            Near(Slide(new Vector3(-9f, 0f, 5f), Vector3.right, 0f), new Vector3(0f, 0f, 5f),
                "Solid shore contact must slide without bouncing.");
            Vector3 diagonal = new Vector3(1f, 0f, 1f).normalized;
            Vector3 tangent = Vector3.Cross(Vector3.up, diagonal) * 6f;
            Near(Slide(tangent - diagonal * 20f, diagonal, 0.65f), tangent + diagonal * 0.65f,
                "Angled bow contact must cap separation regardless of incoming speed.");

            if (!Application.isPlaying)
            {
                Debug.Log("Sailing contact math checks passed. Run this menu in Play Mode to also check live ship, hunt and ocean setup.");
                return;
            }

            int ships = 0, creatures = 0;
            foreach (var ship in UnityEngine.Object.FindObjectsByType<ShipHealth>(FindObjectsSortMode.None))
            {
                if (ship.IsSunk || (ship.GetComponent<ShipMotor>() == null &&
                    ship.GetComponent<EnemyShipController>() == null)) continue;
                ships++;
                var response = ship.GetComponent<ShipContactResponse>();
                Require(response != null && response.enabled, ship.name + ": missing contact response.");
                var body = ship.GetComponent<Rigidbody>();
                Require(body != null && !body.isKinematic, ship.name + ": navigation body must be dynamic.");
                Require((body.constraints & (RigidbodyConstraints.FreezePositionX |
                    RigidbodyConstraints.FreezePositionZ)) == 0, ship.name + ": planar position is frozen.");
                foreach (var collider in ship.GetComponentsInChildren<Collider>())
                    if (!collider.isTrigger && collider.attachedRigidbody == body && collider.enabled)
                        Require(collider.sharedMaterial != null && collider.sharedMaterial.staticFriction == 0f &&
                            collider.sharedMaterial.dynamicFriction == 0f, ship.name + ": sticky collider " + collider.name);
            }
            foreach (var creature in UnityEngine.Object.FindObjectsByType<PrototypeSeaCreature>(FindObjectsSortMode.None))
            {
                if (creature.IsHarvested) continue;
                creatures++;
                var target = creature.GetComponent<CapsuleCollider>();
                Require(target != null && target.enabled && target.isTrigger, creature.name + ": solid/missing hunt target.");
                foreach (var collider in creature.GetComponentsInChildren<Collider>())
                    Require(!collider.enabled || collider.isTrigger, creature.name + ": solid visual collider.");
            }
            var ocean = UnityEngine.Object.FindFirstObjectByType<OceanHorizonExtension>();
            Require(ocean != null, "Ocean horizon is missing.");
            var ring = ocean.transform.Find("Distant Ocean");
            Require(ring != null && ring.GetComponent<Collider>() == null, "Horizon must exist without a collider.");
            Mesh mesh = ring.GetComponent<MeshFilter>().sharedMesh;
            Require(mesh != null && mesh.vertexCount == 8 && mesh.triangles.Length == 24, "Invalid horizon ring.");
            Require(ring.GetComponent<Renderer>().sharedMaterial == ocean.GetComponent<Renderer>().sharedMaterial,
                "Ocean and horizon must share the regional material.");
            Require(ships > 0, "No live ships found; start a sea scene.");
            Debug.Log($"Sailing feel setup checks passed: {ships} ships, {creatures} hunt targets, ocean horizon. " +
                "Still play-test contact, harpoon hits, camera edges and UI at your screen resolution.");
        }

        private static void Near(Vector3 actual, Vector3 expected, string message) =>
            Require((actual - expected).sqrMagnitude < 0.00001f, message);

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException(message);
        }
    }
}
