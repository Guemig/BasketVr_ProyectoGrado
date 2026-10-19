
using System;
using System.Collections;
using UnityEngine;

public class DoubleHandPassBall : MonoBehaviour, IPoolable, ILaunchableBall
{
    public enum BallState
    {
        Inactive,
        ActiveCatch,
        ActiveLetPass,
        Correct,
        Incorrect
    }

    public enum BallAction
    {
        Catch,
        LetPass
    }

    // =====================================================
    // CONFIGURACIÓN DE TIEMPOS
    // =====================================================

    [Header("Tiempo de vida")]
    [SerializeField, Min(0f)] private float catchDuration = 0.3f;
    [SerializeField, Min(0.1f)] private float maxLifeTime = 3f;

    // =====================================================
    // ACTIVACIÓN POR DISTANCIA
    // =====================================================

    [Header("Activación por distancia")]
    [Tooltip(
        "Porcentaje de distancia que debe quedar hasta el objetivo " +
        "para activar el balón. 0.8 significa que se activa al recorrer " +
        "aproximadamente el 20 % de la distancia inicial."
    )]
    [Range(0f, 1f)]
    [SerializeField] private float activationDistancePercent = 0.8f;

    // =====================================================
    // REFERENCIAS
    // =====================================================

    private PassReactionGameManager gameManager;
    private BallLauncher launcher;
    private ObjectPool pool;

    private Rigidbody rb;
    private Collider ballCollider;

    private DoubleHandGripZone currentGripZone;

    // =====================================================
    // ESTADO INTERNO
    // =====================================================

    private bool resolved;
    private float launchTime;
    private float initialDistanceToTarget;

    private Coroutine lifeCoroutine;
    private Coroutine catchCoroutine;

    private BallAction assignedAction = BallAction.Catch;

    private Vector3 playerPositionAtLaunch;
    private Vector3 targetPositionAtLaunch;

    public BallState CurrentState { get; private set; } =
        BallState.Inactive;

    public BallAction AssignedAction => assignedAction;

    public Vector3 PlayerPositionAtLaunch => playerPositionAtLaunch;

    public Vector3 TargetPositionAtLaunch => targetPositionAtLaunch;

    // =====================================================
    // EVENTOS DE RESULTADO
    // =====================================================

