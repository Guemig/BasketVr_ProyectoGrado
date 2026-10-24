using UnityEngine;

public class DribbleBallRotation : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;

    [SerializeField] private Vector3 rotationSpeed =
        new Vector3(90f, 60f, 40f);

    private void Update()
    {
        bool shouldRotate =
            dribbleBall.IsControlled ||
            freeMovement.IsMoving;

        if (!shouldRotate)
            return;

        transform.Rotate(
            rotationSpeed *
            Time.deltaTime,
            Space.Self
        );
    }
}