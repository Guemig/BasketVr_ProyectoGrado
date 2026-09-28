using UnityEngine;

public class ReactionHandInteractable : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField] private ReactionExerciseController.Hand hand;

    [Header("Visual")]
    [SerializeField] private Renderer handRenderer;

    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material reactionMaterial;

    [Header("Collider")]
    [SerializeField] private Collider handCollider;

    private bool isReacting;


    private void Awake()
    {
        if (handCollider == null)
        {
            handCollider = GetComponentInChildren<Collider>(true);

            if (handCollider == null)
            {
                Debug.LogWarning(
                    $"{name}: no se encontró ningún Collider en los hijos."
                );
            }
        }

        // El collider permanece activo siempre.
        if (handCollider != null)
            handCollider.enabled = true;

        SetNormalState();
    }


    public void StartReaction()
    {
        isReacting = true;

        SetReactionState();

        Debug.Log(
            $"[ReactionHandInteractable] {hand} -> ACTIVE"
        );
    }


    public void EndReaction()
    {
        isReacting = false;

        SetNormalState();

        Debug.Log(
            $"[ReactionHandInteractable] {hand} -> NORMAL"
        );
    }


    public void OnHoverReaction()
    {
        Debug.Log(
            $"[ReactionHandInteractable] Hover detected on {hand}"
        );

        // El controlador decide si es correcto,
        // incorrecto o si debe ignorarse.
        if (ReactionController == null)
            return;

        ReactionController.OnHandHover(hand);
    }


    private void SetNormalState()
    {
        if (handRenderer != null)
            handRenderer.material = normalMaterial;

        // IMPORTANTE:
        // No desactivamos el collider.
        if (handCollider != null)
            handCollider.enabled = true;
    }


    private void SetReactionState()
    {
        if (handRenderer != null)
            handRenderer.material = reactionMaterial;

        if (handCollider != null)
            handCollider.enabled = true;
    }


    public ReactionExerciseController.Hand Hand => hand;


    private ReactionExerciseController ReactionController { get; set; }


    public void SetReactionController(
        ReactionExerciseController controller)
    {
        ReactionController = controller;
    }
}