    public event Action<float> OnCorrectCatch;
    public event Action OnWrongCatch;
    public event Action OnMiss;
    public event Action OnCorrectPass;

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
    }

    private void Update()
    {
        if (resolved || CurrentState != BallState.Inactive)
            return;

        CheckActivationDistance();
    }

    // =====================================================
    // POOL
    // =====================================================

    public void SetPool(ObjectPool objectPool)
    {
        pool = objectPool;
    }

    // =====================================================
    // CONFIGURACIÓN DEL LANZAMIENTO
    // =====================================================

    public void SetLaunchData(
        BallAction action,
        Vector3 playerPosition,
        Vector3 targetPosition)
    {
        assignedAction = action;
        playerPositionAtLaunch = playerPosition;
        targetPositionAtLaunch = targetPosition;

        initialDistanceToTarget = Vector3.Distance(
            transform.position,
            targetPositionAtLaunch
        );

        CurrentState = BallState.Inactive;
    }

    // =====================================================
    // PREPARAR BALÓN
    // =====================================================

    public void Setup(
        PassReactionGameManager manager,
        BallLauncher ballLauncher)
    {
        StopAllCoroutines();

        lifeCoroutine = null;
        catchCoroutine = null;
        currentGripZone = null;

        gameManager = manager;
        launcher = ballLauncher;

        resolved = false;

        // El tiempo de reacción se inicializa cuando se activa.
        launchTime = 0f;

        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;

        ResetRigidbody();

        // El balón todavía no acepta atrapadas.
        CurrentState = BallState.Inactive;
    }

    // =====================================================
    // ACTIVACIÓN POR DISTANCIA
    // =====================================================

    private void CheckActivationDistance()
    {
        float remainingDistance = Vector3.Distance(
            transform.position,
            targetPositionAtLaunch
        );

        float activationThreshold =
            initialDistanceToTarget * activationDistancePercent;

        if (remainingDistance <= activationThreshold)
            ActivateBall();
    }

    private void ActivateBall()
    {
        if (resolved || CurrentState != BallState.Inactive)
            return;

        CurrentState = assignedAction == BallAction.Catch
            ? BallState.ActiveCatch
            : BallState.ActiveLetPass;

        launchTime = Time.time;

        // El tiempo de vida empieza a contar desde la activación.
        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    // =====================================================
    // DETECTAR AGARRE A DOS MANOS
    // =====================================================

    public bool TryCatch(DoubleHandGripZone gripZone)
    {
        if (resolved || gripZone == null)
            return false;

        // No se puede atrapar antes de la activación.
        if (CurrentState != BallState.ActiveCatch &&
            CurrentState != BallState.ActiveLetPass)
        {
            return false;
        }

        currentGripZone = gripZone;

        if (CurrentState == BallState.ActiveCatch)
        {
            CatchCorrectBall(gripZone);
        }
        else
        {
            CatchIncorrectBall();
        }

        return true;
    }

    // =====================================================
    // ATRAPADA CORRECTA
    // =====================================================

    private void CatchCorrectBall(DoubleHandGripZone gripZone)
    {
        if (resolved)
            return;

        resolved = true;
        CurrentState = BallState.Correct;

        StopLifeCoroutine();

        float reactionTime = Mathf.Max(
            0f,
            Time.time - launchTime
        );

        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = false;

        transform.SetParent(null);

        transform.SetPositionAndRotation(
            gripZone.transform.position,
            gripZone.transform.rotation
        );

        if (launcher != null)
            launcher.BallResolved();

        OnCorrectCatch?.Invoke(reactionTime);

        catchCoroutine = StartCoroutine(DestroyAfterCatch());
    }

    // =====================================================
    // ATRAPADA INCORRECTA
    // =====================================================

    private void CatchIncorrectBall()
    {
        if (resolved)
            return;

        resolved = true;
        CurrentState = BallState.Incorrect;

        StopLifeCoroutine();

        // Detener el balón para impedir nuevas colisiones.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = false;

        if (launcher != null)
            launcher.BallResolved();

        // El balón debía dejarse pasar, pero fue atrapado.
        OnWrongCatch?.Invoke();

        catchCoroutine = StartCoroutine(DestroyAfterCatch());
    }

    // =====================================================
    // RESOLVER CUANDO TERMINA EL RECORRIDO
    // =====================================================

    public void Missed()
    {
        if (resolved)
            return;

        // No registrar el resultado si el balón todavía no se activó.
        if (CurrentState == BallState.Inactive)
            return;

        resolved = true;

        StopLifeCoroutine();

        if (CurrentState == BallState.ActiveCatch)
        {
            CurrentState = BallState.Incorrect;

            // Debía atraparse, pero no fue atrapado.
            OnMiss?.Invoke();
        }
        else if (CurrentState == BallState.ActiveLetPass)
        {
            CurrentState = BallState.Correct;

            // Debía dejarse pasar y nadie lo atrapó.
            OnCorrectPass?.Invoke();
        }

        if (launcher != null)
            launcher.BallResolved();

        ReturnToPool();
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(maxLifeTime);

        lifeCoroutine = null;

        if (!resolved)
            Missed();
    }

    // =====================================================
    // ESPERA DESPUÉS DE RESOLVER POR AGARRE
    // =====================================================

    private IEnumerator DestroyAfterCatch()
    {
        yield return new WaitForSeconds(catchDuration);

        catchCoroutine = null;

        ReturnToPool();
    }

    // =====================================================
    // CANCELAR SIN REGISTRAR RESULTADO
    // =====================================================

    public void CancelAndReturnToPool()
    {
        if (resolved)
            return;

        resolved = true;

        StopAllCoroutines();

        lifeCoroutine = null;
        catchCoroutine = null;

        ReturnToPool();
    }

    // =====================================================
    // DEVOLVER AL POOL
    // =====================================================

    private void ReturnToPool()
    {
        StopAllCoroutines();

        lifeCoroutine = null;
        catchCoroutine = null;

        if (currentGripZone != null)
        {
            currentGripZone.ResetGrip();
            currentGripZone = null;
        }

        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;

        ResetRigidbody();

        // Cambiar el estado antes de devolverlo al pool.
        CurrentState = BallState.Inactive;

        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Debug.LogError(
                "[DoubleHandPassBall] No tiene un pool asignado.",
                this
            );

            gameObject.SetActive(false);
        }
    }

    private void StopLifeCoroutine()
    {
        if (lifeCoroutine == null)
            return;

        StopCoroutine(lifeCoroutine);
        lifeCoroutine = null;
    }

    // =====================================================
    // REINICIAR FÍSICA
    // =====================================================

    private void ResetRigidbody()
    {
        if (rb == null)
            return;

        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;
    }
}
