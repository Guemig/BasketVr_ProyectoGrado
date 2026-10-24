using UnityEngine;

public class DribbleBallFreeMovement : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallAudio ballAudio;

    [Header("Movimiento")]
    [SerializeField] private float impulseMultiplier = 0.65f;
    [SerializeField] private float maximumSpeed = 3f;
    [SerializeField] private float horizontalDrag = 0.35f;

    [Header("Salida de la mano")]
    [SerializeField] private float releaseUpSpeed = 0f;

    [Header("Rebote")]
    [SerializeField] private float gravity = 9f;
    [SerializeField] private float floorY = 0.2f;
    [SerializeField] private float bounceUpSpeed = 3.3f;
    [SerializeField] private float minimumBounceUpSpeed = 1.6f;
    [SerializeField] private float bounceDecayPerSecond = 0.25f;
    [SerializeField] private float bounceHorizontalDamping = 0.92f;

    [Header("Estado")]
    [SerializeField] private float movementThreshold = 0.05f;

    private Vector3 velocity;

    private bool movingFreely;
    private bool floorHitRegistered;

    private float currentBounceUpSpeed;

    public bool MovingFreely =>
        movingFreely;

    public bool IsMoving =>
        movingFreely &&
        velocity.magnitude >
        movementThreshold;

    private void Awake()
    {
        currentBounceUpSpeed =
            bounceUpSpeed;
    }

    private void Update()
    {
        if (!movingFreely)
            return;

        if (dribbleBall != null &&
            dribbleBall.IsControlled)
        {
            StopFreeMovement();
            return;
        }

        currentBounceUpSpeed =
            Mathf.MoveTowards(
                currentBounceUpSpeed,
                minimumBounceUpSpeed,
                bounceDecayPerSecond *
                Time.deltaTime
            );

        velocity.y -=
            gravity *
            Time.deltaTime;

        velocity.x =
            Mathf.MoveTowards(
                velocity.x,
                0f,
                horizontalDrag *
                Time.deltaTime
            );

        velocity.z =
            Mathf.MoveTowards(
                velocity.z,
                0f,
                horizontalDrag *
                Time.deltaTime
            );

        Vector3 newPosition =
            transform.position +
            velocity *
            Time.deltaTime;

        if (newPosition.y <=
            floorY)
        {
            newPosition.y =
                floorY;

            if (velocity.y < 0f)
            {
                if (!floorHitRegistered)
                {
                    if (ballAudio != null)
                        ballAudio.PlayBounce();

                    floorHitRegistered =
                        true;
                }

                velocity.y =
                    currentBounceUpSpeed;

                velocity.x *=
                    bounceHorizontalDamping;

                velocity.z *=
                    bounceHorizontalDamping;
            }
        }
        else
        {
            if (velocity.y > 0.1f)
            {
                floorHitRegistered =
                    false;
            }
        }

        transform.position =
            newPosition;
    }

    public void Launch(
        Vector3 handVelocity)
    {
        Vector3 horizontalVelocity =
            new Vector3(
                handVelocity.x,
                0f,
                handVelocity.z
            );

        horizontalVelocity *=
            impulseMultiplier;

        if (horizontalVelocity.magnitude >
            maximumSpeed)
        {
            horizontalVelocity =
                horizontalVelocity.normalized *
                maximumSpeed;
        }

        velocity =
            horizontalVelocity;

        velocity.y =
            releaseUpSpeed;

        currentBounceUpSpeed =
            bounceUpSpeed;

        movingFreely =
            true;

        floorHitRegistered =
            false;
    }

    public void StopFreeMovement()
    {
        velocity =
            Vector3.zero;

        movingFreely =
            false;

        floorHitRegistered =
            false;

        currentBounceUpSpeed =
            bounceUpSpeed;
    }
}