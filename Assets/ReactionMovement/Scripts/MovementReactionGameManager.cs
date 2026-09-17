using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementReactionGameManager : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private MovementReactionTarget leftTarget;
    [SerializeField] private MovementReactionTarget rightTarget;

    [Header("Jugador y centro")]
    [SerializeField] private Transform playerHead;
    [SerializeField] private Transform centralZone;
    [SerializeField] private float centerRadius = 0.50f;

    [Header("Configuración")]
    [SerializeField] private float gameDuration = 60f;
    [SerializeField] private float delayAfterReturning = 1f;

    [Header("Botones")]
    [SerializeField] private GameObject startButton;
    [SerializeField] private GameObject exitButton;
    [SerializeField] private float startButtonDelay = 33f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip startSound;
    [SerializeField] private AudioClip targetOnSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip centerConfirmSound;
    [SerializeField] private AudioClip finishSound;

    [Header("Estado")]
    [SerializeField] private bool gameRunning = false;
    [SerializeField] private bool gameFinished = false;
    [SerializeField] private bool waitingForCenter = false;
    [SerializeField] private bool targetActive = false;
    [SerializeField] private float timeRemaining = 0f;
    [SerializeField] private int score = 0;

    [Header("Estadísticas")]
    [SerializeField] private float averageReactionTime = 0f;
    [SerializeField] private float averageReturnTime = 0f;
    [SerializeField] private float lastReactionTime = 0f;
    [SerializeField] private float lastReturnTime = 0f;
    [SerializeField] private int leftHits = 0;
    [SerializeField] private int rightHits = 0;

    private MovementReactionTarget currentTarget;

    private float targetActivationTime;
    private float returnStartTime;

    private bool measuringReturn = false;
    private bool nextTargetScheduled = false;

    private Coroutine nextTargetCoroutine;
    private Coroutine startButtonCoroutine;

    private readonly List<float> reactionTimes = new List<float>();
    private readonly List<float> returnTimes = new List<float>();

    public bool GameRunning => gameRunning;
    public bool GameFinished => gameFinished;

    public float TimeRemaining => timeRemaining;
    public float GameDuration => gameDuration;

    public int Score => score;

    public float AverageReactionTime => averageReactionTime;
    public float AverageReturnTime => averageReturnTime;

    public float LastReactionTime => lastReactionTime;
    public float LastReturnTime => lastReturnTime;

    public int LeftHits => leftHits;
    public int RightHits => rightHits;

    private void Start()
    {
        if (leftTarget != null)
        {
            leftTarget.Setup(this);
            leftTarget.Deactivate();
        }

        if (rightTarget != null)
        {
            rightTarget.Setup(this);
            rightTarget.Deactivate();
        }

        gameRunning = false;
        gameFinished = false;
        waitingForCenter = false;
        targetActive = false;

        timeRemaining = 0f;
        score = 0;

        if (startButton != null)
        {
            startButton.SetActive(false);
            startButtonCoroutine = StartCoroutine(ShowStartButtonAfterDelay());
        }

        if (exitButton != null)
            exitButton.SetActive(true);
    }

    private IEnumerator ShowStartButtonAfterDelay()
    {
        yield return new WaitForSeconds(startButtonDelay);

        if (!gameRunning && !gameFinished && startButton != null)
            startButton.SetActive(true);

        startButtonCoroutine = null;
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
            return;
        }

        if (waitingForCenter)
            CheckCenter();
    }

    public void StartGame()
    {
        if (gameRunning)
            return;

        if (startButtonCoroutine != null)
        {
            StopCoroutine(startButtonCoroutine);
            startButtonCoroutine = null;
        }

        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);
            nextTargetCoroutine = null;
        }

        gameFinished = false;

        if (startButton != null)
            startButton.SetActive(false);

        if (exitButton != null)
            exitButton.SetActive(false);

        if (audioSource != null && startSound != null)
            audioSource.PlayOneShot(startSound);

        score = 0;
        leftHits = 0;
        rightHits = 0;

        averageReactionTime = 0f;
        averageReturnTime = 0f;

        lastReactionTime = 0f;
        lastReturnTime = 0f;

        reactionTimes.Clear();
        returnTimes.Clear();

        timeRemaining = gameDuration;

        currentTarget = null;

        waitingForCenter = false;
        targetActive = false;
        measuringReturn = false;
        nextTargetScheduled = false;

        if (leftTarget != null)
            leftTarget.Deactivate();

        if (rightTarget != null)
            rightTarget.Deactivate();

        gameRunning = true;

        if (IsPlayerInCenter())
        {
            ScheduleNextTarget();
        }
        else
        {
            waitingForCenter = true;
            measuringReturn = false;
        }
    }

    private void CheckCenter()
    {
        if (!IsPlayerInCenter())
            return;

        waitingForCenter = false;

        if (measuringReturn)
        {
            lastReturnTime = Time.time - returnStartTime;

            returnTimes.Add(lastReturnTime);
            CalculateAverageReturnTime();

            if (audioSource != null && centerConfirmSound != null)
                audioSource.PlayOneShot(centerConfirmSound);

            measuringReturn = false;
        }

        ScheduleNextTarget();
    }

    private void ScheduleNextTarget()
    {
        if (!gameRunning)
            return;

        if (nextTargetScheduled)
            return;

        if (targetActive)
            return;

        nextTargetScheduled = true;

        nextTargetCoroutine =
            StartCoroutine(ActivateNextTargetAfterDelay());
    }

    private IEnumerator ActivateNextTargetAfterDelay()
    {
        yield return new WaitForSeconds(delayAfterReturning);

        nextTargetScheduled = false;
        nextTargetCoroutine = null;

        if (!gameRunning)
            yield break;

        if (!IsPlayerInCenter())
        {
            waitingForCenter = true;
            measuringReturn = false;
            yield break;
        }

        ActivateRandomTarget();
    }

    private void ActivateRandomTarget()
    {
        if (!gameRunning || targetActive)
            return;

        int randomSide = Random.Range(0, 2);

        currentTarget =
            randomSide == 0
            ? leftTarget
            : rightTarget;

        if (currentTarget == null)
        {
            targetActive = false;
            ScheduleNextTarget();
            return;
        }

        currentTarget.Activate();
        targetActive = true;

        if (audioSource != null && targetOnSound != null)
            audioSource.PlayOneShot(targetOnSound);

        targetActivationTime = Time.time;
    }

    public void TargetTouched(MovementReactionTarget touchedTarget)
    {
        if (!gameRunning)
            return;

        if (!targetActive)
            return;

        if (touchedTarget == null)
            return;

        if (touchedTarget != currentTarget)
            return;

        lastReactionTime =
            Time.time - targetActivationTime;

        reactionTimes.Add(lastReactionTime);
        CalculateAverageReactionTime();

        score++;

        if (touchedTarget.Side ==
            MovementReactionTarget.TargetSide.Left)
        {
            leftHits++;
        }
        else
        {
            rightHits++;
        }

        if (audioSource != null && hitSound != null)
            audioSource.PlayOneShot(hitSound);

        touchedTarget.Deactivate();

        currentTarget = null;
        targetActive = false;

        returnStartTime = Time.time;

        measuringReturn = true;
        waitingForCenter = true;

        if (IsPlayerInCenter())
            CheckCenter();
    }

    private bool IsPlayerInCenter()
    {
        if (playerHead == null || centralZone == null)
            return false;

        Vector3 playerPosition = playerHead.position;
        Vector3 centerPosition = centralZone.position;

        playerPosition.y = 0f;
        centerPosition.y = 0f;

        float distance =
            Vector3.Distance(
                playerPosition,
                centerPosition
            );

        return distance <= centerRadius;
    }

    private void CalculateAverageReactionTime()
    {
        if (reactionTimes.Count == 0)
        {
            averageReactionTime = 0f;
            return;
        }

        float total = 0f;

        foreach (float value in reactionTimes)
            total += value;

        averageReactionTime =
            total / reactionTimes.Count;
    }

    private void CalculateAverageReturnTime()
    {
        if (returnTimes.Count == 0)
        {
            averageReturnTime = 0f;
            return;
        }

        float total = 0f;

        foreach (float value in returnTimes)
            total += value;

        averageReturnTime =
            total / returnTimes.Count;
    }

    private void EndGame()
    {
        if (!gameRunning)
            return;

        gameRunning = false;
        gameFinished = true;

        waitingForCenter = false;
        measuringReturn = false;
        targetActive = false;
        nextTargetScheduled = false;

        if (nextTargetCoroutine != null)
        {
            StopCoroutine(nextTargetCoroutine);
            nextTargetCoroutine = null;
        }

        if (leftTarget != null)
            leftTarget.Deactivate();

        if (rightTarget != null)
            rightTarget.Deactivate();

        currentTarget = null;

        if (audioSource != null && finishSound != null)
            audioSource.PlayOneShot(finishSound);

        if (startButton != null)
            startButton.SetActive(true);

        if (exitButton != null)
            exitButton.SetActive(true);
    }
}