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

    [SerializeField] private ReactionTutorialController tutorialController;

    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    private bool isPaused;

    private MenuState currentState;

    // =====================================================
    // UNITY
    // =====================================================

    private void Start()
    {
        // Si estamos en Training mostramos el Start.
        // Si estamos en Tutorial lo dejamos oculto.
        bool showStart =
            ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Training;

        if (startButton != null)
        {
            startButton.SetActive(showStart);
        }

        currentState =
            showStart
                ? MenuState.Start
                : MenuState.Exercise;

        // En Training el canvas del tutorial debe estar apagado.
        if (showStart)
        {
            if (tutorialController != null)
                tutorialController.HideTutorialUI();
        }
    }

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

        // -----------------------------------------------
        // OCULTAR TUTORIAL
        // -----------------------------------------------

        if (tutorialController != null)
        {
            tutorialController.HideTutorialUI();
        }

        // -----------------------------------------------
        // MENÚS NORMALES
        // -----------------------------------------------

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
    // START BUTTON
    // =====================================================

    public void SetStartButtonActive(bool active)
    {
        if (startButton != null)
            startButton.SetActive(active);

        currentState =
            active
                ? MenuState.Start
                : MenuState.Exercise;
    }

    // =====================================================
    // SHOW START MENU
    // =====================================================

    public void ShowStartMenu()
    {
        isPaused = false;
        currentState = MenuState.Start;

        if (startButton != null)
        {
            startButton.SetActive(true);
        }

        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (endPanel != null)
        {
            endPanel.SetActive(false);
        }

        if (OptionPanel != null)
        {
            OptionPanel.SetActive(false);
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

    // =====================================================
    // TUTORIAL MENU
    // =====================================================

    public void ShowTutorialMenu()
    {
        isPaused = false;
        currentState = MenuState.Exercise;

        if (startButton != null)
            startButton.SetActive(false);

        if (pausePanel != null)
            pausePanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);

        if (OptionPanel != null)
            OptionPanel.SetActive(false);
    }
}