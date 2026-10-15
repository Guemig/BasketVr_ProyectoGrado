using System.Collections;
using UnityEngine;

public class DoubleHandPassBall : MonoBehaviour, IPoolable, ILaunchableBall
{
    [Header("Tiempo de vida")]
    [SerializeField] private float catchDuration = 0.3f;
    [SerializeField] private float maxLifeTime = 3f;

    private PassReactionGameManager gameManager;
    private BallLauncher launcher;
    private ObjectPool pool;

    private Rigidbody rb;
    private Collider ballCollider;

    // Zona responsable del agarre.
    private DoubleHandGripZone currentGripZone;

    private bool resolved;
    private float launchTime;

    private Coroutine lifeCoroutine;
    private Coroutine catchCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
    }

    // =====================================================
    // POOL
    // =====================================================

    public void SetPool(ObjectPool objectPool)
    {
        pool = objectPool;
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
        launchTime = Time.time;

        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;

        ResetRigidbody();

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    // =====================================================
    // DETECTAR AGARRE A DOS MANOS
    // =====================================================

    public bool TryCatch(DoubleHandGripZone gripZone)
    {
        if (resolved || gripZone == null)
            return false;

        CatchBall(gripZone);

        return true;
    }

private void CatchBall(DoubleHandGripZone gripZone)
    {
        if (resolved || gripZone == null)
            return;

        resolved = true;
        currentGripZone = gripZone;

        StopLifeCoroutine();

        float reactionTime = Time.time - launchTime;

        Vector3 caughtPosition = gripZone.transform.position;
        Quaternion caughtRotation = gripZone.transform.rotation;

        if (launcher != null)
            launcher.BallResolved();

        if (rb != null)
        {
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = false;

        transform.SetParent(null);
        transform.SetPositionAndRotation(
            caughtPosition,
            caughtRotation
        );

        if (gameManager != null)
            gameManager.RegisterCatch(reactionTime);

        catchCoroutine = StartCoroutine(DestroyAfterCatch());
    }


    public void Missed()
    {
        if (resolved)
            return;

        resolved = true;

        StopLifeCoroutine();

        if (launcher != null)
            launcher.BallResolved();

        if (gameManager != null)
            gameManager.RegisterMiss();

        ReturnToPool();
    }


    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(maxLifeTime);

        lifeCoroutine = null;

        if (resolved)
            yield break;

        Missed();
    }

    // =====================================================
    // ESPERA DESPUÉS DE ATRAPAR
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

        // Restablecer el estado visual de la zona.
        if (currentGripZone != null)
        {
            currentGripZone.ResetGrip();
            currentGripZone = null;
        }

        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;

        ResetRigidbody();

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

        // Primero pasar a dinámico para poder limpiar
        // las velocidades de forma segura.
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;
    }
}