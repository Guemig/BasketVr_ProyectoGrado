using UnityEngine;

public class LightMenuController : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Paneles")]
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject endPanel;

    [Header("Gestor del ejercicio")]
    [SerializeField] private ReactionGameManager gameManager;

    // =====================================================
    // INICIALIZACIÓN
    // =====================================================

    private void Start()
    {
        // Mostrar el panel de inicio al entrar.
        if (startPanel != null)
            startPanel.SetActive(true);

        // Ocultar el panel final.
        if (endPanel != null)
            endPanel.SetActive(false);
    }

    // =====================================================
    // INICIAR EJERCICIO
    // =====================================================

    public void StartExercise()
    {
        // Ocultar el panel de inicio.
        if (startPanel != null)
            startPanel.SetActive(false);

        // Ocultar el panel final por seguridad.
        if (endPanel != null)
            endPanel.SetActive(false);

        // Iniciar el ejercicio de luces.
        if (gameManager != null)
        {
            gameManager.StartExercise();
        }
        else
        {
            Debug.LogError(
                "LightMenuController: falta asignar " +
                "ReactionGameManager."
            );
        }
    }

    // =====================================================
    // FINALIZAR EJERCICIO
    // =====================================================

    public void ShowEndMenu()
    {
        // Ocultar el panel de inicio.
        if (startPanel != null)
            startPanel.SetActive(false);

        // Mostrar el panel final.
        if (endPanel != null)
            endPanel.SetActive(true);
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
    }
}