using System.Collections;
using UnityEngine;

public class DribblePlateManager : MonoBehaviour
{
    [Header("Tutorial")]
    [SerializeField] private DribbleTutorialReaction tutorialReaction;

    [Header("Placas Tutorial")]
    [SerializeField] private DribblePlate tutorialPlateLeft;
    [SerializeField] private DribblePlate tutorialPlateRight;

    [Header("Placas Entrenamiento")]
    [SerializeField] private DribblePlate trainingPlate3;
    [SerializeField] private DribblePlate trainingPlate4;
    [SerializeField] private DribblePlate trainingPlate5;

    [Header("Audio")]
    [SerializeField] private AudioSource feedbackAudioSource;
    [SerializeField] private AudioClip checkSound;
    [SerializeField] private AudioClip errorSound;

    [Header("Tiempos")]
    [SerializeField] private float greenTime = 5f;
    [SerializeField] private float orangeTime = 5f;
    [SerializeField] private float delayBetweenTargets = 0.5f;

    private DribblePlate currentPlate;
    private DribblePlate lastPlate;

    private Coroutine targetCoroutine;

    private TutorialMode currentMode;

    private enum TutorialMode
    {
        None,
        SingleGreen,
        OrangePractice,
        MixedPractice
    }

    private void Awake()
    {
        SubscribePlates();
        HideTrainingPlates();
    }

    private void OnDestroy()
    {
        UnsubscribePlates();
    }

    private void SubscribePlates()
    {
        if (tutorialPlateLeft != null)
            tutorialPlateLeft.OnBallTouched += OnPlateTouched;

        if (tutorialPlateRight != null)
            tutorialPlateRight.OnBallTouched += OnPlateTouched;
    }

    private void UnsubscribePlates()
    {
        if (tutorialPlateLeft != null)
            tutorialPlateLeft.OnBallTouched -= OnPlateTouched;

        if (tutorialPlateRight != null)
            tutorialPlateRight.OnBallTouched -= OnPlateTouched;
    }

    public void StartTutorial()
    {
        StopCurrentTarget();

        currentMode = TutorialMode.None;

        HideTrainingPlates();

        ShowTutorialPlatesOff();
    }

    public void StopTutorialAction()
    {
        StopCurrentTarget();

        currentMode = TutorialMode.None;

        ShowTutorialPlatesOff();

        HideTrainingPlates();
    }

    public void StartFrontTutorialPlate()
    {
        StopCurrentTarget();

        currentMode = TutorialMode.SingleGreen;

        ShowTutorialPlatesOff();

        if (tutorialPlateLeft == null)
            return;

        currentPlate = tutorialPlateLeft;

        currentPlate.ShowGreen();

        targetCoroutine =
            StartCoroutine(
                SingleGreenTimeout()
            );
    }

    public void StartOrangeTutorial()
    {
        StopCurrentTarget();

        currentMode =
            TutorialMode.OrangePractice;

        ShowTutorialPlatesOff();

        ActivateNextOrange();
    }

    public void StartGreenOrangeTutorial()
    {
        StopCurrentTarget();

        currentMode =
            TutorialMode.MixedPractice;

        ShowTutorialPlatesOff();

        ActivateNextMixed();
    }

    private void ActivateNextOrange()
    {
        SetCurrentPlateOff();

        currentPlate =
            GetNextTutorialPlate();

        if (currentPlate == null)
            return;

        currentPlate.ShowOrange();

        lastPlate =
            currentPlate;

        targetCoroutine =
            StartCoroutine(
                OrangeTimeout()
            );
    }

    private void ActivateNextMixed()
    {
        SetCurrentPlateOff();

        currentPlate =
            GetNextTutorialPlate();

        if (currentPlate == null)
            return;

        bool green =
            Random.value >= 0.5f;

        if (green)
        {
            currentPlate.ShowGreen();

            targetCoroutine =
                StartCoroutine(
                    GreenTimeout()
                );
        }
        else
        {
            currentPlate.ShowOrange();

            targetCoroutine =
                StartCoroutine(
                    OrangeTimeout()
                );
        }

        lastPlate =
            currentPlate;
    }

    private DribblePlate GetNextTutorialPlate()
    {
        if (tutorialPlateLeft == null &&
            tutorialPlateRight == null)
        {
            return null;
        }

        if (tutorialPlateLeft == null)
            return tutorialPlateRight;

        if (tutorialPlateRight == null)
            return tutorialPlateLeft;

        if (lastPlate ==
            tutorialPlateLeft)
        {
            return tutorialPlateRight;
        }

        if (lastPlate ==
            tutorialPlateRight)
        {
            return tutorialPlateLeft;
        }

        return Random.value >= 0.5f
            ? tutorialPlateLeft
            : tutorialPlateRight;
    }

