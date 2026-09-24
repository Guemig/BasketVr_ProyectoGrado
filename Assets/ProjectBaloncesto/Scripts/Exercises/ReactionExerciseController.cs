using UnityEngine;

public class ReactionExerciseController : MonoBehaviour
{
    public enum Hand
    {
        Left,
        Right
    }

    public enum ReactionLevel
    {
        Level1,
        Level2,
        Level3
    }


    [Header("References")]
    [SerializeField] private CharacterIKController characterIK;

    [SerializeField] private ReactionHandInteractable leftHandInteractable;
    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [SerializeField] private ReactionResultIndicator resultIndicator;


    [Header("Exercise Settings")]
    [SerializeField] private ReactionLevel reactionLevel = ReactionLevel.Level1;

    [SerializeField] private int reactionsPerHand = 2;

    [SerializeField] private float timeBetweenReactions = 1f;


    private Hand[] reactionSequence;

    private int currentReaction;

    private Hand currentHand;

    private bool waitingForReaction;


    private void Reset()
    {
        characterIK = GetComponent<CharacterIKController>();
    }


    private void Awake()
    {
        StartExercise();
    }


    public void StartExercise()
    {
        CancelInvoke();

        DisableAllInteractables();

        CreateReactionSequence();

        currentReaction = 0;

        ExecuteNextReaction();
    }


    // =====================================================
    // SEQUENCE
    // =====================================================

    private void CreateReactionSequence()
    {
        int totalReactions = reactionsPerHand * 2;

        reactionSequence = new Hand[totalReactions];

        int index = 0;

        for (int i = 0; i < reactionsPerHand; i++)
        {
            reactionSequence[index] = Hand.Left;
            index++;

            reactionSequence[index] = Hand.Right;
            index++;
        }

        Debug.Log(
            $"Reaction sequence created with {totalReactions} reactions."
        );

        ShuffleSequence();

        Debug.Log(
            $"Sequence: {string.Join(", ", reactionSequence)}"
        );
    }


