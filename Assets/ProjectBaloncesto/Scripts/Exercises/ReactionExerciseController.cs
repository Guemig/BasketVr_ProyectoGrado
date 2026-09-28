using UnityEngine;

public class ReactionExerciseController : MonoBehaviour
{
    public enum Hand
    {
        Left,
        Right
    }


    [Header("References")]
    [SerializeField] private ReactionSequence sequence;

    [SerializeField] private ReactionLevelController levelController;

    [SerializeField] private ReactionInteraction interaction;

    [SerializeField] private ReactionHandInteractable leftHandInteractable;

    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [SerializeField] private CharacterIKController characterIK;


    [Header("Exercise Settings")]
    [SerializeField] private int reactionsPerHand = 2;

    [SerializeField] private float timeBetweenReactions = 1f;


    private Hand currentHand;

    private bool reactionInProgress;


    private void Awake()
    {
        SetupInteractables();
        StartExercise();
    }


    // =====================================================
    // EXERCISE
    // =====================================================

    public void StartExercise()
    {
        CancelInvoke();

        DisableAllInteractables();

        sequence.CreateSequence(
            reactionsPerHand
        );

        ExecuteNextReaction();
    }


    private void ExecuteNextReaction()
    {
        if (!sequence.HasNext())
        {
            CompleteExercise();
            return;
        }


        currentHand = sequence.GetNext();

        reactionInProgress = true;


        Debug.Log(
            $"[REACTION] STARTING {currentHand}"
        );


        interaction.StartReaction(
            currentHand
        );


        levelController.ExecuteReaction(
            currentHand,
            ActivateCurrentReaction
        );
    }


    // =====================================================
    // REACTION ACTIVATION
    // =====================================================

    private void ActivateCurrentReaction()
    {
        if (!reactionInProgress)
            return;


        Debug.Log(
            $"[REACTION] ACTIVE {currentHand}"
        );


        interaction.ActivateReaction();

        StartCurrentHandReaction();


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


    // =====================================================
    // RETURN
    // =====================================================

    private void ReturnCurrentHand()
    {
        if (!reactionInProgress)
            return;



        interaction.ResolveTimeout();


        EndCurrentHandReaction();

        interaction.EndReaction();

        reactionInProgress = false;


        switch (currentHand)
        {
            case Hand.Left:

                characterIK.ReturnLeftHand();

                break;


            case Hand.Right:

                characterIK.ReturnRightHand();

                break;
        }


        Invoke(
            nameof(ExecuteNextReaction),
            timeBetweenReactions
        );
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
    }


    // =====================================================
    // COMPLETE
    // =====================================================

    private void CompleteExercise()
    {
        reactionInProgress = false;

        Debug.Log(
            "[EXERCISE] Exercise completed."
        );
    }


    // =====================================================
    // INTERACTABLES
    // =====================================================

    private void SetupInteractables()
    {
        if (leftHandInteractable != null)
        {
            leftHandInteractable.SetReactionInteraction(
                interaction
            );
        }


        if (rightHandInteractable != null)
        {
            rightHandInteractable.SetReactionInteraction(
                interaction
            );
        }
    }


    private void DisableAllInteractables()
    {
        if (leftHandInteractable != null)
            leftHandInteractable.EndReaction();


        if (rightHandInteractable != null)
            rightHandInteractable.EndReaction();
    }
}