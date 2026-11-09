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

    }


    // =====================================================
    // HOVER
    // =====================================================

    public void OnHandHover(
        ReactionExerciseController.Hand hand)
    {


        // No procesar entradas durante pausa
        if (ReactionExerciseController.IsPaused)
        {
            return;
        }

        if (!reactionInProgress)
        {

            return;
        }


        if (reactionResolved)
        {

            return;
        }


        // Tocó antes de que la reacción estuviera activa.
        if (!reactionActive)
        {

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