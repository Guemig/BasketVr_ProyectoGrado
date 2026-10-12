
using System;
using System.Collections.Generic;
using UnityEngine;

public class ReactionLightGameStats : MonoBehaviour
{
    [Serializable]
    public class LevelStats
    {
        public int hits;
        public int misses;

        public float averageReactionTime;
        public float bestReactionTime;
        public float worstReactionTime;
        public float totalReactionTime;

        public List<float> reactionTimes = new List<float>();

        public float Precision
        {
            get
            {
                int totalAttempts = hits + misses;

                if (totalAttempts == 0)
                    return 0f;

                return (float)hits / totalAttempts * 100f;
            }
        }

        public void ResetStats()
        {
            hits = 0;
            misses = 0;

            averageReactionTime = 0f;
            bestReactionTime = 0f;
            worstReactionTime = 0f;
            totalReactionTime = 0f;

            reactionTimes.Clear();
        }

        public void AddReactionTime(float time)
        {
            reactionTimes.Add(time);
            totalReactionTime += time;

            if (reactionTimes.Count == 1)
            {
                bestReactionTime = time;
                worstReactionTime = time;
            }
            else
            {
                if (time < bestReactionTime)
                    bestReactionTime = time;

                if (time > worstReactionTime)
                    worstReactionTime = time;
            }

            averageReactionTime =
                totalReactionTime / reactionTimes.Count;
        }
    }

    [Header("ESTADÍSTICAS POR NIVEL")]
    [SerializeField] private LevelStats level1Stats = new LevelStats();
    [SerializeField] private LevelStats level2Stats = new LevelStats();
    [SerializeField] private LevelStats level3Stats = new LevelStats();

    public LevelStats Level1Stats => level1Stats;
    public LevelStats Level2Stats => level2Stats;
    public LevelStats Level3Stats => level3Stats;

    public LevelStats GetStats(int level)
    {
        switch (level)
        {
            case 1:
                return level1Stats;

            case 2:
                return level2Stats;

            case 3:
                return level3Stats;

            default:
                Debug.LogWarning(
                    "ReactionGameStats: nivel inválido: " + level
                );
                return null;
        }
    }

    public void RegisterHit(int level, float reactionTime)
    {
        LevelStats stats = GetStats(level);

        if (stats == null)
            return;

        stats.hits++;
        stats.AddReactionTime(reactionTime);
    }

    public void RegisterMiss(int level)
    {
        LevelStats stats = GetStats(level);

        if (stats == null)
            return;

        stats.misses++;
    }

    public void ResetAllStats()
    {
        level1Stats.ResetStats();
        level2Stats.ResetStats();
        level3Stats.ResetStats();
    }

    public int TotalHits =>
        level1Stats.hits +
        level2Stats.hits +
        level3Stats.hits;

    public int TotalMisses =>
        level1Stats.misses +
        level2Stats.misses +
        level3Stats.misses;

    public float TotalPrecision
    {
        get
        {
            int attempts = TotalHits + TotalMisses;

            if (attempts == 0)
                return 0f;

            return (float)TotalHits / attempts * 100f;
        }
    }

    public float TotalAverageReactionTime
    {
        get
        {
            int count =
                level1Stats.reactionTimes.Count +
                level2Stats.reactionTimes.Count +
                level3Stats.reactionTimes.Count;

            if (count == 0)
                return 0f;

            float total =
                level1Stats.totalReactionTime +
                level2Stats.totalReactionTime +
                level3Stats.totalReactionTime;

            return total / count;
        }
    }
}
