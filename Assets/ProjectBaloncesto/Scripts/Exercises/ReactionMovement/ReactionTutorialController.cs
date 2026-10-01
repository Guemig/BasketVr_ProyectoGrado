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

    [Serializable]
    public class TutorialStep
    {
        [TextArea(2, 6)]
        public string text;
        public AudioClip audio;

        public StepAdvanceType advanceType;

        public bool allowAnyHand;

        public ReactionExerciseController.Hand expectedHand;

        public int requiredCorrectTouches;

        public float interactionTimeoutSeconds;

        // Solo usado cuando advanceType == Error:
        // true  = el "error" que debe ocurrir es timeout (no tocar a tiempo)
        // false = el "error" que debe ocurrir es tocar la mano equivocada
        public bool errorIsTimeout;
    }

    [Header("Contenido del tutorial")]
    [SerializeField] private TutorialStep[] steps;

    [Header("Referencias")]
    [SerializeField] private ReactionMenuController menuController;
    [SerializeField] private ReactionHandInteractable leftHandInteractable;
    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [Header("UI")]
    [SerializeField] private GameObject dialogueContainer;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject nextButtonObject;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Tipowriter")]
    [SerializeField] private float typingDelay = 0.03f;

    private int currentIndex;
    private int currentCorrectCount;
    private bool isTyping;
    private Coroutine typingCoroutine;
    private Coroutine interactionTimeoutCoroutine;

    private void Awake()
    {
        if (leftHandInteractable != null) leftHandInteractable.Hovered += OnHandHovered;
        if (rightHandInteractable != null) rightHandInteractable.Hovered += OnHandHovered;

        if (dialogueText != null) dialogueText.text = string.Empty;
        if (dialogueContainer != null) dialogueContainer.SetActive(false);
        if (nextButtonObject != null) nextButtonObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (leftHandInteractable != null) leftHandInteractable.Hovered -= OnHandHovered;
        if (rightHandInteractable != null) rightHandInteractable.Hovered -= OnHandHovered;
    }

    public void StartTutorial()
    {
        StopAllCoroutines();
        currentIndex = 0;
        currentCorrectCount = 0;

        if (menuController != null) menuController.SetStartButtonActive(false);

        BeginStep(currentIndex);
    }

    private void BeginStep(int index)
    {
        if (steps == null || steps.Length == 0 || index < 0 || index >= steps.Length)
        {
            EndTutorial();
            return;
        }

        if (interactionTimeoutCoroutine != null)
        {
            StopCoroutine(interactionTimeoutCoroutine);
            interactionTimeoutCoroutine = null;
        }

        TutorialStep step = steps[index];
        currentCorrectCount = 0;

        bool hasText = !string.IsNullOrEmpty(step.text);
        if (dialogueContainer != null) dialogueContainer.SetActive(hasText);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (hasText && dialogueText != null)
        {
            typingCoroutine = StartCoroutine(TypeText(step.text));
        }
        else
        {
            isTyping = false;
            if (dialogueText != null) dialogueText.text = string.Empty;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            if (step.audio != null)
            {
                audioSource.clip = step.audio;
                audioSource.Play();
            }
        }

        if (nextButtonObject != null)
            nextButtonObject.SetActive(false);

        switch (step.advanceType)
        {
            case StepAdvanceType.Button:
                if (nextButtonObject != null) nextButtonObject.SetActive(true);
                var btn = nextButtonObject != null ? nextButtonObject.GetComponentInChildren<Button>() : null;
                if (btn != null) btn.interactable = true;
                break;

            case StepAdvanceType.Interaction:
                if (step.allowAnyHand)
                {
                    if (leftHandInteractable != null) leftHandInteractable.StartReaction();
                    if (rightHandInteractable != null) rightHandInteractable.StartReaction();
                }
                else
                {
                    GetHandInteractable(step.expectedHand)?.StartReaction();
                }

                if (step.interactionTimeoutSeconds > 0f)
                    interactionTimeoutCoroutine = StartCoroutine(WaitForInteractionTimeout(step.interactionTimeoutSeconds));
                break;

            case StepAdvanceType.Error:
                // Mostrar las manos para la demostración del error
                if (step.allowAnyHand)
                {
                    if (leftHandInteractable != null) leftHandInteractable.StartReaction();
                    if (rightHandInteractable != null) rightHandInteractable.StartReaction();
                }
                else
                {
                    GetHandInteractable(step.expectedHand)?.StartReaction();
                }

                if (step.errorIsTimeout)
                {
                    // Avanzar cuando ocurra timeout (si se configuró)
                    float timeout = step.interactionTimeoutSeconds > 0f ? step.interactionTimeoutSeconds : 1f;
                    interactionTimeoutCoroutine = StartCoroutine(WaitForInteractionTimeout(timeout));
                }
                // si errorIsTimeout == false => esperamos un toque equivocado; OnHandHovered lo detectará
                break;
        }
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
        if (dialogueContainer != null) dialogueContainer.SetActive(true);
        if (dialogueText != null) dialogueText.text = string.Empty;

        if (string.IsNullOrEmpty(fullText))
        {
            isTyping = false;
            yield break;
        }

        for (int i = 0; i < fullText.Length; i++)
        {
            if (dialogueText != null) dialogueText.text += fullText[i];
            yield return new WaitForSeconds(typingDelay);
        }

        isTyping = false;
    }

    public void OnNextButtonPressed()
    {
        if (isTyping) return;
        AdvanceStep();
    }

    private void OnHandHovered(ReactionExerciseController.Hand hand)
    {
        if (steps == null || currentIndex < 0 || currentIndex >= steps.Length) return;

        TutorialStep step = steps[currentIndex];

        if (isTyping) return;
        if (audioSource != null && audioSource.isPlaying) return;

        if (step.advanceType == StepAdvanceType.Button)
            return;

        if (step.advanceType == StepAdvanceType.Interaction)
        {
            bool validHand = step.allowAnyHand || hand == step.expectedHand;
            if (!validHand)
            {
                // tocar mano equivocada durante una interacción no cuenta: avanzar por timeout si está configurado,
                // o simplemente ignorar. No tratamos esto como el "error" demo.
                return;
            }

            currentCorrectCount++;
            int needed = step.requiredCorrectTouches <= 0 ? 1 : step.requiredCorrectTouches;

            if (currentCorrectCount >= needed)
            {
                if (step.allowAnyHand)
                    GetHandInteractable(hand)?.EndReaction();
                else
                    GetHandInteractable(step.expectedHand)?.EndReaction();

                if (interactionTimeoutCoroutine != null)
                {
                    StopCoroutine(interactionTimeoutCoroutine);
                    interactionTimeoutCoroutine = null;
                }

                AdvanceStep();
            }

            return;
        }

        if (step.advanceType == StepAdvanceType.Error)
        {
            if (step.errorIsTimeout)
            {
                // Si esperamos que el "error" sea por timeout, una interacción del jugador no es el trigger:
                // ignorar toques válidos; si toca antes de tiempo no lo contamos como éxito.
                return;
            }
            else
            {
                // Error esperado = tocar mano equivocada.
                if (step.allowAnyHand)
                {
                    // Si allowAnyHand es true no hay "equivocado", ignorar.
                    return;
                }

                // Si el jugador toca y la mano es distinta a la esperada => trigger de error
                if (hand != step.expectedHand)
                {
                    if (interactionTimeoutCoroutine != null)
                    {
                        StopCoroutine(interactionTimeoutCoroutine);
                        interactionTimeoutCoroutine = null;
                    }

                    GetHandInteractable(hand)?.EndReaction();
                    AdvanceStep();
                }
            }
        }
    }

    private IEnumerator WaitForInteractionTimeout(float timeout)
    {
        yield return new WaitUntil(() => !isTyping && (audioSource == null || !audioSource.isPlaying));
        float elapsed = 0f;
        while (elapsed < timeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        OnInteractionTimeout();
    }

    private void OnInteractionTimeout()
    {
        interactionTimeoutCoroutine = null;
        AdvanceStep();
    }

    private void AdvanceStep()
    {
        currentIndex++;

        if (currentIndex - 1 >= 0 && currentIndex - 1 < steps.Length)
        {
            var prev = steps[currentIndex - 1];
            if (prev.advanceType != StepAdvanceType.Button)
            {
                if (prev.allowAnyHand)
                {
                    if (leftHandInteractable != null) leftHandInteractable.EndReaction();
                    if (rightHandInteractable != null) rightHandInteractable.EndReaction();
                }
                else
                {
                    GetHandInteractable(prev.expectedHand)?.EndReaction();
                }
            }
        }

        if (currentIndex >= (steps?.Length ?? 0))
        {
            EndTutorial();
            return;
        }

        BeginStep(currentIndex);
    }

    private void EndTutorial()
    {
        if (leftHandInteractable != null) leftHandInteractable.EndReaction();
        if (rightHandInteractable != null) rightHandInteractable.EndReaction();

        if (menuController != null) menuController.ShowStartMenu();

        if (audioSource != null) audioSource.Stop();

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (interactionTimeoutCoroutine != null)
        {
            StopCoroutine(interactionTimeoutCoroutine);
            interactionTimeoutCoroutine = null;
        }

        if (nextButtonObject != null) nextButtonObject.SetActive(false);
        if (dialogueContainer != null) dialogueContainer.SetActive(false);
    }

    private ReactionHandInteractable GetHandInteractable(ReactionExerciseController.Hand hand)
    {
        switch (hand)
        {
            case ReactionExerciseController.Hand.Left: return leftHandInteractable;
            case ReactionExerciseController.Hand.Right: return rightHandInteractable;
            default: return null;
        }
    }
}