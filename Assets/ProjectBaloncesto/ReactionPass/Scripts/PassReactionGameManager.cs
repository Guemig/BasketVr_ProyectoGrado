
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

    [Header("Transiciones")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("UI")]
    [SerializeField] private PassReactionUIManager uiManager;

    // =====================================================
    // CONFIGURACIÓN DE LOS NIVELES
    // =====================================================

    [Header("Duración")]
    [SerializeField] private float gameDuration = 60f;

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
    [SerializeField] private bool timeExpired = false;
    [SerializeField] private bool exerciseFinished = false;
    [SerializeField] private float timeRemaining = 0f;

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
    public float TimeRemaining => timeRemaining;

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
        timeExpired = false;
        exerciseFinished = false;
        isTransitionPending = false;
        isFinishingLevel = false;
        timeRemaining = 0f;

        if (ballLauncher != null)
        {
            ballLauncher.StopLauncher();
            ballLauncher.SetLevel(1);
        }

        OnInitialState?.Invoke();
    }

    // =====================================================
    // ACTUALIZACIÓN DEL TIEMPO
    // =====================================================

    private void Update()
    {
        if (!gameRunning || timeExpired)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining > 0f)
            return;

        timeRemaining = 0f;
        timeExpired = true;

        Debug.Log(
            "[PASSES] Tiempo terminado en nivel " +
            currentLevel + "."
        );

        StopNextBallCoroutine();

        // Impide que se generen más balones,
        // incluso si el lanzador estaba preparando uno.
        if (ballLauncher != null)
            ballLauncher.StopLaunching();

        // Si no queda balón activo, terminar inmediatamente.
        if (ballLauncher == null ||
            !ballLauncher.HasActiveBall())
        {
            FinishCurrentLevel();
        }
        else
        {
            Debug.Log(
                "[PASSES] Esperando a que se resuelva " +
                "el último balón."
            );
        }
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
        timeExpired = false;
        exerciseFinished = false;
        isFinishingLevel = false;
        timeRemaining = 0f;

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

        totalReactionTime = 0f;
        averageReactionTime = 0f;

        currentReactionTimes.Clear();

        timeRemaining = gameDuration;
        timeExpired = false;
        gameRunning = true;
        isFinishingLevel = false;

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(currentLevel);
            ballLauncher.LaunchBall();
        }
        else
        {
            Debug.LogError(
                "[PASSES] BallLauncher no está asignado."
            );
        }

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

        if (timeExpired)
        {
            // ReactionPassBall debe llamar a
            // BallResolved() antes de registrar el resultado.
            if (ballLauncher == null ||
                !ballLauncher.HasActiveBall())
            {
                FinishCurrentLevel();
            }

            return;
        }

        ScheduleNextBall();
    }

    // =====================================================
    // PROGRAMAR SIGUIENTE BALÓN
    // =====================================================

    private void ScheduleNextBall()
    {
        if (!gameRunning || timeExpired || isFinishingLevel)
            return;

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
            !timeExpired &&
            !isFinishingLevel &&
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
        timeRemaining = 0f;

        StopNextBallCoroutine();

        // Garantizar que no quede una pelota ni una
        // corrutina del lanzador de este nivel.
        if (ballLauncher != null)
            ballLauncher.StopLauncher();

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
        timeExpired = false;
        exerciseFinished = false;
        timeRemaining = 0f;

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
