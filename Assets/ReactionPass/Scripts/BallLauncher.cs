using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Targets")]
    [SerializeField] private Transform[] passTargets;

    [Header("Velocidades")]
    [SerializeField] private float slowFlightTime = 0.9f;
    [SerializeField] private float mediumFlightTime = 0.7f;
    [SerializeField] private float fastFlightTime = 0.5f;

    [Header("Intervalo")]
    [SerializeField] private float timeBetweenBalls = 2f;

    private Coroutine launchCoroutine;

    private void Start()
    {
        launchCoroutine = StartCoroutine(LaunchLoop());
    }

    private IEnumerator LaunchLoop()
    {
        yield return new WaitForSeconds(2f);

        while (true)
        {
            LaunchBall();

            yield return new WaitForSeconds(timeBetweenBalls);
        }
    }

    private void LaunchBall()
    {
        if (ballPrefab == null || spawnPoint == null)
            return;

        if (passTargets == null || passTargets.Length == 0)
            return;

        Transform target = GetRandomTarget();

        if (target == null)
            return;

        float flightTime = GetRandomFlightTime();

        GameObject ball = Instantiate(
            ballPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Rigidbody rb = ball.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Destroy(ball);
            return;
        }

        Vector3 velocity = CalculateVelocity(
            spawnPoint.position,
            target.position,
            flightTime
        );

        rb.linearVelocity = velocity;

        Destroy(ball, 5f);
    }

    private Transform GetRandomTarget()
    {
        int randomIndex = Random.Range(0, passTargets.Length);

        return passTargets[randomIndex];
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