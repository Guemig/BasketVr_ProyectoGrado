using System.Collections;
using UnityEngine;

public class ReactionPassBall : MonoBehaviour
{
    [SerializeField] private float catchDuration = 0.7f;
    [SerializeField] private float maxLifeTime = 3f;

    private PassReactionGameManager gameManager;
    private BallLauncher launcher;

    private Rigidbody rb;
    private Collider ballCollider;

    private bool resolved = false;
    private float launchTime;

    private Coroutine lifeCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ballCollider = GetComponent<Collider>();
    }

    public void Setup(
        PassReactionGameManager manager,
        BallLauncher ballLauncher)
    {
        gameManager = manager;
        launcher = ballLauncher;

        launchTime = Time.time;

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (resolved)
            return;

        HandCatchDetector hand =
            other.GetComponent<HandCatchDetector>();

        if (hand == null)
            return;

        CatchBall(other.transform);
    }

    private void CatchBall(Transform hand)
    {
        if (resolved)
            return;

        resolved = true;

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }

        float reactionTime = Time.time - launchTime;

        if (gameManager != null)
            gameManager.RegisterCatch(reactionTime);

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

        transform.SetParent(hand);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        StartCoroutine(DestroyAfterCatch());
    }

    public void Missed()
    {
        if (resolved)
            return;

        resolved = true;

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }

        if (gameManager != null)
            gameManager.RegisterMiss();

        if (launcher != null)
            launcher.BallResolved();

        Destroy(gameObject);
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(maxLifeTime);

        if (resolved)
            yield break;

        resolved = true;

        if (gameManager != null)
            gameManager.RegisterMiss();

        if (launcher != null)
            launcher.BallResolved();

        Destroy(gameObject);
    }

    private IEnumerator DestroyAfterCatch()
    {
        yield return new WaitForSeconds(catchDuration);

        Destroy(gameObject);
    }
}