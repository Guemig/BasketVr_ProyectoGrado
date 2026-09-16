using UnityEngine;
using TMPro;

public class ReactionGameUI : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField] private ReactionGameManager gameManager;

    [Header("Textos")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text averageText;


    private void Update()
    {
        if (gameManager == null)
            return;


        // =========================
        // TIEMPO
        // =========================

        timeText.text =
            "TIEMPO: " +
            gameManager.TimeRemaining.ToString("F1") +
            " s";


        // =========================
        // ACIERTOS
        // =========================

        scoreText.text =
            "ACIERTOS: " +
            gameManager.Score;


        // =========================
        // PROMEDIO
        // =========================

        averageText.text =
            "PROMEDIO: " +
            gameManager.AverageReactionTime.ToString("F3") +
            " s";
    }
}