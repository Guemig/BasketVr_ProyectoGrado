using UnityEngine;

public class DribbleBallFreeMovement : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;

    [Header("Movimiento libre")]
    [SerializeField] private float impulseMultiplier = 0.65f;
    [SerializeField] private float maximumSpeed = 3f;
    [SerializeField] private float horizontalDrag = 0.35f;

    [Header("Caída")]
    [SerializeField] private float gravity = 5f;
    [SerializeField] private float floorY = 0.2f;

    [Header("Rebote libre")]
    [SerializeField] private float bounceStrength = 0.9f;
    [SerializeField] private float minimumBounceUpSpeed = 4.5f;
    [SerializeField] private float bounceHorizontalDamping = 0.96f;
    [SerializeField] private float minimumBounceVelocity = 0.25f;

    private Vector3 velocity;
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

        velocity.y -=
            gravity *
            Time.deltaTime;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        horizontalVelocity =
            Vector3.MoveTowards(
                horizontalVelocity,
                Vector3.zero,
                horizontalDrag * Time.deltaTime
            );

        velocity.x =
            horizontalVelocity.x;

        velocity.z =
            horizontalVelocity.z;

        Vector3 newPosition =
            transform.position +
            velocity *
            Time.deltaTime;

        if (newPosition.y <= floorY)
        {
            newPosition.y = floorY;

            float bounceVelocity =
                Mathf.Abs(velocity.y) *
                bounceStrength;

            if (bounceVelocity >
                minimumBounceVelocity)
            {
                velocity.y =
                    Mathf.Max(
                        bounceVelocity,
                        minimumBounceUpSpeed
                    );

                velocity.x *=
                    bounceHorizontalDamping;

                velocity.z *=
                    bounceHorizontalDamping;
            }
            else
            {
                velocity.y = 0f;
            }
        }

        transform.position =
            newPosition;
    }

    public void Launch(Vector3 handVelocity)
    {
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