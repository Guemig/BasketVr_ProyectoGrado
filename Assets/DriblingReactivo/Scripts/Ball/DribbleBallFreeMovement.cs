using UnityEngine;

public class DribbleBallFreeMovement : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallAudio ballAudio;

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

    [Header("Detección de movimiento")]
    [SerializeField] private float movementThreshold = 0.05f;

    private Vector3 velocity;
    private bool movingFreely;
    private bool touchingFloor;

    public bool MovingFreely => movingFreely;

    public bool IsMoving =>
        movingFreely &&
        velocity.magnitude >
        movementThreshold;

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
                horizontalDrag *
                Time.deltaTime
            );

        velocity.x =
            horizontalVelocity.x;

        velocity.z =
            horizontalVelocity.z;

        Vector3 newPosition =
            transform.position +
            velocity *
            Time.deltaTime;

        if (newPosition.y <= floorY &&
            velocity.y < 0f)
        {
            newPosition.y = floorY;

            float impactSpeed =
                Mathf.Abs(velocity.y);

            if (!touchingFloor)
            {
                touchingFloor = true;

                if (ballAudio != null)
                {
                    ballAudio.PlayBounce();
                }
            }

            float bounceVelocity =
                impactSpeed *
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

                touchingFloor = false;
            }
            else
            {
                velocity = Vector3.zero;
                movingFreely = false;
                touchingFloor = true;
            }
        }
        else if (newPosition.y >
                 floorY + 0.03f)
        {
            touchingFloor = false;
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

        touchingFloor = false;
        movingFreely = true;
    }

    public void StopFreeMovement()
    {
        movingFreely = false;
        touchingFloor = false;
        velocity = Vector3.zero;
    }
}