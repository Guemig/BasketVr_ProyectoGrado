using System.Collections;
using UnityEngine;

public class PassReactionGameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private BallLauncher ballLauncher;
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;

    [Header("Configuración")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float delayBetweenBalls = 1f;

    [Header("Estado")]
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private float timeRemaining = 0f;

    [Header("Estadísticas")]
    [SerializeField] private int caughtBalls = 0;
    [SerializeField] private int missedBalls = 0;
    [SerializeField] private float averageReactionTime = 0f;

    private float totalReactionTime = 0f;
    private Coroutine nextBallCoroutine;

    public bool GameRunning => gameRunning;
    public float TimeRemaining => timeRemaining;
    public int CaughtBalls => caughtBalls;
    public int MissedBalls => missedBalls;
    public int TotalBalls => caughtBalls + missedBalls;
    public float AverageReactionTime => averageReactionTime;

    public float Accuracy
    {
        get
        {
            if (TotalBalls == 0)
                return 0f;

            return (float)caughtBalls / TotalBalls * 100f;
        }
    }

    private void Start()
    {
        gameRunning = false;
        timeRemaining = 0f;

        if (startButton != null)
            startButton.SetActive(true);

        if (exitButton != null)
            exitButton.SetActive(true);
    }

    private void Update()
    {
        if (!gameRunning)
            return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }
    }

    public void StartGame()
    {
        if (gameRunning)
            return;

        caughtBalls = 0;
        missedBalls = 0;

        totalReactionTime = 0f;
        averageReactionTime = 0f;

        timeRemaining = gameDuration;
        gameRunning = true;

        if (startButton != null)
            startButton.SetActive(false);

        if (exitButton != null)
            exitButton.SetActive(false);

        if (ballLauncher != null)
            ballLauncher.LaunchBall();
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

    private void EndGame()
    {
        if (!gameRunning)
            return;

        gameRunning = false;

        if (nextBallCoroutine != null)
        {
            StopCoroutine(nextBallCoroutine);
            nextBallCoroutine = null;
        }

        if (ballLauncher != null)
            ballLauncher.StopLauncher();

        if (startButton != null)
            startButton.SetActive(true);

        if (exitButton != null)
            exitButton.SetActive(true);
    }
}