    private void OnPlateTouched(
        DribblePlate plate)
    {
        if (plate == null)
            return;

        if (plate != currentPlate)
            return;

        StopTargetCoroutine();

        DribblePlate.PlateType type =
            plate.Type;

        if (type ==
            DribblePlate.PlateType.Green)
        {
            PlayCheck();
        }
        else if (type ==
                 DribblePlate.PlateType.Orange)
        {
            PlayError();
        }

        plate.ShowOff();

        currentPlate = null;

        if (currentMode ==
            TutorialMode.SingleGreen)
        {
            tutorialReaction
                ?.RegisterFrontPlate();

            return;
        }

        if (currentMode ==
            TutorialMode.OrangePractice)
        {
            tutorialReaction
                ?.RegisterOrangePlate();

            targetCoroutine =
                StartCoroutine(
                    WaitNextOrange()
                );

            return;
        }

        if (currentMode ==
            TutorialMode.MixedPractice)
        {
            if (type ==
                DribblePlate.PlateType.Green)
            {
                tutorialReaction
                    ?.RegisterGreenPlate();
            }

            targetCoroutine =
                StartCoroutine(
                    WaitNextMixed()
                );
        }
    }

    private IEnumerator SingleGreenTimeout()
    {
        yield return new WaitForSecondsRealtime(
            greenTime
        );

        if (currentMode !=
            TutorialMode.SingleGreen)
        {
            yield break;
        }

        if (currentPlate != null)
        {
            currentPlate.ShowOff();
            currentPlate = null;

            PlayError();
        }

        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (currentMode !=
            TutorialMode.SingleGreen)
        {
            yield break;
        }

        if (tutorialPlateLeft != null)
        {
            currentPlate =
                tutorialPlateLeft;

            currentPlate.ShowGreen();

            targetCoroutine =
                StartCoroutine(
                    SingleGreenTimeout()
                );
        }
    }

    private IEnumerator GreenTimeout()
    {
        yield return new WaitForSecondsRealtime(
            greenTime
        );

        if (currentMode !=
            TutorialMode.MixedPractice)
        {
            yield break;
        }

        if (currentPlate != null)
        {
            currentPlate.ShowOff();
            currentPlate = null;

            PlayError();
        }

        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (currentMode ==
            TutorialMode.MixedPractice)
        {
            ActivateNextMixed();
        }
    }

    private IEnumerator OrangeTimeout()
    {
        yield return new WaitForSecondsRealtime(
            orangeTime
        );

        if (currentPlate != null)
        {
            currentPlate.ShowOff();
            currentPlate = null;
        }

        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (currentMode ==
            TutorialMode.OrangePractice)
        {
            ActivateNextOrange();
        }
        else if (currentMode ==
                 TutorialMode.MixedPractice)
        {
            ActivateNextMixed();
        }
    }

    private IEnumerator WaitNextOrange()
    {
        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (currentMode ==
            TutorialMode.OrangePractice)
        {
            ActivateNextOrange();
        }
    }

    private IEnumerator WaitNextMixed()
    {
        yield return new WaitForSecondsRealtime(
            delayBetweenTargets
        );

        if (currentMode ==
            TutorialMode.MixedPractice)
        {
            ActivateNextMixed();
        }
    }

    private void SetCurrentPlateOff()
    {
        if (currentPlate != null)
            currentPlate.ShowOff();

        currentPlate = null;

        ShowTutorialPlatesOff();
    }

    private void ShowTutorialPlatesOff()
    {
        if (tutorialPlateLeft != null)
            tutorialPlateLeft.ShowOff();

        if (tutorialPlateRight != null)
            tutorialPlateRight.ShowOff();
    }

    private void HideTrainingPlates()
    {
        if (trainingPlate3 != null)
            trainingPlate3.Hide();

        if (trainingPlate4 != null)
            trainingPlate4.Hide();

        if (trainingPlate5 != null)
            trainingPlate5.Hide();
    }

    private void StopCurrentTarget()
    {
        StopTargetCoroutine();

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

    public void FinishTutorial()
    {
        StopCurrentTarget();

        currentMode =
            TutorialMode.None;

        if (tutorialPlateLeft != null)
            tutorialPlateLeft.Hide();

        if (tutorialPlateRight != null)
            tutorialPlateRight.Hide();

        HideTrainingPlates();
    }

    private void PlayCheck()
    {
        if (feedbackAudioSource == null ||
            checkSound == null)
        {
            return;
        }

        feedbackAudioSource.PlayOneShot(
            checkSound
        );
    }

    private void PlayError()
    {
        if (feedbackAudioSource == null ||
            errorSound == null)
        {
            return;
        }

        feedbackAudioSource.PlayOneShot(
            errorSound
        );
    }
}