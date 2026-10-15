
using System.Collections.Generic;
using UnityEngine;

public class PassReactionStatsManager : MonoBehaviour
{
    // =====================================================
    // RESULTADOS POR NIVEL
    // =====================================================

    [System.Serializable]
    public class LevelStats
    {
        public int level;

        public int caught;
        public int missed;

        public float accuracy;

        public float averageReaction;
        public float fastestReaction;
        public float slowestReaction;

        public List<float> reactionTimes = new List<float>();

        public int Total => caught + missed;
    }

    [Header("Resultados por nivel")]
    [SerializeField] private LevelStats level1Stats = new LevelStats();
    [SerializeField] private LevelStats level2Stats = new LevelStats();
    [SerializeField] private LevelStats level3Stats = new LevelStats();

    // =====================================================
    // RESULTADOS GENERALES
    // =====================================================

    [Header("Resultados generales")]
    [SerializeField] private int totalCaught;
    [SerializeField] private int totalMissed;
    [SerializeField] private int totalPasses;
    [SerializeField] private float totalAccuracy;
    [SerializeField] private float globalAverageReaction;

    // =====================================================
    // ESTADÍSTICAS DEL NIVEL ACTUAL
    // =====================================================

    private int caughtBalls;
    private int missedBalls;

    private float totalReactionTime;
    private float averageReactionTime;

    private readonly List<float> currentReactionTimes =
        new List<float>();

    // =====================================================
    // PROPIEDADES PÚBLICAS
    // =====================================================

    public int CaughtBalls => caughtBalls;
    public int MissedBalls => missedBalls;
    public int TotalBalls => caughtBalls + missedBalls;

    public float AverageReactionTime => averageReactionTime;

    public LevelStats Level1Stats => level1Stats;
    public LevelStats Level2Stats => level2Stats;
    public LevelStats Level3Stats => level3Stats;

    public int TotalCaught => totalCaught;
    public int TotalMissed => totalMissed;
    public int TotalPasses => totalPasses;

    public float TotalAccuracy => totalAccuracy;
    public float GlobalAverageReaction => globalAverageReaction;

    // =====================================================
    // INICIAR ESTADÍSTICAS DEL NIVEL
    // =====================================================

    public void BeginLevel()
    {
        caughtBalls = 0;
        missedBalls = 0;

        totalReactionTime = 0f;
        averageReactionTime = 0f;

        currentReactionTimes.Clear();
    }

    // =====================================================
    // REGISTRAR ATRAPADA
    // =====================================================

    public void RegisterCatch(float reactionTime)
    {
        caughtBalls++;

        totalReactionTime += reactionTime;
        currentReactionTimes.Add(reactionTime);

        averageReactionTime =
            totalReactionTime / caughtBalls;
    }

    // =====================================================
    // REGISTRAR FALLO
    // =====================================================

    public void RegisterMiss()
    {
        missedBalls++;
    }

    // =====================================================
    // GUARDAR RESULTADOS DEL NIVEL
    // =====================================================

    public void SaveCurrentLevelStats(int level)
    {
        LevelStats stats = new LevelStats
        {
            level = level,
            caught = caughtBalls,
            missed = missedBalls,
            averageReaction = averageReactionTime,
            reactionTimes = new List<float>(currentReactionTimes)
        };

        stats.accuracy = stats.Total > 0
            ? (float)stats.caught / stats.Total * 100f
            : 0f;

        if (currentReactionTimes.Count > 0)
        {
            stats.fastestReaction = currentReactionTimes[0];
            stats.slowestReaction = currentReactionTimes[0];

            foreach (float reactionTime in currentReactionTimes)
            {
                if (reactionTime < stats.fastestReaction)
                    stats.fastestReaction = reactionTime;

                if (reactionTime > stats.slowestReaction)
                    stats.slowestReaction = reactionTime;
            }
        }

        switch (level)
        {
            case 1:
                level1Stats = stats;
                break;

            case 2:
                level2Stats = stats;
                break;

            case 3:
                level3Stats = stats;
                break;

            default:
                Debug.LogError(
                    "[PASSES] No se pueden guardar resultados " +
                    "para el nivel " + level + ".",
                    this
                );
                break;
        }
    }

    // =====================================================
    // CALCULAR RESULTADOS GENERALES
    // =====================================================

    public void CalculateGeneralStats()
    {
        totalCaught =
            level1Stats.caught +
            level2Stats.caught +
            level3Stats.caught;

        totalMissed =
            level1Stats.missed +
            level2Stats.missed +
            level3Stats.missed;

        totalPasses = totalCaught + totalMissed;

        totalAccuracy = totalPasses > 0
            ? (float)totalCaught / totalPasses * 100f
            : 0f;

        float reactionSum = 0f;
        int reactionCount = 0;

        AddReactionTimes(level1Stats, ref reactionSum, ref reactionCount);
        AddReactionTimes(level2Stats, ref reactionSum, ref reactionCount);
        AddReactionTimes(level3Stats, ref reactionSum, ref reactionCount);

        globalAverageReaction = reactionCount > 0
            ? reactionSum / reactionCount
            : 0f;
    }

    private void AddReactionTimes(
        LevelStats stats,
        ref float total,
        ref int count)
    {
        if (stats == null || stats.reactionTimes == null)
            return;

        foreach (float reactionTime in stats.reactionTimes)
        {
            total += reactionTime;
            count++;
        }
    }

    // =====================================================
    // REINICIAR ESTADÍSTICAS
    // =====================================================

    public void ResetAllStats()
    {
        level1Stats = new LevelStats();
        level2Stats = new LevelStats();
        level3Stats = new LevelStats();

        totalCaught = 0;
        totalMissed = 0;
        totalPasses = 0;

        totalAccuracy = 0f;
        globalAverageReaction = 0f;

        BeginLevel();
    }
}
