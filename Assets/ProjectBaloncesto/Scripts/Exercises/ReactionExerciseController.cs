using System;
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

    [SerializeField] private ReactionTransition transition;

    [SerializeField] private ReactionInteraction interaction;

    [SerializeField] private ReactionHandInteractable leftHandInteractable;

    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [SerializeField] private CharacterIKController characterIK;

    [SerializeField] private ReactionMenuController menuController;


    [Header("Reaction Timing By Level")]
    [SerializeField]
    private float[] timeBetweenReactions =
    {
        1f,
        0.8f,
        0.6f
    };


    private Hand currentHand;

    private bool reactionInProgress;


    private void Awake()
    {
        SetupInteractables();
    }


    // =====================================================
    // EXERCISE
    // =====================================================

    public void StartExercise()
    {
        CancelInvoke();

        DisableAllInteractables();


        if (transition != null)
        {
            if (levelController != null)
            {
                transition.SetLevelLabel(
                    levelController.CurrentLevel.ToString()
                );
            }


            transition.PlayTransition(
                StartCurrentLevel
            );

            return;
        }


        StartCurrentLevel();
    }


    private void StartCurrentLevel()
    {
        Debug.Log(
            $"[EXERCISE] Starting " +
            $"{levelController.CurrentLevel}"
        );


        sequence.CreateSequence();

        ExecuteNextReaction();
    }


    // =====================================================
    // NEXT REACTION
    // =====================================================

    private void ExecuteNextReaction()
    {
        if (!sequence.HasNext())
        {
            CompleteCurrentLevel();

            return;
        }


        currentHand =
            sequence.GetNext();


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
            GetTimeBetweenReactions()
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
            GetTimeBetweenReactions()
        );
    }


    // =====================================================
    // LEVEL COMPLETE
    // =====================================================

    private void CompleteCurrentLevel()
    {
        reactionInProgress = false;

        DisableAllInteractables();


        Debug.Log(
            $"[LEVEL] Completed: " +
            $"{levelController.CurrentLevel}"
        );


        if (levelController.HasNextLevel())
        {
            StartLevelTransition();

            return;
        }


        CompleteExercise();
    }


    // =====================================================
    // LEVEL TRANSITION
    // =====================================================

    private void StartLevelTransition()
    {
        Debug.Log(
            "[EXERCISE] Preparing next level."
        );


        levelController.TryAdvanceLevel();


        if (transition != null)
        {
            if (levelController != null)
            {
                transition.SetLevelLabel(
                    levelController.CurrentLevel.ToString()
                );
            }


            transition.PlayTransition(
                StartCurrentLevel
            );
        }
        else
        {
            StartCurrentLevel();
        }
    }


    // =====================================================
    // EXERCISE COMPLETE
    // =====================================================

    private void CompleteExercise()
    {
        reactionInProgress = false;

        Debug.Log(
            "[EXERCISE] Exercise completed."
        );

        if (menuController != null)
        {
            menuController.ShowEndMenu();
        }
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


    // =====================================================
    // TIMING
    // =====================================================

    private float GetTimeBetweenReactions()
    {
        int index =
            (int)levelController.CurrentLevel;


        if (timeBetweenReactions == null ||
            timeBetweenReactions.Length == 0)
        {
            return 1f;
        }


        if (index < 0 ||
            index >= timeBetweenReactions.Length)
        {
            return timeBetweenReactions[
                timeBetweenReactions.Length - 1
            ];
        }


        return Mathf.Max(
            0f,
            timeBetweenReactions[index]
        );
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        int levelCount =
            Enum.GetValues(
                typeof(
                    ReactionLevelController.ReactionLevel
                )
            ).Length;


        if (timeBetweenReactions == null ||
            timeBetweenReactions.Length != levelCount)
        {
            float[] newTimes =
                new float[levelCount];


            for (int i = 0;
                 i < levelCount;
                 i++)
            {
                if (timeBetweenReactions != null &&
                    i < timeBetweenReactions.Length)
                {
                    newTimes[i] =
                        timeBetweenReactions[i];
                }
                else
                {
                    newTimes[i] =
                        1f;
                }
            }


            timeBetweenReactions =
                newTimes;
        }
    }

#endif
}