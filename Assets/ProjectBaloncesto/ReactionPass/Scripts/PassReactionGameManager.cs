
using System;
using System.Collections;
using UnityEngine;

public class PassReactionGameManager : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]
    [SerializeField] private BallLauncher ballLauncher;

    [Header("Transiciones")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("UI")]
    [SerializeField] private PassReactionUIManager uiManager;

    [Header("Configuración de niveles")]
    [SerializeField] private PassLevelConfigManager levelConfigManager;

    [Header("Estadísticas")]
    [SerializeField] private PassReactionStatsManager statsManager;

    // =====================================================
    // ESTADO DEL EJERCICIO
    // =====================================================

    [Header("Estado")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private bool exerciseFinished = false;
    [SerializeField] private int shotsLaunched = 0;

    private Coroutine nextBallCoroutine;

    private bool isTransitionPending;
    private bool isFinishingLevel;

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

    // Se conserva por compatibilidad.
    public float TimeRemaining => 0f;

    public int ShotsLaunched => shotsLaunched;
    public int RequiredShots => GetCurrentLevelShots();

    public int CaughtBalls =>
        statsManager != null ? statsManager.CaughtBalls : 0;

    public int MissedBalls =>
        statsManager != null ? statsManager.MissedBalls : 0;

    public int TotalBalls =>
        statsManager != null ? statsManager.TotalBalls : 0;

    public float AverageReactionTime =>
        statsManager != null ? statsManager.AverageReactionTime : 0f;

    public PassReactionStatsManager.LevelStats Level1Stats =>
        statsManager != null ? statsManager.Level1Stats : null;

    public PassReactionStatsManager.LevelStats Level2Stats =>
        statsManager != null ? statsManager.Level2Stats : null;

    public PassReactionStatsManager.LevelStats Level3Stats =>
        statsManager != null ? statsManager.Level3Stats : null;

    public int TotalCaught =>
        statsManager != null ? statsManager.TotalCaught : 0;

    public int TotalMissed =>
        statsManager != null ? statsManager.TotalMissed : 0;

    public int TotalPasses =>
        statsManager != null ? statsManager.TotalPasses : 0;

    public float TotalAccuracy =>
        statsManager != null ? statsManager.TotalAccuracy : 0f;

    public float GlobalAverageReaction =>
        statsManager != null ? statsManager.GlobalAverageReaction : 0f;

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

        if (statsManager == null)
        {
            Debug.LogError(
                "[PASSES] PassReactionStatsManager no está asignado.",
                this
            );
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

        if (!ValidateReferences())
            return;

        StopNextBallCoroutine();

        ballLauncher.StopLauncher();

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        isFinishingLevel = false;
        shotsLaunched = 0;

        statsManager.ResetAllStats();

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

        if (!ValidateCurrentLevelConfig() || !ValidateReferences())
            return;

        isTransitionPending = true;

        if (ballLauncher != null)
        {
            ballLauncher.InitializeVisualFeed(GetCurrentLevelShots());
        }

        if (reactionTransition == null)
        {
            Debug.LogWarning(
                "[PASSES] ReactionTransition no está asignada. " +
                "El nivel comenzará directamente.",
                this
            );

            StartLevelAfterTransition();
            return;
        }

        reactionTransition.SetLevelLabel("NIVEL " + currentLevel);

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
        if (!ValidateCurrentLevelConfig() || !ValidateReferences())
            return;

        statsManager.BeginLevel();

        shotsLaunched = 0;
        gameRunning = true;
        isFinishingLevel = false;

        ballLauncher.SetLevel(currentLevel);

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
        if (!gameRunning || isFinishingLevel || statsManager == null)
            return;

        statsManager.RegisterCatch(reactionTime);

        BallFinished();
    }

    // =====================================================
    // BALÓN FALLADO
    // =====================================================

    public void RegisterMiss()
    {
        if (!gameRunning || isFinishingLevel || statsManager == null)
            return;

        statsManager.RegisterMiss();

        BallFinished();
    }

    // =====================================================
    // TERMINÓ UN PASE
    // =====================================================

    private void BallFinished()
    {
        if (!gameRunning || isFinishingLevel)
            return;

        int resolvedShots = statsManager.TotalBalls;

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
        yield return new WaitForSeconds(GetCurrentLevelDelay());

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

    // =====================================================
    // CONSULTAR CONFIGURACIÓN DEL NIVEL
    // =====================================================

    private int GetCurrentLevelShots()
    {
        if (levelConfigManager == null)
            return 0;

        return levelConfigManager.GetRequiredShots(currentLevel);
    }

    private float GetCurrentLevelDelay()
    {
        if (levelConfigManager == null)
            return 0f;

        return levelConfigManager.GetShotDelay(currentLevel);
    }

    private bool ValidateCurrentLevelConfig()
    {
        if (levelConfigManager == null)
        {
            Debug.LogError(
                "[PASSES] PassLevelConfigManager no está asignado.",
                this
            );

            return false;
        }

        if (GetCurrentLevelShots() <= 0)
        {
            Debug.LogError(
                "[PASSES] La cantidad de disparos del nivel " +
                currentLevel + " no es válida.",
                this
            );

            return false;
        }

        return true;
    }

    private bool ValidateReferences()
    {
        if (ballLauncher == null)
        {
            Debug.LogError(
                "[PASSES] BallLauncher no está asignado.",
                this
            );

            return false;
        }

        if (statsManager == null)
        {
            Debug.LogError(
                "[PASSES] PassReactionStatsManager no está asignado.",
                this
            );

            return false;
        }

        return true;
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

        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        if (ballLauncher != null)
            ballLauncher.ResetVisualFeed();

        statsManager.SaveCurrentLevelStats(currentLevel);

        OnLevelFinished?.Invoke(currentLevel);

        Debug.Log(
            "[PASSES] Nivel " + currentLevel + " finalizado."
        );

        // ÚLTIMO NIVEL
        if (currentLevel >= 3)
        {
            exerciseFinished = true;
            isTransitionPending = false;

            statsManager.CalculateGeneralStats();

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
                    "en el Inspector.",
                    this
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
    // REINICIAR EJERCICIO
    // =====================================================

    public void RestartExercise()
    {
        if (!ValidateReferences())
            return;

        StopNextBallCoroutine();

        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        isTransitionPending = false;
        isFinishingLevel = false;

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        shotsLaunched = 0;

        statsManager.ResetAllStats();

        ballLauncher.SetLevel(1);

        BeginLevelTransition();
    }
}
