using UnityEngine;

public class DribbleHandManager : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;
    [SerializeField] private DribbleBallStart ballStart;

    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Impulso")]
    [SerializeField] private float releaseSpeed = 0.7f;
    [SerializeField] private float maximumReleaseAngle = 45f;

    private DribbleHandDetector controllingHand;

    private void Update()
    {
        if (controllingHand == null)
            return;

        if (!dribbleBall.IsControlled)
            return;

        if (ballStart != null &&
            ballStart.WaitingToStart)
        {
            return;
        }

        Vector3 handVelocity =
            controllingHand.HandVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                handVelocity.x,
                0f,
                handVelocity.z
            );

        if (horizontalVelocity.magnitude <
            releaseSpeed)
        {
            return;
        }

        Transform oppositeHand =
            controllingHand.IsLeftHand
            ? rightHandTarget
            : leftHandTarget;

        if (oppositeHand == null)
            return;

        Vector3 directionToOtherHand =
            oppositeHand.position -
            dribbleBall.transform.position;

        directionToOtherHand.y = 0f;

        if (directionToOtherHand.sqrMagnitude <=
            0.001f)
        {
            return;
        }

        float angle =
            Vector3.Angle(
                horizontalVelocity.normalized,
                directionToOtherHand.normalized
            );

        if (angle >
            maximumReleaseAngle)
        {
            return;
        }

        ReleaseBall(
            controllingHand,
            horizontalVelocity
        );
    }

    public void TryTakeControl(
        DribbleHandDetector detector)
    {
        if (dribbleBall == null)
            return;

        if (dribbleBall.IsControlled)
            return;

        if (ballStart != null &&
            ballStart.IsAtSpawn())
        {
            Transform hand =
                detector.IsLeftHand
                ? leftHandTarget
                : rightHandTarget;

            controllingHand =
                detector;

            ballStart.StartFromSpawn(
                hand
            );

            return;
        }

        TakeControl(detector);
    }

    private void TakeControl(
        DribbleHandDetector detector)
    {
        controllingHand =
            detector;

        if (freeMovement != null)
        {
            freeMovement.StopFreeMovement();
        }

        if (detector.IsLeftHand)
        {
            dribbleBall.SetHandTarget(
                leftHandTarget
            );
        }
        else
        {
            dribbleBall.SetHandTarget(
                rightHandTarget
            );
        }
    }

    private void ReleaseBall(
        DribbleHandDetector detector,
        Vector3 velocity)
    {
        dribbleBall.ReleaseHand();

        detector.BlockUntilExit();

        controllingHand = null;

        if (freeMovement != null)
        {
            freeMovement.Launch(
                velocity
            );
        }
    }
}