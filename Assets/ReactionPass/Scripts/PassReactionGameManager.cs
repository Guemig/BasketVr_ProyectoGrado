using System.Collections;
using UnityEngine;
using TMPro;

public class PassReactionGameManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelStats
    {
        public int caught;
        public int missed;
        public float averageReaction;
        public float accuracy;

        public int Total => caught + missed;
    }

    [Header("Referencias")]
    [SerializeField] private BallLauncher ballLauncher;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject nextButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private TMP_Text gameStatusText;

    [Header("Configuración")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float delayBetweenBalls = 1f;

    [Header("Estado")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private bool gameRunning = false;
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

    private float totalReactionTime = 0f;
    private Coroutine nextBallCoroutine;

    public bool GameRunning => gameRunning;
    public bool ExerciseFinished => exerciseFinished;
    public int CurrentLevel => currentLevel;
    public float TimeRemaining => timeRemaining;

    public int CaughtBalls => caughtBalls;
    public int MissedBalls => missedBalls;
    public int TotalBalls => caughtBalls + missedBalls;
    public float AverageReactionTime => averageReactionTime;

    private void Start()
    {
        currentLevel = 1;
        gameRunning = false;
        exerciseFinished = false;
        timeRemaining = 0f;

        if (ballLauncher != null)
            ballLauncher.SetLevel(1);

        if (startButton != null)
            startButton.SetActive(true);

        if (nextButton != null)
            nextButton.SetActive(false);

        if (exitButton != null)
            exitButton.SetActive(true);

        ShowInitialText();
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            FinishCurrentLevel();
        }
    }

    public void StartGame()
    {
        if (gameRunning)
            return;

        currentLevel = 1;
        exerciseFinished = false;

        ResetAllStats();
        StartLevel();
    }

    public void NextLevel()
    {
        if (gameRunning)
            return;

        if (currentLevel >= 3)
            return;

        currentLevel++;

        StartLevel();
    }

    private void StartLevel()
    {
        caughtBalls = 0;
        missedBalls = 0;
        totalReactionTime = 0f;
        averageReactionTime = 0f;

        timeRemaining = gameDuration;
        gameRunning = true;

        if (gameStatusText != null)
        {
            gameStatusText.text =
                "NIVEL " + currentLevel;
        }

        if (ballLauncher != null)
        {
            ballLauncher.SetLevel(currentLevel);
            ballLauncher.LaunchBall();
        }

        if (startButton != null)
            startButton.SetActive(false);

        if (nextButton != null)
            nextButton.SetActive(false);

        if (exitButton != null)
            exitButton.SetActive(false);
    }

    public void RegisterCatch(float reactionTime)
    {
        if (!gameRunning)
            return;

        caughtBalls++;

        totalReactionTime += reactionTime;

        averageReactionTime =
            totalReactionTime / caughtBalls;

        ScheduleNextBall();
    }

    public void RegisterMiss()
    {
        if (!gameRunning)
            return;

        missedBalls++;

        ScheduleNextBall();
    }

    private void ScheduleNextBall()
    {
        if (!gameRunning)
            return;

        if (nextBallCoroutine != null)
            StopCoroutine(nextBallCoroutine);

        nextBallCoroutine =
            StartCoroutine(NextBall());
    }

    private IEnumerator NextBall()
    {
        yield return new WaitForSeconds(delayBetweenBalls);

        nextBallCoroutine = null;

        if (gameRunning && ballLauncher != null)
            ballLauncher.LaunchBall();
    }

    private void FinishCurrentLevel()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        SaveCurrentLevelStats();

        if (nextBallCoroutine != null)
        {
            StopCoroutine(nextBallCoroutine);
            nextBallCoroutine = null;
        }

        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        if (currentLevel < 3)
        {
            if (gameStatusText != null)
            {
                gameStatusText.text =
                    "NIVEL " + currentLevel +
                    " FINALIZADO\n\n" +
                    "Presiona SIGUIENTE para continuar";
            }

            if (nextButton != null)
                nextButton.SetActive(true);

            if (exitButton != null)
                exitButton.SetActive(true);
        }
        else
        {
            exerciseFinished = true;

            if (nextButton != null)
                nextButton.SetActive(false);

            if (exitButton != null)
                exitButton.SetActive(true);

            ShowFinalResults();
        }
    }

    private void SaveCurrentLevelStats()
    {
        LevelStats stats = new LevelStats();

        stats.caught = caughtBalls;
        stats.missed = missedBalls;
        stats.averageReaction = averageReactionTime;

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

        if (currentLevel == 1)
            level1Stats = stats;

        else if (currentLevel == 2)
            level2Stats = stats;

        else if (currentLevel == 3)
            level3Stats = stats;
    }

    private void ResetAllStats()
    {
        level1Stats = new LevelStats();
        level2Stats = new LevelStats();
        level3Stats = new LevelStats();
    }

    private void ShowInitialText()
    {
        if (gameStatusText == null)
            return;

        gameStatusText.text =
            "REACCIÓN A PASES\n\n" +
            "Presiona INICIAR para comenzar";
    }

    private void ShowFinalResults()
    {
        if (gameStatusText == null)
            return;

        gameStatusText.text =
            "¡EJERCICIO COMPLETADO!\n\n" +

            "NIVEL 1\n" +
            "Atrapadas: " + level1Stats.caught + "\n" +
            "Falladas: " + level1Stats.missed + "\n" +
            "Precisión: " + level1Stats.accuracy.ToString("F1") + "%\n" +
            "Reacción: " + level1Stats.averageReaction.ToString("F3") + " s\n\n" +

            "NIVEL 2\n" +
            "Atrapadas: " + level2Stats.caught + "\n" +
            "Falladas: " + level2Stats.missed + "\n" +
            "Precisión: " + level2Stats.accuracy.ToString("F1") + "%\n" +
            "Reacción: " + level2Stats.averageReaction.ToString("F3") + " s\n\n" +

            "NIVEL 3\n" +
            "Atrapadas: " + level3Stats.caught + "\n" +
            "Falladas: " + level3Stats.missed + "\n" +
            "Precisión: " + level3Stats.accuracy.ToString("F1") + "%\n" +
            "Reacción: " + level3Stats.averageReaction.ToString("F3") + " s";
    }
}