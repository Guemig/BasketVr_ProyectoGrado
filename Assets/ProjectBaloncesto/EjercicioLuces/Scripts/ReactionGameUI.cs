
using TMPro;
using UnityEngine;

public class ReactionGameUI : MonoBehaviour
{
    [Header("REFERENCIAS")]
    [SerializeField] private ReactionGameManager gameManager;

    [Header("RESULTADOS NIVEL 1")]
    [SerializeField] private TMP_Text level1HitsText;
    [SerializeField] private TMP_Text level1PrecisionText;
    [SerializeField] private TMP_Text level1AverageText;

    [Header("RESULTADOS NIVEL 2")]
    [SerializeField] private TMP_Text level2HitsText;
    [SerializeField] private TMP_Text level2PrecisionText;
    [SerializeField] private TMP_Text level2AverageText;

    [Header("RESULTADOS NIVEL 3")]
    [SerializeField] private TMP_Text level3HitsText;
    [SerializeField] private TMP_Text level3PrecisionText;
    [SerializeField] private TMP_Text level3AverageText;

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    private void Start()
    {
        HideFinalResults();
    }

    // =========================================================
    // MOSTRAR RESULTADOS FINALES
    // =========================================================

    public void ShowFinalResults()
    {
        if (gameManager == null)
        {
            Debug.LogWarning(
                "ReactionGameUI: falta asignar ReactionGameManager."
            );

            return;
        }

        UpdateLevelResults(
            gameManager.Level1Stats,
            level1HitsText,
            level1PrecisionText,
            level1AverageText
        );

        UpdateLevelResults(
            gameManager.Level2Stats,
            level2HitsText,
            level2PrecisionText,
            level2AverageText
        );

        UpdateLevelResults(
            gameManager.Level3Stats,
            level3HitsText,
            level3PrecisionText,
            level3AverageText
        );

        Debug.Log("ReactionGameUI: resultados finales actualizados.");
    }

    // =========================================================
    // ACTUALIZAR RESULTADOS POR NIVEL
    // =========================================================

    private void UpdateLevelResults(
        ReactionLightGameStats.LevelStats stats,
        TMP_Text hitsText,
        TMP_Text precisionText,
        TMP_Text averageText)
    {
        if (stats == null)
        {
            Debug.LogWarning(
                "ReactionGameUI: no se encontraron estadísticas del nivel."
            );

            return;
        }
        if (hitsText != null)
        {
            hitsText.text = stats.hits + "/" + (stats.hits + stats.misses);
        }
        if (precisionText != null)
        {
            precisionText.text = stats.Precision.ToString("F1") + "%"; 
        }

        if (averageText != null)
        {
            averageText.text = stats.averageReactionTime.ToString("F3") + " s";
        }
    }

    // =========================================================
    // OCULTAR RESULTADOS
    // =========================================================

    public void HideFinalResults()
    {
        ClearLevelResults(
            level1HitsText,
            level1PrecisionText,
            level1AverageText
        );

        ClearLevelResults(
            level2HitsText,
            level2PrecisionText,
            level2AverageText
        );

        ClearLevelResults(
            level3HitsText,
            level3PrecisionText,
            level3AverageText
        );
    }

    private void ClearLevelResults(
        TMP_Text hitsText,
        TMP_Text precisionText,
        TMP_Text averageText)
    {
        if (hitsText != null)
            hitsText.text = "";

        if (precisionText != null)
            precisionText.text = "";

        if (averageText != null)
            averageText.text = "";
    }
}
