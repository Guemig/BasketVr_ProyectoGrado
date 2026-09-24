using UnityEngine;

public class ReactionResultIndicator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer leftRenderer;
    [SerializeField] private Renderer rightRenderer;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material correctMaterial;
    [SerializeField] private Material wrongMaterial;


    public void SetResult(
        ReactionExerciseController.Hand hand,
        Result result)
    {
        Renderer targetRenderer = GetRenderer(hand);

        if (targetRenderer == null)
            return;

        switch (result)
        {
            case Result.Normal:
                targetRenderer.material = normalMaterial;
                break;

            case Result.Correct:
                targetRenderer.material = correctMaterial;
                break;

            case Result.Wrong:
                targetRenderer.material = wrongMaterial;
                break;
        }
    }


    private Renderer GetRenderer(
        ReactionExerciseController.Hand hand)
    {
        return hand switch
        {
            ReactionExerciseController.Hand.Left => leftRenderer,
            ReactionExerciseController.Hand.Right => rightRenderer,
            _ => null
        };
    }


    public enum Result
    {
        Normal,
        Correct,
        Wrong
    }
}