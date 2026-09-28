using UnityEngine;

public class ReactionMenuController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject startButton;

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


        if (exerciseController != null)
        {
            exerciseController.StartExercise();
        }
    }
}

