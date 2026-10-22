using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PassTutoController : MonoBehaviour
{
    public enum StepType
    {
        Button,
        Demonstration
    }

    [Serializable]
    public class TutorialStep
    {
        [Header("Contenido")]
        [TextArea(2, 6)]
        public string text;

        public AudioClip audio;

        [Header("Tipo de paso")]
        public StepType stepType;

        [Header("Demostración")]
        public DoubleHandPassBall.BallAction action;

        [Tooltip(
            "CorrectCatch: atrapar correctamente. " +
            "CorrectPass: dejar pasar correctamente."
        )]
        public PassTutorialReaction.ExpectedResult expectedResult;

        [Header("Cantidad de aciertos")]
        [Min(1)]
        public int requiredCorrectBalls = 1;

        [Tooltip(
            "Si está activado, los errores reinician " +
            "los aciertos acumulados."
        )]
        public bool resetProgressOnFailure = false;

        [TextArea(2, 4)]
        public string failureText =
            "¡Inténtalo de nuevo! Observa la indicación del balón.";
    }

    [Header("Pasos del tutorial")]
    [SerializeField]
    private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Control de demostraciones")]
    [SerializeField]
    private PassTutorialReaction tutorialReaction;

    public event Action OnTutorialFinished;

    [Header("Interfaz")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private TextMeshProUGUI nextButtonText;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Texto progresivo")]
    [SerializeField, Min(0f)]
    private float typingDelay = 0.03f;

    private int currentStepIndex = -1;
    private int successfulBalls;

    private bool isTyping;
    private bool demonstrationRunning;
    private bool demonstrationCompleted;

    private string currentButtonLabel = "Continuar";
    private bool buttonShouldBeDisabled;

    private Coroutine typingCoroutine;
    private Coroutine continueDemoCoroutine;

    private TutorialStep CurrentStep =>
        currentStepIndex >= 0 && currentStepIndex < steps.Count
            ? steps[currentStepIndex]
            : null;

    private void Awake()
    {
        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(AdvanceTutorial);
            nextButton.interactable = false;
        }

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);
    }

    public void StartTutorial()
    {
        StopTyping();
        StopAutomaticDemonstration();

        if (tutorialReaction != null)
            tutorialReaction.StopTutorialReaction();

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning(
                "[PASS TUTORIAL] No hay pasos configurados.",
                this
            );
            return;
        }

        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Tutorial
        );

        currentStepIndex = -1;
        successfulBalls = 0;

        demonstrationRunning = false;
        demonstrationCompleted = false;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        BeginNextStep();
    }

    private void BeginNextStep()
    {
        StopAutomaticDemonstration();

        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            FinishTutorial();
            return;
        }

        TutorialStep step = CurrentStep;

        successfulBalls = 0;
        demonstrationRunning = false;
        demonstrationCompleted = false;

        if (step == null)
        {
            FinishTutorial();
            return;
        }

        ShowText(step.text, step.audio);

        if (step.stepType == StepType.Button)
        {
            SetButton("Continuar", false);
        }
        else
        {
            SetButton("Continuar", true);
            StartAutomaticDemonstration();
        }
    }
    public void AdvanceTutorial()
    {
        if (isTyping || demonstrationRunning)
            return;

        TutorialStep step = CurrentStep;

        if (step == null || step.stepType != StepType.Button)
            return;

        BeginNextStep();
    }

    private void StartCurrentDemonstration()
    {
        if (tutorialReaction == null)
        {
            demonstrationRunning = false;

            Debug.LogError(
                "[PASS TUTORIAL] Falta PassTutorialReaction.",
                this
            );

            return;
        }

        TutorialStep step = CurrentStep;

        if (step == null || demonstrationCompleted)
            return;

        if (!IsConfigurationValid(step))
        {
            demonstrationRunning = false;

            Debug.LogError(
                "[PASS TUTORIAL] Acción y resultado esperado " +
                "no válidos en el paso " + (currentStepIndex + 1) + ".",
                this
            );

            return;
        }

        demonstrationRunning = true;

        tutorialReaction.StartDemonstration(
            step.action,
            step.expectedResult,
            OnDemonstrationResult
        );
    }
    private bool IsConfigurationValid(TutorialStep step)
    {
        switch (step.action)
        {
            case DoubleHandPassBall.BallAction.Catch:
                return step.expectedResult ==
                           PassTutorialReaction.ExpectedResult.CorrectCatch ||
                       step.expectedResult ==
                           PassTutorialReaction.ExpectedResult.WrongCatch;

            case DoubleHandPassBall.BallAction.LetPass:
                return step.expectedResult ==
                           PassTutorialReaction.ExpectedResult.CorrectPass ||
                       step.expectedResult ==
                           PassTutorialReaction.ExpectedResult.MissCatch;

            default:
                return false;
        }
    }

    private void OnDemonstrationResult(bool success)
    {
        demonstrationRunning = false;

        TutorialStep step = CurrentStep;

        if (step == null)
            return;

        int requiredBalls = Mathf.Max(1, step.requiredCorrectBalls);

        if (success)
        {
            successfulBalls++;

            if (successfulBalls >= requiredBalls)
            {
                successfulBalls = requiredBalls;
                demonstrationCompleted = true;

                ShowText(
                    $"¡Excelente! Has conseguido {successfulBalls} " +
                    $"de {requiredBalls} aciertos.",
                    null
                );

                StopAutomaticDemonstration();

                continueDemoCoroutine =
                    StartCoroutine(AdvanceAfterCompletionText());

                return;
            }

            int remainingBalls = requiredBalls - successfulBalls;

            ShowText(
                $"¡Correcto! Llevas {successfulBalls} de " +
                $"{requiredBalls} aciertos. Te faltan {remainingBalls}.",
                null
            );
        }
        else
        {
            if (step.resetProgressOnFailure)
                successfulBalls = 0;

            string errorMessage =
                string.IsNullOrWhiteSpace(step.failureText)
                    ? "¡Inténtalo de nuevo! Observa la indicación del balón."
                    : step.failureText;

            ShowText(
                errorMessage + "\nAciertos: " +
                successfulBalls + " de " + requiredBalls + ".",
                null
            );
        }

        StartAutomaticDemonstration();
    }

    private IEnumerator AdvanceAfterCompletionText()
    {
        while (isTyping)
            yield return null;

        continueDemoCoroutine = null;

        if (CurrentStep == null || !demonstrationCompleted)
            yield break;

        BeginNextStep();
    }
    private void StartAutomaticDemonstration()
    {
        StopAutomaticDemonstration();

        continueDemoCoroutine =
            StartCoroutine(ContinueDemonstrationAfterText());
    }

    private IEnumerator ContinueDemonstrationAfterText()
    {
        while (isTyping)
            yield return null;

        continueDemoCoroutine = null;

        if (CurrentStep == null ||
            demonstrationCompleted ||
            demonstrationRunning)
        {
            yield break;
        }

        StartCurrentDemonstration();
    }

    private void ShowText(string message, AudioClip clip)
    {
        StopTyping();

        if (audioSource != null)
        {
            audioSource.Stop();

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        if (dialogueText == null)
            return;

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;

        if (nextButton != null)
            nextButton.interactable = false;

        dialogueText.text = string.Empty;

        foreach (char character in message)
        {
            dialogueText.text += character;

            if (typingDelay > 0f)
                yield return new WaitForSecondsRealtime(typingDelay);
            else
                yield return null;
        }

        dialogueText.text = message;

        isTyping = false;
        typingCoroutine = null;

        SetButton(currentButtonLabel, buttonShouldBeDisabled);
    }

    private void SetButton(string label, bool disabled)
    {
        currentButtonLabel = label;
        buttonShouldBeDisabled = disabled;

        if (nextButtonText != null)
            nextButtonText.text = label;

        if (nextButton != null)
            nextButton.interactable = !disabled && !isTyping;
    }

    private void StopTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isTyping = false;
    }

    private void StopAutomaticDemonstration()
    {
        if (continueDemoCoroutine != null)
        {
            StopCoroutine(continueDemoCoroutine);
            continueDemoCoroutine = null;
        }
    }

    private void FinishTutorial()
    {
        StopTyping();
        StopAutomaticDemonstration();

        if (tutorialReaction != null)
            tutorialReaction.StopTutorialReaction();

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        Debug.Log("[PASS TUTORIAL] Tutorial finalizado.", this);

        OnTutorialFinished?.Invoke();
    }

    private void OnDestroy()
    {
        StopAutomaticDemonstration();

        if (nextButton != null)
            nextButton.onClick.RemoveListener(AdvanceTutorial);
    }
}