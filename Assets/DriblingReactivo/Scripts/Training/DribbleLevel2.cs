using System;
using System.Collections;
using UnityEngine;

public class DribbleLevel2 : MonoBehaviour
{
    [Header("Placas")]
    [SerializeField] private DribblePlate plate1;
    [SerializeField] private DribblePlate plate2;
    [SerializeField] private DribblePlate plate3;
    [SerializeField] private DribblePlate plate4;

    [Header("Sistemas")]
    [SerializeField] private DribbleTrainingStats stats;
    [SerializeField] private DribbleIndicatorUI indicatorUI;
    [SerializeField] private DribbleBallRespawn ballRespawn;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip checkSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Tiempo")]
    [SerializeField] private float levelDuration = 30f;
    [SerializeField] private float targetTime = 5f;
    [SerializeField] private float delayBetweenTargets = 0.4f;

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
        SubscribePlate(plate1);
        SubscribePlate(plate2);
        SubscribePlate(plate3);
        SubscribePlate(plate4);
    }

    private void Unsubscribe()
    {
        UnsubscribePlate(plate1);
        UnsubscribePlate(plate2);
        UnsubscribePlate(plate3);
        UnsubscribePlate(plate4);
    }

    private void SubscribePlate(
        DribblePlate plate)
    {
        if (plate != null)
        {
            plate.OnBallTouched +=
                OnPlateTouched;
        }
    }

    private void UnsubscribePlate(
        DribblePlate plate)
    {
        if (plate != null)
        {
            plate.OnBallTouched -=
                OnPlateTouched;
        }
    }

    public void StartLevel()
    {
        StopLevel();

        levelActive = true;

        if (stats != null)
            stats.level2.Reset();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        SetAllOff();

        if (indicatorUI != null)
            indicatorUI.ShowLevel2Mode();

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
            GetRandomPlate();

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

    private DribblePlate GetRandomPlate()
    {
        DribblePlate[] plates =
        {
            plate1,
            plate2,
            plate3,
            plate4
        };

        for (int i = 0; i < 10; i++)
        {
            DribblePlate selected =
                plates[
                    UnityEngine.Random.Range(
                        0,
                        plates.Length
                    )
                ];

            if (selected != null &&
                selected != lastPlate)
            {
                return selected;
            }
        }

        foreach (DribblePlate plate in plates)
        {
            if (plate != null &&
                plate != lastPlate)
            {
                return plate;
            }
        }

        return null;
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
            {
                stats.level2.AddHit(
                    reactionTime
                );
            }

            PlayCheck();
        }
        else if (type ==
                 DribblePlate.PlateType.Orange)
        {
            if (stats != null)
            {
                stats.level2
                    .orangeErrors++;
            }

            PlayError();
        }

        targetCoroutine =
            StartCoroutine(
                WaitNextTarget()
            );
    }

    private IEnumerator TargetTimeout()
    {
        yield return new WaitForSecondsRealtime(
            targetTime
        );

        if (!levelActive)
            yield break;

        if (currentPlate != null)
        {
            if (currentPlate.Type ==
                DribblePlate.PlateType.Green)
            {
                if (stats != null)
                {
                    stats.level2
                        .missedGreens++;
                }

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

        SetAllOff();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        OnLevelCompleted?.Invoke();
    }

    public void StopLevel()
    {
        levelActive = false;

        if (levelCoroutine != null)
        {
            StopCoroutine(
                levelCoroutine
            );

            levelCoroutine = null;
        }

        StopTargetCoroutine();

        SetAllOff();
    }

    private void SetAllOff()
    {
        if (plate1 != null)
            plate1.ShowOff();

        if (plate2 != null)
            plate2.ShowOff();

        if (plate3 != null)
            plate3.ShowOff();

        if (plate4 != null)
            plate4.ShowOff();
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

        StopCoroutine(
            targetCoroutine
        );

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