using UnityEngine;

public class DribbleBallFreeMovement : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;

    [Header("Movimiento libre")]
    [SerializeField] private float impulseMultiplier = 0.65f;
    [SerializeField] private float maximumSpeed = 3f;
    [SerializeField] private float maximumDistance = 1.2f;

    [Header("Caída")]
    [SerializeField] private float gravity = 4f;

    private Vector3 velocity;
    private Vector3 releasePosition;
    private bool movingFreely;

    public bool MovingFreely => movingFreely;

    private void Update()
    {
        if (!movingFreely)
            return;

        if (dribbleBall.IsControlled)
        {
            StopFreeMovement();
            return;
        }

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        float distance =
            Vector3.Distance(
                new Vector3(
                    transform.position.x,
                    0f,
                    transform.position.z
                ),
                new Vector3(
                    releasePosition.x,
                    0f,
                    releasePosition.z
                )
            );

        if (distance >= maximumDistance)
        {
            horizontalVelocity = Vector3.zero;
        }

        velocity.x = horizontalVelocity.x;
        velocity.z = horizontalVelocity.z;

        velocity.y -=
            gravity *
            Time.deltaTime;

        transform.position +=
            velocity *
            Time.deltaTime;
    }

    public void Launch(Vector3 handVelocity)
    {
        releasePosition =
            transform.position;

        Vector3 horizontalImpulse =
            new Vector3(
                handVelocity.x,
                0f,
                handVelocity.z
            );

        horizontalImpulse *=
            impulseMultiplier;

        if (horizontalImpulse.magnitude >
            maximumSpeed)
        {
            horizontalImpulse =
                horizontalImpulse.normalized *
                maximumSpeed;
        }

        velocity =
            new Vector3(
                horizontalImpulse.x,
                0f,
                horizontalImpulse.z
            );

        movingFreely = true;
    }

    public void StopFreeMovement()
    {
        movingFreely = false;
        velocity = Vector3.zero;
    }
}