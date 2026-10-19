
using System;
using System.Collections;
using UnityEngine;

public class BallLauncher : MonoBehaviour
{
    public enum LaunchState
    {
        Idle,
        Warning,
        Firing
    }

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

    [Tooltip("Centro de referencia del jugador.")]
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
    // GIRO DE MÁQUINA
    // =========================================================

    [Header("Giro de máquina")]
    [SerializeField] private float machineTurnSpeed = 8f;

    [SerializeField, Min(0f)]
    private float warningDuration = 0.6f;

    // =========================================================
    // TIPO DE ESTÍMULO
    // =========================================================

    [Header("Tipo de estímulo")]
    [Tooltip("Probabilidad de que el balón deba atraparse.")]
    [Range(0f, 1f)]
    [SerializeField] private float catchBallProbability = 0.7f;

    private DoubleHandPassBall.BallAction nextBallAction;

    // =========================================================
    // NIVEL Y CONFIGURACIÓN
    // =========================================================

    [Header("Nivel")]
    [SerializeField, Range(1, 3)] private int currentLevel = 1;

    private PassLevelConfigManager.LevelConfig currentLevelConfig;

    public event Action<BallLauncher, LaunchState> OnLaunchStateChanged;

    public event Action<
        BallLauncher,
        DoubleHandPassBall.BallState
    > OnNextBallStateChanged;

    public LaunchState CurrentLaunchState { get; private set; }
        = LaunchState.Idle;

    // =========================================================
    // ESTADO INTERNO
    // =========================================================

    private GameObject currentBall;
    private bool launchAllowed = true;

