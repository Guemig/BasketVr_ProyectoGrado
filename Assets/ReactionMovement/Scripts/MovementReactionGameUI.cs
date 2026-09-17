using UnityEngine;
using TMPro;

public class MovementReactionGameUI : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    private MovementReactionGameManager gameManager;

    [Header("Texto")]
    [SerializeField]
    private TMP_Text infoText;


    private void Start()
    {
        ShowTitle();
    }


    private void Update()
    {
        if (gameManager == null || infoText == null)
            return;


        // ==========================================
        // SI TERMINÓ EL EJERCICIO
        // ==========================================

        if (gameManager.GameFinished)
        {
            ShowResults();

            return;
        }


        // ==========================================
        // ANTES Y DURANTE EL EJERCICIO
        // ==========================================

        ShowTitle();
    }


    // ==============================================
    // MOSTRAR SOLO TÍTULO
    // ==============================================

    private void ShowTitle()
    {
        infoText.text =
            "REACCIÓN EN MOVIMIENTO";
    }


    // ==============================================
    // MOSTRAR RESULTADOS
    // ==============================================

    private void ShowResults()
    {
        infoText.text =
            "REACCIÓN EN MOVIMIENTO\n\n" +

            "TIEMPO: " +
            gameManager.GameDuration.ToString("F1") +
            " s\n" +

            "ACIERTOS: " +
            gameManager.Score + "\n" +

            "REACCIÓN: " +
            gameManager.AverageReactionTime.ToString("F3") +
            " s\n" +

            "REGRESO: " +
            gameManager.AverageReturnTime.ToString("F3") +
            " s";
    }
}