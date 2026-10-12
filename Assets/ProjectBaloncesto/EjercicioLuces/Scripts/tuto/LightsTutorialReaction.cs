
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsTutorialReaction : MonoBehaviour
{
    private enum ReactionMode
    {
        None,
        Interaction,
        Error
    }

    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private ReactionGameManager gameManager;

    [Header("Tiempos")]
    [SerializeField, Min(0f)]
    private float timeBetweenReactions = 0.5f;

    // =====================================================
    // ESTADO
    // =====================================================

    private LightsTutoController tutorialController;

    private ReactionMode currentMode = ReactionMode.None;

    private LightsTutoController.TargetSelection targetSelection;

    private LightTargetReaction fixedTarget;
    private List<LightTargetReaction> targetPool;

    private LightTargetReaction currentTarget;

    private int requiredCorrectTouches;
    private int currentCorrectTouches;

    private int errorDemonstrations;
    private int currentErrorCount;

    private float interactionTimeoutSeconds;
    private float errorTimeoutSeconds;

    private bool waitingForResult;
    private bool waitingForFeedback;
    private bool lastResultSuccess;

    private Coroutine reactionCoroutine;

    // =====================================================
    // CONTROLADOR
    // =====================================================

    public void SetTutorialController(
        LightsTutoController controller)
    {
        tutorialController = controller;
    }

    // =====================================================
    // INICIAR INTERACCIÓN
    // =====================================================

    public void StartInteraction(
        LightsTutoController.TargetSelection selection,
        LightTargetReaction target,
        List<LightTargetReaction> targets,
        int correctTouchesRequired,
        float timeoutSeconds)
    {
        StopCurrentReaction();

        currentMode = ReactionMode.Interaction;

        targetSelection = selection;
        fixedTarget = target;
        targetPool = targets;

        requiredCorrectTouches = Mathf.Max(1, correctTouchesRequired);
        currentCorrectTouches = 0;

        interactionTimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);

        StartNextInteraction();
    }

    // =====================================================
    // SIGUIENTE INTENTO DE INTERACCIÓN
    // =====================================================

    private void StartNextInteraction()
    {
        if (currentMode != ReactionMode.Interaction)
            return;

        if (!SelectAndActivateTarget())
        {
            Debug.LogError(
                "LightsTutorialReaction: no hay luces válidas " +
                "para ejecutar la interacción."
            );

            FinishTutorialReaction();
            return;
        }

        waitingForResult = true;
        waitingForFeedback = false;

        reactionCoroutine = StartCoroutine(WaitForInteraction());
    }

    private IEnumerator WaitForInteraction()
    {
        yield return new WaitForSecondsRealtime(
            interactionTimeoutSeconds
        );

        if (currentMode != ReactionMode.Interaction ||
            !waitingForResult)
        {
            yield break;
        }

        // No se tocó la luz a tiempo: mostrar error.
        waitingForResult = false;
        waitingForFeedback = true;
        lastResultSuccess = false;

        currentTarget.ShowResult(false);

        reactionCoroutine = null;
    }

    // =====================================================
    // INICIAR DEMOSTRACIÓN DE ERROR
    // =====================================================

    public void StartError(
        LightsTutoController.TargetSelection selection,
        LightTargetReaction target,
        List<LightTargetReaction> targets,
        int demonstrations,
        float timeoutSeconds)
    {
        StopCurrentReaction();

        currentMode = ReactionMode.Error;

        targetSelection = selection;
        fixedTarget = target;
        targetPool = targets;

        errorDemonstrations = Mathf.Max(1, demonstrations);
        currentErrorCount = 0;

        errorTimeoutSeconds = Mathf.Max(0.1f, timeoutSeconds);

        StartTimeoutDemonstration();
    }

    // =====================================================
    // DEMOSTRACIÓN DE TIEMPO AGOTADO
    // =====================================================

    private void StartTimeoutDemonstration()
    {
        if (currentMode != ReactionMode.Error)
            return;

        if (!SelectAndActivateTarget())
        {
            Debug.LogError(
                "LightsTutorialReaction: no hay luces válidas " +
                "para la demostración de error."
            );

            FinishTutorialReaction();
            return;
        }

        waitingForResult = true;
        waitingForFeedback = false;

        reactionCoroutine = StartCoroutine(WaitForErrorTimeout());
    }

    private IEnumerator WaitForErrorTimeout()
    {
        yield return new WaitForSecondsRealtime(
            errorTimeoutSeconds
        );

        if (currentMode != ReactionMode.Error ||
            !waitingForResult)
        {
            yield break;
        }

        // El tiempo se agotó: mostrar el feedback de error.
        waitingForResult = false;
        waitingForFeedback = true;
        lastResultSuccess = false;

        currentTarget.ShowResult(false);

        reactionCoroutine = null;
    }

    // =====================================================
    // SELECCIONAR Y ACTIVAR LUZ
    // =====================================================

    private bool SelectAndActivateTarget()
    {
        LightTargetReaction selectedTarget = SelectTarget();

        if (selectedTarget == null)
            return false;

        UnsubscribeCurrentTarget();

        currentTarget = selectedTarget;

        // Activa el grupo correspondiente para reutilizar
        // las luces que ya existen en el ejercicio.

        if (gameManager != null)
        {
            gameManager.PrepareTutorialTarget(currentTarget);
        }

        int targetLevel = gameManager != null
            ? gameManager.GetTargetLevel(currentTarget)
            : 0;

        if (tutorialController != null)
        {
            tutorialController.ShowTutorialTargetDirection(
                currentTarget.TargetDirection,
                targetLevel
            );
        }

        currentTarget.TutorialPoked += OnTargetPoked;
        currentTarget.TutorialFeedbackFinished +=
            OnTargetFeedbackFinished;

        currentTarget.Deactivate();
        currentTarget.Activate();

        return true;
    }

    private LightTargetReaction SelectTarget()
    {
        if (targetSelection ==
            LightsTutoController.TargetSelection.FixedTarget)
        {
            return fixedTarget;
        }

        if (targetPool == null || targetPool.Count == 0)
        {
            Debug.LogWarning(
                "LightsTutorialReaction: la lista de selección " +
                "aleatoria está vacía."
            );

            return null;
        }

        List<LightTargetReaction> validTargets =
            new List<LightTargetReaction>();

        foreach (LightTargetReaction target in targetPool)
        {
            if (target != null)
                validTargets.Add(target);
        }

        if (validTargets.Count == 0)
            return null;

        int randomIndex = Random.Range(0, validTargets.Count);

        return validTargets[randomIndex];
    }

    // =====================================================
    // TOQUE DE LA LUZ
    // =====================================================

    private void OnTargetPoked(LightTargetReaction touchedTarget)
    {
        // En la demostración de error, el toque no debe
        // detener la espera: necesitamos mostrar el timeout.
        if (currentMode != ReactionMode.Interaction)
            return;

        if (!waitingForResult || waitingForFeedback)
            return;

        if (touchedTarget != currentTarget)
            return;

        waitingForResult = false;

        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
            reactionCoroutine = null;
        }

        lastResultSuccess = true;
        waitingForFeedback = true;

        currentTarget.ShowResult(true);
    }

    // =====================================================
    // FINALIZACIÓN DEL FEEDBACK
    // =====================================================

    private void OnTargetFeedbackFinished(
        LightTargetReaction target,
        bool success)
    {
        if (!waitingForFeedback || target != currentTarget)
            return;

        waitingForFeedback = false;

        if (currentMode == ReactionMode.Interaction)
        {
            if (lastResultSuccess && success)
                currentCorrectTouches++;

            if (currentCorrectTouches >= requiredCorrectTouches)
            {
                reactionCoroutine =
                    StartCoroutine(FinishInteractionAfterDelay());
            }
            else
            {
                reactionCoroutine =
                    StartCoroutine(NextInteractionAfterDelay());
            }

            return;
        }

        if (currentMode == ReactionMode.Error)
        {
            // Solo cuenta la demostración si se agotó el tiempo
            // y la luz terminó mostrando el feedback de error.
            if (!lastResultSuccess && !success)
                currentErrorCount++;

            if (currentErrorCount >= errorDemonstrations)
            {
                reactionCoroutine =
                    StartCoroutine(FinishErrorAfterDelay());
            }
            else
            {
                reactionCoroutine =
                    StartCoroutine(NextErrorAfterDelay());
            }
        }
    }

    // =====================================================
    // ESPERAS ENTRE INTENTOS
    // =====================================================

    private IEnumerator NextInteractionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(timeBetweenReactions);

        reactionCoroutine = null;

        if (currentMode != ReactionMode.Interaction)
            yield break;

        StartNextInteraction();
    }

    private IEnumerator FinishInteractionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(timeBetweenReactions);

        reactionCoroutine = null;

        if (currentMode != ReactionMode.Interaction)
            yield break;

        FinishTutorialReaction();
    }

    private IEnumerator NextErrorAfterDelay()
    {
        yield return new WaitForSecondsRealtime(timeBetweenReactions);

        reactionCoroutine = null;

        if (currentMode != ReactionMode.Error)
            yield break;

        StartTimeoutDemonstration();
    }

    private IEnumerator FinishErrorAfterDelay()
    {
        yield return new WaitForSecondsRealtime(timeBetweenReactions);

        reactionCoroutine = null;

        if (currentMode != ReactionMode.Error)
            yield break;

        FinishTutorialReaction();
    }

    // =====================================================
    // FINALIZAR PASO
    // =====================================================

    private void FinishTutorialReaction()
    {
        waitingForResult = false;
        waitingForFeedback = false;

        UnsubscribeCurrentTarget();

        if (currentTarget != null)
        {
            currentTarget.Deactivate();
            currentTarget = null;
        }

        currentMode = ReactionMode.None;

        tutorialController?.OnReactionTutorialCompleted();
    }

    // =====================================================
    // DESUSCRIBIR EVENTOS
    // =====================================================

    private void UnsubscribeCurrentTarget()
    {
        if (currentTarget == null)
            return;

        currentTarget.TutorialPoked -= OnTargetPoked;

        currentTarget.TutorialFeedbackFinished -=
            OnTargetFeedbackFinished;
    }

    // =====================================================
    // DETENER
    // =====================================================

    public void StopCurrentReaction()
    {
        if (reactionCoroutine != null)
        {
            StopCoroutine(reactionCoroutine);
            reactionCoroutine = null;
        }

        StopAllCoroutines();

        waitingForResult = false;
        waitingForFeedback = false;

        UnsubscribeCurrentTarget();

        if (currentTarget != null)
        {
            currentTarget.Deactivate();
            currentTarget = null;
        }

        currentMode = ReactionMode.None;
    }

    private void OnDisable()
    {
        StopCurrentReaction();
    }
}
