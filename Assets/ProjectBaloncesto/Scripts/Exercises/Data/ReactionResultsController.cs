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

        public void Reset()
        {
            totalReactions = 0;
            correctReactions = 0;
            totalReactionTime = 0f;
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
        float reactionTime)
    {
        int index =
            (int)level;


        if (!IsValidLevel(index))
            return;


        LevelResults results =
            levelResults[index];


        results.totalReactions++;


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


    private bool IsValidLevel(int index)
    {
        return
            levelResults != null &&
            index >= 0 &&
            index < levelResults.Length;
    }
}