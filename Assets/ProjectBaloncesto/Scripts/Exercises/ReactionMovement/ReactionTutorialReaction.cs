using System;
using System.Collections;
using UnityEngine;

public class ReactionTutorialReaction : MonoBehaviour
{
    // =====================================================
    // ENUMS
    // =====================================================

    private enum ReactionMode
    {
        None,
        Interaction,
        WrongHand,
        Timeout
    }

    // =====================================================
    // REFERENCES
    // =====================================================

    [Header("References")]
    [SerializeField] private CharacterIKController characterIK;
    [SerializeField] private ReactionInteraction interaction;

    [SerializeField] private ReactionHandInteractable leftHandInteractable;
    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [Header("Timing")]
    [SerializeField] private float timeBetweenReactions = 1f;

    // Tiempo máximo que la interacción permanece activa
    // esperando una respuesta.
    [SerializeField] private float interactionTimeoutSeconds = 2f;

    // =====================================================
    // STATE
    // =====================================================

    private ReactionTutorialController tutorialController;

    private ReactionMode currentMode =
        ReactionMode.None;

    private ReactionExerciseController.Hand currentReactionHand;

    private ReactionExerciseController.Hand nextInteractionHand;

    private int requiredCorrectTouches;
    private int currentCorrectTouches;

    private int errorDemonstrations;
    private int currentErrorCount;

    private float currentTimeout;

    private bool waitingForResult;
    private bool errorDetected;

    private Coroutine reactionCoroutine;

    // =====================================================
    // SET CONTROLLER
    // =====================================================

    public void SetTutorialController(
        ReactionTutorialController controller)
    {
        tutorialController = controller;
    }

    // =====================================================
    // START INTERACTION
    // =====================================================

    public void StartInteraction(
        ReactionExerciseController.Hand firstHand,
        int correctTouchesRequired)
    {
        StopCurrentReaction();

        currentMode =
            ReactionMode.Interaction;

        requiredCorrectTouches =
            Mathf.Max(1, correctTouchesRequired);

        currentCorrectTouches = 0;

        nextInteractionHand = firstHand;

        StartNextInteraction();
    }

    // =====================================================
    // INTERACTION LOOP
    // =====================================================

    private void StartNextInteraction()
    {
        if (currentMode != ReactionMode.Interaction)
            return;

        currentReactionHand =
            nextInteractionHand;

        // Alternar para el siguiente intento.
        nextInteractionHand =
            currentReactionHand ==
            ReactionExerciseController.Hand.Left
                ? ReactionExerciseController.Hand.Right
                : ReactionExerciseController.Hand.Left;

        waitingForResult = true;
        errorDetected = false;

        interaction.StartReaction(
            currentReactionHand
        );

        StartTutorialHandReaction(
            currentReactionHand
        );

        MoveHand(
            currentReactionHand,
            ActivateCurrentReaction
        );
    }

    private void ActivateCurrentReaction()
    {
        if (currentMode != ReactionMode.Interaction)
            return;

        if (!waitingForResult)
            return;

        interaction.ActivateReaction();

        reactionCoroutine =
            StartCoroutine(
                WaitForInteractionResult()
            );
    }

    private IEnumerator WaitForInteractionResult()
    {
        float timer = 0f;

        while (waitingForResult)
        {
            timer += Time.deltaTime;

            if (timer >= interactionTimeoutSeconds)
            {
                HandleInteractionTimeout();
                yield break;
            }

            yield return null;
        }
    }

    // =====================================================
    // INTERACTION TIMEOUT
    // =====================================================

    private void HandleInteractionTimeout()
    {
        if (!waitingForResult)
            return;

        waitingForResult = false;

        if (interaction != null)
            interaction.ResolveTimeout();

        ReturnCurrentHand();

        reactionCoroutine = null;

        StartCoroutine(
            WaitAndStartNextInteraction()
        );
    }

    private IEnumerator WaitAndStartNextInteraction()
    {
        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.Interaction)
            yield break;

