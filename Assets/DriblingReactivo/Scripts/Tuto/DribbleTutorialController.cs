using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DribbleTutorialController : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private DribbleTutorialReaction tutorialReaction;
    [SerializeField] private DribblePlateManager plateManager;
    [SerializeField] private DribbleBallRespawn ballRespawn;
    [SerializeField] private DribbleIndicatorUI indicatorUI;

    [Header("Canvas")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private GameObject nextButtonObject;
    [SerializeField] private Button nextButton;

    [Header("Typing")]
    [SerializeField] private float typingDelay = 0.03f;

    [Header("Audio Dialogo")]
    [SerializeField] private AudioSource dialogueAudioSource;
    [SerializeField] private AudioClip step1Audio;
    [SerializeField] private AudioClip step2Audio;
    [SerializeField] private AudioClip step3Audio;
    [SerializeField] private AudioClip step4Audio;
    [SerializeField] private AudioClip step5Audio;
    [SerializeField] private AudioClip step6Audio;
    [SerializeField] private AudioClip step7Audio;
    [SerializeField] private AudioClip step8Audio;

    [Header("Audio Confirmacion")]
    [SerializeField] private AudioSource completionAudioSource;
    [SerializeField] private AudioClip stepCompleteSound;

    [Header("Transicion")]
    [SerializeField] private float transitionDelay = 0.5f;

    [Header("Textos")]
    [TextArea(2, 5)]
    [SerializeField] private string step1Text =
        "Bienvenido al ejercicio de Dribling.";

    [TextArea(2, 5)]
    [SerializeField] private string step2Text =
        "La idea de este ejercicio será que oprimas las placas encendidas en el piso antes de que se acabe el tiempo.";

    [TextArea(2, 5)]
    [SerializeField] private string step3Text =
        "Primero agarra el balón y suéltalo para que drible. El balón seguirá tu mano. Mantén el dribling durante 3 segundos.";

    [TextArea(2, 5)]
    [SerializeField] private string step4Text =
        "Ahora recoge el balón y dríbalo encima de la placa encendida frente a ti.";

    [TextArea(2, 5)]
    [SerializeField] private string step5Text =
        "Puedes cambiar el balón de mano. Impúlsalo hacia tu otra mano y recíbelo para volver a tener el control. Hazlo 3 veces.";

    [TextArea(2, 5)]
    [SerializeField] private string step6Text =
        "Ahora aprenderás cómo te puedes equivocar. Tocar una placa naranja te resta un punto. Toca tres placas naranjas para continuar.";

    [TextArea(2, 5)]
    [SerializeField] private string step7Text =
        "Ahora cambia de mano, toca las placas verdes y deja pasar las naranjas. Consigue tres puntos para continuar.";

    [TextArea(2, 5)]
    [SerializeField] private string step8Text =
        "¡Felicidades! Acabas de terminar el tutorial.";

    private int currentStep;
    private bool waitingForInteraction;
    private bool transitioning;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (startCanvas != null)
            startCanvas.SetActive(false);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        if (indicatorUI != null)
            indicatorUI.HideIndicator();
    }

    private void Start()
    {
        if (ReactionModeController.GetMode() ==
            ReactionModeController.ReactionMode.Tutorial)
        {
            StartTutorial();
        }
        else
        {
            if (tutorialCanvas != null)
                tutorialCanvas.SetActive(false);

            if (indicatorUI != null)
                indicatorUI.HideIndicator();

            if (startCanvas != null)
                startCanvas.SetActive(true);
        }
    }

    private void OnEnable()
    {
        if (tutorialReaction != null)
        {
            tutorialReaction.OnRequirementCompleted +=
                OnRequirementCompleted;
        }
    }

    private void OnDisable()
    {
        if (tutorialReaction != null)
        {
            tutorialReaction.OnRequirementCompleted -=
                OnRequirementCompleted;
        }

        StopTyping();
    }

    public void StartTutorial()
    {
        currentStep = 1;

        waitingForInteraction = false;
        transitioning = false;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (startCanvas != null)
            startCanvas.SetActive(false);

        if (plateManager != null)
            plateManager.StartTutorial();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        ShowCurrentStep();
    }

    public void NextStep()
    {
        if (waitingForInteraction)
            return;

        if (transitioning)
            return;

        if (currentStep == 8)
        {
            FinishTutorial();
            return;
        }

        StartCoroutine(
            ButtonStepTransition()
        );
    }

    private IEnumerator ButtonStepTransition()
    {
        transitioning = true;

        StopTyping();

        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();

        if (plateManager != null)
            plateManager.StopTutorialAction();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        yield return new WaitForSecondsRealtime(
            0.15f
        );

        currentStep++;

        transitioning = false;

        ShowCurrentStep();
    }

    private IEnumerator AutomaticStepTransition()
    {
        transitioning = true;

        StopTyping();

        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();

        PlayCompleteSound();

        if (plateManager != null)
            plateManager.StopTutorialAction();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        yield return new WaitForSecondsRealtime(
            transitionDelay
        );

        currentStep++;

        transitioning = false;

        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        if (tutorialReaction != null)
            tutorialReaction.StopRequirement();

        if (plateManager != null)
            plateManager.StopTutorialAction();

        if (indicatorUI != null &&
            currentStep != 7)
        {
            indicatorUI.HideIndicator();
        }

        waitingForInteraction = false;

        switch (currentStep)
        {
            case 1:
                ShowButtonStep(
                    step1Text,
                    step1Audio
                );
                break;

            case 2:
                ShowButtonStep(
                    step2Text,
                    step2Audio
                );
                break;

            case 3:
                ShowInteractionStep(
                    step3Text,
                    step3Audio,
                    DribbleTutorialReaction
                        .TutorialRequirement
                        .DribbleThreeSeconds
                );
                break;

            case 4:
                ShowInteractionStep(
                    step4Text,
                    step4Audio,
                    DribbleTutorialReaction
                        .TutorialRequirement
                        .FrontPlate
                );

                if (plateManager != null)
                {
                    plateManager
                        .StartFrontTutorialPlate();
                }

                break;

            case 5:
                ShowInteractionStep(
                    step5Text,
                    step5Audio,
                    DribbleTutorialReaction
                        .TutorialRequirement
                        .ThreeHandChanges
                );
                break;

            case 6:
                ShowInteractionStep(
                    step6Text,
                    step6Audio,
                    DribbleTutorialReaction
                        .TutorialRequirement
                        .ThreeOrangePlates
                );

                if (plateManager != null)
                {
                    plateManager
                        .StartOrangeTutorial();
                }

                break;

            case 7:
                if (indicatorUI != null)
                {
                    indicatorUI
                        .ShowTutorialMode();
                }

                ShowInteractionStep(
                    step7Text,
                    step7Audio,
                    DribbleTutorialReaction
                        .TutorialRequirement
                        .ThreeGreenPlates
                );

                if (plateManager != null)
                {
                    plateManager
                        .StartGreenOrangeTutorial();
                }

                break;

            case 8:
                ShowButtonStep(
                    step8Text,
                    step8Audio
                );
                break;
        }
    }

    private void ShowButtonStep(
        string text,
        AudioClip clip)
    {
        StartTyping(text);

        PlayDialogue(clip);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(true);

        if (nextButton != null)
            nextButton.interactable = true;
    }

    private void ShowInteractionStep(
        string text,
        AudioClip clip,
        DribbleTutorialReaction
            .TutorialRequirement requirement)
    {
        StartTyping(text);

        PlayDialogue(clip);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        waitingForInteraction = true;

        if (tutorialReaction != null)
        {
            tutorialReaction.StartRequirement(
                requirement
            );
        }
    }

    private void StartTyping(
        string text)
    {
        StopTyping();

        typingCoroutine =
            StartCoroutine(
                TypeText(text)
            );
    }

    private IEnumerator TypeText(
        string text)
    {
        if (dialogueText == null)
            yield break;

        dialogueText.text = "";

        foreach (char character in text)
        {
            dialogueText.text +=
                character;

            yield return
                new WaitForSecondsRealtime(
                    typingDelay
                );
        }

        typingCoroutine = null;
    }

    private void StopTyping()
    {
        if (typingCoroutine == null)
            return;

        StopCoroutine(
            typingCoroutine
        );

        typingCoroutine = null;
    }

    private void OnRequirementCompleted()
    {
        if (!waitingForInteraction)
            return;

        if (transitioning)
            return;

        waitingForInteraction = false;

        StartCoroutine(
            AutomaticStepTransition()
        );
    }

    private void PlayDialogue(
        AudioClip clip)
    {
        if (dialogueAudioSource == null)
            return;

        dialogueAudioSource.Stop();

        if (clip == null)
            return;

        dialogueAudioSource.clip = clip;
        dialogueAudioSource.Play();
    }

    private void PlayCompleteSound()
    {
        if (completionAudioSource == null)
            return;

        if (stepCompleteSound == null)
            return;

        completionAudioSource.PlayOneShot(
            stepCompleteSound
        );
    }

    private void FinishTutorial()
    {
        StopTyping();

        if (tutorialReaction != null)
            tutorialReaction.StopRequirement();

        if (plateManager != null)
            plateManager.FinishTutorial();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        if (dialogueAudioSource != null)
            dialogueAudioSource.Stop();

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (startCanvas != null)
            startCanvas.SetActive(true);
    }
}