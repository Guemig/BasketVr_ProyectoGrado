using UnityEngine;

public class DribbleBallRespawn : MonoBehaviour
{
    [SerializeField] private FakeDribbleBall dribbleBall;
    [SerializeField] private DribbleBallFreeMovement freeMovement;

    [SerializeField] private Transform ballSpawn;

    [SerializeField] private float respawnDelay = 2f;

    private float uncontrolledTime;

    private void Update()
    {
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
            Respawn();
        }
    }

    public void Respawn()
    {
        uncontrolledTime = 0f;

        dribbleBall.ReleaseHand();

        freeMovement.StopFreeMovement();

        transform.position =
            ballSpawn.position;

        transform.rotation =
            ballSpawn.rotation;
    }
}