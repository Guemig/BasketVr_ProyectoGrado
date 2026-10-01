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

    private float activationTime;


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
        activationTime = 0f;

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

        reactionActive = true;
        activationTime = Time.time;

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
            reactionActive && activationTime > 0f
            ? Time.time - activationTime
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
            reactionActive && activationTime > 0f
            ? Time.time - activationTime
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