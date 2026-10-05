using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PassReactionGameManager : MonoBehaviour
{
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

    [Header("Referencias")]
    [SerializeField] private BallLauncher ballLauncher;

    [Header("Duración")]
    [SerializeField] private float gameDuration = 60f;

    [Header("Tiempo entre pases")]
    [SerializeField] private float level1Delay = 1f;
    [SerializeField] private float level2Delay = 0.7f;
    [SerializeField] private float level3Delay = 0.5f;

    [Header("Estado")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private bool timeExpired = false;
    [SerializeField] private bool exerciseFinished = false;
    [SerializeField] private float timeRemaining = 0f;

    [Header("Nivel actual")]
    [SerializeField] private int caughtBalls = 0;
    [SerializeField] private int missedBalls = 0;
    [SerializeField] private float averageReactionTime = 0f;

    [Header("Resultados")]
    [SerializeField] private LevelStats level1Stats = new LevelStats();
    [SerializeField] private LevelStats level2Stats = new LevelStats();
    [SerializeField] private LevelStats level3Stats = new LevelStats();

    [Header("Resultados generales")]
    [SerializeField] private int totalCaught = 0;
    [SerializeField] private int totalMissed = 0;
    [SerializeField] private int totalPasses = 0;
    [SerializeField] private float totalAccuracy = 0f;
    [SerializeField] private float globalAverageReaction = 0f;

    private float totalReactionTime = 0f;

    private List<float> currentReactionTimes =
        new List<float>();

    private Coroutine nextBallCoroutine;

    // EVENTOS PARA LA UI

    public event Action OnInitialState;

    public event Action<int> OnLevelStarted;

    public event Action<int> OnLevelFinished;

    public event Action OnExerciseFinished;

    // DATOS PÚBLICOS

    public bool GameRunning => gameRunning;

    public bool ExerciseFinished =>
        exerciseFinished;

    public int CurrentLevel =>
        currentLevel;

    public float TimeRemaining =>
        timeRemaining;

    public int CaughtBalls =>
        caughtBalls;

    public int MissedBalls =>
        missedBalls;

    public int TotalBalls =>
        caughtBalls + missedBalls;

    public float AverageReactionTime =>
        averageReactionTime;

    public LevelStats Level1Stats =>
        level1Stats;

    public LevelStats Level2Stats =>
        level2Stats;

    public LevelStats Level3Stats =>
        level3Stats;

    public int TotalCaught =>
        totalCaught;

    public int TotalMissed =>
        totalMissed;

    public int TotalPasses =>
        totalPasses;

    public float TotalAccuracy =>
        totalAccuracy;

    public float GlobalAverageReaction =>
        globalAverageReaction;


    private void Start()
    {
        currentLevel = 1;

        gameRunning = false;

        timeExpired = false;

        exerciseFinished = false;

        timeRemaining = 0f;

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(1);
        }

        OnInitialState?.Invoke();
    }


    private void Update()
    {
        if (!gameRunning || timeExpired)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;

            timeExpired = true;

            if (nextBallCoroutine != null)
            {
                StopCoroutine(
                    nextBallCoroutine
                );

                nextBallCoroutine = null;
            }

            // Si ya no hay balón activo,
            // termina inmediatamente el nivel.

            if (ballLauncher == null ||
                !ballLauncher.HasActiveBall())
            {
                FinishCurrentLevel();
            }
        }
    }


    // =========================
    // INICIAR EJERCICIO
    // =========================

    public void StartGame()
    {
        if (gameRunning)
            return;

        currentLevel = 1;

        exerciseFinished = false;

        ResetAllStats();

        StartLevel();
    }


    // =========================
    // SIGUIENTE NIVEL
    // =========================

    public void NextLevel()
    {
        if (gameRunning)
            return;

        if (currentLevel >= 3)
            return;

        currentLevel++;

        StartLevel();
    }


    // =========================
    // INICIAR NIVEL
    // =========================

    private void StartLevel()
    {
        caughtBalls = 0;

        missedBalls = 0;

        totalReactionTime = 0f;

        averageReactionTime = 0f;

        currentReactionTimes.Clear();

        timeRemaining = gameDuration;

        timeExpired = false;

        gameRunning = true;

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(
                currentLevel
            );

            ballLauncher.LaunchBall();
        }

        OnLevelStarted?.Invoke(
            currentLevel
        );
    }


    // =========================
    // BALÓN ATRAPADO
    // =========================

    public void RegisterCatch(
        float reactionTime)
    {
        if (!gameRunning)
            return;

        caughtBalls++;

        totalReactionTime +=
            reactionTime;

        currentReactionTimes.Add(
            reactionTime
        );

        averageReactionTime =
            totalReactionTime /
            caughtBalls;

        BallFinished();
    }


    // =========================
    // BALÓN FALLADO
    // =========================

    public void RegisterMiss()
    {
        if (!gameRunning)
            return;

        missedBalls++;

        BallFinished();
    }


    // =========================
    // TERMINÓ UN PASE
    // =========================

    private void BallFinished()
    {
        if (timeExpired)
        {
            FinishCurrentLevel();

            return;
        }

        ScheduleNextBall();
    }


    private void ScheduleNextBall()
    {
        if (!gameRunning ||
            timeExpired)
        {
            return;
        }

        if (nextBallCoroutine != null)
        {
            StopCoroutine(
                nextBallCoroutine
            );
        }

        nextBallCoroutine =
            StartCoroutine(
                NextBall()
            );
    }


    private IEnumerator NextBall()
    {
        float delay =
            GetCurrentLevelDelay();

        yield return
            new WaitForSeconds(delay);

        nextBallCoroutine = null;

        if (gameRunning &&
            !timeExpired &&
            ballLauncher != null)
        {
            ballLauncher.LaunchBall();
        }
    }


    private float GetCurrentLevelDelay()
    {
        if (currentLevel == 1)
            return level1Delay;

        if (currentLevel == 2)
            return level2Delay;

        return level3Delay;
    }


    // =========================
    // TERMINAR NIVEL
    // =========================

    private void FinishCurrentLevel()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        SaveCurrentLevelStats();

        if (nextBallCoroutine != null)
        {
            StopCoroutine(
                nextBallCoroutine
            );

            nextBallCoroutine = null;
        }

        OnLevelFinished?.Invoke(
            currentLevel
        );

        if (currentLevel >= 3)
        {
            exerciseFinished = true;

            CalculateGeneralStats();

            OnExerciseFinished?.Invoke();
        }
    }


    // =========================
    // GUARDAR ESTADÍSTICAS
    // =========================

    private void SaveCurrentLevelStats()
    {
        LevelStats stats =
            new LevelStats();

        stats.level =
            currentLevel;

        stats.caught =
            caughtBalls;

        stats.missed =
            missedBalls;

        stats.averageReaction =
            averageReactionTime;

        stats.reactionTimes =
            new List<float>(
                currentReactionTimes
            );

        if (stats.Total > 0)
        {
            stats.accuracy =
                (float)stats.caught /
                stats.Total *
                100f;
        }
        else
        {
            stats.accuracy = 0f;
        }

        // Mejor y peor reacción

        if (currentReactionTimes.Count > 0)
        {
            stats.fastestReaction =
                currentReactionTimes[0];

            stats.slowestReaction =
                currentReactionTimes[0];

            foreach (
                float reactionTime
                in currentReactionTimes)
            {
                if (reactionTime <
                    stats.fastestReaction)
                {
                    stats.fastestReaction =
                        reactionTime;
                }

                if (reactionTime >
                    stats.slowestReaction)
                {
                    stats.slowestReaction =
                        reactionTime;
                }
            }
        }
        else
        {
            stats.fastestReaction = 0f;

            stats.slowestReaction = 0f;
        }

        if (currentLevel == 1)
        {
            level1Stats = stats;
        }
        else if (currentLevel == 2)
        {
            level2Stats = stats;
        }
        else
        {
            level3Stats = stats;
        }
    }


    // =========================
    // RESULTADO GENERAL
    // =========================

    private void CalculateGeneralStats()
    {
        totalCaught =
            level1Stats.caught +
            level2Stats.caught +
            level3Stats.caught;

        totalMissed =
            level1Stats.missed +
            level2Stats.missed +
            level3Stats.missed;

        totalPasses =
            totalCaught +
            totalMissed;

        if (totalPasses > 0)
        {
            totalAccuracy =
                (float)totalCaught /
                totalPasses *
                100f;
        }
        else
        {
            totalAccuracy = 0f;
        }

        float reactionSum = 0f;

        int reactionCount = 0;

        AddReactionTimes(
            level1Stats,
            ref reactionSum,
            ref reactionCount
        );

        AddReactionTimes(
            level2Stats,
            ref reactionSum,
            ref reactionCount
        );

        AddReactionTimes(
            level3Stats,
            ref reactionSum,
            ref reactionCount
        );

        if (reactionCount > 0)
        {
            globalAverageReaction =
                reactionSum /
                reactionCount;
        }
        else
        {
            globalAverageReaction = 0f;
        }
    }


    private void AddReactionTimes(
        LevelStats stats,
        ref float total,
        ref int count)
    {
        foreach (
            float reactionTime
            in stats.reactionTimes)
        {
            total += reactionTime;

            count++;
        }
    }


    // =========================
    // REINICIAR
    // =========================

    public void RestartExercise()
    {
        if (nextBallCoroutine != null)
        {
            StopCoroutine(
                nextBallCoroutine
            );

            nextBallCoroutine = null;
        }

        currentLevel = 1;

        gameRunning = false;

        timeExpired = false;

        exerciseFinished = false;

        ResetAllStats();

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(1);
        }

        StartLevel();
    }


    // =========================
    // LIMPIAR DATOS
    // =========================

    private void ResetAllStats()
    {
        level1Stats =
            new LevelStats();

        level2Stats =
            new LevelStats();

        level3Stats =
            new LevelStats();

        totalCaught = 0;

        totalMissed = 0;

        totalPasses = 0;

        totalAccuracy = 0f;

        globalAverageReaction = 0f;

        currentReactionTimes.Clear();
    }
}