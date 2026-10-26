using System;
using UnityEngine;

public class DribbleHandManager : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;
    [SerializeField] private DribbleBallStart ballStart;

    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [Header("Captura")]
    [SerializeField] private float grabLockTime = 0.25f;
    [SerializeField] private bool snapToHandOnCatch = true;

    [Header("Impulso")]
    [SerializeField] private float releaseSpeed = 0.7f;
    [SerializeField] private float maximumReleaseAngle = 55f;

    private DribbleHandDetector controllingHand;

    private bool waitingForHandChange;
    private bool releasedFromLeftHand;

    private float grabLockTimer;

    public bool HasControl =>
        controllingHand != null &&
        dribbleBall != null &&
        dribbleBall.IsControlled;

    public bool IsLeftHandControlling =>
        HasControl &&
        controllingHand.IsLeftHand;

    public bool IsRightHandControlling =>
        HasControl &&
        !controllingHand.IsLeftHand;

    public event Action OnSuccessfulHandChange;

    private void Update()
    {
        if (grabLockTimer > 0f)
        {
            grabLockTimer -=
                Time.deltaTime;
        }

        if (controllingHand == null)
            return;

        if (dribbleBall == null)
            return;

        if (!dribbleBall.IsControlled)
            return;

        if (grabLockTimer > 0f)
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
        if (detector == null)
            return;

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

            waitingForHandChange =
                false;

            grabLockTimer =
                grabLockTime;

            if (snapToHandOnCatch)
                SnapBallHorizontal(hand);

            ballStart.StartFromSpawn(
                hand
            );

            return;
        }

        TakeControl(
            detector
        );
    }

    private void TakeControl(
        DribbleHandDetector detector)
    {
        controllingHand =
            detector;

        grabLockTimer =
            grabLockTime;

        if (freeMovement != null)
            freeMovement.StopFreeMovement();

        Transform hand =
            detector.IsLeftHand
            ? leftHandTarget
            : rightHandTarget;

        if (snapToHandOnCatch)
            SnapBallHorizontal(hand);

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

        CheckSuccessfulHandChange(
            detector
        );
    }

    private void SnapBallHorizontal(
        Transform hand)
    {
        if (hand == null)
            return;

        Vector3 position =
            dribbleBall.transform.position;

        position.x =
            hand.position.x;

        position.z =
            hand.position.z;

        dribbleBall.transform.position =
            position;
    }

    private void CheckSuccessfulHandChange(
        DribbleHandDetector detector)
    {
        if (!waitingForHandChange)
            return;

        bool changedHand =
            releasedFromLeftHand !=
            detector.IsLeftHand;

        waitingForHandChange =
            false;

        if (!changedHand)
            return;

        OnSuccessfulHandChange?.Invoke();
    }

    private void ReleaseBall(
        DribbleHandDetector detector,
        Vector3 velocity)
    {
        releasedFromLeftHand =
            detector.IsLeftHand;

        waitingForHandChange =
            true;

        dribbleBall.ReleaseHand();

        detector.BlockUntilExit();

        controllingHand =
            null;

        grabLockTimer =
            0f;

        if (freeMovement != null)
        {
            freeMovement.Launch(
                velocity
            );
        }
    }

    public void ResetBallControl()
    {
        if (controllingHand != null)
            controllingHand.BlockUntilExit();

        controllingHand =
            null;

        waitingForHandChange =
            false;

        grabLockTimer =
            0f;

        if (dribbleBall != null)
            dribbleBall.ReleaseHand();

        if (freeMovement != null)
            freeMovement.StopFreeMovement();
    }
}