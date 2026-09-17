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
        // Siempre comienza apagado
        Deactivate();
    }


    public void Setup(ReactionGameManager manager)
    {
        gameManager = manager;
    }


    public void Activate()
    {
        isActive = true;

        if (targetRenderer != null)
        {
            targetRenderer.material = onMaterial;
        }
    }


    public void Deactivate()
    {
        isActive = false;

        if (targetRenderer != null)
        {
            targetRenderer.material = offMaterial;
        }
    }


    public void Poked()
    {
        // Si está apagado,
        // ignoramos el toque
        if (!isActive)
            return;


        // Si no tiene GameManager,
        // ignoramos el toque
        if (gameManager == null)
            return;


        // Avisar al GameManager
        // cuál target fue tocado
        gameManager.TargetTouched(this);
    }
}