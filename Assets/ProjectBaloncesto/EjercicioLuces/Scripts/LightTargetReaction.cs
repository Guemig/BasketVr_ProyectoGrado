
using System;
using System.Collections;
using UnityEngine;

public class LightTargetReaction : MonoBehaviour
{
    public enum Direction
    {
        Frente,
        FrenteDerecha,
        Derecha,
        AtrasDerecha,
        Atras,
        AtrasIzquierda,
        Izquierda,
        FrenteIzquierda
    }

    public enum LightState
    {
        Off,
        Inactive,
        Warning,
        Active,
        Success,
        Error
    }

    // =========================================================
    // DIRECCIÓN
    // =========================================================

    [Header("Dirección del target")]
    [SerializeField] private Direction direction = Direction.Frente;

    public Direction TargetDirection => direction;

    // =========================================================
    // VISUAL
    // =========================================================

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;

    [Header("Color Apagado")]
    [SerializeField] private Color offColor = Color.black;
    [SerializeField] private Color offEmissionColor = Color.black;

    [Header("Color Inactivo")]
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color inactiveEmissionColor = Color.black;

    [Header("Color Warning")]
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color warningEmissionColor = Color.yellow;

    [Header("Color Active")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color activeEmissionColor = Color.green;

    [Header("Color Éxito")]
    [SerializeField] private Color successColor = Color.cyan;
    [SerializeField] private Color successEmissionColor = Color.cyan;

    [Header("Color Error")]
    [SerializeField] private Color errorColor = Color.red;
    [SerializeField] private Color errorEmissionColor = Color.red;

    // =========================================================
    // AUDIO INDIVIDUAL
    // =========================================================

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip successSound;
    [SerializeField] private AudioClip errorSound;

    // =========================================================
    // FEEDBACK
    // =========================================================

    [Header("Duración del feedback")]
    [SerializeField, Min(0f)]
    private float feedbackDuration = 0.5f;

    // =========================================================
    // ESTADO
    // =========================================================

    [Header("Estado")]
    [SerializeField] private LightState currentState = LightState.Off;

    private ReactionGameManager gameManager;
    private Material targetMaterial;
    private Coroutine feedbackCoroutine;

    public LightState CurrentState => currentState;
    public bool IsActive => currentState == LightState.Active;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning(
                "LightTargetReaction | " + gameObject.name +
                " no tiene Renderer asignado."
            );

            return;
        }

        Material[] materials = targetRenderer.materials;

        if (materials.Length <= 1)
        {
            Debug.LogWarning(
                "LightTargetReaction | " + gameObject.name +
                " necesita al menos 2 materiales."
            );

            return;
        }

        // Se utiliza el segundo material.
        targetMaterial = materials[1];

        ApplyOffVisual();
    }

    // =========================================================
    // CONFIGURACIÓN
    // =========================================================

    public void Setup(ReactionGameManager manager)
    {
        gameManager = manager;
        Deactivate();
    }

    // =========================================================
    // WARNING
    // =========================================================

    public void Warning()
    {
        currentState = LightState.Warning;

        SetMaterialColor(
            warningColor,
            warningEmissionColor
        );
    }

    // =========================================================
    // ACTIVAR
    // =========================================================

    public void Activate()
    {
        currentState = LightState.Active;

        SetMaterialColor(
            activeColor,
            activeEmissionColor
        );
    }

    // =========================================================
    // MOSTRAR ÉXITO O ERROR
    // =========================================================

    public void ShowResult(bool success)
    {
        if (feedbackCoroutine != null)
            StopCoroutine(feedbackCoroutine);

        feedbackCoroutine = StartCoroutine(
            ShowResultCoroutine(success)
        );
    }

    private IEnumerator ShowResultCoroutine(bool success)
    {
        if (success)
        {
            currentState = LightState.Success;

            SetMaterialColor(
                successColor,
                successEmissionColor
            );

            PlaySound(successSound);
        }
        else
        {
            currentState = LightState.Error;

            SetMaterialColor(
                errorColor,
                errorEmissionColor
            );

            PlaySound(errorSound);
        }

        yield return new WaitForSecondsRealtime(feedbackDuration);

        feedbackCoroutine = null;

        Deactivate();

        if (gameManager != null)
            gameManager.TargetFeedbackFinished(this);
    }

    // =========================================================
    // INACTIVAR
    // =========================================================

    public void Deactivate()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        currentState = LightState.Inactive;
        ApplyInactiveVisual();
    }

    // =========================================================
    // APAGAR
    // =========================================================

    public void TurnOff()
    {
        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
            feedbackCoroutine = null;
        }

        currentState = LightState.Off;
        ApplyOffVisual();
    }

    // =========================================================
    // VISUALES
    // =========================================================

    private void ApplyOffVisual()
    {
        SetMaterialColor(offColor, offEmissionColor);
    }

    private void ApplyInactiveVisual()
    {
        SetMaterialColor(inactiveColor, inactiveEmissionColor);
    }

    private void SetMaterialColor(
        Color baseColor,
        Color emissionColor)
    {
        if (targetMaterial == null)
            return;

        if (targetMaterial.HasProperty("_BaseColor"))
        {
            targetMaterial.SetColor("_BaseColor", baseColor);
        }
        else if (targetMaterial.HasProperty("_Color"))
        {
            targetMaterial.SetColor("_Color", baseColor);
        }

        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            targetMaterial.EnableKeyword("_EMISSION");

            targetMaterial.SetColor(
                "_EmissionColor",
                emissionColor
            );
        }
    }

    // =========================================================
    // AUDIO
    // =========================================================

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }

    // =========================================================
    // INTERACCIÓN
    // =========================================================

    public void Poked()
    {
        if (currentState != LightState.Active)
            return;

        if (gameManager == null)
            return;

        gameManager.TargetTouched(this);
    }
}
