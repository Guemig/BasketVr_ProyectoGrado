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

        public float Accuracy
        {
            get
            {
                int total =
                    hits +
                    orangeErrors +
                    missedGreens +
                    wrongHandErrors;

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

            totalReactionTime = 0f;
            bestReactionTime = 0f;
            worstReactionTime = 0f;

            reactionTimes.Clear();
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
    }

    public LevelStats level1 =
        new LevelStats();

    public LevelStats level2 =
        new LevelStats();

    public LevelStats level3 =
        new LevelStats();

    public void ResetAll()
    {
        level1.Reset();
        level2.Reset();
        level3.Reset();
    }
}