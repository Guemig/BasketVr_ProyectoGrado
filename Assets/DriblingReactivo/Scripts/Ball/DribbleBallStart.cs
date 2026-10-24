using System.Collections;
using UnityEngine;

public class DribbleBallStart : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;
    [SerializeField] private Transform ballSpawn;
    [SerializeField] private float startDelay = 1f;
    [SerializeField] private float spawnTolerance = 0.15f;

    private bool waitingToStart;
    private bool dribbleEnabled = true;
    private Coroutine startCoroutine;

    public bool DribbleEnabled => dribbleEnabled;
    public bool WaitingToStart => waitingToStart;

    public bool IsAtSpawn()
    {
        if (ballSpawn == null)
            return false;

        return Vector3.Distance(
            transform.position,
            ballSpawn.position
        ) <= spawnTolerance;
    }

    public void StartFromSpawn(Transform hand)
    {
        if (!IsAtSpawn())
            return;

        if (waitingToStart)
            return;

        if (startCoroutine != null)
            StopCoroutine(startCoroutine);

        startCoroutine =
            StartCoroutine(
                StartDribbleRoutine(hand)
            );
    }

    private IEnumerator StartDribbleRoutine(Transform hand)
    {
        waitingToStart = true;
        dribbleEnabled = false;

        if (freeMovement != null)
            freeMovement.StopFreeMovement();

        dribbleBall.SetHandTarget(hand);

        float timer = 0f;

        while (timer < startDelay)
        {
            if (hand == null)
                break;

            transform.position =
                hand.position;

            timer += Time.deltaTime;

            yield return null;
        }

        waitingToStart = false;
        dribbleEnabled = true;
        startCoroutine = null;
    }

    public void ResetToSpawnState()
    {
        if (startCoroutine != null)
        {
            StopCoroutine(startCoroutine);
            startCoroutine = null;
        }

        waitingToStart = false;
        dribbleEnabled = true;

        dribbleBall.ReleaseHand();
    }
}