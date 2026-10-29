using System;
using System.Collections.Generic;
using UnityEngine;

public class ReactionGameManager : MonoBehaviour
{
    // =========================================================
    // CONFIGURACIÓN DEL EJERCICIO
    // =========================================================

    [Header("CONFIGURACIÓN")]
    [SerializeField] private LightTargetExerciseConfig exerciseConfig;

    // =========================================================
    // ESTADÍSTICAS
    // =========================================================

    [Header("ESTADÍSTICAS")]
    [SerializeField] private ReactionLightGameStats statsController;

    // =========================================================
    // INTERFAZ
    // =========================================================

    [Header("INTERFAZ")]
    [SerializeField] private ReactionGameUI gameUI;

    // =========================================================
    // TRANSICIÓN Y SECUENCIA
    // =========================================================

    [Header("TRANSICIÓN")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("SECUENCIA DE LUCES")]
    [SerializeField] private LightTargetSequence targetSequence;

    // =========================================================
    // TUTORIAL
    // =========================================================

    [Header("TUTORIAL")]
    [SerializeField] private LightsTutoController lightsTutoController;

    // =========================================================
    // ESTADO
    // =========================================================

    [Header("ESTADO")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private bool gameRunning = false;

    [Header("PROGRESO DEL NIVEL")]
    [SerializeField] private int reactionsResolved = 0;

    [Header("MENÚ")]
    [SerializeField] private LightMenuController menuController;

    // Bloquea nuevas reacciones mientras se muestra el feedback.
    private bool isResolvingReaction = false;
    private bool resultsSaveStarted = false;

    // =========================================================
    // TARGETS
    // =========================================================

    private List<LightTargetReaction> level1Targets;
    private List<LightTargetReaction> level2Targets;
    private List<LightTargetReaction> level3Targets;

    private List<LightTargetReaction> activeTargets;

    // =========================================================
    // PROPIEDADES PÚBLICAS
    // =========================================================

    public int CurrentLevel => currentLevel;
    public bool GameRunning => gameRunning;
    public int ReactionsResolved => reactionsResolved;

    public int RequiredReactions =>
        exerciseConfig != null
            ? exerciseConfig.GetRequiredReactions(currentLevel)
            : 0;

    public ReactionLightGameStats.LevelStats Level1Stats =>
        statsController != null ? statsController.Level1Stats : null;

    public ReactionLightGameStats.LevelStats Level2Stats =>
        statsController != null ? statsController.Level2Stats : null;

    public ReactionLightGameStats.LevelStats Level3Stats =>
        statsController != null ? statsController.Level3Stats : null;

    public int TotalHits =>
        statsController != null ? statsController.TotalHits : 0;

    public int TotalMisses =>
        statsController != null ? statsController.TotalMisses : 0;

    public float TotalPrecision =>
        statsController != null ? statsController.TotalPrecision : 0f;

    public float TotalAverageReactionTime =>
        statsController != null
            ? statsController.TotalAverageReactionTime
            : 0f;

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    private void Start()
    {
        if (exerciseConfig == null)
        {
            Debug.LogError(
                "ReactionGameManager: falta asignar " +
                "LightTargetExerciseConfig."
            );

            enabled = false;
            return;
        }

        if (statsController == null)
        {
            Debug.LogError(
                "ReactionGameManager: falta asignar " +
                "ReactionLightGameStats."
            );

            enabled = false;
            return;
        }

        if (targetSequence == null)
            targetSequence = GetComponent<LightTargetSequence>();

        if (targetSequence == null)
        {
            Debug.LogError(
                "ReactionGameManager: falta asignar " +
                "LightTargetSequence."
            );

            enabled = false;
            return;
        }

        // Inicializar los sistemas compartidos.
        targetSequence.Initialize(this);

        GetTargetsFromGroups();
        SetupTargets();
        DeactivateAllGroups();

        ResetAllStats();

        // Decidir qué experiencia iniciar.
        if (ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Tutorial)
        {
            if (lightsTutoController == null)
            {
                Debug.LogError(
                    "ReactionGameManager: el modo es Tutorial, " +
                    "pero no se asignó LightsTutoController."
                );

                return;
            }


            AudioManager.Instance?.PlayBackground(
                BackgroundContext.Tutorial
            );

            lightsTutoController.StartTutorial();
        }
        else
        {
            AudioManager.Instance?.PlayBackground(
BackgroundContext.Tutorial
);

        }
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlayLevelMusic()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "[ReactionGameManager] No se encontró AudioManager."
            );

            return;
        }

