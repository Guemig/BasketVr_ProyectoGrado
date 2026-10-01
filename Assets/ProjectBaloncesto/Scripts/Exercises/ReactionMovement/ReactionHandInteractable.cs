using System;
using UnityEngine;

public class ReactionHandInteractable : MonoBehaviour
{
    [Header("Hand")]
    [SerializeField]
    private ReactionExerciseController.Hand hand;


    [Header("Visual")]
    [SerializeField]
    private Renderer handRenderer;

    [SerializeField]
    private Material normalMaterial;

    [SerializeField]
    private Material reactionMaterial;


    [Header("Collider")]
    [SerializeField]
    private Collider handCollider;


    private ReactionInteraction reactionInteraction;

    // Nuevo: evento que notifica que el jugador ha hecho hover/interacción en esta mano.
    public event Action<ReactionExerciseController.Hand> Hovered;


    private void Awake()
    {
        if (handCollider == null)
        {
            handCollider =
                GetComponentInChildren<Collider>(true);

            if (handCollider == null)
            {
                Debug.LogWarning(
                    $"{name}: no se encontró ningún Collider en los hijos."
                );
            }
        }


        if (handCollider != null)
            handCollider.enabled = true;


        SetNormalState();
    }


    public void StartReaction()
    {
        SetReactionState();

        Debug.Log(
            $"[ReactionHandInteractable] {hand} -> ACTIVE"
        );
    }


    public void EndReaction()
    {
        SetNormalState();

        Debug.Log(
            $"[ReactionHandInteractable] {hand} -> NORMAL"
        );
    }


    public void OnHoverReaction()
    {
        Debug.Log(
            $"[ReactionHandInteractable] " +
            $"Hover -> {hand}"
        );


        if (reactionInteraction == null)
        {
            Debug.LogWarning(
                $"{name}: ReactionInteraction no asignado."
            );

            return;
        }


        reactionInteraction.OnHandHover(
            hand
        );

        Hovered?.Invoke(hand);
    }


    private void SetNormalState()
    {
        if (handRenderer != null)
            handRenderer.material = normalMaterial;


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


    public ReactionExerciseController.Hand Hand =>
        hand;


    public void SetReactionInteraction(
        ReactionInteraction interaction)
    {
        reactionInteraction = interaction;
    }
}