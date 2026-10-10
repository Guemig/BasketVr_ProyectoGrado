
using System.Collections.Generic;
using UnityEngine;

public class ReactionGameManager : MonoBehaviour
{
    [System.Serializable]
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

    // =========================================================
    // CONFIGURACIÓN DEL EJERCICIO
    // =========================================================

    [Header("CONFIGURACIÓN")]
    [SerializeField] private LightTargetExerciseConfig exerciseConfig;

    // =========================================================
    // CANVAS FINAL
    // =========================================================

    [Header("CANVAS FINAL")]
    [SerializeField] private GameObject finalCanvas;

    // =========================================================
    // TRANSICIÓN Y SECUENCIA
    // =========================================================

    [Header("TRANSICIÓN")]
    [SerializeField] private ReactionTransition reactionTransition;

    [Header("SECUENCIA DE LUCES")]
    [SerializeField] private LightTargetSequence targetSequence;

    // =========================================================
    // AUDIO DE INICIO
    // =========================================================

    [Header("AUDIO DE INICIO")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;

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

    // =========================================================
    // TARGETS
    // =========================================================

    private List<LightTargetReaction> level1Targets;
    private List<LightTargetReaction> level2Targets;
    private List<LightTargetReaction> level3Targets;

    private List<LightTargetReaction> activeTargets;

    // =========================================================
    // ESTADÍSTICAS
    // =========================================================

    private LevelStats level1Stats = new LevelStats();
    private LevelStats level2Stats = new LevelStats();
    private LevelStats level3Stats = new LevelStats();

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

    public LevelStats Level1Stats => level1Stats;
    public LevelStats Level2Stats => level2Stats;
    public LevelStats Level3Stats => level3Stats;

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
            int totalAttempts = TotalHits + TotalMisses;

            if (totalAttempts == 0)
                return 0f;

            return (float)TotalHits / totalAttempts * 100f;
        }
    }

    public float TotalAverageReactionTime
    {
        get
        {
            int totalReactions =
                level1Stats.reactionTimes.Count +
                level2Stats.reactionTimes.Count +
                level3Stats.reactionTimes.Count;

            if (totalReactions == 0)
                return 0f;

            float totalTime =
                level1Stats.totalReactionTime +
                level2Stats.totalReactionTime +
                level3Stats.totalReactionTime;

            return totalTime / totalReactions;
        }
    }

    // =========================================================
    // START
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

        if (targetSequence == null)
            targetSequence = GetComponent<LightTargetSequence>();

        if (targetSequence != null)
        {
            targetSequence.Initialize(this);
        }
        else
        {
            Debug.LogError(
                "ReactionGameManager: falta asignar " +
                "LightTargetSequence."
            );

            enabled = false;
            return;
        }

        GetTargetsFromGroups();
        SetupTargets();
        DeactivateAllGroups();

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        ResetAllStats();
    }

    // =========================================================
    // OBTENER TARGETS
    // =========================================================

    private void GetTargetsFromGroups()
    {
        level1Targets =
            GetTargetsFromGroup(exerciseConfig.Level1Group);

        level2Targets =
            GetTargetsFromGroup(exerciseConfig.Level2Group);

        level3Targets =
            GetTargetsFromGroup(exerciseConfig.Level3Group);
    }

    private List<LightTargetReaction> GetTargetsFromGroup(
        GameObject group)
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
    // INICIAR EJERCICIO
    // =========================================================

    public void StartExercise()
    {
        if (gameRunning)
            return;

        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;

        ResetAllStats();

        currentLevel = 1;
        reactionsResolved = 0;

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        PlayStartSound();
        StartLevelWithTransition();
    }

    // =========================================================
    // TRANSICIÓN
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

        LevelStats stats = GetCurrentStats();

        stats.hits++;
        stats.AddReactionTime(reactionTime);

        Debug.Log(
            "NIVEL " + currentLevel +
            " | ACIERTO | " + reactionTime.ToString("F3") + " s"
        );

        // El target gestiona su color y sonido de éxito.
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

        LevelStats stats = GetCurrentStats();
        stats.misses++;

        if (targetSequence != null)
            targetSequence.ClearCurrentTarget(target);

        Debug.Log(
            "NIVEL " + currentLevel + " | FALLO"
        );

        if (target != null)
        {
            // El target gestiona su color y sonido de error.
            target.ShowResult(false);
        }
        else
        {
            // Si no hay target, no existe feedback que esperar.
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

        // Solo continuar si el nivel no terminó.
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

        // Bloquea nuevas reacciones y detiene la secuencia.
        gameRunning = false;
        isResolvingReaction = false;

        StopTargetSequence();

        DeactivateTargetList(activeTargets);
        DeactivateAllGroups();

        DebugLevelStats();

        if (currentLevel < 3)
        {
            currentLevel++;

            PlayStartSound();
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

        if (finalCanvas != null)
            finalCanvas.SetActive(true);

        // Mostrar el panel final del menú.
        if (menuController != null)
            menuController.ShowEndMenu();

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
    }
    // =========================================================
    // REINICIAR EJERCICIO
    // =========================================================

    public void RestartExercise()
    {
        StopTargetSequence();

        gameRunning = false;
        isResolvingReaction = false;
        reactionsResolved = 0;

        DeactivateAllGroups();
        ResetAllStats();

        currentLevel = 1;

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        PlayStartSound();
        StartLevelWithTransition();
    }

    // =========================================================
    // OBTENER ESTADÍSTICAS
    // =========================================================

    private LevelStats GetCurrentStats()
    {
        switch (currentLevel)
        {
            case 1:
                return level1Stats;

            case 2:
                return level2Stats;

            case 3:
                return level3Stats;

            default:
                return level1Stats;
        }
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
        level1Stats.ResetStats();
        level2Stats.ResetStats();
        level3Stats.ResetStats();
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
    // AUDIO DE INICIO
    // =========================================================

    private void PlayStartSound()
    {
        if (audioSource != null && startSound != null)
            audioSource.PlayOneShot(startSound);
    }

    // =========================================================
    // DEBUG ESTADÍSTICAS
    // =========================================================

    private void DebugLevelStats()
    {
        LevelStats stats = GetCurrentStats();

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
