using UnityEngine;

public class ReactionTarget : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;

    [Header("Estado")]
    [SerializeField] private bool isActive;

    private ReactionGameManager gameManager;

    public bool IsActive => isActive;

    private void Start()
    {
        Deactivate();
    }

    public void Setup(ReactionGameManager manager)
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

        //gameManager.TargetTouched(this);
    }
}