using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LightsTutoController : MonoBehaviour
{
    public enum StepAdvanceType
    {
        Button,
        Interaction,
        Error
    }

    public enum TargetSelection
    {
        FixedTarget,
        RandomFromList
    }

    [Serializable]
    public class TutorialStep
    {
        [Header("Contenido")]
        [TextArea(2, 6)]
        public string text;

        public AudioClip audio;

        [Header("Tipo de paso")]
        public StepAdvanceType advanceType;

        [Header("Configuración de luces")]
        public TargetSelection targetSelection =
            TargetSelection.FixedTarget;

        [Tooltip("Luz individual para el modo FixedTarget.")]
        public LightTargetReaction fixedTarget;

        [Tooltip("Objeto padre que contiene las luces del paso.")]
        public GameObject targetGroup;

        [Header("Interacción")]
        [Min(1)]
        public int requiredCorrectTouches = 1;

        [Min(0.1f)]
        public float interactionTimeoutSeconds = 3f;

        [Header("Demostración de error")]
        [Min(1)]
        public int errorDemonstrations = 3;

        [Min(0.1f)]
        public float errorTimeoutSeconds = 2f;
    }

    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Pasos")]
    [SerializeField]
    private List<TutorialStep> steps = new List<TutorialStep>();

    [Header("Controlador de reacciones")]
    [SerializeField]
    private LightsTutorialReaction tutorialReaction;

    [Header("Control del indicador visual")]
    [SerializeField]
    private LightMenuController menuController;

    [Header("Indicador de direcciones del tutorial")]
    [SerializeField]
    private LightTargetDirectionIndicator tutorialDirectionIndicator;

    [Header("Interfaz")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextButtonObject;
    [SerializeField] private Button nextButton;
    [SerializeField] private GameObject tutorialCanvas;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Texto progresivo")]
    [SerializeField, Min(0f)]
    private float typingDelay = 0.03f;

    // =====================================================
    // ESTADO
    // =====================================================

    private int currentStepIndex;
    private bool isTyping;
    private bool contentFinished;
    private bool waitingForAction;

    private Coroutine typingCoroutine;
    private Coroutine contentCoroutine;

    private GameObject activeTargetGroup;
    private bool targetGroupWasActive;

    private readonly List<GameObject> activatedParents =
        new List<GameObject>();

    private TutorialStep CurrentStep =>
        currentStepIndex >= 0 && currentStepIndex < steps.Count
            ? steps[currentStepIndex]
            : null;

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Awake()
    {
        if (tutorialReaction != null)
            tutorialReaction.SetTutorialController(this);

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        SetNextButtonInteractable(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();
    }

    // =====================================================
    // CONTROL DEL INDICADOR GENERAL
    // =====================================================

    private void SetIndicatorVisible(bool visible)
    {
        if (menuController != null)
            menuController.SetIndicatorVisible(visible);
    }

    // =====================================================
    // CONTROL DEL INDICADOR DIRECCIONAL DEL TUTORIAL
    // =====================================================

    private void ShowTutorialDirectionIndicator()
    {
        if (tutorialDirectionIndicator == null)
        {
            Debug.LogWarning(
                "LightsTutoController: no se ha asignado " +
                "el indicador direccional del tutorial."
            );

            return;
        }

        tutorialDirectionIndicator.SetTutorialMode();
    }


    public void ShowTutorialTargetDirection(
        LightTargetReaction.Direction direction,
        int level)
    {
        if (tutorialDirectionIndicator == null)
        {
            Debug.LogWarning(
                "LightsTutoController: falta asignar " +
                "LightTargetDirectionIndicator."
            );
            return;
        }

        tutorialDirectionIndicator.SetTutorialMode(level);
        tutorialDirectionIndicator.ShowDirection(direction);
    }


    private void HideTutorialDirectionIndicator()
    {
        if (tutorialDirectionIndicator != null)
            tutorialDirectionIndicator.HideAllDirections();
    }

    // =====================================================
    // INICIAR TUTORIAL
    // =====================================================

    public void StartTutorial()
    {
        StopTutorialCoroutines();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        CleanupCurrentTargets();

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();

        if (steps == null || steps.Count == 0)
        {
            Debug.LogWarning(
                "LightsTutoController: no hay pasos configurados."
            );

            return;
        }

        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Tutorial
        );

        currentStepIndex = 0;
        waitingForAction = false;
        contentFinished = false;
        isTyping = false;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        BeginStep();
    }

    // =====================================================
    // INICIAR PASO
    // =====================================================

    private void BeginStep()
    {
        TutorialStep step = CurrentStep;

        if (step == null)
        {
            FinishTutorial();
            return;
        }

        StopTutorialCoroutines();

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();

        contentFinished = false;
        waitingForAction = false;
        isTyping = false;

        ConfigureNextButton(step);

        if (dialogueContainer != null)
            dialogueContainer.SetActive(true);

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (audioSource != null)
        {
            audioSource.Stop();

            if (step.audio != null)
            {
                audioSource.clip = step.audio;
                audioSource.Play();
            }
        }

        typingCoroutine = StartCoroutine(TypeText(step.text));
        contentCoroutine = StartCoroutine(WaitForContent());
    }

    // =====================================================
    // CONFIGURAR BOTÓN
    // =====================================================

    private void ConfigureNextButton(TutorialStep step)
    {
        bool showButton =
            step.advanceType == StepAdvanceType.Button;

        if (nextButtonObject != null)
            nextButtonObject.SetActive(showButton);

        SetNextButtonInteractable(false);
    }

    private void SetNextButtonInteractable(bool interactable)
    {
        if (nextButton != null)
            nextButton.interactable = interactable;
    }

    // =====================================================
    // ESPERAR AUDIO Y TEXTO
    // =====================================================

    private IEnumerator WaitForContent()
    {
        while (isTyping ||
               (audioSource != null && audioSource.isPlaying))
        {
            yield return null;
        }

        contentFinished = true;
        contentCoroutine = null;

        TutorialStep step = CurrentStep;

        if (step == null)
            yield break;

        switch (step.advanceType)
        {
            case StepAdvanceType.Button:
                SetIndicatorVisible(false);
                HideTutorialDirectionIndicator();
                SetNextButtonInteractable(true);
                break;

            case StepAdvanceType.Interaction:
                if (PrepareTargets(step))
                {
                    SetIndicatorVisible(true);
                    ShowTutorialDirectionIndicator();

                    waitingForAction = true;

                    tutorialReaction.StartInteraction(
                        step.targetSelection,
                        step.fixedTarget,
                        GetTargetsFromGroup(step),
                        step.requiredCorrectTouches,
                        step.interactionTimeoutSeconds
                    );
                }
                break;

            case StepAdvanceType.Error:
                if (PrepareTargets(step))
                {
                    SetIndicatorVisible(true);
                    ShowTutorialDirectionIndicator();

                    waitingForAction = true;

                    tutorialReaction.StartError(
                        step.targetSelection,
                        step.fixedTarget,
                        GetTargetsFromGroup(step),
                        step.errorDemonstrations,
                        step.errorTimeoutSeconds
                    );
                }
                break;
        }
    }

    // =====================================================
    // PREPARAR TARGETS DEL PASO
    // =====================================================

    private bool PrepareTargets(TutorialStep step)
    {
        if (tutorialReaction == null)
        {
            Debug.LogWarning(
                "LightsTutoController: falta LightsTutorialReaction."
            );

            return false;
        }

        CleanupCurrentTargets();

        if (step.targetSelection == TargetSelection.FixedTarget)
        {
            if (step.fixedTarget == null)
            {
                Debug.LogWarning(
                    $"Paso {currentStepIndex}: no tiene una luz asignada."
                );

                return false;
            }

            Transform current = step.fixedTarget.transform;

            while (current != null)
            {
                if (!current.gameObject.activeSelf)
                    activatedParents.Add(current.gameObject);

                current = current.parent;
            }

            for (int i = activatedParents.Count - 1; i >= 0; i--)
                activatedParents[i].SetActive(true);

            return true;
        }

        if (step.targetGroup == null)
        {
            Debug.LogWarning(
                $"Paso {currentStepIndex}: no tiene un grupo asignado."
            );

            return false;
        }

        activeTargetGroup = step.targetGroup;
        targetGroupWasActive = activeTargetGroup.activeSelf;

        activeTargetGroup.SetActive(true);

        LightTargetReaction[] targets =
            activeTargetGroup.GetComponentsInChildren<LightTargetReaction>(
                true
            );

        if (targets.Length == 0)
        {
            Debug.LogWarning(
                $"El grupo '{activeTargetGroup.name}' no contiene luces."
            );

            CleanupCurrentTargets();
            return false;
        }

        return true;
    }

    // =====================================================
    // OBTENER LUCES DEL GRUPO
    // =====================================================

    private List<LightTargetReaction> GetTargetsFromGroup(
        TutorialStep step)
    {
        List<LightTargetReaction> targets =
            new List<LightTargetReaction>();

        if (step.targetSelection != TargetSelection.RandomFromList ||
            step.targetGroup == null)
        {
            return targets;
        }

        LightTargetReaction[] childTargets =
            step.targetGroup.GetComponentsInChildren<LightTargetReaction>(
                true
            );

        targets.AddRange(childTargets);

        return targets;
    }

    // =====================================================
    // LIMPIAR TARGETS DEL PASO
    // =====================================================

    private void CleanupCurrentTargets()
    {
        if (activeTargetGroup != null)
        {
            activeTargetGroup.SetActive(targetGroupWasActive);
            activeTargetGroup = null;
        }

        for (int i = 0; i < activatedParents.Count; i++)
        {
            if (activatedParents[i] != null)
                activatedParents[i].SetActive(false);
        }

        activatedParents.Clear();
    }

    // =====================================================
    // BOTÓN SIGUIENTE
    // =====================================================

    public void OnNextButtonPressed()
    {
        if (!contentFinished || isTyping)
            return;

        if (audioSource != null && audioSource.isPlaying)
            return;

        TutorialStep step = CurrentStep;

        if (step == null ||
            step.advanceType != StepAdvanceType.Button)
        {
            return;
        }

        AdvanceStep();
    }

    // =====================================================
    // REACCIÓN TERMINADA
    // =====================================================

    public void OnReactionTutorialCompleted()
    {
        if (!contentFinished || !waitingForAction)
            return;

        waitingForAction = false;

        AdvanceStep();
    }

    // =====================================================
    // AVANZAR PASO
    // =====================================================

    private void AdvanceStep()
    {
        StopTutorialCoroutines();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        CleanupCurrentTargets();

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();

        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            FinishTutorial();
            return;
        }

        BeginStep();
    }

    // =====================================================
    // TEXTO PROGRESIVO
    // =====================================================

    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        if (dialogueText != null)
            dialogueText.text = string.Empty;

        if (string.IsNullOrEmpty(text))
        {
            isTyping = false;
            typingCoroutine = null;
            yield break;
        }

        foreach (char character in text)
        {
            if (dialogueText != null)
                dialogueText.text += character;

            if (typingDelay > 0f)
                yield return new WaitForSecondsRealtime(typingDelay);
            else
                yield return null;
        }

        isTyping = false;
        typingCoroutine = null;
    }

    // =====================================================
    // FINALIZAR TUTORIAL
    // =====================================================

    private void FinishTutorial()
    {
        StopTutorialCoroutines();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        CleanupCurrentTargets();

        if (audioSource != null)
            audioSource.Stop();

        waitingForAction = false;
        contentFinished = false;
        isTyping = false;

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        // Cambiar al modo entrenamiento y mostrar su menú.
        if (menuController != null)
        {
            menuController.ShowTrainingStartMenu();
        }
        else
        {
            // Respaldo por si no se asignó el menú.
            ReactionModeController.SetMode(
                ReactionModeController.ReactionMode.Training
            );

            Debug.LogWarning(
                "LightsTutoController: falta asignar LightMenuController."
            );
        }

        Debug.Log("Tutorial de luces finalizado.");

        Debug.Log("Tutorial de luces finalizado.");
    }

    // =====================================================
    // DETENER CORRUTINAS
    // =====================================================

    private void StopTutorialCoroutines()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (contentCoroutine != null)
        {
            StopCoroutine(contentCoroutine);
            contentCoroutine = null;
        }

        isTyping = false;
    }

    // =====================================================
    // DESACTIVAR CONTROLADOR
    // =====================================================

    private void OnDisable()
    {
        StopTutorialCoroutines();

        if (audioSource != null)
            audioSource.Stop();

        if (tutorialReaction != null)
            tutorialReaction.StopCurrentReaction();

        CleanupCurrentTargets();

        SetIndicatorVisible(false);
        HideTutorialDirectionIndicator();
    }
}