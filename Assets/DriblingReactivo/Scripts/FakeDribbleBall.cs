using UnityEngine;

public class FakeDribbleBall : MonoBehaviour
{
    [SerializeField] private Transform handTarget;

    [Header("Dribling")]
    [SerializeField] private float floorY = 0.2f;
    [SerializeField] private float dribbleSpeed = 4f;
    [SerializeField] private float handOffsetY = -0.15f;

    [Header("Seguimiento")]
    [SerializeField] private float followSmoothTime = 0.05f;
    [SerializeField] private float maxFollowSpeed = 10f;

    [Header("Rotación")]
    [SerializeField] private Vector3 rotationSpeed =
        new Vector3(90f, 60f, 40f);

    private float dribbleTime;
    private Vector3 horizontalVelocity;

    public bool IsControlled => handTarget != null;

    public Transform CurrentHand => handTarget;

    public void SetHandTarget(Transform newHand)
    {
        handTarget = newHand;
        horizontalVelocity = Vector3.zero;
    }

    public void ReleaseHand()
    {
        handTarget = null;
        horizontalVelocity = Vector3.zero;
    }

    private void Update()
    {
        transform.Rotate(
            rotationSpeed * Time.deltaTime,
            Space.Self
        );

        if (handTarget == null)
            return;

        dribbleTime +=
            Time.deltaTime *
            dribbleSpeed;

        float bounce =
            (Mathf.Sin(dribbleTime) + 1f) /
            2f;

        float topY =
            handTarget.position.y +
            handOffsetY;

        float newY =
            Mathf.Lerp(
                floorY,
                topY,
                bounce
            );

        Vector3 targetPosition =
            new Vector3(
                handTarget.position.x,
                transform.position.y,
                handTarget.position.z
            );

        Vector3 smoothPosition =
            Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref horizontalVelocity,
                followSmoothTime,
                maxFollowSpeed
            );

        transform.position =
            new Vector3(
                smoothPosition.x,
                newY,
                smoothPosition.z
            );
    }
}