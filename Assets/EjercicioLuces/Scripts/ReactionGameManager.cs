using System.Collections;
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

                return ((float)hits / totalAttempts) * 100f;
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

    [Header("GRUPOS DE NIVELES")]
    [SerializeField] private GameObject level1Group;
    [SerializeField] private GameObject level2Group;
    [SerializeField] private GameObject level3Group;

    [Header("TARGETS NIVEL 1")]
    [SerializeField] private List<ReactionTarget> level1Targets;

    [Header("TARGETS NIVEL 2")]
    [SerializeField] private List<ReactionTarget> level2Targets;

    [Header("TARGETS NIVEL 3")]
    [SerializeField] private List<ReactionTarget> level3Targets;

    [Header("CONFIGURACIÓN")]
    [SerializeField] private float levelDuration = 30f;
    [SerializeField] private float delayBetweenTargets = 0.5f;
    [SerializeField] private float maxReactionTime = 1.5f;

    [Header("BOTONES")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject nextLevelButton;

    [Header("CANVAS FINAL")]
    [SerializeField] private GameObject finalCanvas;

    [Header("AUDIO")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip missSound;

    [Header("VOCES")]
    [SerializeField] private AudioClip level1FinishedVoice;
    [SerializeField] private AudioClip level2FinishedVoice;
    [SerializeField] private AudioClip exerciseFinishedVoice;

    [Header("ESTADO")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private float timeRemaining = 0f;

    private ReactionTarget currentTarget;
    private ReactionTarget previousTarget;

    private List<ReactionTarget> activeTargets;

    private Coroutine nextTargetCoroutine;
    private Coroutine targetTimeoutCoroutine;

    private float targetActivationTime;

    private LevelStats level1Stats = new LevelStats();
    private LevelStats level2Stats = new LevelStats();
    private LevelStats level3Stats = new LevelStats();

    public int CurrentLevel => currentLevel;
    public bool GameRunning => gameRunning;
    public float TimeRemaining => timeRemaining;

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

            return ((float)TotalHits / totalAttempts) * 100f;
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

    private void Start()
    {
        SetupTargets();

        DeactivateAllGroups();

        if (startButton != null)
            startButton.SetActive(true);

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        ResetAllStats();
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndLevel();
        }
    }

    private void SetupTargets()
    {
        SetupTargetList(level1Targets);
        SetupTargetList(level2Targets);
        SetupTargetList(level3Targets);
    }

    private void SetupTargetList(List<ReactionTarget> list)
    {
        if (list == null)
            return;

        foreach (ReactionTarget target in list)
        {
            if (target == null)
                continue;

            target.Setup(this);
            target.Deactivate();
        }
    }

    public void StartExercise()
    {
        if (gameRunning)
            return;

        ResetAllStats();

        currentLevel = 1;

        if (startButton != null)
            startButton.SetActive(false);

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        PlaySound(startSound);

        StartLevel();
    }

    public void StartNextLevel()
    {
        if (gameRunning)
            return;

        if (currentLevel >= 3)
            return;

        currentLevel++;

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        PlaySound(startSound);

        StartLevel();
    }

    private void StartLevel()
    {
        StopAllGameCoroutines();

        DeactivateAllGroups();

        switch (currentLevel)
        {
            case 1:

                if (level1Group != null)
                    level1Group.SetActive(true);

                activeTargets = level1Targets;

                break;

            case 2:

                if (level2Group != null)
                    level2Group.SetActive(true);

                activeTargets = level2Targets;

                break;

            case 3:

                if (level3Group != null)
                    level3Group.SetActive(true);

                activeTargets = level3Targets;

                break;
        }

        DeactivateTargetList(activeTargets);

        previousTarget = null;
        currentTarget = null;

        timeRemaining = levelDuration;
        gameRunning = true;

        Debug.Log("INICIANDO NIVEL " + currentLevel);

        nextTargetCoroutine =
            StartCoroutine(ActivateNextTarget());
    }

    private IEnumerator ActivateNextTarget()
    {
        yield return new WaitForSeconds(delayBetweenTargets);

        if (!gameRunning)
            yield break;

        if (activeTargets == null || activeTargets.Count == 0)
            yield break;

        int randomIndex;

        do
        {
            randomIndex = Random.Range(0, activeTargets.Count);
        }
        while (
            activeTargets.Count > 1 &&
            activeTargets[randomIndex] == previousTarget
        );

        currentTarget = activeTargets[randomIndex];

        previousTarget = currentTarget;

        if (currentTarget != null)
        {
            currentTarget.Activate();

            targetActivationTime = Time.time;

            targetTimeoutCoroutine =
                StartCoroutine(TargetTimeout(currentTarget));
        }
    }

    private IEnumerator TargetTimeout(ReactionTarget target)
    {
        yield return new WaitForSeconds(maxReactionTime);

        if (!gameRunning)
            yield break;

        if (currentTarget != target)
            yield break;

        RegisterMiss(target);
    }

    public void TargetTouched(ReactionTarget touchedTarget)
    {
        if (!gameRunning)
            return;

        if (touchedTarget != currentTarget)
            return;

        if (targetTimeoutCoroutine != null)
        {
            StopCoroutine(targetTimeoutCoroutine);
            targetTimeoutCoroutine = null;
        }

        float reactionTime =
            Time.time - targetActivationTime;

        LevelStats stats = GetCurrentStats();

        stats.hits++;
        stats.AddReactionTime(reactionTime);

        PlaySound(hitSound);

        touchedTarget.Deactivate();

        currentTarget = null;

        Debug.Log(
            "NIVEL " + currentLevel +
            " | ACIERTO | " +
            reactionTime.ToString("F3") + " s"
        );

        if (gameRunning)
        {
            nextTargetCoroutine =
                StartCoroutine(ActivateNextTarget());
        }
    }

    private void RegisterMiss(ReactionTarget target)
    {
        LevelStats stats = GetCurrentStats();

        stats.misses++;

        PlaySound(missSound);

        if (target != null)
            target.Deactivate();

        currentTarget = null;
        targetTimeoutCoroutine = null;

        Debug.Log(
            "NIVEL " + currentLevel +
            " | FALLO"
        );

        if (gameRunning)
        {
            nextTargetCoroutine =
                StartCoroutine(ActivateNextTarget());
        }
    }

    private void EndLevel()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        StopAllGameCoroutines();

        DeactivateTargetList(activeTargets);

        currentTarget = null;

        DeactivateAllGroups();

        DebugLevelStats();

        if (currentLevel == 1)
        {
            PlaySound(level1FinishedVoice);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(true);
        }
        else if (currentLevel == 2)
        {
            PlaySound(level2FinishedVoice);

            if (nextLevelButton != null)
                nextLevelButton.SetActive(true);
        }
        else if (currentLevel == 3)
        {
            PlaySound(exerciseFinishedVoice);

            EndExercise();
        }
    }

    private void EndExercise()
    {
        gameRunning = false;

        StopAllGameCoroutines();

        DeactivateAllGroups();

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        if (finalCanvas != null)
            finalCanvas.SetActive(true);

        Debug.Log("EJERCICIO FINALIZADO");

        Debug.Log(
            "ACIERTOS TOTALES: " +
            TotalHits
        );

        Debug.Log(
            "FALLOS TOTALES: " +
            TotalMisses
        );

        Debug.Log(
            "PRECISIÓN TOTAL: " +
            TotalPrecision.ToString("F1") +
            "%"
        );

        Debug.Log(
            "PROMEDIO TOTAL: " +
            TotalAverageReactionTime.ToString("F3") +
            " s"
        );
    }

    public void RestartExercise()
    {
        StopAllGameCoroutines();

        DeactivateAllGroups();

        ResetAllStats();

        currentLevel = 1;

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        if (nextLevelButton != null)
            nextLevelButton.SetActive(false);

        PlaySound(startSound);

        StartLevel();
    }

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
        }

        return level1Stats;
    }

    private void ResetAllStats()
    {
        level1Stats.ResetStats();
        level2Stats.ResetStats();
        level3Stats.ResetStats();
    }

    private void DeactivateTargetList(List<ReactionTarget> list)
    {
        if (list == null)
            return;

        foreach (ReactionTarget target in list)
        {
            if (target != null)
                target.Deactivate();
        }
    }

    private void DeactivateAllGroups()
    {
        if (level1Group != null)
            level1Group.SetActive(false);

        if (level2Group != null)
            level2Group.SetActive(false);

        if (level3Group != null)
            level3Group.SetActive(false);
    }

    private void StopAllGameCoroutines()
    {
        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);
            nextTargetCoroutine = null;
        }

        if (targetTimeoutCoroutine != null)
        {
            StopCoroutine(targetTimeoutCoroutine);
            targetTimeoutCoroutine = null;
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void DebugLevelStats()
    {
        LevelStats stats = GetCurrentStats();

        Debug.Log(
            "NIVEL " + currentLevel +
            " TERMINADO"
        );

        Debug.Log(
            "Aciertos: " +
            stats.hits
        );

        Debug.Log(
            "Fallos: " +
            stats.misses
        );

        Debug.Log(
            "Precisión: " +
            stats.Precision.ToString("F1") +
            "%"
        );

        Debug.Log(
            "Promedio: " +
            stats.averageReactionTime.ToString("F3") +
            " s"
        );

        Debug.Log(
            "Mejor: " +
            stats.bestReactionTime.ToString("F3") +
            " s"
        );

        Debug.Log(
            "Peor: " +
            stats.worstReactionTime.ToString("F3") +
            " s"
        );

        Debug.Log(
            "Tiempo total reacción: " +
            stats.totalReactionTime.ToString("F3") +
            " s"
        );
    }
}