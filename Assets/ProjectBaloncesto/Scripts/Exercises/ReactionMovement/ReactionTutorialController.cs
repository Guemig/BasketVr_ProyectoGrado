using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReactionTutorialController : MonoBehaviour
{
    public enum StepAdvanceType
    {
        Button,
        Interaction,
        Error
    }

    public enum ErrorType
    {
        Timeout,
        WrongHand
    }

    [Serializable]
    public class TutorialStep
    {
        [TextArea(2, 6)]
        public string text;

        public AudioClip audio;

        public StepAdvanceType advanceType;

        [Header("Interacción")]
        public ReactionExerciseController.Hand expectedHand;
        public int requiredCorrectTouches = 1;

        [Header("Error")]
        public ErrorType errorType;
        public int errorDemonstrations = 3;
        public float interactionTimeoutSeconds = 2f;
    }

    [Header("Contenido del tutorial")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Referencias")]
    [SerializeField] private ReactionMenuController menuController;
    [SerializeField] private ReactionTutorialReaction tutorialReaction;

    [Header("UI")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextButtonObject;
    [SerializeField] private GameObject TutorialCanvas;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Typewriter")]
    [SerializeField] private float typingDelay = 0.03f;

    private int currentIndex;
    private bool isTyping;
    private bool contentFinished;

    private Coroutine typingCoroutine;
    private Coroutine stepCoroutine;

    // =====================================================
    // AWAKE
    // =====================================================

    private void Awake()
    {
        if (tutorialReaction != null)
        {
            tutorialReaction.SetTutorialController(this);
        }

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        // El canvas del tutorial comienza apagado.
        // Solo se activa cuando se inicia el tutorial.
        if (TutorialCanvas != null)
            TutorialCanvas.SetActive(false);
    }

    // =====================================================
    // TUTORIAL CANVAS
    // =====================================================

    public void SetTutorialCanvasActive(bool active)
    {
        if (TutorialCanvas != null)
            TutorialCanvas.SetActive(active);
    }

    public void HideTutorialUI()
    {
        if (TutorialCanvas != null)
            TutorialCanvas.SetActive(false);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);
    }

    // =====================================================
    // START TUTORIAL
    // =====================================================

    public void StartTutorial()
    {
        StopAllCoroutines();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        currentIndex = 0;

        if (menuController != null)
            menuController.ShowTutorialMenu();

        // Activar canvas completo del tutorial.
        if (TutorialCanvas != null)
            TutorialCanvas.SetActive(true);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        BeginStep();
    }

    // =====================================================
    // BEGIN STEP
    // =====================================================

    private void BeginStep()
    {
        if (steps == null ||
            steps.Length == 0 ||
            currentIndex >= steps.Length)
        {
            EndTutorial();
            return;
        }

        StopStepCoroutine();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        TutorialStep step = steps[currentIndex];

        contentFinished = false;

        UpdateNextButtonForStep();

        // -------------------------------------------------
        // AUDIO
        // -------------------------------------------------

        if (audioSource != null)
        {
            audioSource.Stop();

            if (step.audio != null)
            {
                audioSource.clip = step.audio;
                audioSource.Play();
            }
        }

        // -------------------------------------------------
        // TEXTO
        // -------------------------------------------------

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        if (!string.IsNullOrEmpty(step.text))
        {
            typingCoroutine =
                StartCoroutine(TypeText(step.text));
        }
        else
        {
            isTyping = false;

            if (dialogueText != null)
                dialogueText.text = string.Empty;
        }

        // -------------------------------------------------
        // ESPERAR CONTENIDO
        // -------------------------------------------------

        stepCoroutine =
            StartCoroutine(
                WaitForContent(step)
            );
    }

    // =====================================================
    // NEXT BUTTON
    // =====================================================

    private void UpdateNextButtonForStep()
    {
        if (nextButtonObject == null)
            return;

        TutorialStep step =
            steps[currentIndex];

        Button button =
            nextButtonObject.GetComponentInChildren<Button>();

        if (step.advanceType == StepAdvanceType.Button)
        {
            nextButtonObject.SetActive(true);

            if (button != null)
                button.interactable = false;
        }
        else
        {
            nextButtonObject.SetActive(false);
        }
    }

    private void EnableNextButton()
    {
        if (nextButtonObject == null)
            return;

        nextButtonObject.SetActive(true);

        Button button =
            nextButtonObject.GetComponentInChildren<Button>();

        if (button != null)
            button.interactable = true;
    }

    public void OnNextButtonPressed()
    {
        if (!contentFinished)
            return;

        if (isTyping)
            return;

        if (audioSource != null &&
            audioSource.isPlaying)
            return;

        AdvanceStep();
    }

    // =====================================================
    // WAIT FOR CONTENT
    // =====================================================

    private IEnumerator WaitForContent(
        TutorialStep step)
    {
        yield return new WaitUntil(
            () =>
                !isTyping &&
                (
                    audioSource == null ||
                    !audioSource.isPlaying
                )
        );

        contentFinished = true;

        switch (step.advanceType)
        {
            case StepAdvanceType.Button:

                EnableNextButton();

                break;

            case StepAdvanceType.Interaction:

                StartInteractionStep(step);

                break;

            case StepAdvanceType.Error:

                StartErrorStep(step);

                break;
        }

        stepCoroutine = null;
    }

    // =====================================================
    // INTERACTION
    // =====================================================

    private void StartInteractionStep(
        TutorialStep step)
    {
        if (tutorialReaction == null)
            return;

        tutorialReaction.StartInteraction(
            step.expectedHand,
            Mathf.Max(1, step.requiredCorrectTouches)
        );
    }

    // =====================================================
    // ERROR
    // =====================================================

    private void StartErrorStep(
        TutorialStep step)
    {
        if (tutorialReaction == null)
            return;

        tutorialReaction.StartError(
            step.expectedHand,
            step.errorType,
            Mathf.Max(1, step.errorDemonstrations),
            step.interactionTimeoutSeconds
        );
    }

    // =====================================================
    // REACTION FINISHED
    // =====================================================

    public void OnReactionTutorialCompleted()
    {
        if (!contentFinished)
            return;

        AdvanceStep();
    }

    // =====================================================
    // ADVANCE
    // =====================================================

    private void AdvanceStep()
    {
        if (!contentFinished)
            return;

        StopStepCoroutine();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        currentIndex++;

        if (currentIndex >= steps.Length)
        {
            EndTutorial();
            return;
        }

        BeginStep();
    }

    // =====================================================
    // TYPEWRITER
    // =====================================================

    private IEnumerator TypeText(
        string fullText)
    {
        isTyping = true;

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        for (int i = 0;
             i < fullText.Length;
             i++)
        {
            if (dialogueText != null)
                dialogueText.text += fullText[i];

            yield return new WaitForSeconds(
                typingDelay
            );
        }

        isTyping = false;
        typingCoroutine = null;
    }

    // =====================================================
    // STOP COROUTINE
    // =====================================================

    private void StopStepCoroutine()
    {
        if (stepCoroutine != null)
        {
            StopCoroutine(stepCoroutine);
            stepCoroutine = null;
        }
    }

    // =====================================================
    // END
    // =====================================================

    private void EndTutorial()
    {
        StopStepCoroutine();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        if (audioSource != null)
            audioSource.Stop();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        // Al terminar el tutorial se apaga todo el canvas.
        if (TutorialCanvas != null)
            TutorialCanvas.SetActive(false);

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(false);

        if (menuController != null)
            menuController.ShowStartMenu();
    }
}