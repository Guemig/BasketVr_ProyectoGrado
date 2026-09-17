using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class MetaBasketball : MonoBehaviour
{
    private Rigidbody rb;
    private bool wasKinematic = true;
    private bool isGrabbed = false;
    private Vector3 lastPos;
    private Vector3 releaseVelocity;

    [Header("Física del balón")]
    public float gravityScale = 1.0f;      // Escala de gravedad
    public float bounceFactor = 0.8f;      // Rebote vertical
    public float friction = 0.3f;          // Friccion lateral
    public float energyLoss = 0.9f;        // Perdida de energia en rebotes
    public float minBounceSpeed = 0.3f;    // Velocidad minima antes de detenerse

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        wasKinematic = rb.isKinematic;
    }

    void Update()
    {
        // Detectar si el sistema de agarre cambió el estado de kinematic
        if (rb.isKinematic && !wasKinematic)
        {
            // Se agarro
            OnGrabbed();
        }
        else if (!rb.isKinematic && wasKinematic)
        {
            // Se solto
            OnReleased();
        }

        wasKinematic = rb.isKinematic;

        // Calcular velocidad de la mano mientras se agarra
        if (isGrabbed)
        {
            releaseVelocity = (transform.position - lastPos) / Time.deltaTime;
            lastPos = transform.position;
        }
    }

    private void OnGrabbed()
    {
        isGrabbed = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        lastPos = transform.position;
    }

    private void OnReleased()
    {
        isGrabbed = false;
        rb.useGravity = true;

        // Aplicar la velocidad capturada al soltar (simula impulso)
        rb.linearVelocity = releaseVelocity;
    }

    void FixedUpdate()
    {
        // Aplicar gravedad personalizada (por si la del sistema está desactivada)
        if (!isGrabbed && rb.useGravity)
            rb.AddForce(Physics.gravity * gravityScale, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isGrabbed || collision.contacts.Length == 0)
            return;

        Vector3 velocity = rb.linearVelocity;
        Vector3 normal = collision.contacts[0].normal;
        float speed = velocity.magnitude;

        if (speed > minBounceSpeed)
        {
            // Rebote
            Vector3 reflected = Vector3.Reflect(velocity, normal) * bounceFactor * energyLoss;

            // Frena lateralmente
            reflected.x *= (1 - friction);
            reflected.z *= (1 - friction);

            rb.linearVelocity = reflected;
        }
        else
        {
            // Se detiene
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
        }
    }
}