        switch (currentLevel)
        {
            case 1:
                AudioManager.Instance.PlayBackground(
                    BackgroundContext.Level1
                );
                break;

            case 2:
                AudioManager.Instance.PlayBackground(
                    BackgroundContext.Level2
                );
                break;

            case 3:
                AudioManager.Instance.PlayBackground(
                    BackgroundContext.Level3
                );
                break;

            default:
                Debug.LogWarning(
                    "[ReactionGameManager] No hay música configurada " +
                    "para el nivel " + currentLevel
                );
                break;
        }
    }

    private void PlayLevelStartSound()
    {
        AudioManager.Instance?.PlaySFX(
            SFXEvent.LevelStart
        );
    }

    private void PlayResultsMusic()
    {
        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Results
        );
    }

    // =========================================================
    // OBTENER TARGETS
    // =========================================================

    private void GetTargetsFromGroups()
    {
        level1Targets = GetTargetsFromGroup(exerciseConfig.Level1Group);
        level2Targets = GetTargetsFromGroup(exerciseConfig.Level2Group);
        level3Targets = GetTargetsFromGroup(exerciseConfig.Level3Group);
    }

    private List<LightTargetReaction> GetTargetsFromGroup(GameObject group)
    {
        if (group == null)
        {
            Debug.LogWarning(
                "ReactionGameManager: el grupo de nivel es NULL."
            );

            return new List<LightTargetReaction>();
        }

        LightTargetReaction[] targets =
            group.GetComponentsInChildren<LightTargetReaction>(true);

        List<LightTargetReaction> targetList =
            new List<LightTargetReaction>(targets);

        Debug.Log(
            "Grupo '" + group.name +
            "' | Targets encontrados: " + targetList.Count
        );

        return targetList;
    }

    // =========================================================
    // CONFIGURAR TARGETS
    // =========================================================

    private void SetupTargets()
    {
        SetupTargetList(level1Targets);
        SetupTargetList(level2Targets);
        SetupTargetList(level3Targets);
    }

    private void SetupTargetList(List<LightTargetReaction> list)
    {
        if (list == null)
            return;

        foreach (LightTargetReaction target in list)
        {
            if (target == null)
                continue;

            target.Setup(this);
            target.Deactivate();
        }
    }

    // =========================================================
    // INICIAR TUTORIAL
    // =========================================================

    public void StartTutorial()
    {
        if (lightsTutoController == null)
        {
            Debug.LogError(
                "ReactionGameManager: falta asignar " +
                "LightsTutoController."
            );

            return;
        }

        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;
        reactionsResolved = 0;
        activeTargets = null;

        DeactivateAllGroups();

        if (gameUI != null)
            gameUI.HideFinalResults();

        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Tutorial
        );

        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Tutorial
        );

        lightsTutoController.StartTutorial();
    }

    // =========================================================
    // PREPARAR TARGET PARA EL TUTORIAL
    // =========================================================

    public void PrepareTutorialTarget(LightTargetReaction target)
    {
        if (target == null || exerciseConfig == null)
            return;

        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;
        activeTargets = null;

        DeactivateAllGroups();

        GameObject targetGroup = GetGroupContainingTarget(target);

        if (targetGroup != null)
        {
            targetGroup.SetActive(true);
        }
        else
        {


            target.gameObject.SetActive(true);
        }

        DeactivateTargetList(level1Targets);
        DeactivateTargetList(level2Targets);
        DeactivateTargetList(level3Targets);

        target.Deactivate();
    }

    private GameObject GetGroupContainingTarget(
        LightTargetReaction target)
    {
        if (target == null || exerciseConfig == null)
            return null;

        if (IsTargetInGroup(target, exerciseConfig.Level1Group))
            return exerciseConfig.Level1Group;

        if (IsTargetInGroup(target, exerciseConfig.Level2Group))
            return exerciseConfig.Level2Group;

        if (IsTargetInGroup(target, exerciseConfig.Level3Group))
            return exerciseConfig.Level3Group;

        return null;
    }

    private bool IsTargetInGroup(
        LightTargetReaction target,
        GameObject group)
    {
        if (target == null || group == null)
            return false;

        Transform targetTransform = target.transform;
        Transform groupTransform = group.transform;

        return targetTransform == groupTransform ||
               targetTransform.IsChildOf(groupTransform);
    }

    // =========================================================
    // INICIAR EJERCICIO
    // =========================================================

    public void StartExercise()
    {
        if (gameRunning)
            return;

        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Training
        );

        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;
        resultsSaveStarted = false;

        if (gameUI != null)
            gameUI.HideFinalResults();

        ResetAllStats();

        currentLevel = 1;
        reactionsResolved = 0;
        activeTargets = null;

        // La música y el sonido comienzan junto a la transición.
        PlayLevelMusic();
        AudioManager.Instance?.PlaySFX(
            SFXEvent.StartGame
        );
        StartLevelWithTransition();
    }

    // =========================================================
    // TRANSICIÓN ENTRE NIVELES
    // =========================================================

    private void StartLevelWithTransition()
    {
        if (reactionTransition != null)
        {
            reactionTransition.SetLevelLabel(
                "NIVEL " + currentLevel
            );

            reactionTransition.PlayTransition(StartLevel);
        }
        else
        {
            StartLevel();
        }
    }

    // =========================================================
    // INICIAR NIVEL
    // =========================================================

    private void StartLevel()
    {
        StopTargetSequence();
        DeactivateAllGroups();

        isResolvingReaction = false;

        switch (currentLevel)
        {
            case 1:
                if (exerciseConfig.Level1Group == null)
                {
                    Debug.LogError(
                        "Falta asignar el grupo del nivel 1."
                    );
                    return;
                }

                exerciseConfig.Level1Group.SetActive(true);
                activeTargets = level1Targets;
                break;

            case 2:
                if (exerciseConfig.Level2Group == null)
                {
                    Debug.LogError(
                        "Falta asignar el grupo del nivel 2."
                    );
                    return;
                }

                exerciseConfig.Level2Group.SetActive(true);
                activeTargets = level2Targets;
                break;

            case 3:
                if (exerciseConfig.Level3Group == null)
                {
                    Debug.LogError(
                        "Falta asignar el grupo del nivel 3."
                    );
                    return;
                }

                exerciseConfig.Level3Group.SetActive(true);
                activeTargets = level3Targets;
                break;

            default:
                Debug.LogError(
                    "ReactionGameManager: nivel inválido."
                );
                return;
        }

        if (activeTargets == null || activeTargets.Count == 0)
        {
            Debug.LogError(
                "ReactionGameManager: el nivel " + currentLevel +
                " no tiene targets configurados."
            );

            DeactivateAllGroups();
            return;
        }

        DeactivateTargetList(activeTargets);

        reactionsResolved = 0;
        gameRunning = true;

        Debug.Log(
            "INICIANDO NIVEL " + currentLevel +
            " | Reacciones requeridas: " + RequiredReactions
        );

        targetSequence.StartSequence(activeTargets);
    }

    // =========================================================
    // TARGET TOCADO: ACIERTO
    // =========================================================

    public void TargetTouched(LightTargetReaction touchedTarget)
    {
        if (!gameRunning || isResolvingReaction)
            return;

        if (targetSequence == null ||
            touchedTarget != targetSequence.CurrentTarget)
            return;

        if (!touchedTarget.IsActive)
            return;

        float reactionTime =
            Time.time - targetSequence.TargetActivationTime;

        isResolvingReaction = true;

        targetSequence.CancelTimeout();
        targetSequence.ClearCurrentTarget(touchedTarget);

        statsController.RegisterHit(currentLevel, reactionTime);

        Debug.Log(
            "NIVEL " + currentLevel +
            " | ACIERTO | " + reactionTime.ToString("F3") + " s"
        );

        touchedTarget.ShowResult(true);
    }

    // =========================================================
    // REGISTRAR FALLO
    // =========================================================

    public void RegisterMissFromSequence(LightTargetReaction target)
    {
        if (!gameRunning || isResolvingReaction)
            return;

        isResolvingReaction = true;

        statsController.RegisterMiss(currentLevel);

        if (targetSequence != null)
            targetSequence.ClearCurrentTarget(target);

        Debug.Log("NIVEL " + currentLevel + " | FALLO");

        if (target != null)
        {
            target.ShowResult(false);
        }
        else
        {
            TargetFeedbackFinished(null);
        }
    }

    // =========================================================
    // FINALIZAR FEEDBACK DEL TARGET
    // =========================================================

    public void TargetFeedbackFinished(LightTargetReaction target)
    {
        if (!gameRunning || !isResolvingReaction)
            return;

        isResolvingReaction = false;

        RegisterResolvedReaction();

        if (gameRunning && targetSequence != null)
            targetSequence.ScheduleNextTarget();
    }

    // =========================================================
    // CONTAR REACCIONES RESUELTAS
    // =========================================================

    private void RegisterResolvedReaction()
    {
        if (!gameRunning)
            return;

        reactionsResolved++;

        Debug.Log(
            "PROGRESO NIVEL " + currentLevel +
            " | " + reactionsResolved +
            "/" + RequiredReactions
        );

        if (reactionsResolved >= RequiredReactions)
            EndLevel();
    }

    // =========================================================
    // TERMINAR NIVEL
    // =========================================================

    private void EndLevel()
    {
        if (!gameRunning)
            return;

        gameRunning = false;
        isResolvingReaction = false;

        StopTargetSequence();

        DeactivateTargetList(activeTargets);
        DeactivateAllGroups();

        DebugLevelStats();

        if (currentLevel < 3)
        {
            currentLevel++;

            // Cambiar música y reproducir el sonido al comenzar
            // la transición hacia el siguiente nivel.
            PlayLevelMusic();

            StartLevelWithTransition();
        }
        else
        {
            EndExercise();
        }
    }

    // =========================================================
    // TERMINAR EJERCICIO
    // =========================================================

    private void EndExercise()
    {
        gameRunning = false;
        isResolvingReaction = false;

        StopTargetSequence();
        DeactivateAllGroups();

        if (menuController != null)
            menuController.ShowEndMenu();

        if (gameUI != null)
            gameUI.ShowFinalResults();

        // Cambiar a la música de resultados.
        PlayResultsMusic();

        Debug.Log("EJERCICIO FINALIZADO");
        Debug.Log("ACIERTOS TOTALES: " + TotalHits);
        Debug.Log("FALLOS TOTALES: " + TotalMisses);

        Debug.Log(
            "PRECISIÓN TOTAL: " +
            TotalPrecision.ToString("F1") + "%"
        );

        Debug.Log(
            "PROMEDIO TOTAL: " +
            TotalAverageReactionTime.ToString("F3") + " s"
        );

        SaveLightsResults();
    }

    // =========================================================
    // GUARDAR RESULTADOS EN FIREBASE
    // =========================================================

    private async void SaveLightsResults()
    {
        if (resultsSaveStarted)
            return;

        FirestoreService firestore = FirestoreService.Instance;

        if (firestore == null)
        {
            Debug.LogError(
                "[LIGHTS FIREBASE] No existe una instancia de FirestoreService."
            );
            return;
        }

        if (!firestore.HasCurrentStudent)
        {
            Debug.LogError(
                "[LIGHTS FIREBASE] No hay un estudiante identificado. " +
                "No se guardaron los resultados."
            );
            return;
        }

        FirestoreService.LightsSession session =
            firestore.CreateLightsSession();

        for (int level = 1; level <= 3; level++)
        {
            ReactionLightGameStats.LevelStats stats =
                statsController.GetStats(level);

            if (stats == null)
            {
                Debug.LogError(
                    "[LIGHTS FIREBASE] No se encontraron estadísticas " +
                    "para el nivel " + level + "."
                );
                return;
            }

            FirestoreService.LightsLevel levelData =
                new FirestoreService.LightsLevel
                {
                    correctAnswers = stats.hits,
                    accuracy = stats.Precision,
                    averageReaction = stats.averageReactionTime
                };

            firestore.AddLightsLevelToSession(
                session,
                levelData
            );
        }

        resultsSaveStarted = true;

        try
        {
            string attemptId = await firestore.SaveLightsAttempt(
                firestore.CurrentStudentCode,
                session
            );

            Debug.Log(
                "[LIGHTS FIREBASE] Intento guardado correctamente. " +
                "ID: " + attemptId
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[LIGHTS FIREBASE] Error al guardar el intento: " +
                exception
            );
        }
    }

    // =========================================================
    // REINICIAR EJERCICIO
    // =========================================================

    public void RestartExercise()
    {
        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Training
        );

        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;
        resultsSaveStarted = false;
        reactionsResolved = 0;
        activeTargets = null;

        DeactivateAllGroups();
        ResetAllStats();

        if (gameUI != null)
            gameUI.HideFinalResults();

        currentLevel = 1;

        // Volver a la música del nivel 1 antes de la transición.
        PlayLevelMusic();

        StartLevelWithTransition();
    }

    // =========================================================
    // OBTENER ESTADÍSTICAS DEL NIVEL ACTUAL
    // =========================================================

    private ReactionLightGameStats.LevelStats GetCurrentStats()
    {
        if (statsController == null)
            return null;

        return statsController.GetStats(currentLevel);
    }

    // =========================================================
    // CONFIGURACIÓN DE TIEMPOS POR NIVEL
    // =========================================================

    public float GetWarningTime()
    {
        return exerciseConfig != null
            ? exerciseConfig.GetWarningTime(currentLevel)
            : 0.3f;
    }

    public float GetDelayBetweenTargets()
    {
        return exerciseConfig != null
            ? exerciseConfig.GetDelayBetweenTargets(currentLevel)
            : 0.5f;
    }

    public float GetMaxReactionTime()
    {
        return exerciseConfig != null
            ? exerciseConfig.GetMaxReactionTime(currentLevel)
            : 1.5f;
    }

    // =========================================================
    // REINICIAR ESTADÍSTICAS
    // =========================================================

    private void ResetAllStats()
    {
        if (statsController != null)
            statsController.ResetAllStats();
    }

    // =========================================================
    // DESACTIVAR TARGETS
    // =========================================================

    private void DeactivateTargetList(List<LightTargetReaction> list)
    {
        if (list == null)
            return;

        foreach (LightTargetReaction target in list)
        {
            if (target != null)
                target.Deactivate();
        }
    }

    // =========================================================
    // DESACTIVAR GRUPOS
    // =========================================================

    private void DeactivateAllGroups()
    {
        if (exerciseConfig == null)
            return;

        if (exerciseConfig.Level1Group != null)
            exerciseConfig.Level1Group.SetActive(false);

        if (exerciseConfig.Level2Group != null)
            exerciseConfig.Level2Group.SetActive(false);

        if (exerciseConfig.Level3Group != null)
            exerciseConfig.Level3Group.SetActive(false);
    }

    // =========================================================
    // DETENER SECUENCIA
    // =========================================================

    private void StopTargetSequence()
    {
        if (targetSequence != null)
            targetSequence.StopSequence();
    }

    // =========================================================
    // OBTENER NIVEL DE UN TARGET
    // =========================================================

    public int GetTargetLevel(LightTargetReaction target)
    {
        if (target == null || exerciseConfig == null)
            return 0;

        if (IsTargetInGroup(target, exerciseConfig.Level1Group))
            return 1;

        if (IsTargetInGroup(target, exerciseConfig.Level2Group))
            return 2;

        if (IsTargetInGroup(target, exerciseConfig.Level3Group))
            return 3;

        return 0;
    }

    // =========================================================
    // DEBUG ESTADÍSTICAS
    // =========================================================

    private void DebugLevelStats()
    {
        ReactionLightGameStats.LevelStats stats = GetCurrentStats();

        if (stats == null)
            return;

        Debug.Log("NIVEL " + currentLevel + " TERMINADO");
        Debug.Log("Reacciones resueltas: " + reactionsResolved);
        Debug.Log("Aciertos: " + stats.hits);
        Debug.Log("Fallos: " + stats.misses);

        Debug.Log(
            "Precisión: " + stats.Precision.ToString("F1") + "%"
        );

        Debug.Log(
            "Promedio: " + stats.averageReactionTime.ToString("F3") + " s"
        );

        Debug.Log(
            "Mejor: " + stats.bestReactionTime.ToString("F3") + " s"
        );

        Debug.Log(
            "Peor: " + stats.worstReactionTime.ToString("F3") + " s"
        );

        Debug.Log(
            "Tiempo total reacción: " +
            stats.totalReactionTime.ToString("F3") + " s"
        );
    }
}