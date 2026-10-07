using UnityEngine;

public class DribbleHandManager : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;

    [SerializeField] private Transform leftHandTarget;
    [SerializeField] private Transform rightHandTarget;

    [SerializeField] private float releaseSpeed = 0.7f;

    private DribbleHandDetector controllingHand;

    private void Update()
    {
        if (controllingHand == null)
            return;

        if (!dribbleBall.IsControlled)
            return;

        Vector3 velocity =
            controllingHand.HandVelocity;

        Vector3 horizontalVelocity =
            new Vector3(
                velocity.x,
                0f,
                velocity.z
            );

        if (horizontalVelocity.magnitude >= releaseSpeed)
        {
            ReleaseBall(
                controllingHand,
                horizontalVelocity
            );
        }
    }

    public void TryTakeControl(
        DribbleHandDetector detector)
    {
        if (dribbleBall.IsControlled)
            return;

        TakeControl(detector);
    }

    private void TakeControl(
        DribbleHandDetector detector)
    {
        controllingHand = detector;

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