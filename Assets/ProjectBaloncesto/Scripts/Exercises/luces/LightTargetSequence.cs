
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controla la secuencia de aparición, aviso, activación y timeout
/// de los targets del ejercicio de luces.
/// </summary>
public class LightTargetSequence : MonoBehaviour
{
    [Header("Indicador de direcciones")]
    [SerializeField] private LightTargetDirectionIndicator directionIndicator;

    private ReactionGameManager gameManager;

    private List<LightTargetReaction> activeTargets;
    private LightTargetReaction currentTarget;
    private LightTargetReaction previousTarget;

    private Coroutine nextTargetCoroutine;
    private Coroutine targetWarningCoroutine;
    private Coroutine targetTimeoutCoroutine;

    private float targetActivationTime;

    public LightTargetReaction CurrentTarget => currentTarget;
    public float TargetActivationTime => targetActivationTime;

    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    public void Initialize(ReactionGameManager manager)
    {
        gameManager = manager;

        if (directionIndicator == null)
        {
            Debug.LogWarning(
                "LightTargetSequence: falta asignar " +
                "LightTargetDirectionIndicator."
            );
        }
    }

    // =========================================================
    // INICIAR SECUENCIA
    // =========================================================

    public void StartSequence(List<LightTargetReaction> targets)
    {
        StopSequence();

        activeTargets = targets;
        previousTarget = null;
        currentTarget = null;

        if (directionIndicator != null && gameManager != null)
            directionIndicator.SetLevel(gameManager.CurrentLevel);

        ScheduleNextTarget();
    }

    // =========================================================
    // PROGRAMAR SIGUIENTE TARGET
    // =========================================================

    public void ScheduleNextTarget()
    {
        if (gameManager == null || !gameManager.GameRunning)
            return;

        if (nextTargetCoroutine != null)
            StopCoroutine(nextTargetCoroutine);

        nextTargetCoroutine = StartCoroutine(ActivateNextTarget());
    }

    private IEnumerator ActivateNextTarget()
    {
        yield return new WaitForSeconds(gameManager.GetDelayBetweenTargets());

        nextTargetCoroutine = null;

        if (gameManager == null || !gameManager.GameRunning)
            yield break;

        if (activeTargets == null || activeTargets.Count == 0)
            yield break;

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, activeTargets.Count);
        }
        while (activeTargets.Count > 1 &&
               activeTargets[randomIndex] == previousTarget);

        currentTarget = activeTargets[randomIndex];
        previousTarget = currentTarget;

        if (currentTarget != null)
        {
            targetWarningCoroutine = StartCoroutine(
                WarningAndActivate(currentTarget)
            );
        }
    }

    // =========================================================
    // AVISO Y ACTIVACIÓN
    // =========================================================

    private IEnumerator WarningAndActivate(LightTargetReaction target)
    {
        // Mostrar primero la dirección que corresponde al target.
        if (directionIndicator != null)
        {
            directionIndicator.ShowDirection(target.TargetDirection);
        }

        target.Warning();

        float warningTime = gameManager.GetWarningTime();

        Debug.Log(
            "NIVEL " + gameManager.CurrentLevel +
            " | AVISO | " + target.TargetDirection +
            " | " + warningTime.ToString("F3") + " s"
        );

        yield return new WaitForSeconds(warningTime);

        targetWarningCoroutine = null;

        if (gameManager == null || !gameManager.GameRunning)
            yield break;

        if (currentTarget != target)
            yield break;

        target.Activate();

        // El tiempo de reacción comienza exactamente al activar el target.
        targetActivationTime = Time.time;

        Debug.Log(
            "NIVEL " + gameManager.CurrentLevel +
            " | TARGET ACTIVADO"
        );

        targetTimeoutCoroutine = StartCoroutine(TargetTimeout(target));
    }

    // =========================================================
    // TIMEOUT
    // =========================================================

    private IEnumerator TargetTimeout(LightTargetReaction target)
    {
        yield return new WaitForSeconds(gameManager.GetMaxReactionTime());

        targetTimeoutCoroutine = null;

        if (gameManager == null || !gameManager.GameRunning)
            yield break;

        if (currentTarget != target)
            yield break;

        gameManager.RegisterMissFromSequence(target);
    }

    // =========================================================
    // CANCELAR TIMEOUT
    // =========================================================

    public void CancelTimeout()
    {
        if (targetTimeoutCoroutine == null)
            return;

        StopCoroutine(targetTimeoutCoroutine);
        targetTimeoutCoroutine = null;
    }

    // =========================================================
    // LIMPIAR TARGET ACTUAL
    // =========================================================

    public void ClearCurrentTarget(LightTargetReaction target)
    {
        if (currentTarget != target)
            return;

        currentTarget = null;

        if (directionIndicator != null)
            directionIndicator.ClearDirection();
    }

    // =========================================================
    // DETENER SECUENCIA
    // =========================================================

    public void StopSequence()
    {
        StopCoroutineIfRunning(ref nextTargetCoroutine);
        StopCoroutineIfRunning(ref targetWarningCoroutine);
        StopCoroutineIfRunning(ref targetTimeoutCoroutine);

        currentTarget = null;
        previousTarget = null;
        activeTargets = null;

        if (directionIndicator != null)
            directionIndicator.HideAllDirections();
    }

    private void StopCoroutineIfRunning(ref Coroutine coroutine)
    {
        if (coroutine == null)
            return;

        StopCoroutine(coroutine);
        coroutine = null;
    }
}
