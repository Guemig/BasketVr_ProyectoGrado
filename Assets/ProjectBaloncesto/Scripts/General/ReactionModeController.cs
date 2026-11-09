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


    public void SelectTrainingMode()
    {
        CurrentMode = ReactionMode.Training;
    }

    public void SelectTutorialMode()
    {
        CurrentMode = ReactionMode.Tutorial;
    }

    public static void SetMode(ReactionMode mode)
    {
        CurrentMode = mode;
    }

    public static ReactionMode GetMode() => CurrentMode;
}