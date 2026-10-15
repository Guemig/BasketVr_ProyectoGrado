
using System.Collections;
using UnityEngine;

public class DoubleHandPassBall : MonoBehaviour, IPoolable
{
    [Header("Tiempo de vida")]
    [SerializeField] private float catchDuration = 0.7f;
    [SerializeField] private float maxLifeTime = 3f;

    private PassReactionGameManager gameManager;
    private BallLauncher launcher;
    private ObjectPool pool;

    private Rigidbody rb;
    private Collider ballCollider;

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

        gameManager = manager;
        launcher = ballLauncher;

        resolved = false;
        launchTime = Time.time;

        transform.SetParent(null);

        if (ballCollider != null)
            ballCollider.enabled = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    // =====================================================
    // DETECTAR AGARRE A DOS MANOS
    // =====================================================

    public bool TryCatch(DoubleHandGripZone gripZone)
    {
        if (resolved || gripZone == null)
            return false;

        CatchBall(gripZone.transform);

        return true;
    }

    private void CatchBall(Transform gripAnchor)
    {
        if (resolved || gripAnchor == null)
            return;

        resolved = true;

        StopLifeCoroutine();

        float reactionTime = Time.time - launchTime;

        // Liberar el lanzador antes de registrar el resultado.
        if (launcher != null)
            launcher.BallResolved();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = false;

        // El balón sigue el punto medio de las dos manos.
        transform.SetParent(gripAnchor, true);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (gameManager != null)
            gameManager.RegisterCatch(reactionTime);

        catchCoroutine = StartCoroutine(DestroyAfterCatch());
    }

    // =====================================================
    // BALÓN FALLADO
    // =====================================================

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

    // =====================================================
    // TEMPORIZADOR
    // =====================================================

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

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = true;

        transform.SetParent(null);

        if (pool != null)
        {
            pool.Return(gameObject);
        }
        else
        {
            Debug.LogError(
                "[ReactionPassBall] No tiene un pool asignado.",
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
}
