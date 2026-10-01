using UnityEngine;
using UnityEngine.InputSystem;

public class ReactionMenuController : MonoBehaviour
{
    private enum MenuState
    {
        Start,
        Exercise,
        End
    }


    [Header("References")]
    [SerializeField] private GameObject startButton;

    [SerializeField] private GameObject endPanel;

    [SerializeField] private GameObject pausePanel;

    [SerializeField] private GameObject OptionPanel;

    [SerializeField] private ReactionExerciseController exerciseController;


    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;


    private bool isPaused;

    private MenuState currentState;


    // =====================================================
    // UNITY
    // =====================================================

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }


    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }


    private void OnPausePerformed(
        InputAction.CallbackContext context)
    {
        if (isPaused)
        {
            ResumeExercise();
        }
        else
        {
            PauseExercise();
        }
    }


    // =====================================================
    // START EXERCISE
    // =====================================================

    public void StartExercise()
    {
        isPaused = false;
        currentState = MenuState.Exercise;


        if (startButton != null)
        {
            startButton.SetActive(false);
        }


        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }


        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        if (exerciseController != null)
        {
            exerciseController.StartExercise();
        }
    }


    // =====================================================
    // PAUSE / RESUME
    // =====================================================

    public void PauseExercise()
    {
        if (isPaused)
            return;


        isPaused = true;


        if (startButton != null)
        {
            startButton.SetActive(false);
        }


        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }


        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }


        if (OptionPanel != null)
        {
            OptionPanel.SetActive(false);
        }

        if (exerciseController != null)
        {
            exerciseController.PauseExercise();
        }
    }


    public void ResumeExercise()
    {
        if (!isPaused)
            return;


        isPaused = false;


        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        switch (currentState)
        {
            case MenuState.Start:

                if (startButton != null)
                {
                    startButton.SetActive(true);
                }

                break;


            case MenuState.Exercise:

                // Durante el ejercicio no mostramos
                // ningún otro menú.

                break;


            case MenuState.End:

                if (endPanel != null)
                {
                    endPanel.SetActive(true);
                }

                break;
        }


        if (exerciseController != null)
        {
            exerciseController.ResumeExercise();
        }
    }


    // =====================================================
    // EXERCISE COMPLETE
    // =====================================================

    public void ShowEndMenu()
    {
        isPaused = false;
        currentState = MenuState.End;


        if (startButton != null)
        {
            startButton.SetActive(false);
        }


        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }


        if (endPanel != null)
        {
            endPanel.SetActive(true);
        }
    }
}