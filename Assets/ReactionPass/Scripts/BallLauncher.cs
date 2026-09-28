using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] passTargets;
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Velocidades")]
    [SerializeField] private float slowFlightTime = 0.9f;
    [SerializeField] private float mediumFlightTime = 0.7f;
    [SerializeField] private float fastFlightTime = 0.5f;

    private GameObject currentBall;

    public void LaunchBall()
    {
        if (gameManager == null || !gameManager.GameRunning)
            return;

        if (currentBall != null)
            return;

        if (ballPrefab == null || spawnPoint == null)
            return;

        if (passTargets == null || passTargets.Length == 0)
            return;

        Transform target =
            passTargets[Random.Range(0, passTargets.Length)];

        float flightTime = GetRandomFlightTime();

        currentBall = Instantiate(
            ballPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        ReactionPassBall ball =
            currentBall.GetComponent<ReactionPassBall>();

        if (ball != null)
            ball.Setup(gameManager, this);

        Rigidbody rb =
            currentBall.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Destroy(currentBall);
            currentBall = null;
            return;
        }

        Vector3 velocity = CalculateVelocity(
            spawnPoint.position,
            target.position,
            flightTime
        );

        rb.linearVelocity = velocity;
    }

    public void BallResolved()
    {
        currentBall = null;
    }

    public void StopLauncher()
    {
        if (currentBall != null)
        {
            Destroy(currentBall);
            currentBall = null;
        }
    }

    private float GetRandomFlightTime()
    {
        int randomSpeed = Random.Range(0, 3);

        if (randomSpeed == 0)
            return slowFlightTime;

        if (randomSpeed == 1)
            return mediumFlightTime;

        return fastFlightTime;
    }

    private Vector3 CalculateVelocity(
        Vector3 start,
        Vector3 end,
        float time)
    {
        if (time <= 0f)
            time = 0.1f;

        Vector3 displacement = end - start;

        Vector3 horizontal = new Vector3(
            displacement.x,
            0f,
            displacement.z
        );

        Vector3 horizontalVelocity =
            horizontal / time;

        float verticalVelocity =
            (displacement.y -
             0.5f * Physics.gravity.y * time * time)
            / time;

        return horizontalVelocity +
               Vector3.up * verticalVelocity;
    }
}