using UnityEngine;

public class MovementReactionTarget : MonoBehaviour
{
    public enum TargetSide
    {
        Left,
        Right
    }

    [Header("Configuración")]
    [SerializeField] private TargetSide side;

    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;

    [Header("Estado")]
    [SerializeField] private bool isActive = false;

    private MovementReactionGameManager gameManager;

    public bool IsActive => isActive;
    public TargetSide Side => side;

    private void Start()
    {
        Deactivate();
    }

    public void Setup(MovementReactionGameManager manager)
    {
        gameManager = manager;
    }

    public void Activate()
    {
        isActive = true;

        if (targetRenderer != null && onMaterial != null)
        {
            targetRenderer.material = onMaterial;
        }
    }

    public void Deactivate()
    {
        isActive = false;

        if (targetRenderer != null && offMaterial != null)
        {
            targetRenderer.material = offMaterial;
        }
    }

    public void Poked()
    {
        if (!isActive)
            return;

        if (gameManager == null)
            return;

        gameManager.TargetTouched(this);
    }
}