
using UnityEngine;

public class LightMenuController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;
    [SerializeField] private GameObject indicatorPanel;

    [Header("Gestor del ejercicio")]
    [SerializeField] private ReactionGameManager gameManager;

    private void Start()
    {
        // El panel de entrenamiento no debe aparecer
        // mientras se está ejecutando el tutorial.
        bool isTutorialMode =
            ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Tutorial;

        if (startPanel != null)
            startPanel.SetActive(!isTutorialMode);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(false);
    }

    // =====================================================
    // INDICADOR VISUAL
    // =====================================================

    public void SetIndicatorVisible(bool visible)
    {
        if (indicatorPanel != null)
            indicatorPanel.SetActive(visible);
    }

    // =====================================================
    // MOSTRAR MENÚ PARA INICIAR ENTRENAMIENTO
    // =====================================================

    public void ShowTrainingStartMenu()
    {
        // Cambiar al modo entrenamiento.
        ReactionModeController.SetMode(
            ReactionModeController.ReactionMode.Training
        );

        if (startPanel != null)
            startPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(false);

        Debug.Log(
            "Tutorial finalizado. Menú de entrenamiento disponible."
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
            gameManager.StartExercise();
        }
        else
        {
            Debug.LogError(
                "LightMenuController: falta asignar ReactionGameManager."
            );
        }
    }

    // =====================================================
    // FINALIZAR EJERCICIO
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
    // VOLVER AL INICIO
    // =====================================================

    public void ShowStartMenu()
    {
        if (startPanel != null)
            startPanel.SetActive(true);

        if (endPanel != null)
            endPanel.SetActive(false);

        SetIndicatorVisible(false);
    }
}
