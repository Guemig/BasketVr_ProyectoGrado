using UnityEngine;

public class LightTargetReaction : MonoBehaviour
{
    public enum LightState
    {
        Off,
        Inactive,
        Warning,
        Active
    }

    // =========================================================
    // VISUAL
    // =========================================================

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;

    // =========================================================
    // COLOR APAGADO
    // =========================================================

    [Header("Color Apagado")]
    [SerializeField] private Color offColor = Color.black;
    [SerializeField] private Color offEmissionColor = Color.black;

    // =========================================================
    // COLOR INACTIVO
    // =========================================================

    [Header("Color Inactivo")]
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color inactiveEmissionColor = Color.black;

    // =========================================================
    // COLOR WARNING
    // =========================================================

    [Header("Color Warning")]
    [SerializeField] private Color warningColor = Color.yellow;
    [SerializeField] private Color warningEmissionColor = Color.yellow;

    // =========================================================
    // COLOR ACTIVE
    // =========================================================

    [Header("Color Active")]
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Color activeEmissionColor = Color.green;

    // =========================================================
    // ESTADO
    // =========================================================

    [Header("Estado")]
    [SerializeField]
    private LightState currentState = LightState.Off;

    private ReactionGameManager gameManager;

    private Material targetMaterial;

    // =========================================================
    // PROPIEDADES
    // =========================================================

    public LightState CurrentState => currentState;

    public bool IsActive =>
        currentState == LightState.Active;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (targetRenderer == null)
        {
            Debug.LogWarning(
                "LightTargetReaction | " +
                gameObject.name +
                " no tiene Renderer asignado."
            );

            return;
        }

        Material[] materials =
            targetRenderer.materials;

        if (materials.Length <= 1)
        {
            Debug.LogWarning(
                "LightTargetReaction | " +
                gameObject.name +
                " necesita al menos 2 materiales."
            );

            return;
        }

        // Segundo material.
        targetMaterial = materials[1];

        ApplyOffVisual();
    }

    // =========================================================
    // SETUP
    // =========================================================

    public void Setup(
        ReactionGameManager manager)
    {
        gameManager = manager;

        Deactivate();
    }

    // =========================================================
    // WARNING
    // =========================================================

    public void Warning()
    {
        currentState =
            LightState.Warning;

        SetMaterialColor(
            warningColor,
            warningEmissionColor
        );
    }

    // =========================================================
    // ACTIVE
    // =========================================================

    public void Activate()
    {
        currentState =
            LightState.Active;

        SetMaterialColor(
            activeColor,
            activeEmissionColor
        );
    }

    // =========================================================
    // INACTIVE
    // =========================================================

    public void Deactivate()
    {
        currentState =
            LightState.Inactive;

        ApplyInactiveVisual();
    }

    // =========================================================
    // OFF
    // =========================================================

    public void TurnOff()
    {
        currentState =
            LightState.Off;

        ApplyOffVisual();
    }

    // =========================================================
    // VISUAL APAGADO
    // =========================================================

    private void ApplyOffVisual()
    {
        SetMaterialColor(
            offColor,
            offEmissionColor
        );
    }

    // =========================================================
    // VISUAL INACTIVO
    // =========================================================

    private void ApplyInactiveVisual()
    {
        SetMaterialColor(
            inactiveColor,
            inactiveEmissionColor
        );
    }

    // =========================================================
    // CAMBIAR COLOR DEL MATERIAL
    // =========================================================

    private void SetMaterialColor(
        Color baseColor,
        Color emissionColor)
    {
        if (targetMaterial == null)
            return;

        // -----------------------------------------------------
        // COLOR BASE
        // -----------------------------------------------------

        if (targetMaterial.HasProperty("_BaseColor"))
        {
            targetMaterial.SetColor(
                "_BaseColor",
                baseColor
            );
        }
        else if (targetMaterial.HasProperty("_Color"))
        {
            targetMaterial.SetColor(
                "_Color",
                baseColor
            );
        }

        // -----------------------------------------------------
        // EMISSION
        // -----------------------------------------------------

        if (targetMaterial.HasProperty("_EmissionColor"))
        {
            targetMaterial.EnableKeyword(
                "_EMISSION"
            );

            targetMaterial.SetColor(
                "_EmissionColor",
                emissionColor
            );
        }
    }

    // =========================================================
    // POKED
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