using UnityEngine;

public class ReactionMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject startButton;

    [SerializeField] private GameObject endPanel;

    [SerializeField] private ReactionExerciseController exerciseController;


    // =====================================================
    // START EXERCISE
    // =====================================================

    public void StartExercise()
    {
        if (startButton != null)
        {
            startButton.SetActive(false);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (exerciseController != null)
        {
            exerciseController.StartExercise();
        }
    }


    // =====================================================
    // EXERCISE COMPLETE
    // =====================================================

    public void ShowEndMenu()
    {
        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }
    }
}