        StartNextInteraction();
    }

    // =====================================================
    // START ERROR
    // =====================================================

    public void StartError(
        ReactionExerciseController.Hand expectedHand,
        ReactionTutorialController.ErrorType errorType,
        int demonstrations,
        float timeoutSeconds)
    {
        StopCurrentReaction();

        errorDemonstrations =
            Mathf.Max(1, demonstrations);

        currentErrorCount = 0;

        currentTimeout =
            Mathf.Max(0.1f, timeoutSeconds);

        errorDetected = false;

        if (errorType ==
            ReactionTutorialController.ErrorType.WrongHand)
        {
            currentMode =
                ReactionMode.WrongHand;

            currentReactionHand =
                expectedHand;

            StartWrongHandDemonstration();
        }
        else
        {
            currentMode =
                ReactionMode.Timeout;

            currentReactionHand =
                expectedHand;

            StartTimeoutDemonstration();
        }
    }

    // =====================================================
    // WRONG HAND
    // =====================================================

    private void StartWrongHandDemonstration()
    {
        if (currentMode != ReactionMode.WrongHand)
            return;

        waitingForResult = true;
        errorDetected = false;

        interaction.StartReaction(
            currentReactionHand
        );

        StartTutorialHandReaction(
            currentReactionHand
        );

        MoveHand(
            currentReactionHand,
            ActivateWrongHandReaction
        );
    }

    private void ActivateWrongHandReaction()
    {
        if (currentMode != ReactionMode.WrongHand)
            return;

        if (!waitingForResult)
            return;

        interaction.ActivateReaction();

        reactionCoroutine =
            StartCoroutine(
                WaitForWrongHandResult()
            );
    }

    private IEnumerator WaitForWrongHandResult()
    {
        float timer = 0f;

        while (waitingForResult)
        {
            timer += Time.deltaTime;

            if (timer >= currentTimeout)
            {
                HandleWrongHandTimeout();
                yield break;
            }

            yield return null;
        }
    }

    private void HandleWrongHandTimeout()
    {
        if (!waitingForResult)
            return;

        waitingForResult = false;

        if (interaction != null)
            interaction.ResolveTimeout();

        ReturnCurrentHand();

        reactionCoroutine = null;

        StartCoroutine(
            RetryWrongHandAfterDelay()
        );
    }

    private IEnumerator RetryWrongHandAfterDelay()
    {
        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.WrongHand)
            yield break;

        StartWrongHandDemonstration();
    }

    // =====================================================
    // TIMEOUT ERROR
    // =====================================================

    private void StartTimeoutDemonstration()
    {
        if (currentMode != ReactionMode.Timeout)
            return;

        waitingForResult = true;
        errorDetected = false;

        interaction.StartReaction(
            currentReactionHand
        );

        StartTutorialHandReaction(
            currentReactionHand
        );

        MoveHand(
            currentReactionHand,
            ActivateTimeoutReaction
        );
    }

    private void ActivateTimeoutReaction()
    {
        if (currentMode != ReactionMode.Timeout)
            return;

        if (!waitingForResult)
            return;

        interaction.ActivateReaction();

        reactionCoroutine =
            StartCoroutine(
                WaitForTimeout()
            );
    }

    private IEnumerator WaitForTimeout()
    {
        yield return new WaitForSeconds(
            currentTimeout
        );

        if (!waitingForResult)
            yield break;

        waitingForResult = false;

        if (interaction != null)
            interaction.ResolveTimeout();

        ReturnCurrentHand();

        reactionCoroutine = null;

        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.Timeout)
            yield break;

        currentErrorCount++;

        if (currentErrorCount >=
            errorDemonstrations)
        {
            FinishTutorialReaction();
            yield break;
        }

        StartTimeoutDemonstration();
    }

    // =====================================================
    // REACTION RESULT
    // =====================================================

    private void OnReactionResolved(
        bool correct,
        float reactionTime)
    {
        if (!waitingForResult)
            return;

        // =================================================
        // INTERACTION NORMAL
        // =================================================

        if (currentMode ==
            ReactionMode.Interaction)
        {
            waitingForResult = false;

            if (correct)
            {
                currentCorrectTouches++;

                ReturnCurrentHand();

                if (currentCorrectTouches >=
                    requiredCorrectTouches)
                {
                    StartCoroutine(
                        FinishInteractionAfterDelay()
                    );
                }
                else
                {
                    StartCoroutine(
                        WaitAndStartNextInteraction()
                    );
                }
            }
            else
            {
                ReturnCurrentHand();

                StartCoroutine(
                    WaitAndStartNextInteraction()
                );
            }

            return;
        }

        // =================================================
        // WRONG HAND
        // =================================================

        if (currentMode ==
            ReactionMode.WrongHand)
        {
            waitingForResult = false;

            if (!correct)
            {
                errorDetected = true;

                ReturnCurrentHand();

                StartCoroutine(
                    FinishWrongHandAfterDelay()
                );
            }
            else
            {
                errorDetected = false;

                ReturnCurrentHand();

                StartCoroutine(
                    RetryWrongHandAfterDelay()
                );
            }

            return;
        }

        // =================================================
        // TIMEOUT
        // =================================================

        if (currentMode ==
            ReactionMode.Timeout)
        {
            waitingForResult = false;

            errorDetected = false;

            ReturnCurrentHand();

            StartCoroutine(
                RetryTimeoutAfterDelay()
            );
        }
    }

    // =====================================================
    // FINISH COROUTINES
    // =====================================================

    private IEnumerator FinishWrongHandAfterDelay()
    {
        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.WrongHand)
            yield break;

        FinishTutorialReaction();
    }

    private IEnumerator RetryTimeoutAfterDelay()
    {
        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.Timeout)
            yield break;

        StartTimeoutDemonstration();
    }

    private IEnumerator FinishInteractionAfterDelay()
    {
        yield return new WaitForSeconds(
            timeBetweenReactions
        );

        if (currentMode != ReactionMode.Interaction)
            yield break;

        FinishTutorialReaction();
    }

    // =====================================================
    // FINISH
    // =====================================================

    private void FinishTutorialReaction()
    {
        waitingForResult = false;

        ReturnCurrentHand();

        if (interaction != null)
            interaction.EndReaction();

        currentMode =
            ReactionMode.None;

        tutorialController?
            .OnReactionTutorialCompleted();
    }

    // =====================================================
    // MOVE HAND
    // =====================================================

    private void MoveHand(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        if (characterIK == null)
            return;

        if (hand ==
            ReactionExerciseController.Hand.Left)
        {
            characterIK.MoveLeftHandMiddle(
                onActivated
            );
        }
        else
        {
            characterIK.MoveRightHandMiddle(
                onActivated
            );
        }
    }

    // =====================================================
    // RETURN HAND
    // =====================================================

    private void ReturnCurrentHand()
    {
        EndTutorialHandReaction(
            currentReactionHand
        );

        if (characterIK == null)
            return;

        if (currentReactionHand ==
            ReactionExerciseController.Hand.Left)
        {
            characterIK.ReturnLeftHand();
        }
        else
        {
            characterIK.ReturnRightHand();
        }
    }

    private void ReturnAllHands()
    {
        if (characterIK == null)
            return;

        characterIK.ReturnLeftHand();
        characterIK.ReturnRightHand();
    }

    // =====================================================
    // HAND INTERACTABLE
    // =====================================================

    private void StartTutorialHandReaction(
        ReactionExerciseController.Hand hand)
    {
        if (hand ==
            ReactionExerciseController.Hand.Left)
        {
            if (leftHandInteractable != null)
                leftHandInteractable.StartReaction();
        }
        else
        {
            if (rightHandInteractable != null)
                rightHandInteractable.StartReaction();
        }
    }

    private void EndTutorialHandReaction(
        ReactionExerciseController.Hand hand)
    {
        if (hand ==
            ReactionExerciseController.Hand.Left)
        {
            if (leftHandInteractable != null)
                leftHandInteractable.EndReaction();
        }
        else
        {
            if (rightHandInteractable != null)
                rightHandInteractable.EndReaction();
        }
    }

    private void EndTutorialHandReactions()
    {
        if (leftHandInteractable != null)
            leftHandInteractable.EndReaction();

        if (rightHandInteractable != null)
            rightHandInteractable.EndReaction();
    }

    // =====================================================
    // STOP
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
        errorDetected = false;

        if (interaction != null)
            interaction.EndReaction();

        EndTutorialHandReactions();

        ReturnAllHands();

        currentMode =
            ReactionMode.None;
    }

    // =====================================================
    // ENABLE / DISABLE
    // =====================================================

    private void OnEnable()
    {
        if (interaction != null)
            interaction.ReactionResolved +=
                OnReactionResolved;
    }

    private void OnDisable()
    {
        if (interaction != null)
            interaction.ReactionResolved -=
                OnReactionResolved;
    }
}

