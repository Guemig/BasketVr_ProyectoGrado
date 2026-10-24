using UnityEngine;

public class DribbleBallRespawn : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;
    [SerializeField] private DribbleBallStart ballStart;
    [SerializeField] private DribbleHandManager handManager;

    [SerializeField] private Transform ballSpawn;

    [SerializeField] private float respawnDelay = 2f;

    private float uncontrolledTime;

    private void Update()
    {
        if (dribbleBall == null)
            return;

        if (dribbleBall.IsControlled)
        {
            uncontrolledTime = 0f;
            return;
        }

        uncontrolledTime +=
            Time.deltaTime;

        if (uncontrolledTime >=
            respawnDelay)
        {
            RespawnNow();
        }
    }

    public void RespawnNow()
    {
        uncontrolledTime = 0f;

        if (handManager != null)
            handManager.ResetBallControl();

        if (dribbleBall != null)
            dribbleBall.ReleaseHand();

        if (freeMovement != null)
            freeMovement.StopFreeMovement();

        if (ballStart != null)
            ballStart.ResetToSpawnState();

        if (ballSpawn != null)
        {
            transform.position =
                ballSpawn.position;

            transform.rotation =
                ballSpawn.rotation;
        }
    }
}