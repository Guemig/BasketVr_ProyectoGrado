using UnityEngine;

public class ReactionModeController : MonoBehaviour
{
    public enum ReactionMode
    {
        Training,
        Tutorial
    }

    public static ReactionMode CurrentMode { get; set; }
}