using System;
using System.Collections;
using UnityEngine;

public class DribbleLevel1 : MonoBehaviour
{
    [Header("Placas")]
    [SerializeField] private DribblePlate plate1;
    [SerializeField] private DribblePlate plate2;

    [Header("Sistemas")]
    [SerializeField] private DribbleTrainingStats stats;
    [SerializeField] private DribbleIndicatorUI indicatorUI;
    [SerializeField] private DribbleBallRespawn ballRespawn;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Tiempo Nivel")]
    [SerializeField] private float levelDuration = 30f;

    [Header("Tiempo Objetivos")]
    [SerializeField] private float greenTargetTime = 5f;
    [SerializeField] private float orangeTargetTime = 3f;
    [SerializeField] private float delayBetweenTargets = 0.5f;

    [Header("Probabilidad")]
    [Range(0f, 1f)]
    [SerializeField] private float greenProbability = 0.5f;

    private DribblePlate currentPlate;
    private DribblePlate lastPlate;

    private Coroutine targetCoroutine;
    private Coroutine levelCoroutine;

    private float targetStartTime;

    private bool levelActive;

    public event Action OnLevelCompleted;

    private void Awake()
    {
        Subscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (plate1 != null)
            plate1.OnBallTouched += OnPlateTouched;

        if (plate2 != null)
            plate2.OnBallTouched += OnPlateTouched;
    }

    private void Unsubscribe()
    {
        if (plate1 != null)
            plate1.OnBallTouched -= OnPlateTouched;

        if (plate2 != null)
            plate2.OnBallTouched -= OnPlateTouched;
    }

    public void StartLevel()
    {
        StopLevel();

        levelActive = true;

        if (stats != null)
            stats.level1.Reset();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (plate1 != null)
            plate1.ShowOff();

        if (plate2 != null)
            plate2.ShowOff();

        if (indicatorUI != null)
            indicatorUI.ShowLevel1Mode();

        ActivateNextTarget();

        levelCoroutine =
            StartCoroutine(
                LevelTimer()
            );
    }

    private void ActivateNextTarget()
    {
        if (!levelActive)
            return;

        SetCurrentOff();

        currentPlate =
            GetNextPlate();

        if (currentPlate == null)
            return;

        bool green =
            UnityEngine.Random.value <
            greenProbability;

        if (green)
            currentPlate.ShowGreen();
        else
            currentPlate.ShowOrange();

        lastPlate =
            currentPlate;

        targetStartTime =
            Time.realtimeSinceStartup;

        targetCoroutine =
            StartCoroutine(
                TargetTimeout()
            );
    }

    private DribblePlate GetNextPlate()
    {
        if (plate1 == null)
            return plate2;

        if (plate2 == null)
            return plate1;

        if (lastPlate == plate1)
            return plate2;

        if (lastPlate == plate2)
            return plate1;

        return UnityEngine.Random.value >= 0.5f
            ? plate1
            : plate2;
    }

    private void OnPlateTouched(
        DribblePlate plate)
    {
        if (!levelActive)
            return;

        if (plate != currentPlate)
            return;

        StopTargetCoroutine();

        DribblePlate.PlateType type =
            plate.Type;

        float reactionTime =
            Time.realtimeSinceStartup -
            targetStartTime;

        plate.ShowOff();

        currentPlate = null;

        if (type ==
            DribblePlate.PlateType.Green)
        {
            if (stats != null)
                stats.level1.AddHit(reactionTime);

            PlayCheck();
        }
        else if (type ==
                 DribblePlate.PlateType.Orange)
        {
            if (stats != null)
                stats.level1.orangeErrors++;

            PlayError();
        }

        targetCoroutine =
            StartCoroutine(
                WaitNextTarget()
            );
    }

    private IEnumerator TargetTimeout()
    {
        if (currentPlate == null)
            yield break;

        DribblePlate.PlateType initialType =
            currentPlate.Type;

        float targetDuration =
            initialType ==
            DribblePlate.PlateType.Green
            ? greenTargetTime
            : orangeTargetTime;

        yield return new WaitForSecondsRealtime(
            targetDuration
        );

        if (!levelActive)
            yield break;

        if (currentPlate != null)
        {
            if (currentPlate.Type ==
                DribblePlate.PlateType.Green)
            {
                if (stats != null)
                    stats.level1.missedGreens++;

                PlayError();
            }

            currentPlate.ShowOff();
            currentPlate = null;
        }

        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (levelActive)
            ActivateNextTarget();
    }

    private IEnumerator WaitNextTarget()
    {
        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (levelActive)
            ActivateNextTarget();
    }

    private IEnumerator LevelTimer()
    {
        yield return new WaitForSecondsRealtime(
            levelDuration
        );

        CompleteLevel();
    }

    private void CompleteLevel()
    {
        if (!levelActive)
            return;

        levelActive = false;

        StopTargetCoroutine();

        if (currentPlate != null)
            currentPlate.ShowOff();

        currentPlate = null;

        if (plate1 != null)
            plate1.ShowOff();

        if (plate2 != null)
            plate2.ShowOff();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        OnLevelCompleted?.Invoke();
    }

    public void StopLevel()
    {
        levelActive = false;

        if (levelCoroutine != null)
        {
            StopCoroutine(levelCoroutine);
            levelCoroutine = null;
        }

        StopTargetCoroutine();

        if (plate1 != null)
            plate1.ShowOff();

        if (plate2 != null)
            plate2.ShowOff();
    }

    private void SetCurrentOff()
    {
        if (currentPlate != null)
            currentPlate.ShowOff();

        currentPlate = null;
    }

    private void StopTargetCoroutine()
    {
        if (targetCoroutine == null)
            return;

        StopCoroutine(targetCoroutine);

        targetCoroutine = null;
    }

    private void PlayCheck()
    {
        if (audioSource != null &&
            checkSound != null)
        {
            audioSource.PlayOneShot(
                checkSound
            );
        }
    }

    private void PlayError()
    {
        if (audioSource != null &&
            errorSound != null)
        {
            audioSource.PlayOneShot(
                errorSound
            );
        }
    }
}