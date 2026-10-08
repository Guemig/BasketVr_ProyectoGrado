using TMPro;
using UnityEngine;

public class ReactionGameUI : MonoBehaviour
{
    [Header("GAME MANAGER")]
    [SerializeField] private ReactionGameManager gameManager;


    [Header("UI DURANTE EL EJERCICIO")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text timeText;


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


    private void Update()
    {
        if (gameManager == null)
            return;


        UpdateGameUI();
        UpdateFinalResults();
    }


    // =========================================
    // UI DURANTE EL EJERCICIO
    // =========================================

    private void UpdateGameUI()
    {
        if (levelText != null)
        {
            if (gameManager.CurrentLevel > 0)
            {
                levelText.text =
                    "NIVEL " +
                    gameManager.CurrentLevel;
            }
            else
            {
                levelText.text = "";
            }
        }


        if (timeText != null)
        {
            if (gameManager.GameRunning)
            {
                timeText.text =
                    "TIEMPO: " +
                    gameManager.TimeRemaining.ToString("F1") +
                    " s";
            }
            else
            {
                timeText.text = "";
            }
        }
    }


    // =========================================
    // CANVAS FINAL
    // =========================================

    private void UpdateFinalResults()
    {
        UpdateLevel1Results();
        UpdateLevel2Results();
        UpdateLevel3Results();
    }


    // =========================================
    // NIVEL 1
    // =========================================

    private void UpdateLevel1Results()
    {
        ReactionGameManager.LevelStats stats =
            gameManager.Level1Stats;


        if (level1HitsText != null)
        {
            level1HitsText.text =
                "ACIERTOS: " +
                stats.hits;
        }


        if (level1PrecisionText != null)
        {
            level1PrecisionText.text =
                "PRECISIÓN: " +
                stats.Precision.ToString("F1") +
                "%";
        }


        if (level1AverageText != null)
        {
            level1AverageText.text =
                "TIEMPO PROMEDIO: " +
                stats.averageReactionTime.ToString("F3") +
                " s";
        }
    }


    // =========================================
    // NIVEL 2
    // =========================================

    private void UpdateLevel2Results()
    {
        ReactionGameManager.LevelStats stats =
            gameManager.Level2Stats;


        if (level2HitsText != null)
        {
            level2HitsText.text =
                "ACIERTOS: " +
                stats.hits;
        }


        if (level2PrecisionText != null)
        {
            level2PrecisionText.text =
                "PRECISIÓN: " +
                stats.Precision.ToString("F1") +
                "%";
        }


        if (level2AverageText != null)
        {
            level2AverageText.text =
                "TIEMPO PROMEDIO: " +
                stats.averageReactionTime.ToString("F3") +
                " s";
        }
    }


    // =========================================
    // NIVEL 3
    // =========================================

    private void UpdateLevel3Results()
    {
        ReactionGameManager.LevelStats stats =
            gameManager.Level3Stats;


        if (level3HitsText != null)
        {
            level3HitsText.text =
                "ACIERTOS: " +
                stats.hits;
        }


        if (level3PrecisionText != null)
        {
            level3PrecisionText.text =
                "PRECISIÓN: " +
                stats.Precision.ToString("F1") +
                "%";
        }


        if (level3AverageText != null)
        {
            level3AverageText.text =
                "TIEMPO PROMEDIO: " +
                stats.averageReactionTime.ToString("F3") +
                " s";
        }
    }
}