
using UnityEngine;

public class PassReactionUIManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;
   // [SerializeField] private GameObject indicatorPanel;

    [Header("Gestor del ejercicio")]
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Tutorial")]
    [SerializeField] private PassTutoController tutorialController;

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Awake()
    {
        if (tutorialController != null)
        {
            tutorialController.OnTutorialFinished +=
                ShowTrainingStartMenu;
        }
    }

    private void Start()
    {
        bool isTutorialMode =
            ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Tutorial;

        if (endPanel != null)
            endPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(!isTutorialMode);

        SetIndicatorVisible(false);

        if (isTutorialMode)
        {
            if (tutorialController != null)
            {
                tutorialController.StartTutorial();
            }
            else
            {
                Debug.LogError(
                    "[PASSES] Falta asignar PassTutoController.",
                    this
                );
            }
        }
    }

    // =====================================================
    // INDICADOR VISUAL
    // =====================================================

    public void SetIndicatorVisible(bool visible)
    {
        /* if (indicatorPanel != null)
            indicatorPanel.SetActive(visible);*/
    }

    // =====================================================
    // MOSTRAR MENÚ DE ENTRENAMIENTO
    // =====================================================

    public void ShowTrainingStartMenu()
    {
        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Training
        );

        if (startPanel != null)
            startPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(false);

        Debug.Log(
            "[PASSES] Tutorial finalizado. " +
            "Menú de entrenamiento disponible."
        );
    }

    // =====================================================
    // INICIAR EJERCICIO
    // =====================================================

    public void StartExercise()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(true);

        if (gameManager != null)
        {
            gameManager.StartGame();
        }
        else
        {
            Debug.LogError(
                "PassReactionUIManager: falta asignar " +
                "PassReactionGameManager.",
                this
            );
        }
    }

    // =====================================================
    // MOSTRAR MENÚ FINAL
    // =====================================================

    public void ShowEndMenu()
    {
        if (startPanel != null)
            startPanel.SetActive(false);

        if (endPanel != null)
            endPanel.SetActive(true);

        SetIndicatorVisible(false);
    }

    // =====================================================
    // VOLVER AL MENÚ INICIAL
    // =====================================================

    public void ShowStartMenu()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(false);
    }

    // =====================================================
    // REINICIAR EJERCICIO
    // =====================================================

    public void RestartExercise()
    {
        if (endPanel != null)
            endPanel.SetActive(false);

        if (startPanel != null)
            startPanel.SetActive(false);

        SetIndicatorVisible(true);

        if (gameManager != null)
        {
            gameManager.RestartExercise();
        }
        else
        {
            Debug.LogError(
                "PassReactionUIManager: falta asignar " +
                "PassReactionGameManager.",
                this
            );
        }
    }

    private void OnDestroy()
    {
        if (tutorialController != null)
        {
            tutorialController.OnTutorialFinished -=
                ShowTrainingStartMenu;
        }
    }
}
