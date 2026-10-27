using System.Collections.Generic;
using UnityEngine;

public class DribbleTrainingStats : MonoBehaviour
{
    [System.Serializable]
    public class LevelStats
    {
        public int hits;
        public int orangeErrors;
        public int missedGreens;
        public int wrongHandErrors;

        public int totalGreenTargets;
        public int totalOrangeTargets;

        public float totalReactionTime;
        public float bestReactionTime;
        public float worstReactionTime;

        public List<float> reactionTimes =
            new List<float>();

        public float AverageReactionTime
        {
            get
            {
                if (reactionTimes.Count == 0)
                    return 0f;

                return totalReactionTime /
                       reactionTimes.Count;
            }
        }

        public int TotalErrors
        {
            get
            {
                return orangeErrors +
                       missedGreens +
                       wrongHandErrors;
            }
        }

        public int TotalAttempts
        {
            get
            {
                return hits +
                       TotalErrors;
            }
        }

        public float Accuracy
        {
            get
            {
                int total =
                    TotalAttempts;

                if (total <= 0)
                    return 0f;

                return
                    (float)hits /
                    total *
                    100f;
            }
        }

        public void Reset()
        {
            hits = 0;
            orangeErrors = 0;
            missedGreens = 0;
            wrongHandErrors = 0;

            totalGreenTargets = 0;
            totalOrangeTargets = 0;

            totalReactionTime = 0f;
            bestReactionTime = 0f;
            worstReactionTime = 0f;

            reactionTimes.Clear();
        }

        public void RegisterGreenTarget()
        {
            totalGreenTargets++;
        }

        public void RegisterOrangeTarget()
        {
            totalOrangeTargets++;
        }

        public void AddHit(
            float reactionTime)
        {
            hits++;

            reactionTimes.Add(
                reactionTime
            );

            totalReactionTime +=
                reactionTime;

            if (bestReactionTime == 0f ||
                reactionTime <
                bestReactionTime)
            {
                bestReactionTime =
                    reactionTime;
            }

            if (reactionTime >
                worstReactionTime)
            {
                worstReactionTime =
                    reactionTime;
            }
        }

        public void AddOrangeError()
        {
            orangeErrors++;
        }

        public void AddMissedGreen()
        {
            missedGreens++;
        }

        public void AddWrongHandError()
        {
            wrongHandErrors++;
        }
    }

    [Header("Nivel 1")]
    public LevelStats level1 =
        new LevelStats();

    [Header("Nivel 2")]
    public LevelStats level2 =
        new LevelStats();

    [Header("Nivel 3")]
    public LevelStats level3 =
        new LevelStats();

    public void ResetAll()
    {
        level1.Reset();
        level2.Reset();
        level3.Reset();
    }
}