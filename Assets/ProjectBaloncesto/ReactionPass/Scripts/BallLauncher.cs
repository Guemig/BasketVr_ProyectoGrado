using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    // =========================================================
    // REFERENCIAS
    // =========================================================

    [Header("Referencias")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private PassReactionGameManager gameManager;
    [SerializeField] private Transform machineModel;
    [SerializeField] private LauncherBallFeedEffect feedEffect;

    [Header("Objetivos")]
    [Tooltip("Objeto situado delante de la máquina que define su dirección.")]
    [SerializeField] private Transform shotDirectionTarget;

    [Tooltip("Centro del objetivo que sigue al jugador.")]
    [SerializeField] private Transform playerTargetCenter;

    // =========================================================
    // OBJECT POOL
    // =========================================================

    [Header("Object Pool")]
    [SerializeField] private Transform poolContainer;
    [SerializeField, Min(1)] private int initialPoolSize = 10;

    private ObjectPool ballPool;

    // =========================================================
    // OFFSETS
    // =========================================================

    [Header("Offsets del objetivo")]
    [SerializeField, Min(0f)] private float horizontalOffset = 0.75f;
    [SerializeField, Min(0f)] private float verticalOffset = 0.75f;

    // =========================================================
    // TRAYECTORIA
    // =========================================================

    [Header("Semicurva")]
    [SerializeField] private float curveHeight = 0.20f;

    [Range(0.5f, 0.9f)]
    [SerializeField] private float curvePoint = 0.75f;

    // =========================================================
    // GIRO DEL BALÓN
    // =========================================================

    [Header("Giro del balón")]
    [SerializeField] private float spinSpeed = 700f;

    // =========================================================
    // GIRO DEL GRUPO COMPLETO
    // =========================================================

    [Header("Giro de máquina")]
    [SerializeField] private float machineTurnSpeed = 8f;
    [SerializeField] private float launchDelay = 0.15f;

    // =========================================================
    // NIVEL Y CONFIGURACIÓN
    // =========================================================

    [Header("Nivel")]
    [SerializeField, Range(1, 3)] private int currentLevel = 1;

    private PassLevelConfigManager.LevelConfig currentLevelConfig;

    // =========================================================
    // ESTADO INTERNO
    // =========================================================

    private GameObject currentBall;
    private bool launchAllowed = true;

    private Coroutine movementCoroutine;
    private Coroutine prepareLaunchCoroutine;

    // Rotaciones mundiales del objeto raíz.
    private Quaternion machineCenterRotation;
    private Quaternion machineTargetRotation;

    // =========================================================
    // GIZMOS
    // =========================================================

    [Header("Gizmos")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField, Min(0.01f)] private float gizmoRadius = 0.12f;
    [SerializeField, Min(0.1f)] private float directionLineLength = 1.5f;

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    private void Awake()
    {
        if (ballPrefab == null)
        {
            Debug.LogError(
                "[BallLauncher] No se ha asignado el prefab del balón.",
                this
            );
            return;
        }

        if (poolContainer == null)
        {
            GameObject containerObject = new GameObject("BallPool");
            containerObject.transform.SetParent(transform, false);
            poolContainer = containerObject.transform;
        }

        ballPool = new ObjectPool(
            ballPrefab,
            initialPoolSize,
            poolContainer
        );
    }

    private void Start()
    {
        // Orientación inicial del grupo completo.
        machineCenterRotation = transform.rotation;
        machineTargetRotation = machineCenterRotation;
    }

    private void Update()
    {
        // Girar el objeto raíz completo para apuntar.
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            machineTargetRotation,
            machineTurnSpeed * Time.deltaTime
        );
    }

    // =========================================================
    // CONFIGURACIÓN DE ORIENTACIÓN BASE
    // =========================================================

    public void SetMachineBaseRotation(Quaternion baseRotation)
    {
        machineCenterRotation = baseRotation;
        machineTargetRotation = baseRotation;
    }
    // =========================================================
    // CONFIGURACIÓN DEL NIVEL
    // =========================================================

    public void SetLevel(
        int level,
        PassLevelConfigManager.LevelConfig config)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);
        currentLevelConfig = config;
        launchAllowed = true;

        if (currentLevelConfig == null)
        {
            Debug.LogError(
                "[BallLauncher] No se recibió la configuración del nivel.",
                this
            );
        }
    }

    // =========================================================
    // LANZAMIENTO
    // =========================================================

    public void LaunchBall()
    {
        if (!launchAllowed)
            return;

        if (ballPool == null)
        {
            Debug.LogError(
                "[BallLauncher] El pool no está inicializado.",
                this
            );
            return;
        }

        if (gameManager == null || !gameManager.GameRunning)
            return;

        if (currentBall != null || prepareLaunchCoroutine != null)
            return;

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[BallLauncher] No se ha asignado el Spawn Point.",
                this
            );
            return;
        }

        if (shotDirectionTarget == null || playerTargetCenter == null)
        {
            Debug.LogError(
                "[BallLauncher] Asigna Shot Direction Target y Player Target Center.",
                this
            );
            return;
        }

        if (currentLevelConfig == null)
        {
            Debug.LogError(
                "[BallLauncher] No hay configuración para el nivel actual.",
                this
            );
            return;
        }

        prepareLaunchCoroutine = StartCoroutine(PrepareLaunch());
    }

    private IEnumerator PrepareLaunch()
    {
        if (!TryGetRandomOffset(out float horizontal, out float vertical))
        {
            Debug.LogWarning(
                "[BallLauncher] No se pudo seleccionar un offset.",
                this
            );

            prepareLaunchCoroutine = null;
            yield break;
        }

        if (!TryCalculateTargetPosition(
                horizontal,
                vertical,
                out Vector3 targetPosition))
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        RotateMachineToTarget(targetPosition);

        yield return new WaitForSeconds(launchDelay);

        if (!launchAllowed ||
            gameManager == null ||
            !gameManager.GameRunning ||
            currentBall != null)
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        // Actualizar el destino por si el jugador se ha movido.
        if (!TryCalculateTargetPosition(
                horizontal,
                vertical,
                out targetPosition))
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        RotateMachineToTarget(targetPosition);

        float speed = GetRandomSpeed();

        if (speed <= 0f)
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        currentBall = ballPool.Get();

        if (currentBall == null)
        {
            Debug.LogError(
                "[BallLauncher] No se pudo obtener un balón del pool.",
                this
            );

            prepareLaunchCoroutine = null;
            yield break;
        }

        currentBall.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        ILaunchableBall reactionBall =
            currentBall.GetComponent<ILaunchableBall>();

        if (reactionBall == null)
        {
            Debug.LogError(
                "[BallLauncher] El prefab necesita ILaunchableBall.",
                currentBall
            );

            ballPool.Return(currentBall);
            currentBall = null;
            prepareLaunchCoroutine = null;
            yield break;
        }

        reactionBall.Setup(gameManager, this);

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (rb.isKinematic)
                rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        movementCoroutine = StartCoroutine(
            MoveBall(currentBall, targetPosition, speed)
        );

        if (feedEffect != null)
            feedEffect.OnBallLaunched();

        prepareLaunchCoroutine = null;
    }

    // =========================================================
    // CÁLCULO DE ORIENTACIÓN Y OBJETIVO
    // =========================================================

    private bool TryGetShotAxes(
        out Vector3 forward,
        out Vector3 right)
    {
        forward = Vector3.zero;
        right = Vector3.zero;

        if (shotDirectionTarget == null)
            return false;

        Vector3 direction =
            shotDirectionTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
        {
            Debug.LogWarning(
                "[BallLauncher] Shot Direction Target debe estar separado " +
                "horizontalmente de la máquina.",
                this
            );

            return false;
        }

        forward = direction.normalized;
        right = Vector3.Cross(Vector3.up, forward).normalized;

        return true;
    }

    private bool TryCalculateTargetPosition(
        float horizontal,
        float vertical,
        out Vector3 targetPosition)
    {
        targetPosition = Vector3.zero;

        if (playerTargetCenter == null)
            return false;

        if (!TryGetShotAxes(out _, out Vector3 right))
            return false;

        targetPosition =
            playerTargetCenter.position +
            right * horizontal +
            Vector3.up * vertical;

        return true;
    }

    /// <summary>
    /// Gira el objeto raíz que contiene este script.
    /// No modifica machineModel directamente.
    /// </summary>
    private void RotateMachineToTarget(Vector3 targetPosition)
    {
        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        machineTargetRotation = Quaternion.LookRotation(
            direction.normalized,
            Vector3.up
        );
    }

    // =========================================================
    // SELECCIÓN ALEATORIA DE OFFSETS POR NIVEL
    // =========================================================

    private bool TryGetRandomOffset(
        out float horizontal,
        out float vertical)
    {
        horizontal = 0f;
        vertical = 0f;

        int heightCount = currentLevel == 1 ? 1 :
                          currentLevel == 2 ? 2 : 3;

        int heightIndex = Random.Range(0, heightCount);

        if (currentLevel == 1)
        {
            vertical = 0f;
        }
        else if (currentLevel == 2)
        {
            vertical = heightIndex == 0 ? 0f : verticalOffset;
        }
        else
        {
            vertical = heightIndex switch
            {
                0 => -verticalOffset,
                1 => 0f,
                _ => verticalOffset
            };
        }

        int horizontalIndex = Random.Range(0, 3);

        horizontal = horizontalIndex switch
        {
            0 => -horizontalOffset,
            1 => 0f,
            _ => horizontalOffset
        };

        return true;
    }

    // =========================================================
    // MOVIMIENTO DEL BALÓN
    // =========================================================

    private IEnumerator MoveBall(
        GameObject ball,
        Vector3 targetPosition,
        float speed)
    {
        Vector3 startPosition = ball.transform.position;

        float distance = Vector3.Distance(
            startPosition,
            targetPosition
        );

        float duration = Mathf.Max(distance / speed, 0.05f);

        Vector3 controlPoint = Vector3.Lerp(
            startPosition,
            targetPosition,
            curvePoint
        );

        controlPoint.y += curveHeight;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (ball == null || !ball.activeInHierarchy)
                yield break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
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

        if (ball != null && ball.activeInHierarchy)
        {
            ball.transform.position = targetPosition;

            Rigidbody rb = ball.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;

                Vector3 finalDirection = GetBezierDirection(
                    startPosition,
                    controlPoint,
                    targetPosition
                );

                rb.linearVelocity = finalDirection * speed;

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
        return (end - control).normalized;
    }

    // =========================================================
    // VELOCIDAD ALEATORIA SEGÚN CONFIGURACIÓN DEL NIVEL
    // =========================================================

    private float GetRandomSpeed()
    {
        if (currentLevelConfig == null)
        {
            Debug.LogError(
                "[BallLauncher] No hay configuración de velocidad.",
                this
            );

            return 0f;
        }

        float slowWeight = Mathf.Max(
            0f, currentLevelConfig.slowWeight
        );

        float mediumWeight = Mathf.Max(
            0f, currentLevelConfig.mediumWeight
        );

        float fastWeight = Mathf.Max(
            0f, currentLevelConfig.fastWeight
        );

        float totalWeight =
            slowWeight + mediumWeight + fastWeight;

        if (totalWeight <= 0f)
        {
            Debug.LogError(
                "[BallLauncher] Los pesos de velocidad no pueden ser todos cero.",
                this
            );

            return 0f;
        }

        float random = Random.value * totalWeight;

        if (random < slowWeight)
            return currentLevelConfig.slowSpeed;

        random -= slowWeight;

        if (random < mediumWeight)
            return currentLevelConfig.mediumSpeed;

        return currentLevelConfig.fastSpeed;
    }

    // =========================================================
    // CONTROL DEL LANZADOR
    // =========================================================

    public bool HasActiveBall()
    {
        return currentBall != null;
    }

    public void StopLaunching()
    {
        launchAllowed = false;

        if (prepareLaunchCoroutine != null)
        {
            StopCoroutine(prepareLaunchCoroutine);
            prepareLaunchCoroutine = null;
        }
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

    public void InitializeVisualFeed(int totalShots)
    {
        if (feedEffect != null)
            feedEffect.InitializeForLevel(totalShots);
    }

    public void ResetVisualFeed()
    {
        if (feedEffect != null)
            feedEffect.ResetFeed();
    }

    public void StopLauncher()
    {
        StopLaunching();

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        if (currentBall != null)
        {
            ILaunchableBall reactionBall =
                currentBall.GetComponent<ILaunchableBall>();

            if (reactionBall != null)
            {
                reactionBall.CancelAndReturnToPool();
            }
            else if (ballPool != null)
            {
                ballPool.Return(currentBall);
            }

            currentBall = null;
        }

    }

    // =========================================================
    // GIZMOS DE VISUALIZACIÓN
    // =========================================================

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

        // 1. Spawn Point.
        if (spawnPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(spawnPoint.position, gizmoRadius);

            Gizmos.DrawLine(
                spawnPoint.position,
                spawnPoint.position +
                spawnPoint.forward * directionLineLength
            );
        }

        // 2. Dirección frontal de la máquina.
        if (shotDirectionTarget != null)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawWireSphere(
                shotDirectionTarget.position,
                gizmoRadius
            );

            Gizmos.DrawLine(
                transform.position,
                shotDirectionTarget.position
            );

            Vector3 direction =
                shotDirectionTarget.position - transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                direction.Normalize();

                Vector3 arrowRight = Vector3.Cross(
                    Vector3.up,
                    direction
                ).normalized;

                Vector3 tip =
                    transform.position +
                    direction * directionLineLength;

                Gizmos.DrawLine(transform.position, tip);

                Gizmos.DrawLine(
                    tip,
                    tip - direction * 0.25f +
                    arrowRight * 0.12f
                );

                Gizmos.DrawLine(
                    tip,
                    tip - direction * 0.25f -
                    arrowRight * 0.12f
                );
            }
        }

        // 3. Centro del jugador y destinos posibles.
        if (playerTargetCenter == null)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            playerTargetCenter.position,
            gizmoRadius * 1.5f
        );

        if (!TryGetShotAxes(out _, out Vector3 right))
            return;

        Vector3 center = playerTargetCenter.position;

        Vector3 left = center - right * horizontalOffset;
        Vector3 middle = center;
        Vector3 rightTarget = center + right * horizontalOffset;

        DrawTargetGizmo(left, Color.yellow);
        DrawTargetGizmo(middle, Color.yellow);
        DrawTargetGizmo(rightTarget, Color.yellow);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(left, middle);
        Gizmos.DrawLine(middle, rightTarget);

        // 4. Destinos altos y bajos.
        Vector3 leftHigh = left + Vector3.up * verticalOffset;
        Vector3 middleHigh = middle + Vector3.up * verticalOffset;
        Vector3 rightHigh = rightTarget + Vector3.up * verticalOffset;

        Vector3 leftLow = left - Vector3.up * verticalOffset;
        Vector3 middleLow = middle - Vector3.up * verticalOffset;
        Vector3 rightLow = rightTarget - Vector3.up * verticalOffset;

        DrawTargetGizmo(leftHigh, Color.red);
        DrawTargetGizmo(middleHigh, Color.red);
        DrawTargetGizmo(rightHigh, Color.red);

        DrawTargetGizmo(leftLow, Color.magenta);
        DrawTargetGizmo(middleLow, Color.magenta);
        DrawTargetGizmo(rightLow, Color.magenta);

        // 5. Líneas de referencia desde el Spawn Point.
        if (spawnPoint != null)
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
            Gizmos.DrawLine(spawnPoint.position, middle);

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawLine(spawnPoint.position, middleHigh);

            Gizmos.color = new Color(1f, 0f, 1f, 0.35f);
            Gizmos.DrawLine(spawnPoint.position, middleLow);
        }
    }

    private void DrawTargetGizmo(Vector3 position, Color color)
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(position, gizmoRadius);
    }
}
