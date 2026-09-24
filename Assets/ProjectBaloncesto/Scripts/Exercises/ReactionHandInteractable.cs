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
        // Si no se asignó en el inspector, buscar en los hijos (incluyendo inactivos)
        if (handCollider == null)
        {
            handCollider = GetComponentInChildren<Collider>(true);
            if (handCollider == null)
                Debug.LogWarning($"{name}: no se encontró ningún Collider en los hijos. Asigne 'handCollider' en el inspector si procede.");
        }

        SetNormalState();
    }


    public void StartReaction()
    {
        isReacting = true;

        SetReactionState();
    }


    public void EndReaction()
    {
        isReacting = false;

        SetNormalState();
    }


    // Este método se arrastra al evento On Hover
    public void OnHoverReaction()
    {
        if (!isReacting)
            return;

        Debug.Log(
            $"Correct reaction with {hand} hand."
        );
    }


    private void SetNormalState()
    {
        if (handRenderer != null)
            handRenderer.material = normalMaterial;

        if (handCollider != null)
            handCollider.enabled = false;
    }


    private void SetReactionState()
    {
        if (handRenderer != null)
            handRenderer.material = reactionMaterial;

        if (handCollider != null)
            handCollider.enabled = true;
    }


    public ReactionExerciseController.Hand Hand => hand;
}