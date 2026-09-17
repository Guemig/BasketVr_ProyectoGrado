using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class RealisticBasketball : MonoBehaviour
{
    private Rigidbody rb;
    private float lastYVelocity;
    private bool groundedRecently = false;
    private float groundTimer = 0f;

    [Header("Physics Settings")]
    [Range(0.1f, 1f)] public float bounceEnergyLoss = 0.85f; // 1 = rebote perfecto
    [Range(0f, 0.2f)] public float minBounceVelocity = 0.05f; // velocidad minima antes de detenerse
    [Range(0f, 0.5f)] public float groundStickDelay = 0.15f;  // tiempo antes de “dormir” el balón

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void FixedUpdate()
    {
        // Si el balon está muy quieto cerca del suelo, se duerme para evitar vibraciones
        if (groundedRecently)
        {
            groundTimer += Time.fixedDeltaTime;
            if (groundTimer >= groundStickDelay && rb.linearVelocity.magnitude < minBounceVelocity)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep();
                groundedRecently = false;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Rebote manual más realista
        if (collision.contacts.Length > 0)
        {
            ContactPoint contact = collision.contacts[0];

            // Calcula la velocidad de impacto vertical
            float impactY = Mathf.Abs(lastYVelocity);

            // Si viene cayendo y el impacto es relevante
            if (impactY > minBounceVelocity)
            {
                Vector3 vel = rb.linearVelocity;
                vel.y = impactY * bounceEnergyLoss; // conserva parte de la energia
                rb.linearVelocity = vel;
            }

            groundedRecently = true;
            groundTimer = 0f;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        groundedRecently = false;
    }

    void LateUpdate()
    {
        // Guarda la velocidad vertical previa
        lastYVelocity = rb.linearVelocity.y;
    }
}
