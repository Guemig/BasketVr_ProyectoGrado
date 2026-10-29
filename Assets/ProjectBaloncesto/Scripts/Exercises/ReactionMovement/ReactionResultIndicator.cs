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

                Debug.Log(
                    $"[ReactionResultIndicator] {hand} -> Normal"
                );
                break;

            case Result.Correct:
                targetRenderer.material = correctMaterial;

                PlaySFX(SFXEvent.Correct);

                Debug.Log(
                    $"[ReactionResultIndicator] {hand} -> CORRECT"
                );
                break;

            case Result.Wrong:
                targetRenderer.material = wrongMaterial;

                PlaySFX(SFXEvent.Incorrect);

                Debug.Log(
                    $"[ReactionResultIndicator] {hand} -> WRONG"
                );
                break;
        }
    }

    private void PlaySFX(SFXEvent audioEvent)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning(
                "[ReactionResultIndicator] No se encontró AudioManager."
            );
            return;
        }

        AudioManager.Instance.PlaySFX(audioEvent);
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