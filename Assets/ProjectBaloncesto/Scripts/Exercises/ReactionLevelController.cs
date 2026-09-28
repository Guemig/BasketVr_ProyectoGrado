using System;
using UnityEngine;

public class ReactionLevelController : MonoBehaviour
{
    public enum ReactionLevel
    {
        Level1,
        Level2,
        Level3
    }


    [Header("References")]
    [SerializeField] private CharacterIKController characterIK;


    [Header("Level")]
    [SerializeField]
    private ReactionLevel currentLevel =
        ReactionLevel.Level1;


    public ReactionLevel CurrentLevel =>
        currentLevel;


    // =====================================================
    // LEVEL
    // =====================================================

    public void SetLevel(ReactionLevel level)
    {
        currentLevel = level;

        Debug.Log(
            $"[LEVEL] Changed to {currentLevel}"
        );
    }


    public bool HasNextLevel()
    {
        return currentLevel != ReactionLevel.Level3;
    }


    public bool TryAdvanceLevel()
    {
        if (!HasNextLevel())
        {
            Debug.Log(
                "[LEVEL] No more levels."
            );

            return false;
        }


        currentLevel++;


        Debug.Log(
            $"[LEVEL] Advanced to {currentLevel}"
        );

        return true;
    }


    // =====================================================
    // REACTION
    // =====================================================

    public void ExecuteReaction(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        switch (currentLevel)
        {
            case ReactionLevel.Level1:

                ExecuteLevel1(
                    hand,
                    onActivated
                );

                break;


            case ReactionLevel.Level2:

                ExecuteLevel2(
                    hand,
                    onActivated
                );

                break;


            case ReactionLevel.Level3:

                ExecuteLevel3(
                    hand,
                    onActivated
                );

                break;
        }
    }


    // =====================================================
    // LEVEL 1
    // =====================================================

    private void ExecuteLevel1(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        MoveHandMiddle(
            hand,
            onActivated
        );
    }


    // =====================================================
    // LEVEL 2
    // =====================================================

    private void ExecuteLevel2(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        bool useHigh =
            UnityEngine.Random.value > 0.5f;


        if (useHigh)
        {
            MoveHandHigh(
                hand,
                onActivated
            );
        }
        else
        {
            MoveHandMiddle(
                hand,
                onActivated
            );
        }
    }


    // =====================================================
    // LEVEL 3
    // =====================================================

    private void ExecuteLevel3(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        int randomHeight =
            UnityEngine.Random.Range(0, 3);


        switch (randomHeight)
        {
            case 0:

                MoveHandMiddle(
                    hand,
                    onActivated
                );

                break;


            case 1:

                MoveHandHigh(
                    hand,
                    onActivated
                );

                break;


            case 2:

                MoveHandLow(
                    hand,
                    onActivated
                );

                break;
        }
    }


    // =====================================================
    // MOVEMENT
    // =====================================================

    private void MoveHandMiddle(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        switch (hand)
        {
            case ReactionExerciseController.Hand.Left:

                characterIK.MoveLeftHandMiddle(
                    onActivated
                );

                break;


            case ReactionExerciseController.Hand.Right:

                characterIK.MoveRightHandMiddle(
                    onActivated
                );

                break;
        }
    }


    private void MoveHandHigh(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        switch (hand)
        {
            case ReactionExerciseController.Hand.Left:

                characterIK.MoveLeftHandHigh(
                    onActivated
                );

                break;


            case ReactionExerciseController.Hand.Right:

                characterIK.MoveRightHandHigh(
                    onActivated
                );

                break;
        }
    }


    private void MoveHandLow(
        ReactionExerciseController.Hand hand,
        Action onActivated)
    {
        switch (hand)
        {
            case ReactionExerciseController.Hand.Left:

                characterIK.MoveLeftHandLow(
                    onActivated
                );

                break;


            case ReactionExerciseController.Hand.Right:

                characterIK.MoveRightHandLow(
                    onActivated
                );

                break;
        }
    }
}