    private void ShuffleSequence()
    {
        for (int i = reactionSequence.Length - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);

            Hand temporary = reactionSequence[i];

            reactionSequence[i] = reactionSequence[randomIndex];

            reactionSequence[randomIndex] = temporary;
        }
    }


    // =====================================================
    // REACTION
    // =====================================================

    private void ExecuteNextReaction()
    {
        if (characterIK == null)
            return;

        if (currentReaction >= reactionSequence.Length)
        {
            Debug.Log("Reaction exercise completed.");
            return;
        }

        currentHand = reactionSequence[currentReaction];

        // Todavía no se puede reaccionar.
        waitingForReaction = false;

        SetResultNormal(currentHand);

        // La mano comienza a moverse.
        // El interactable todavía está apagado.
        ExecuteReaction(currentHand);
    }


    // =====================================================
    // REACTION ACTIVATION
    // =====================================================

    private void ActivateCurrentHandReaction()
    {
        // Evita activar dos veces la misma reacción.
        if (waitingForReaction)
            return;

        waitingForReaction = true;

        Debug.Log(
            $"Reaction activated: {currentHand}"
        );

        StartCurrentHandReaction();

        // Después del tiempo permitido,
        // la mano regresará.
        Invoke(
            nameof(ReturnCurrentHand),
            timeBetweenReactions
        );
    }


    private void StartCurrentHandReaction()
    {
        switch (currentHand)
        {
            case Hand.Left:

                if (leftHandInteractable != null)
                    leftHandInteractable.StartReaction();

                break;

            case Hand.Right:

                if (rightHandInteractable != null)
                    rightHandInteractable.StartReaction();

                break;
        }
    }


    private void EndCurrentHandReaction()
    {
        switch (currentHand)
        {
            case Hand.Left:

                if (leftHandInteractable != null)
                    leftHandInteractable.EndReaction();

                break;

            case Hand.Right:

                if (rightHandInteractable != null)
                    rightHandInteractable.EndReaction();

                break;
        }

        waitingForReaction = false;
    }


    // =====================================================
    // LEVELS
    // =====================================================

    private void ExecuteReaction(Hand hand)
    {
        switch (reactionLevel)
        {
            case ReactionLevel.Level1:
                ExecuteLevel1(hand);
                break;

            case ReactionLevel.Level2:
                ExecuteLevel2(hand);
                break;

            case ReactionLevel.Level3:
                ExecuteLevel3(hand);
                break;
        }
    }


    private void ExecuteLevel1(Hand hand)
    {
        switch (hand)
        {
            case Hand.Left:

                characterIK.MoveLeftHandMiddle(
                    ActivateCurrentHandReaction
                );

                break;

            case Hand.Right:

                characterIK.MoveRightHandMiddle(
                    ActivateCurrentHandReaction
                );

                break;
        }
    }


    private void ExecuteLevel2(Hand hand)
    {
        bool useHigh = Random.value > 0.5f;

        if (useHigh)
        {
            MoveHandHigh(hand);
        }
        else
        {
            MoveHandMiddle(hand);
        }
    }


    private void ExecuteLevel3(Hand hand)
    {
        int randomHeight = Random.Range(0, 3);

        switch (randomHeight)
        {
            case 0:
                MoveHandMiddle(hand);
                break;

            case 1:
                MoveHandHigh(hand);
                break;

            case 2:
                MoveHandLow(hand);
                break;
        }
    }


    // =====================================================
    // MOVE HAND
    // =====================================================

    private void MoveHandMiddle(Hand hand)
    {
        switch (hand)
        {
            case Hand.Left:

                characterIK.MoveLeftHandMiddle(
                    ActivateCurrentHandReaction
                );

                break;

            case Hand.Right:

                characterIK.MoveRightHandMiddle(
                    ActivateCurrentHandReaction
                );

                break;
        }
    }


    private void MoveHandHigh(Hand hand)
    {
        switch (hand)
        {
            case Hand.Left:

                characterIK.MoveLeftHandHigh(
                    ActivateCurrentHandReaction
                );

                break;

            case Hand.Right:

                characterIK.MoveRightHandHigh(
                    ActivateCurrentHandReaction
                );

                break;
        }
    }


    private void MoveHandLow(Hand hand)
    {
        switch (hand)
        {
            case Hand.Left:

                characterIK.MoveLeftHandLow(
                    ActivateCurrentHandReaction
                );

                break;

            case Hand.Right:

                characterIK.MoveRightHandLow(
                    ActivateCurrentHandReaction
                );

                break;
        }
    }


    // =====================================================
    // HOVER / REACTION
    // =====================================================

    public void OnHandHover(Hand hand)
    {
        if (!waitingForReaction)
            return;

        Debug.Log(
            $"Hover detected: {hand}"
        );

        if (hand == currentHand)
        {
            HandleCorrectReaction();
        }
        else
        {
            HandleWrongReaction();
        }
    }


    private void HandleCorrectReaction()
    {
        Debug.Log(
            $"CORRECT - {currentHand} hand."
        );

        waitingForReaction = false;

        SetResultCorrect(currentHand);
    }


    private void HandleWrongReaction()
    {
        Debug.Log(
            $"WRONG - Expected {currentHand}."
        );

        waitingForReaction = false;

        SetResultWrong(currentHand);
    }


    // =====================================================
    // RESULT INDICATOR
    // =====================================================

    private void SetResultNormal(Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Normal
        );
    }


    private void SetResultCorrect(Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Correct
        );
    }


    private void SetResultWrong(Hand hand)
    {
        if (resultIndicator == null)
            return;

        resultIndicator.SetResult(
            hand,
            ReactionResultIndicator.Result.Wrong
        );
    }


    // =====================================================
    // RETURN HAND
    // =====================================================

    private void ReturnCurrentHand()
    {
        if (characterIK == null)
            return;

        EndCurrentHandReaction();

        switch (currentHand)
        {
            case Hand.Left:
                characterIK.ReturnLeftHand();
                break;

            case Hand.Right:
                characterIK.ReturnRightHand();
                break;
        }

        currentReaction++;

        Invoke(
            nameof(ExecuteNextReaction),
            timeBetweenReactions
        );
    }


    // =====================================================
    // INTERACTABLES
    // =====================================================

    private void DisableAllInteractables()
    {
        if (leftHandInteractable != null)
            leftHandInteractable.EndReaction();

        if (rightHandInteractable != null)
            rightHandInteractable.EndReaction();
    }
}