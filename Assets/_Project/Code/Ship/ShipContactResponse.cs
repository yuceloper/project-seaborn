using UnityEngine;

namespace Seaborn.Ship
{
    // Runs after both navigation controllers so they cannot erase separation velocity.
    [DefaultExecutionOrder(100)]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public sealed class ShipContactResponse : MonoBehaviour
    {
        private readonly Vector3[] normals = new Vector3[12];
        private readonly bool[] hullContacts = new bool[12];
        private Rigidbody body;
        private ShipHealth health;
        private PhysicsMaterial slidingMaterial;
        private int contactCount;
        private float contactStep = float.NegativeInfinity;

        public bool HasContact => contactCount > 0 &&
            Time.fixedTime - contactStep <= Time.fixedDeltaTime * 1.5f;

        public static ShipContactResponse Ensure(GameObject ship)
        {
            var response = ship.GetComponent<ShipContactResponse>();
            return response != null ? response : ship.AddComponent<ShipContactResponse>();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            health = GetComponent<ShipHealth>();
            slidingMaterial = new PhysicsMaterial("Sliding ship hull")
            {
                staticFriction = 0f,
                dynamicFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            RefreshColliders();
        }

        // Runtime ship assemblers may add colliders after Awake.
        private void Start() => RefreshColliders();

        private void RefreshColliders()
        {
            foreach (var collider in GetComponentsInChildren<Collider>(true))
                if (!collider.isTrigger && collider.attachedRigidbody == body)
                    collider.sharedMaterial = slidingMaterial;
        }

        private void OnCollisionEnter(Collision collision) => Register(collision);
        private void OnCollisionStay(Collision collision) => Register(collision);

        private void Register(Collision collision)
        {
            if (contactStep != Time.fixedTime)
            {
                contactStep = Time.fixedTime;
                contactCount = 0;
            }
            bool hull = collision.collider.GetComponentInParent<ShipHealth>() != null;
            for (int i = 0; i < collision.contactCount && contactCount < normals.Length; i++)
            {
                Vector3 normal = collision.GetContact(i).normal;
                // Ignore the ocean floor and deck/support contacts.
                if (Mathf.Abs(normal.y) > 0.65f) continue;
                normal.y = 0f;
                if (normal.sqrMagnitude < 0.01f) continue;
                normal.Normalize();
                bool duplicate = false;
                for (int j = 0; j < contactCount; j++)
                    if (Vector3.Dot(normals[j], normal) > 0.98f)
                    {
                        hullContacts[j] |= hull;
                        duplicate = true;
                        break;
                    }
                if (duplicate) continue;
                normals[contactCount] = normal;
                hullContacts[contactCount++] = hull;
            }
        }

        private void FixedUpdate()
        {
            if (health == null) health = GetComponent<ShipHealth>();
            if (!HasContact || body.isKinematic || (health != null && health.IsSunk)) return;
            Vector3 velocity = body.linearVelocity;
            // Preserve tangential travel. Small, capped outward motion replaces sticky contact;
            // there is deliberately no teleport, recoil impulse, or collision damage here.
            for (int i = 0; i < contactCount; i++)
                velocity = SlideVelocity(velocity, normals[i], hullContacts[i] ? 0.65f : 0f);
            body.linearVelocity = velocity;
        }

        internal static Vector3 SlideVelocity(Vector3 velocity, Vector3 normal, float separationSpeed)
        {
            float normalSpeed = Vector3.Dot(velocity, normal);
            return normalSpeed < separationSpeed
                ? velocity + normal * (separationSpeed - normalSpeed)
                : velocity;
        }

        private void OnDisable()
        {
            contactCount = 0;
            contactStep = float.NegativeInfinity;
        }

        private void OnDestroy()
        {
            if (slidingMaterial != null) Destroy(slidingMaterial);
        }
    }
}
