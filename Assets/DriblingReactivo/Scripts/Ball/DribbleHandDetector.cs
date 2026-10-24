using UnityEngine;

public class DribbleHandDetector : MonoBehaviour
{
    [SerializeField] private DribbleHandManager handManager;
    [SerializeField] private bool isLeftHand;

    private Vector3 previousPosition;
    private Vector3 handVelocity;
    private bool blocked;

    public bool IsLeftHand => isLeftHand;

    public Vector3 HandVelocity =>
        handVelocity;

    private void Start()
    {
        previousPosition =
            transform.position;
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f)
            return;

        handVelocity =
            (transform.position -
            previousPosition) /
            Time.deltaTime;

        previousPosition =
            transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("DribbleBall"))
            return;

        if (blocked)
            return;

        handManager.TryTakeControl(this);
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("DribbleBall"))
            return;

        if (blocked)
            return;

        handManager.TryTakeControl(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("DribbleBall"))
            return;

        blocked = false;
    }

    public void BlockUntilExit()
    {
        blocked = true;
    }
}