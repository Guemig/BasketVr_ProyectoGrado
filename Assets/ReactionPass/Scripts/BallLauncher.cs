using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Targets Altos")]
    [SerializeField] private Transform highLeft;
    [SerializeField] private Transform highCenter;
    [SerializeField] private Transform highRight;

    [Header("Targets Medios")]
    [SerializeField] private Transform middleLeft;
    [SerializeField] private Transform middleCenter;
    [SerializeField] private Transform middleRight;

    [Header("Targets Bajos")]
    [SerializeField] private Transform lowLeft;
    [SerializeField] private Transform lowCenter;
    [SerializeField] private Transform lowRight;

    [Header("Velocidades")]
    [SerializeField] private float slowSpeed = 8f;
    [SerializeField] private float mediumSpeed = 11f;
    [SerializeField] private float fastSpeed = 14f;

    [Header("Semicurva")]
    [SerializeField] private float curveHeight = 0.20f;
    [Range(0.5f, 0.9f)]
    [SerializeField] private float curvePoint = 0.75f;

    [Header("Giro")]
    [SerializeField] private float spinSpeed = 700f;

    [Header("Nivel")]
    [SerializeField] private int currentLevel = 1;

    private GameObject currentBall;
    private Coroutine movementCoroutine;

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);
    }

    public void LaunchBall()
    {
        if (gameManager == null || !gameManager.GameRunning)
            return;

        if (currentBall != null)
            return;

        if (ballPrefab == null || spawnPoint == null)
            return;

        Transform target = GetRandomTarget();

        if (target == null)
            return;

        float speed = GetRandomSpeed();

        currentBall = Instantiate(
            ballPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        ReactionPassBall reactionBall =
            currentBall.GetComponent<ReactionPassBall>();

        if (reactionBall != null)
            reactionBall.Setup(gameManager, this);

        Rigidbody rb =
            currentBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        movementCoroutine = StartCoroutine(
            MoveBall(currentBall, target.position, speed)
        );
    }

    private IEnumerator MoveBall(
        GameObject ball,
        Vector3 targetPosition,
        float speed)
    {
        Vector3 startPosition = ball.transform.position;

        float distance =
            Vector3.Distance(startPosition, targetPosition);

        float duration = distance / speed;

        if (duration < 0.05f)
            duration = 0.05f;

        // Punto elevado cerca del 75% del recorrido
        Vector3 controlPoint =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                curvePoint
            );

        controlPoint.y += curveHeight;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (ball == null)
                yield break;

            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / duration);

            float oneMinusT = 1f - t;

            Vector3 position =
                oneMinusT * oneMinusT * startPosition +
                2f * oneMinusT * t * controlPoint +
                t * t * targetPosition;

            ball.transform.position = position;

            ball.transform.Rotate(
                Vector3.right,
                spinSpeed * Time.deltaTime,
                Space.Self
            );

            yield return null;
        }

        if (ball != null)
        {
            ball.transform.position = targetPosition;

            Rigidbody rb =
                ball.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;

                Vector3 finalDirection =
                    GetBezierDirection(
                        startPosition,
                        controlPoint,
                        targetPosition
                    );

                rb.linearVelocity =
                    finalDirection * speed;

                rb.angularVelocity =
                    ball.transform.right *
                    (spinSpeed * Mathf.Deg2Rad);
            }
        }

        movementCoroutine = null;
    }

    private Vector3 GetBezierDirection(
        Vector3 start,
        Vector3 control,
        Vector3 end)
    {
        Vector3 direction =
            2f * (end - control);

        return direction.normalized;
    }

    private Transform GetRandomTarget()
    {
        if (currentLevel == 1)
        {
            Transform[] level1Targets =
            {
                middleLeft,
                middleCenter,
                middleRight
            };

            return level1Targets[
                Random.Range(0, level1Targets.Length)
            ];
        }

        if (currentLevel == 2)
        {
            Transform[] level2Targets =
            {
                highLeft,
                highCenter,
                highRight,
                middleLeft,
                middleCenter,
                middleRight
            };

            return level2Targets[
                Random.Range(0, level2Targets.Length)
            ];
        }

        Transform[] level3Targets =
        {
            highLeft,
            highCenter,
            highRight,

            middleLeft,
            middleCenter,
            middleRight,

            lowLeft,
            lowCenter,
            lowRight
        };

        return level3Targets[
            Random.Range(0, level3Targets.Length)
        ];
    }

    private float GetRandomSpeed()
    {
        if (currentLevel == 1)
            return slowSpeed;

        if (currentLevel == 2)
        {
            return Random.Range(0, 2) == 0
                ? slowSpeed
                : mediumSpeed;
        }

        int randomSpeed = Random.Range(0, 3);

        if (randomSpeed == 0)
            return slowSpeed;

        if (randomSpeed == 1)
            return mediumSpeed;

        return fastSpeed;
    }

    public void BallResolved()
    {
        currentBall = null;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    public void StopLauncher()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (currentBall != null)
        {
            Destroy(currentBall);
            currentBall = null;
        }
    }
}