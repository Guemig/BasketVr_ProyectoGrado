using System;
using UnityEngine;

public class ReactionInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ReactionResultIndicator resultIndicator;


    private ReactionExerciseController.Hand expectedHand;

    private bool reactionInProgress;
    private bool reactionActive;
    private bool reactionResolved;

    // Guardar tiempo normalizado: Time.realtimeSinceStartup - TotalPausedTime al activarse.
    private float activationTimeNormalized;


    public bool IsResolved =>
        reactionResolved;


    // Evento que notifica resultado: (correcto, reactionTime)
    public event Action<bool, float> ReactionResolved;


    // =====================================================
    // START
    // =====================================================

    public void StartReaction(
        ReactionExerciseController.Hand hand)
    {
        expectedHand = hand;

        reactionInProgress = true;
        reactionActive = false;
        reactionResolved = false;
        activationTimeNormalized = 0f;

        SetResultNormal(hand);

        Debug.Log(
            $"[INTERACTION] Waiting for {hand}"
        );
    }


    // =====================================================
    // ACTIVATION
    // =====================================================

    public void ActivateReaction()
    {
        if (!reactionInProgress)
            return;

        if (reactionResolved)
            return;

        // No activar durante pausa
        if (ReactionExerciseController.IsPaused)
            return;

        reactionActive = true;
        // Normalizamos tiempo para excluir pausas acumuladas
        activationTimeNormalized =
            Time.realtimeSinceStartup - ReactionExerciseController.TotalPausedTime;

        Debug.Log(
            $"[INTERACTION] Active - Expected: {expectedHand}"
        );
    }


    // =====================================================
    // HOVER
    // =====================================================

    public void OnHandHover(
        ReactionExerciseController.Hand hand)
    {
        Debug.Log(
            $"[INTERACTION] Hover received: {hand}"
        );

        // No procesar entradas durante pausa
        if (ReactionExerciseController.IsPaused)
        {
            Debug.Log("[INTERACTION] Ignored: exercise paused.");
            return;
        }

        if (!reactionInProgress)
        {
            Debug.Log(
                "[INTERACTION] Ignored: no reaction in progress."
            );

            return;
        }


        if (reactionResolved)
        {
            Debug.Log(
                "[INTERACTION] Ignored: reaction already resolved."
            );

            return;
        }


        // Tocó antes de que la reacción estuviera activa.
        if (!reactionActive)
        {
            Debug.Log(
                $"[INTERACTION] WRONG: " +
                $"{hand} touched before activation."
            );

            HandleWrongReaction(
                $"Touched {hand} before activation."
            );

            return;
        }


        // Mano correcta.
        if (hand == expectedHand)
        {
            HandleCorrectReaction();
        }
        // Mano incorrecta.
        else
        {
            HandleWrongReaction(
                $"Expected {expectedHand}, received {hand}."
            );
        }
    }


    // =====================================================
    // TIMEOUT
    // =====================================================

    public void ResolveTimeout()
    {
        if (!reactionInProgress)
            return;

        if (reactionResolved)
            return;

        // No resolver timeout durante pausa
        if (ReactionExerciseController.IsPaused)
            return;

        HandleWrongReaction(
            $"No reaction detected from {expectedHand}."
        );
    }


    // =====================================================
    // END
    // =====================================================

    public void EndReaction()
    {
        reactionInProgress = false;
        reactionActive = false;
    }


    // =====================================================
    // RESULT
    // =====================================================

    private void HandleCorrectReaction()
    {
        if (reactionResolved)
            return;

        float reactionTime =
            reactionActive && activationTimeNormalized > 0f
            ? Time.realtimeSinceStartup - ReactionExerciseController.TotalPausedTime - activationTimeNormalized
            : 0f;

        reactionResolved = true;

        Debug.Log(
            $"[INTERACTION] CORRECT: {expectedHand}"
        );

        SetResultCorrect(expectedHand);

        ReactionResolved?.Invoke(true, reactionTime);
    }

    private void HandleWrongReaction(
        string reason)
    {
        if (reactionResolved)
            return;

        float reactionTime =
            reactionActive && activationTimeNormalized > 0f
            ? Time.realtimeSinceStartup - ReactionExerciseController.TotalPausedTime - activationTimeNormalized
            : 0f;

        reactionResolved = true;

        Debug.Log(
            $"[INTERACTION] WRONG: {reason}"
        );

        SetResultWrong(expectedHand);

        ReactionResolved?.Invoke(false, reactionTime);
    }


    // =====================================================
    // RESULT INDICATOR
    // =====================================================

    private void SetResultNormal(
        ReactionExerciseController.Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Normal
        );
    }


    private void SetResultCorrect(
        ReactionExerciseController.Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Correct
        );
    }


    private void SetResultWrong(
        ReactionExerciseController.Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Wrong
        );
    }
}