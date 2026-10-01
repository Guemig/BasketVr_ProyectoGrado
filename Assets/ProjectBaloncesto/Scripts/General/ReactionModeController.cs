using UnityEngine;

public class ReactionModeController : MonoBehaviour
{
    public enum ReactionMode
    {
        Training,
        Tutorial
    }

    // Valor por defecto: Training
    public static ReactionMode CurrentMode { get; set; } = ReactionMode.Training;

    private void Awake()
    {
        Debug.Log($"[ReactionMode] CurrentMode = {CurrentMode}");
    }

    public void SelectTrainingMode()
    {
        CurrentMode = ReactionMode.Training;
        Debug.Log("[ReactionMode] Selected: Training");
    }

    public void SelectTutorialMode()
    {
        CurrentMode = ReactionMode.Tutorial;
        Debug.Log("[ReactionMode] Selected: Tutorial");
    }

    public static void SetMode(ReactionMode mode)
    {
        CurrentMode = mode;
        Debug.Log($"[ReactionMode] SetMode: {mode}");
    }

    public static ReactionMode GetMode() => CurrentMode;
}