    private Coroutine movementCoroutine;
    private Coroutine prepareLaunchCoroutine;

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
        machineCenterRotation = transform.rotation;
        machineTargetRotation = machineCenterRotation;
    }

    private void Update()
    {
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


    private void HandleCorrectCatch(float reactionTime)
    {
        if (gameManager != null)
            gameManager.RegisterCatch(reactionTime);
    }

    private void HandleWrongCatch()
    {
        if (gameManager != null)
            gameManager.RegisterWrongCatch();
    }

    private void HandleMiss()
    {
        if (gameManager != null)
            gameManager.RegisterMiss();
    }

    private void HandleCorrectPass()
    {
        if (gameManager != null)
            gameManager.RegisterCorrectPass();
    }

    private void SubscribeBallEvents(DoubleHandPassBall ball)
    {
        if (ball == null)
            return;

        UnsubscribeBallEvents(ball);

        ball.OnCorrectCatch += HandleCorrectCatch;
        ball.OnWrongCatch += HandleWrongCatch;
        ball.OnMiss += HandleMiss;
        ball.OnCorrectPass += HandleCorrectPass;
    }

    private void UnsubscribeBallEvents(DoubleHandPassBall ball)
    {
        if (ball == null)
            return;

        ball.OnCorrectCatch -= HandleCorrectCatch;
        ball.OnWrongCatch -= HandleWrongCatch;
        ball.OnMiss -= HandleMiss;
        ball.OnCorrectPass -= HandleCorrectPass;
    }

    private IEnumerator PrepareLaunch()
    {
        if (!TryGetRandomOffset(
                out float horizontal,
                out float vertical))
        {
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

        // 1. La máquina determina la acción del próximo balón.
        nextBallAction = UnityEngine.Random.value < catchBallProbability
            ? DoubleHandPassBall.BallAction.Catch
            : DoubleHandPassBall.BallAction.LetPass;

        // 2. Notificar el estado antes del aviso visual.
        DoubleHandPassBall.BallState nextState =
            nextBallAction == DoubleHandPassBall.BallAction.Catch
                ? DoubleHandPassBall.BallState.ActiveCatch
                : DoubleHandPassBall.BallState.ActiveLetPass;

        OnNextBallStateChanged?.Invoke(this, nextState);

        // 3. Avisar que esta máquina va a disparar.
        SetLaunchState(LaunchState.Warning);

        // 4. Orientar la máquina hacia el destino.
        RotateMachineToTarget(targetPosition);

        // 5. Esperar el tiempo de aviso.
        yield return new WaitForSeconds(warningDuration);

        if (!launchAllowed ||
            gameManager == null ||
            !gameManager.GameRunning ||
            currentBall != null)
        {
            SetLaunchState(LaunchState.Idle);
            prepareLaunchCoroutine = null;
            yield break;
        }

        // 6. Actualizar el destino por si el jugador se movió.
        if (!TryCalculateTargetPosition(
                horizontal,
                vertical,
                out targetPosition))
        {
            SetLaunchState(LaunchState.Idle);
            prepareLaunchCoroutine = null;
            yield break;
        }

        RotateMachineToTarget(targetPosition);

        float speed = GetRandomSpeed();

        if (speed <= 0f)
        {
            SetLaunchState(LaunchState.Idle);
            prepareLaunchCoroutine = null;
            yield break;
        }

        // 7. Obtener un balón del pool.
        currentBall = ballPool.Get();

        if (currentBall == null)
        {
            Debug.LogError(
                "[BallLauncher] No se pudo obtener un balón del pool.",
                this
            );

            SetLaunchState(LaunchState.Idle);
            prepareLaunchCoroutine = null;
            yield break;
        }

        currentBall.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        ILaunchableBall reactionBall =
            currentBall.GetComponent<ILaunchableBall>();

        DoubleHandPassBall passBall =
            currentBall.GetComponent<DoubleHandPassBall>();

        if (reactionBall == null || passBall == null)
        {
            Debug.LogError(
                "[BallLauncher] El prefab necesita ILaunchableBall " +
                "y DoubleHandPassBall.",
                currentBall
            );

            ballPool.Return(currentBall);
            currentBall = null;

            SetLaunchState(LaunchState.Idle);
            prepareLaunchCoroutine = null;
            yield break;
        }

        // 8. Transmitir acción y referencias espaciales.
        // Se guarda una posición estable del jugador para este lanzamiento.
        passBall.SetLaunchData(
            nextBallAction,
            playerTargetCenter.position,
            targetPosition
        );


        // 9. Preparar el balón sin activar todavía su estado de reacción.
        reactionBall.Setup(gameManager, this);
        SubscribeBallEvents(passBall);

        Rigidbody rb = currentBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // 10. Finalizar el aviso e iniciar el movimiento.
        SetLaunchState(LaunchState.Firing);

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
    // SELECCIÓN ALEATORIA DE OFFSETS
    // =========================================================

    private bool TryGetRandomOffset(
        out float horizontal,
        out float vertical)
    {
        horizontal = 0f;
        vertical = 0f;

        int heightIndex = UnityEngine.Random.Range(0, 3);

        switch (heightIndex)
        {
            case 0:
                vertical = -verticalOffset;
                break;

            case 1:
                vertical = 0f;
                break;

            case 2:
                vertical = verticalOffset;
                break;
        }

        int horizontalIndex = UnityEngine.Random.Range(0, 3);

        switch (horizontalIndex)
        {
            case 0:
                horizontal = -horizontalOffset;
                break;

            case 1:
                horizontal = 0f;
                break;

            case 2:
                horizontal = horizontalOffset;
                break;
        }

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
    // VELOCIDAD ALEATORIA SEGÚN EL NIVEL
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

        float random = UnityEngine.Random.value * totalWeight;

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

        SetLaunchState(LaunchState.Idle);
    }

    private void SetLaunchState(LaunchState newState)
    {
        if (CurrentLaunchState == newState)
            return;

        CurrentLaunchState = newState;
        OnLaunchStateChanged?.Invoke(this, newState);
    }

    public void BallResolved()
    {
        SetLaunchState(LaunchState.Idle);

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
        SetLaunchState(LaunchState.Idle);

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
    // GIZMOS
    // =========================================================

    private void OnDrawGizmos()
    {
        if (!showGizmos)
            return;

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
