using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DribbleLevel3 : MonoBehaviour
{
    public enum RequiredHand
    {
        Left,
        Right
    }

    [Header("Placas")]
    [SerializeField] private DribblePlate plate1;
    [SerializeField] private DribblePlate plate2;
    [SerializeField] private DribblePlate plate3;
    [SerializeField] private DribblePlate plate4;

    [Header("Sistemas")]
    [SerializeField] private DribbleTrainingStats stats;
    [SerializeField] private DribbleIndicatorUI indicatorUI;
    [SerializeField] private DribbleBallRespawn ballRespawn;
    [SerializeField] private DribbleHandManager handManager;

    [Header("Indicador Mano")]
    [SerializeField] private TextMeshProUGUI handText;

    [Header("Audio Feedback")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip checkSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Audio Mano")]
    [SerializeField] private AudioSource handAudioSource;
    [SerializeField] private AudioClip leftHandSound;
    [SerializeField] private AudioClip rightHandSound;

    [Header("Tiempo Nivel")]
    [SerializeField] private float levelDuration = 30f;

    [Header("Tiempo Objetivos")]
    [SerializeField] private float greenTargetTime = 5f;
    [SerializeField] private float orangeTargetTime = 3f;
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

    private RequiredHand currentRequiredHand;

    public RequiredHand CurrentRequiredHand =>
        currentRequiredHand;

    public event Action OnLevelCompleted;

    public event Action<RequiredHand>
        OnRequiredHandChanged;

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
            plate.OnBallTouched += OnPlateTouched;
    }

    private void UnsubscribePlate(
        DribblePlate plate)
    {
        if (plate != null)
            plate.OnBallTouched -= OnPlateTouched;
    }

    public void StartLevel()
    {
        StopLevel();

        levelActive = true;

        if (stats != null)
            stats.level3.Reset();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        SetAllOff();

        if (indicatorUI != null)
            indicatorUI.ShowLevel3Mode();

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

        ChooseRequiredHand();

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

    private void ChooseRequiredHand()
    {
        currentRequiredHand =
            UnityEngine.Random.value >= 0.5f
            ? RequiredHand.Right
            : RequiredHand.Left;

        if (currentRequiredHand ==
            RequiredHand.Right)
        {
            if (handText != null)
                handText.text = "DERECHA";

            PlayRightHandAudio();
        }
        else
        {
            if (handText != null)
                handText.text = "IZQUIERDA";

            PlayLeftHandAudio();
        }

        OnRequiredHandChanged?.Invoke(
            currentRequiredHand
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
            DribblePlate.PlateType.Orange)
        {
            if (stats != null)
                stats.level3.orangeErrors++;

            PlayError();

            StartNext();

            return;
        }

        if (IsCorrectHand())
        {
            if (stats != null)
                stats.level3.AddHit(reactionTime);

            PlayCheck();
        }
        else
        {
            if (stats != null)
                stats.level3.wrongHandErrors++;

            PlayError();
        }

        StartNext();
    }

    private bool IsCorrectHand()
    {
        if (handManager == null)
            return false;

        if (!handManager.HasControl)
            return false;

        if (currentRequiredHand ==
            RequiredHand.Left)
        {
            return handManager
                .IsLeftHandControlling;
        }

        return handManager
            .IsRightHandControlling;
    }

    private void StartNext()
    {
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
                    stats.level3.missedGreens++;

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

        if (handText != null)
            handText.text = "";

        if (handAudioSource != null)
            handAudioSource.Stop();

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

        SetAllOff();

        if (handText != null)
            handText.text = "";

        if (handAudioSource != null)
            handAudioSource.Stop();
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

        StopCoroutine(targetCoroutine);

        targetCoroutine = null;
    }

    private void PlayCheck()
    {
        if (feedbackAudioSource != null &&
            checkSound != null)
        {
            feedbackAudioSource.PlayOneShot(
                checkSound
            );
        }
    }

    private void PlayError()
    {
        if (feedbackAudioSource != null &&
            errorSound != null)
        {
            feedbackAudioSource.PlayOneShot(
                errorSound
            );
        }
    }

    private void PlayLeftHandAudio()
    {
        if (handAudioSource == null ||
            leftHandSound == null)
        {
            return;
        }

        handAudioSource.Stop();

        handAudioSource.PlayOneShot(
            leftHandSound
        );
    }

    private void PlayRightHandAudio()
    {
        if (handAudioSource == null ||
            rightHandSound == null)
        {
            return;
        }

        handAudioSource.Stop();

        handAudioSource.PlayOneShot(
            rightHandSound
        );
    }
}