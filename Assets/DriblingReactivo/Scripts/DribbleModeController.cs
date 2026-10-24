using UnityEngine;

public class DribbleModeController : MonoBehaviour
{
    public enum DribbleMode
    {
        Training,
        Tutorial
    }

    public static DribbleMode CurrentMode { get; set; } =
        DribbleMode.Training;

    public void SelectTrainingMode()
    {
        CurrentMode = DribbleMode.Training;
    }

    public void SelectTutorialMode()
    {
        CurrentMode = DribbleMode.Tutorial;
    }

    public static void SetMode(DribbleMode mode)
    {
        CurrentMode = mode;
    }

    public static DribbleMode GetMode()
    {
        return CurrentMode;
    }
}