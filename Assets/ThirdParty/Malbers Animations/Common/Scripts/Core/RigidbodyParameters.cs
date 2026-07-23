using UnityEngine;

namespace MalbersAnimations
{
    public struct RigidbodyParameters
    {
        public bool useGravity;
        public bool isKinematic;
        public CollisionDetectionMode collisionDetectionMode;
        public RigidbodyConstraints constraints;
        public bool detectCollisions;
        public RigidbodyInterpolation interpolation;
        public float mass;
        public float drag;
        public float angularDrag;
        public Vector3 centerOfMass;
        public Vector3 inertiaTensor;

        public RigidbodyParameters(Rigidbody rb)
        {
            useGravity = rb.useGravity;
            isKinematic = rb.isKinematic;
            collisionDetectionMode = rb.collisionDetectionMode;
            constraints = rb.constraints;
            detectCollisions = rb.detectCollisions;
            interpolation = rb.interpolation;
            mass = rb.mass;
            drag = rb.drag;
            angularDrag = rb.angularDrag;
            centerOfMass = rb.centerOfMass;
            inertiaTensor = rb.inertiaTensor;
        }

        public readonly void RestoreRigidBody(Rigidbody rb)
        {
            if (rb == null) return;
            rb.useGravity = useGravity;
            rb.isKinematic = isKinematic;
            rb.collisionDetectionMode = collisionDetectionMode;
            rb.constraints = constraints;
            rb.detectCollisions = detectCollisions;
            rb.interpolation = interpolation;
            rb.mass = mass;
            rb.drag = drag;
            rb.angularDrag = angularDrag;
            rb.centerOfMass = centerOfMass;
            rb.inertiaTensor = inertiaTensor;
        }

    }
}