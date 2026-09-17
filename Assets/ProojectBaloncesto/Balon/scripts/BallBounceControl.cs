using UnityEngine;

public class BallBounceControl : MonoBehaviour
{
    private Rigidbody rb;
    public float stopThreshold = 0.05f;   // Velocidad min detenerse
    public float sleepDelay = 0.2f;       // Tiempo para dormir
    private float lowSpeedTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // Si se mueve muy lento, acumula tiempo
        if (rb.linearVelocity.magnitude < stopThreshold && rb.IsSleeping() == false)
        {
            lowSpeedTimer += Time.fixedDeltaTime;
            if (lowSpeedTimer >= sleepDelay)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.Sleep(); // detiene física y evita microrebotes
            }
        }
        else
        {
            // Si se mueve rapido, resetea el contador
            lowSpeedTimer = 0f;
        }
    }
}
