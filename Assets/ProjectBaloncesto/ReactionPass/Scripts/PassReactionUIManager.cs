
using UnityEngine;
using UnityEngine.SceneManagement;

public class PassReactionUIManager : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;

    [Header("Gestor del ejercicio")]
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Escena de salida")]
    [SerializeField] private string exitSceneName = "MenuPrincipal";

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Start()
    {
        ShowStartMenu();
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

        if (gameManager != null)
        {
            gameManager.StartGame();
        }
        else
        {
            Debug.LogError(
                "PassReactionUIManager: falta asignar PassReactionGameManager."
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

        if (gameManager != null)
        {
            gameManager.RestartExercise();
        }
        else
        {
            Debug.LogError(
                "PassReactionUIManager: falta asignar PassReactionGameManager."
            );
        }
    }

    // =====================================================
    // SALIR DEL EJERCICIO
    // =====================================================

    public void ExitExercise()
    {
        SceneManager.LoadScene(exitSceneName);
    }
}
