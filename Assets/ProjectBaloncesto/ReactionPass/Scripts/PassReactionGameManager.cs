
using System;
using System.Collections;
using UnityEngine;

public class PassReactionGameManager : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Gestión de máquinas")]
    [SerializeField] private PassMachineManager machineManager;

    [Header("Transiciones")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("UI")]
    [SerializeField] private PassReactionUIManager uiManager;

    [Header("Configuración de niveles")]
    [SerializeField] private PassLevelConfigManager levelConfigManager;

    [Header("Estadísticas")]
    [SerializeField] private PassReactionStatsManager statsManager;

    [Header("Indicador visual de máquinas")]
    [SerializeField] private PassMachineIndicator machineIndicator;

    // =====================================================
    // ESTADO DEL EJERCICIO
    // =====================================================

    [Header("Estado")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool gameRunning;
    [SerializeField] private bool exerciseFinished;
    [SerializeField] private int shotsLaunched;

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

        if (machineManager != null)
        {
            machineManager.ConfigureLevel(1);
            machineManager.StopAllMachines();
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

        machineManager.StopAllMachines();

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        isFinishingLevel = false;
        shotsLaunched = 0;

        statsManager.ResetAllStats();

        if (!ConfigureMachinesForLevel(currentLevel))
            return;

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

        machineManager.InitializeVisualFeeds(GetCurrentLevelShots());

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

        // Configurar las máquinas para el nivel actual.
        if (!ConfigureMachinesForLevel(currentLevel))
            return;

        // Crear la distribución completa de acciones del nivel.
        if (!machineManager.PrepareActionSequence(currentLevel))
        {
            Debug.LogError(
                $"[PASSES] No se pudo preparar la secuencia " +
                $"del nivel {currentLevel}.",
                this
            );

            return;
        }

        statsManager.BeginLevel();

        shotsLaunched = 0;
        gameRunning = true;
        isFinishingLevel = false;

        OnLevelStarted?.Invoke(currentLevel);

        Debug.Log(
            $"[PASSES] Nivel {currentLevel} iniciado. " +
            $"Lanzamientos previstos: {GetCurrentLevelShots()}.",
            this
        );

        LaunchNextBall();
    }


    // =====================================================
    // BALÓN ATRAPADO
    // =====================================================

    public void RegisterCatch(float reactionTime)
    {
        if (!gameRunning ||
            isFinishingLevel ||
            statsManager == null)
        {
            return;
        }

        statsManager.RegisterCatch(reactionTime);
        BallFinished();
    }

    // =====================================================
    // BALÓN FALLADO
    // =====================================================

    public void RegisterMiss()
    {
        if (!gameRunning ||
            isFinishingLevel ||
            statsManager == null)
        {
            return;
        }

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

        if (statsManager.TotalBalls >= GetCurrentLevelShots())
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

        if (gameRunning && !isFinishingLevel)
            LaunchNextBall();
    }

    // =====================================================
    // LANZAR SIGUIENTE BALÓN
    // =====================================================


    private void LaunchNextBall()
    {
        if (!gameRunning ||
            isFinishingLevel ||
            shotsLaunched >= GetCurrentLevelShots() ||
            machineManager == null)
        {
            return;
        }

        BallLauncher selectedLauncher =
            machineManager.GetNextLauncher();

        if (selectedLauncher == null)
        {
            Debug.LogError(
                "[PASSES] No se pudo seleccionar una máquina.",
                this
            );

            return;
        }

        DoubleHandPassBall.BallAction action =
            machineManager.GetNextBallAction();

        shotsLaunched++;

        Debug.Log(
            $"[PASSES] Disparo {shotsLaunched}/" +
            $"{GetCurrentLevelShots()} del nivel {currentLevel}. " +
            $"Máquina: {selectedLauncher.name}. " +
            $"Acción: {action}.",
            this
        );

        selectedLauncher.LaunchBall(action);
    }

    // =====================================================
    // CONFIGURACIÓN DE MÁQUINAS
    // =====================================================


    private bool ConfigureMachinesForLevel(int level)
    {
        if (machineManager == null)
        {
            Debug.LogError(
                "[PASSES] PassMachineManager no está asignado.",
                this
            );

            return false;
        }

        if (!machineManager.ConfigureLevel(level))
            return false;

        if (machineIndicator != null)
        {
            machineIndicator.BindMachines(machineManager);
            machineIndicator.SetLevel(level);
        }

        return true;
    }

    private int GetCurrentLevelShots()
    {
        return levelConfigManager != null
            ? levelConfigManager.GetRequiredShots(currentLevel)
            : 0;
    }

    private float GetCurrentLevelDelay()
    {
        return levelConfigManager != null
            ? levelConfigManager.GetShotDelay(currentLevel)
            : 0f;
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

        if (!levelConfigManager.ValidateLevelConfig(currentLevel))
            return false;

        if (GetCurrentLevelShots() <= 0)
        {
            Debug.LogError(
                $"[PASSES] La cantidad de disparos del nivel " +
                $"{currentLevel} no es válida.",
                this
            );

            return false;
        }

        return true;
    }

    private bool ValidateReferences()
    {
        if (machineManager == null)
        {
            Debug.LogError(
                "[PASSES] PassMachineManager no está asignado.",
                this
            );

            return false;
        }

        if (levelConfigManager == null)
        {
            Debug.LogError(
                "[PASSES] PassLevelConfigManager no está asignado.",
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

        machineManager.StopAllMachines();
        machineManager.ResetVisualFeeds();

        statsManager.SaveCurrentLevelStats(currentLevel);

        OnLevelFinished?.Invoke(currentLevel);

        Debug.Log(
            $"[PASSES] Nivel {currentLevel} finalizado.",
            this
        );

        if (currentLevel >= 3)
        {
            exerciseFinished = true;
            isTransitionPending = false;

            statsManager.CalculateGeneralStats();

            OnExerciseFinished?.Invoke();

            if (uiManager != null)
            {
                uiManager.ShowEndMenu();
            }
            else
            {
                Debug.LogError(
                    "[PASSES] uiManager no está asignado.",
                    this
                );
            }

            return;
        }

        currentLevel++;
        isFinishingLevel = false;

        if (!ConfigureMachinesForLevel(currentLevel))
            return;

        BeginLevelTransition();
    }

    // =====================================================
    // ATRAPADA INCORRECTA
    // =====================================================

    // =====================================================
    // ATRAPADA INCORRECTA
    // =====================================================

    // =====================================================
    // ATRAPADA INCORRECTA
    // =====================================================

    public void RegisterWrongCatch(float reactionTime)
    {
        if (!gameRunning ||
            isFinishingLevel ||
            statsManager == null)
        {
            return;
        }

        statsManager.RegisterWrongCatch(reactionTime);

        Debug.Log(
            $"[PASSES] Atrapada incorrecta registrada. " +
            $"Tiempo de reacción: {reactionTime:F3} s. " +
            $"Resultados: {statsManager.TotalBalls}/" +
            $"{GetCurrentLevelShots()}",
            this
        );

        BallFinished();
    }
    // =====================================================
    // PASE CORRECTO
    // =====================================================

    public void RegisterCorrectPass()
    {
        if (!gameRunning ||
            isFinishingLevel ||
            statsManager == null)
        {
            return;
        }

        statsManager.RegisterCorrectPass();

        Debug.Log(
            $"[PASSES] Pase correcto registrado. " +
            $"Resultados: {statsManager.TotalBalls}/" +
            $"{GetCurrentLevelShots()}",
            this
        );

        BallFinished();
    }

    // =====================================================
    // REINICIAR EJERCICIO
    // =====================================================

    public void RestartExercise()
    {
        if (!ValidateReferences())
            return;

        StopNextBallCoroutine();
        machineManager.StopAllMachines();

        isTransitionPending = false;
        isFinishingLevel = false;

        currentLevel = 1;

        gameRunning = false;
        exerciseFinished = false;
        shotsLaunched = 0;

        statsManager.ResetAllStats();

        if (!ConfigureMachinesForLevel(1))
            return;

        BeginLevelTransition();
    }
}
