
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

    // =========================================================
    // OBJECT POOL
    // =========================================================

    [Header("Object Pool")]
    [SerializeField] private Transform poolContainer;
    [SerializeField, Min(1)] private int initialPoolSize = 10;

    private ObjectPool ballPool;

    // =========================================================
    // TARGETS ALTOS
    // =========================================================

    [Header("Targets Altos")]
    [SerializeField] private Transform highLeft;
    [SerializeField] private Transform highCenter;
    [SerializeField] private Transform highRight;

    // =========================================================
    // TARGETS MEDIOS
    // =========================================================

    [Header("Targets Medios")]
    [SerializeField] private Transform middleLeft;
    [SerializeField] private Transform middleCenter;
    [SerializeField] private Transform middleRight;

    // =========================================================
    // TARGETS BAJOS
    // =========================================================

    [Header("Targets Bajos")]
    [SerializeField] private Transform lowLeft;
    [SerializeField] private Transform lowCenter;
    [SerializeField] private Transform lowRight;

    // =========================================================
    // VELOCIDADES
    // =========================================================

    [Header("Velocidades")]
    [SerializeField] private float slowSpeed = 8f;
    [SerializeField] private float mediumSpeed = 11f;
    [SerializeField] private float fastSpeed = 14f;

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

    [Header("Giro del Balón")]
    [SerializeField] private float spinSpeed = 700f;

    // =========================================================
    // GIRO DE MÁQUINA
    // =========================================================

    [Header("Giro de Máquina")]
    [SerializeField] private float machineTurnAngle = 15f;
    [SerializeField] private float machineTurnSpeed = 8f;
    [SerializeField] private float launchDelay = 0.15f;

    // =========================================================
    // NIVEL
    // =========================================================

    [Header("Nivel")]
    [SerializeField] private int currentLevel = 1;

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

        // Si no se asignó un contenedor desde el Inspector,
        // se crea uno como hijo de este lanzador.
        if (poolContainer == null)
        {
            GameObject containerObject = new GameObject("BallPool");
            containerObject.transform.SetParent(transform, false);

            poolContainer = containerObject.transform;
        }

        // Crear el pool y precargar los balones.
        ballPool = new ObjectPool(
            ballPrefab,
            initialPoolSize,
            poolContainer
        );
    }

    private void Start()
    {
        if (machineModel != null)
        {
            machineCenterRotation = machineModel.localRotation;
            machineTargetRotation = machineCenterRotation;
        }
    }

    private void Update()
    {
        if (machineModel == null)
            return;

        machineModel.localRotation = Quaternion.Slerp(
            machineModel.localRotation,
            machineTargetRotation,
            machineTurnSpeed * Time.deltaTime
        );
    }

    // =========================================================
    // CONFIGURACIÓN DEL NIVEL
    // =========================================================

    public void SetLevel(int level)
    {
        currentLevel = Mathf.Clamp(level, 1, 3);

        // Permitir lanzamientos en el nuevo nivel.
        launchAllowed = true;
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

        if (currentBall != null)
            return;

        if (prepareLaunchCoroutine != null)
            return;

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[BallLauncher] No se ha asignado el Spawn Point.",
                this
            );

            return;
        }

        prepareLaunchCoroutine = StartCoroutine(PrepareLaunch());
    }


    private IEnumerator PrepareLaunch()
    {
        Transform target = GetRandomTarget();

        if (target == null)
        {
            Debug.LogWarning(
                "[BallLauncher] No hay un target válido para este nivel.",
                this
            );

            prepareLaunchCoroutine = null;
            yield break;
        }

        // Orientar la máquina hacia el objetivo.
        RotateMachineToTarget(target);

        yield return new WaitForSeconds(launchDelay);

        // Verificar que el ejercicio siga activo.
        if (!launchAllowed ||
            gameManager == null ||
            !gameManager.GameRunning)
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        if (currentBall != null)
        {
            prepareLaunchCoroutine = null;
            yield break;
        }

        float speed = GetRandomSpeed();

        // Solicitar un balón al pool.
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

        // Colocar el balón en el punto de lanzamiento.
        currentBall.transform.SetPositionAndRotation(
            spawnPoint.position,
            spawnPoint.rotation
        );

        // Obtener el componente que gestiona las capturas.
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

        // Preparar el balón. Setup reinicia su física.
        reactionBall.Setup(gameManager, this);

        // Asegurar el estado cinemático antes de la trayectoria.
        Rigidbody rb = currentBall.GetComponent<Rigidbody>();

        if (rb != null)
        {
            // No asignar velocidades mientras sea cinemático.
            if (rb.isKinematic)
                rb.isKinematic = false;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        // Ejecutar la trayectoria original.
        movementCoroutine = StartCoroutine(
            MoveBall(
                currentBall,
                target.position,
                speed
            )
        );

        if (feedEffect != null)
        {
            feedEffect.OnBallLaunched();
        }

        prepareLaunchCoroutine = null;
    }

    // =========================================================
    // ORIENTACIÓN DE LA MÁQUINA
    // =========================================================

    private void RotateMachineToTarget(Transform target)
    {
        if (machineModel == null)
            return;

        float angle = 0f;

        if (target == highLeft ||
            target == middleLeft ||
            target == lowLeft)
        {
            angle = machineTurnAngle;
        }
        else if (
            target == highRight ||
            target == middleRight ||
            target == lowRight)
        {
            angle = -machineTurnAngle;
        }

        machineTargetRotation =
            machineCenterRotation *
            Quaternion.Euler(0f, angle, 0f);
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

        float duration = distance / speed;

        if (duration < 0.05f)
            duration = 0.05f;

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

            // Curva de Bézier cuadrática.
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
        Vector3 direction = 2f * (end - control);
        return direction.normalized;
    }

    // =========================================================
    // SELECCIÓN DE TARGETS
    // =========================================================

    private Transform GetRandomTarget()
    {
        Transform[] targets;

        if (currentLevel == 1)
        {
            targets = new Transform[]
            {
                middleLeft,
                middleCenter,
                middleRight
            };
        }
        else if (currentLevel == 2)
        {
            targets = new Transform[]
            {
                highLeft,
                highCenter,
                highRight,
                middleLeft,
                middleCenter,
                middleRight
            };
        }
        else
        {
            targets = new Transform[]
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
        }

        // Evitar seleccionar una referencia vacía.
        int validCount = 0;

        foreach (Transform candidate in targets)
        {
            if (candidate != null)
                validCount++;
        }

        if (validCount == 0)
            return null;

        int randomIndex = Random.Range(0, validCount);

        foreach (Transform candidate in targets)
        {
            if (candidate == null)
                continue;

            if (randomIndex == 0)
                return candidate;

            randomIndex--;
        }

        return null;
    }

    // =========================================================
    // VELOCIDAD ALEATORIA POR NIVEL
    // =========================================================

    private float GetRandomSpeed()
    {
        float random = Random.value;

        // NIVEL 1: 100 % lentas.
        if (currentLevel == 1)
            return slowSpeed;

        // NIVEL 2: 30 % lentas, 70 % medias.
        if (currentLevel == 2)
        {
            if (random < 0.30f)
                return slowSpeed;

            return mediumSpeed;
        }

        // NIVEL 3: 10 % lentas, 35 % medias, 55 % rápidas.
        if (random < 0.10f)
            return slowSpeed;

        if (random < 0.45f)
            return mediumSpeed;

        return fastSpeed;
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

    public void StopLauncher()
    {
        // Impedir nuevos lanzamientos y cancelar la preparación.
        StopLaunching();

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }

        // Devolver el balón al pool en lugar de destruirlo.
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

        // Devolver la máquina a su orientación inicial.
        if (machineModel != null)
        {
            machineTargetRotation = machineCenterRotation;
        }
    }
}
