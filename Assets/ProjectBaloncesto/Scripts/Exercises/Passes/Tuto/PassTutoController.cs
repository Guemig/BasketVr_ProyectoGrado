
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
        [Tooltip(
            "CorrectCatch: atrapar el balón correctamente. " +
            "MissCatch: dejar pasar un balón que debía atraparse. " +
            "CorrectPass: dejar pasar correctamente un balón. " +
            "WrongCatch: atrapar por error un balón que debía dejarse pasar."
        )]
        public PassTutorialReaction.ExpectedResult expectedResult;

        [Header("Cantidad de respuestas correctas requeridas")]
        [Min(0)]
        public int requiredCorrectBalls = 1;

        [Header("Cantidad de errores requeridos")]
        [Min(0)]
        public int requiredErrors = 0;
    }

    // =====================================================
    // REFERENCIAS
    // =====================================================

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
    [SerializeField] private GameObject nextButtonObject;
    [SerializeField] private Button nextButton;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Texto progresivo")]
    [SerializeField, Min(0f)]
    private float typingDelay = 0.03f;

    // =====================================================
    // ESTADO
    // =====================================================

    private int currentStepIndex = -1;

    private bool isTyping;
    private bool demonstrationRunning;
    private bool demonstrationCompleted;

    private Coroutine typingCoroutine;
    private Coroutine continueDemoCoroutine;

    private TutorialStep CurrentStep =>
        currentStepIndex >= 0 &&
        currentStepIndex < steps.Count
            ? steps[currentStepIndex]
            : null;

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Awake()
    {
        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (nextButton != null)
        {
            nextButton.onClick.AddListener(AdvanceTutorial);
            nextButton.interactable = false;
        }

        SetNextButtonVisible(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);
    }

    // =====================================================
    // INICIAR TUTORIAL
    // =====================================================

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
        demonstrationRunning = false;
        demonstrationCompleted = false;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        BeginNextStep();
    }

    // =====================================================
    // SIGUIENTE PASO
    // =====================================================

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

        demonstrationRunning = false;
        demonstrationCompleted = false;

        if (step == null)
        {
            FinishTutorial();
            return;
        }

        SetNextButtonVisible(step.stepType == StepType.Button);

        ShowText(step.text, step.audio);

        if (step.stepType == StepType.Demonstration)
            StartAutomaticDemonstration();
    }

    // =====================================================
    // BOTÓN DE AVANCE MANUAL
    // =====================================================

    private void SetNextButtonVisible(bool visible)
    {
        if (nextButtonObject != null)
            nextButtonObject.SetActive(visible);

        if (nextButton != null)
            nextButton.interactable = visible && !isTyping;
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

    // =====================================================
    // CONVERTIR RESULTADO EN ACCIÓN
    // =====================================================

    private DoubleHandPassBall.BallAction GetBallAction(
        PassTutorialReaction.ExpectedResult result)
    {
        switch (result)
        {
            case PassTutorialReaction.ExpectedResult.CorrectCatch:
            case PassTutorialReaction.ExpectedResult.MissCatch:
                return DoubleHandPassBall.BallAction.Catch;

            case PassTutorialReaction.ExpectedResult.CorrectPass:
            case PassTutorialReaction.ExpectedResult.WrongCatch:
                return DoubleHandPassBall.BallAction.LetPass;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(result),
                    result,
                    "Resultado de tutorial no válido."
                );
        }
    }

    // =====================================================
    // INICIAR DEMOSTRACIÓN
    // =====================================================

    private void StartCurrentDemonstration()
    {
        if (tutorialReaction == null)
        {
            Debug.LogError(
                "[PASS TUTORIAL] Falta PassTutorialReaction.",
                this
            );
            return;
        }

        TutorialStep step = CurrentStep;

        if (step == null || demonstrationCompleted)
            return;

        // Cada tipo de resultado utiliza su propio contador.
        bool isErrorObjective =
            step.expectedResult == PassTutorialReaction.ExpectedResult.MissCatch ||
            step.expectedResult == PassTutorialReaction.ExpectedResult.WrongCatch;

        if (isErrorObjective && step.requiredErrors <= 0)
        {
            Debug.LogError(
                "[PASS TUTORIAL] El paso " + (currentStepIndex + 1) +
                " espera el resultado " + step.expectedResult +
                ", por lo que requiredErrors debe ser mayor que cero.",
                this
            );
            return;
        }

        if (!isErrorObjective && step.requiredCorrectBalls <= 0)
        {
            Debug.LogError(
                "[PASS TUTORIAL] El paso " + (currentStepIndex + 1) +
                " espera el resultado " + step.expectedResult +
                ", por lo que requiredCorrectBalls debe ser mayor que cero.",
                this
            );
            return;
        }

        SetNextButtonVisible(false);
        demonstrationRunning = true;

        Debug.Log(
            "[PASS TUTORIAL] Iniciando paso " +
            (currentStepIndex + 1) +
            ". Resultado esperado: " + step.expectedResult +
            ". Acción: " + GetBallAction(step.expectedResult) +
            ". Cantidad requerida: " +
            (isErrorObjective
                ? step.requiredErrors
                : step.requiredCorrectBalls),
            this
        );

        tutorialReaction.StartDemonstration(
            GetBallAction(step.expectedResult),
            step.expectedResult,
            step.requiredCorrectBalls,
            step.requiredErrors,
            OnDemonstrationCompleted
        );
    }

    // =====================================================
    // SECUENCIA COMPLETADA
    // =====================================================

    private void OnDemonstrationCompleted()
    {
        if (!demonstrationRunning)
            return;

        demonstrationRunning = false;

        TutorialStep step = CurrentStep;

        if (step == null || demonstrationCompleted)
            return;

        demonstrationCompleted = true;

        SetNextButtonVisible(false);

        Debug.Log(
            "[PASS TUTORIAL] Paso " +
            (currentStepIndex + 1) +
            " completado. Avanzando al siguiente paso.",
            this
        );

        continueDemoCoroutine =
            StartCoroutine(AdvanceAfterCompletionText());
    }

    // =====================================================
    // AVANCE AUTOMÁTICO
    // =====================================================

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

    // =====================================================
    // TEXTO Y AUDIO
    // =====================================================

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
        {
            isTyping = false;
            return;
        }

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

        TutorialStep step = CurrentStep;

        SetNextButtonVisible(
            step != null && step.stepType == StepType.Button
        );
    }

    // =====================================================
    // DETENER CORRUTINAS
    // =====================================================

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

    // =====================================================
    // FINALIZAR TUTORIAL
    // =====================================================

    private void FinishTutorial()
    {
        StopTyping();
        StopAutomaticDemonstration();

        demonstrationRunning = false;
        demonstrationCompleted = true;

        if (tutorialReaction != null)
            tutorialReaction.StopTutorialReaction();

        SetNextButtonVisible(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        Debug.Log(
            "[PASS TUTORIAL] Tutorial finalizado.",
            this
        );

        OnTutorialFinished?.Invoke();
    }

    private void OnDisable()
    {
        StopTyping();
        StopAutomaticDemonstration();

        if (tutorialReaction != null)
            tutorialReaction.StopTutorialReaction();

        demonstrationRunning = false;
    }

    private void OnDestroy()
    {
        if (nextButton != null)
            nextButton.onClick.RemoveListener(AdvanceTutorial);
    }
}
