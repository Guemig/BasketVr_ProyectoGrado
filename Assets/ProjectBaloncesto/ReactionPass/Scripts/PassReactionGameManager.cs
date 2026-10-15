
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

    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private BallLauncher ballLauncher;
    [SerializeField] private LauncherBallFeedEffect feedEffect;

    [Header("Transiciones")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("UI")]
    [SerializeField] private PassReactionUIManager uiManager;

    // =====================================================
    // CONFIGURACIÓN DE LOS NIVELES
    // =====================================================

    [Header("Cantidad de disparos por nivel")]
    [SerializeField, Min(1)] private int level1Shots = 10;
    [SerializeField, Min(1)] private int level2Shots = 12;
    [SerializeField, Min(1)] private int level3Shots = 15;

    [Header("Tiempo entre pases")]
    [SerializeField] private float level1Delay = 1f;
    [SerializeField] private float level2Delay = 0.7f;
    [SerializeField] private float level3Delay = 0.5f;

    // =====================================================
    // ESTADO
    // =====================================================

    [Header("Estado")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private bool exerciseFinished = false;
    [SerializeField] private int shotsLaunched = 0;

    [Header("Nivel actual")]
    [SerializeField] private int caughtBalls = 0;
    [SerializeField] private int missedBalls = 0;
    [SerializeField] private float averageReactionTime = 0f;

    // =====================================================
    // ESTADÍSTICAS POR NIVEL
    // =====================================================

    [Header("Resultados")]
    [SerializeField] private LevelStats level1Stats = new LevelStats();
    [SerializeField] private LevelStats level2Stats = new LevelStats();
    [SerializeField] private LevelStats level3Stats = new LevelStats();

    // =====================================================
    // ESTADÍSTICAS GENERALES
    // =====================================================

    [Header("Resultados generales")]
    [SerializeField] private int totalCaught = 0;
    [SerializeField] private int totalMissed = 0;
    [SerializeField] private int totalPasses = 0;
    [SerializeField] private float totalAccuracy = 0f;
    [SerializeField] private float globalAverageReaction = 0f;

    // =====================================================
    // VARIABLES INTERNAS
    // =====================================================

    private float totalReactionTime = 0f;

    private readonly List<float> currentReactionTimes =
        new List<float>();

    private Coroutine nextBallCoroutine;

    private bool isTransitionPending = false;

    // Evita finalizar un nivel más de una vez.
    private bool isFinishingLevel = false;

    // =====================================================
    // EVENTOS
    // =====================================================

    public event Action OnInitialState;
    public event Action<int> OnLevelStarted;
    public event Action<int> OnLevelFinished;
    public event Action OnExerciseFinished;

    // =====================================================
    // DATOS PÚBLICOS
    // =====================================================

    public bool GameRunning => gameRunning;
    public bool ExerciseFinished => exerciseFinished;
    public int CurrentLevel => currentLevel;
    // Se conserva esta propiedad por compatibilidad con otros scripts.
    // El ejercicio ahora se controla por cantidad de disparos, no por tiempo.
    public float TimeRemaining => 0f;

    public int ShotsLaunched => shotsLaunched;
    public int RequiredShots => GetCurrentLevelShots();

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
    // INICIALIZACIÓN
    // =====================================================

    private void Start()
    {
        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        isTransitionPending = false;
        isFinishingLevel = false;
        shotsLaunched = 0;

        if (ballLauncher != null)
        {
            ballLauncher.StopLauncher();
            ballLauncher.SetLevel(1);
        }

        OnInitialState?.Invoke();
    }

    // =====================================================
    // INICIAR EJERCICIO
    // =====================================================

    public void StartGame()
    {
        if (gameRunning ||
            isTransitionPending ||
            exerciseFinished)
        {
            return;
        }

        StopNextBallCoroutine();

        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        isFinishingLevel = false;
        shotsLaunched = 0;

        ResetAllStats();

        if (ballLauncher != null)
            ballLauncher.SetLevel(1);

        BeginLevelTransition();
    }

    // =====================================================
    // TRANSICIÓN ANTES DE CADA NIVEL
    // =====================================================

    private void BeginLevelTransition()
    {
        if (gameRunning ||
            exerciseFinished ||
            isTransitionPending)
        {
            return;
        }

        isTransitionPending = true;

        if (feedEffect != null)
        {
            feedEffect.InitializeForLevel(GetCurrentLevelShots());
        }

        if (reactionTransition == null)
        {
            Debug.LogWarning(
                "[PASSES] ReactionTransition no está asignada. " +
                "El nivel comenzará directamente."
            );

            StartLevelAfterTransition();
            return;
        }

        reactionTransition.SetLevelLabel(
            "NIVEL " + currentLevel
        );

        reactionTransition.PlayTransition(() =>
        {
            StartLevelAfterTransition();
        });
    }

    private void StartLevelAfterTransition()
    {
        if (!isTransitionPending ||
            exerciseFinished ||
            gameRunning)
        {
            return;
        }

        isTransitionPending = false;
        StartLevel();
    }

    // =====================================================
    // INICIAR NIVEL
    // =====================================================

    private void StartLevel()
    {
        caughtBalls = 0;
        missedBalls = 0;
        shotsLaunched = 0;

        totalReactionTime = 0f;
        averageReactionTime = 0f;

        currentReactionTimes.Clear();

        gameRunning = true;
        isFinishingLevel = false;

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(currentLevel);
        }
        else
        {
            Debug.LogError(
                "[PASSES] BallLauncher no está asignado."
            );
        }

        // El primer disparo cuenta dentro del total configurado.
        LaunchNextBall();

        OnLevelStarted?.Invoke(currentLevel);

        Debug.Log(
            "[PASSES] Nivel " + currentLevel + " iniciado."
        );
    }

    // =====================================================
    // BALÓN ATRAPADO
    // =====================================================

    public void RegisterCatch(float reactionTime)
    {
        if (!gameRunning || isFinishingLevel)
            return;

        caughtBalls++;

        totalReactionTime += reactionTime;
        currentReactionTimes.Add(reactionTime);

        averageReactionTime =
            totalReactionTime / caughtBalls;

        BallFinished();
    }

    // =====================================================
    // BALÓN FALLADO
    // =====================================================

    public void RegisterMiss()
    {
        if (!gameRunning || isFinishingLevel)
            return;

        missedBalls++;

        BallFinished();
    }

    // =====================================================
    // TERMINÓ UN PASE
    // =====================================================

    private void BallFinished()
    {
        if (!gameRunning || isFinishingLevel)
            return;

        // Cada balón resuelto (atrapado o fallado) cuenta como un disparo.
        int resolvedShots = caughtBalls + missedBalls;

        if (resolvedShots >= GetCurrentLevelShots())
        {
            FinishCurrentLevel();
            return;
        }

        ScheduleNextBall();
    }

    // =====================================================
    // PROGRAMAR SIGUIENTE BALÓN
    // =====================================================

    private void ScheduleNextBall()
    {
        if (!gameRunning ||
            isFinishingLevel ||
            shotsLaunched >= GetCurrentLevelShots())
        {
            return;
        }

        StopNextBallCoroutine();
        nextBallCoroutine = StartCoroutine(NextBall());
    }

    private IEnumerator NextBall()
    {
        yield return new WaitForSeconds(
            GetCurrentLevelDelay()
        );

        nextBallCoroutine = null;

        if (gameRunning &&
            !isFinishingLevel &&
            ballLauncher != null)
        {
            LaunchNextBall();
        }
    }

    private void LaunchNextBall()
    {
        if (!gameRunning ||
            isFinishingLevel ||
            ballLauncher == null ||
            shotsLaunched >= GetCurrentLevelShots())
        {
            return;
        }

        shotsLaunched++;
        ballLauncher.LaunchBall();

        Debug.Log(
            "[PASSES] Disparo " + shotsLaunched +
            "/" + GetCurrentLevelShots() +
            " del nivel " + currentLevel + "."
        );
    }

    private int GetCurrentLevelShots()
    {
        if (currentLevel == 1)
            return Mathf.Max(1, level1Shots);

        if (currentLevel == 2)
            return Mathf.Max(1, level2Shots);

        return Mathf.Max(1, level3Shots);
    }

    private float GetCurrentLevelDelay()
    {
        if (currentLevel == 1)
            return level1Delay;

        if (currentLevel == 2)
            return level2Delay;

        return level3Delay;
    }

    private void StopNextBallCoroutine()
    {
        if (nextBallCoroutine == null)
            return;

        StopCoroutine(nextBallCoroutine);
        nextBallCoroutine = null;
    }

    // =====================================================
    // TERMINAR NIVEL
    // =====================================================

    private void FinishCurrentLevel()
    {
        if (!gameRunning || isFinishingLevel)
            return;

        isFinishingLevel = true;
        gameRunning = false;

        StopNextBallCoroutine();

        // Detiene el lanzador y limpia la reserva visual,
        // evitando balones congelados o atravesando el modelo.
        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        if (feedEffect != null)
            feedEffect.ResetFeed();

        SaveCurrentLevelStats();

        OnLevelFinished?.Invoke(currentLevel);

        Debug.Log(
            "[PASSES] Nivel " + currentLevel + " finalizado."
        );

        // ÚLTIMO NIVEL
        if (currentLevel >= 3)
        {
            exerciseFinished = true;
            isTransitionPending = false;

            CalculateGeneralStats();

            OnExerciseFinished?.Invoke();

            Debug.Log(
                "[PASSES] Ejercicio completado. " +
                "Mostrando menú final."
            );

            if (uiManager != null)
            {
                uiManager.ShowEndMenu();

                Debug.Log(
                    "[PASSES] ShowEndMenu() ejecutado."
                );
            }
            else
            {
                Debug.LogError(
                    "[PASSES] uiManager no está asignado " +
                    "en el Inspector."
                );
            }

            return;
        }

        // SIGUIENTE NIVEL
        currentLevel++;

        isFinishingLevel = false;

        if (ballLauncher != null)
            ballLauncher.SetLevel(currentLevel);

        BeginLevelTransition();
    }

    // =====================================================
    // GUARDAR ESTADÍSTICAS DEL NIVEL
    // =====================================================

    private void SaveCurrentLevelStats()
    {
        LevelStats stats = new LevelStats();

        stats.level = currentLevel;

        stats.caught = caughtBalls;
        stats.missed = missedBalls;

        stats.averageReaction = averageReactionTime;

        stats.reactionTimes =
            new List<float>(currentReactionTimes);

        if (stats.Total > 0)
        {
            stats.accuracy =
                (float)stats.caught /
                stats.Total * 100f;
        }
        else
        {
            stats.accuracy = 0f;
        }

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
        else
        {
            stats.fastestReaction = 0f;
            stats.slowestReaction = 0f;
        }

        if (currentLevel == 1)
            level1Stats = stats;
        else if (currentLevel == 2)
            level2Stats = stats;
        else
            level3Stats = stats;
    }

    // =====================================================
    // CALCULAR ESTADÍSTICAS GENERALES
    // =====================================================

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
        foreach (float reactionTime in stats.reactionTimes)
        {
            total += reactionTime;
            count++;
        }
    }

    // =====================================================
    // REINICIAR EJERCICIO
    // =====================================================

    public void RestartExercise()
    {
        StopNextBallCoroutine();

        // Detener también cualquier balón o preparación
        // que haya quedado de la sesión anterior.
        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        isTransitionPending = false;
        isFinishingLevel = false;

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        shotsLaunched = 0;

        ResetAllStats();

        if (ballLauncher != null)
            ballLauncher.SetLevel(1);

        BeginLevelTransition();
    }

    // =====================================================
    // LIMPIAR ESTADÍSTICAS
    // =====================================================

    private void ResetAllStats()
    {
        level1Stats = new LevelStats();
        level2Stats = new LevelStats();
        level3Stats = new LevelStats();

        totalCaught = 0;
        totalMissed = 0;
        totalPasses = 0;

        totalAccuracy = 0f;
        globalAverageReaction = 0f;

        caughtBalls = 0;
        missedBalls = 0;

        totalReactionTime = 0f;
        averageReactionTime = 0f;

        currentReactionTimes.Clear();
    }
}
