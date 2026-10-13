
using System.Collections;
using UnityEngine;

public class ReactionPassBall : MonoBehaviour, IPoolable
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
    // PREPARAR BALÓN PARA UN NUEVO LANZAMIENTO
    // =====================================================

    public void Setup(
        PassReactionGameManager manager,
        BallLauncher ballLauncher)
    {
        StopLifeCoroutine();

        gameManager = manager;
        launcher = ballLauncher;

        resolved = false;
        launchTime = Time.time;

        if (ballCollider != null)
            ballCollider.enabled = true;

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    // =====================================================
    // DETECTAR ATRAPADA
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (resolved)
            return;

        HandCatchDetector hand =
            other.GetComponent<HandCatchDetector>();

        if (hand == null)
            return;

        CatchBall(hand.transform);
    }

    private void CatchBall(Transform hand)
    {
        if (resolved)
            return;

        resolved = true;
        StopLifeCoroutine();

        float reactionTime = Time.time - launchTime;

        // Liberar el lanzador antes de registrar el resultado.
        if (launcher != null)
            launcher.BallResolved();

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (ballCollider != null)
            ballCollider.enabled = false;

        transform.SetParent(hand);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (gameManager != null)
            gameManager.RegisterCatch(reactionTime);

        StartCoroutine(DestroyAfterCatch());
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

        // Liberar primero la referencia del balón activo.
        if (launcher != null)
            launcher.BallResolved();

        if (gameManager != null)
            gameManager.RegisterMiss();

        ReturnToPool();
    }

    // =====================================================
    // TEMPORIZADOR DEL BALÓN
    // =====================================================

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(maxLifeTime);

        lifeCoroutine = null;

        if (resolved)
            yield break;

        // Si no fue atrapado, registrar el fallo y devolverlo.
        Missed();
    }

    // =====================================================
    // ESPERA DESPUÉS DE ATRAPAR
    // =====================================================

    private IEnumerator DestroyAfterCatch()
    {
        yield return new WaitForSeconds(catchDuration);

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
        StopLifeCoroutine();

        ReturnToPool();
    }

    // =====================================================
    // DEVOLVER AL POOL
    // =====================================================

    private void ReturnToPool()
    {
        StopLifeCoroutine();

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

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
                "[ReactionPassBall] No tiene un pool asignado."
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
