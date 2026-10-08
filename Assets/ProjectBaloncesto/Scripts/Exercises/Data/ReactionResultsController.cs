using System;
using UnityEngine;

public class ReactionResultsController : MonoBehaviour
{
    [Serializable]
    private class LevelResults
    {
        public int totalReactions;
        public int correctReactions;

        public float totalReactionTime;

        // -----------------------------
        // LEFT HAND
        // -----------------------------

        public int leftHandCorrect;
        public int leftHandAttempts;

        public float leftHandTotalReactionTime;


        // -----------------------------
        // RIGHT HAND
        // -----------------------------

        public int rightHandCorrect;
        public int rightHandAttempts;

        public float rightHandTotalReactionTime;


        public int CorrectReactions =>
            correctReactions;


        public float AverageReactionTime
        {
            get
            {
                if (correctReactions <= 0)
                    return 0f;

                return totalReactionTime /
                       correctReactions;
            }
        }


        public float LeftHandReaction
        {
            get
            {
                if (leftHandCorrect <= 0)
                    return 0f;

                return leftHandTotalReactionTime /
                       leftHandCorrect;
            }
        }


        public float RightHandReaction
        {
            get
            {
                if (rightHandCorrect <= 0)
                    return 0f;

                return rightHandTotalReactionTime /
                       rightHandCorrect;
            }
        }


        public float Accuracy
        {
            get
            {
                if (totalReactions <= 0)
                    return 0f;

                return
                    (float)correctReactions /
                    totalReactions *
                    100f;
            }
        }


        public float LeftHandAccuracy
        {
            get
            {
                if (leftHandAttempts <= 0)
                    return 0f;

                return
                    (float)leftHandCorrect /
                    leftHandAttempts *
                    100f;
            }
        }


        public float RightHandAccuracy
        {
            get
            {
                if (rightHandAttempts <= 0)
                    return 0f;

                return
                    (float)rightHandCorrect /
                    rightHandAttempts *
                    100f;
            }
        }


        public void Reset()
        {
            totalReactions = 0;
            correctReactions = 0;
            totalReactionTime = 0f;

            leftHandCorrect = 0;
            leftHandAttempts = 0;
            leftHandTotalReactionTime = 0f;

            rightHandCorrect = 0;
            rightHandAttempts = 0;
            rightHandTotalReactionTime = 0f;
        }
    }


    [Header("Level Results")]
    [SerializeField]
    private LevelResults[] levelResults;


    private void Awake()
    {
        InitializeLevels();
    }


    // =====================================================
    // INITIALIZATION
    // =====================================================

    private void InitializeLevels()
    {
        int levelCount =
            Enum.GetValues(
                typeof(
                    ReactionLevelController.ReactionLevel
                )
            ).Length;


        if (levelResults == null ||
            levelResults.Length != levelCount)
        {
            levelResults =
                new LevelResults[levelCount];
        }


        for (int i = 0;
             i < levelResults.Length;
             i++)
        {
            if (levelResults[i] == null)
            {
                levelResults[i] =
                    new LevelResults();
            }
        }
    }


    // =====================================================
    // REGISTER REACTION
    // =====================================================

    public void RegisterReaction(
        ReactionLevelController.ReactionLevel level,
        bool correct,
        float reactionTime,
        ReactionExerciseController.Hand hand)
    {
        int index =
            (int)level;


        if (!IsValidLevel(index))
            return;


        LevelResults results =
            levelResults[index];


        results.totalReactions++;


        // -----------------------------------------
        // LEFT HAND
        // -----------------------------------------

        if (hand ==
            ReactionExerciseController.Hand.Left)
        {
            results.leftHandAttempts++;

            if (correct)
            {
                results.leftHandCorrect++;

                results.leftHandTotalReactionTime +=
                    Mathf.Max(0f, reactionTime);
            }
        }


        // -----------------------------------------
        // RIGHT HAND
        // -----------------------------------------

        else if (hand ==
                 ReactionExerciseController.Hand.Right)
        {
            results.rightHandAttempts++;

            if (correct)
            {
                results.rightHandCorrect++;

                results.rightHandTotalReactionTime +=
                    Mathf.Max(0f, reactionTime);
            }
        }


        // -----------------------------------------
        // GENERAL
        // -----------------------------------------

        if (!correct)
            return;


        results.correctReactions++;

        results.totalReactionTime +=
            Mathf.Max(0f, reactionTime);
    }


    // =====================================================
    // RESET
    // =====================================================

    public void ResetAllResults()
    {
        if (levelResults == null)
            return;


        foreach (
            LevelResults results
            in levelResults)
        {
            if (results != null)
                results.Reset();
        }
    }


    public void ResetLevel(
        ReactionLevelController.ReactionLevel level)
    {
        int index =
            (int)level;


        if (!IsValidLevel(index))
            return;


        levelResults[index].Reset();
    }


    // =====================================================
    // GET RESULTS
    // =====================================================

    public int GetTotalReactions(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.totalReactions
            : 0;
    }


    public int GetCorrectReactions(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.CorrectReactions
            : 0;
    }


    public float GetAverageReactionTime(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.AverageReactionTime
            : 0f;
    }


    public float GetAccuracy(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.Accuracy
            : 0f;
    }


    // =====================================================
    // HAND RESULTS
    // =====================================================

    public int GetLeftHandCorrect(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.leftHandCorrect
            : 0;
    }


    public int GetLeftHandAttempts(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.leftHandAttempts
            : 0;
    }


    public float GetLeftHandReaction(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.LeftHandReaction
            : 0f;
    }


    public float GetLeftHandAccuracy(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.LeftHandAccuracy
            : 0f;
    }


    public int GetRightHandCorrect(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.rightHandCorrect
            : 0;
    }


    public int GetRightHandAttempts(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.rightHandAttempts
            : 0;
    }


    public float GetRightHandReaction(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.RightHandReaction
            : 0f;
    }


    public float GetRightHandAccuracy(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResults results =
            GetLevelResults(level);

        return results != null
            ? results.RightHandAccuracy
            : 0f;
    }


    // =====================================================
    // INTERNAL
    // =====================================================

    private LevelResults GetLevelResults(
        ReactionLevelController.ReactionLevel level)
    {
        int index =
            (int)level;


        if (!IsValidLevel(index))
            return null;


        return levelResults[index];
    }

    public FirestoreService.MovementLevel
    GetFirestoreLevelData(
        ReactionLevelController.ReactionLevel level)
    {
        return new FirestoreService.MovementLevel
        {
            level = (int)level + 1,

            averageReaction =
                GetAverageReactionTime(level),

            correctAnswers =
                GetCorrectReactions(level),

            totalAttempts =
                GetTotalReactions(level),

            leftHandReaction =
                GetLeftHandReaction(level),

            rightHandReaction =
                GetRightHandReaction(level),

            leftHandCorrect =
                GetLeftHandCorrect(level),

            leftHandAttempts =
                GetLeftHandAttempts(level),

            rightHandCorrect =
                GetRightHandCorrect(level),

            rightHandAttempts =
                GetRightHandAttempts(level)
        };
    }


    private bool IsValidLevel(int index)
    {
        return
            levelResults != null &&
            index >= 0 &&
            index < levelResults.Length;
    }
}