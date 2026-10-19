
using TMPro;
using UnityEngine;

public class PassLevelResultsUI : MonoBehaviour
{
    [Header("Manager del ejercicio")]
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Resultados del nivel 1")]
    [SerializeField] private TMP_Text level1HitsText;
    [SerializeField] private TMP_Text level1MissesText;
    [SerializeField] private TMP_Text level1ReactionTimeText;

    [Header("Resultados del nivel 2")]
    [SerializeField] private TMP_Text level2HitsText;
    [SerializeField] private TMP_Text level2MissesText;
    [SerializeField] private TMP_Text level2ReactionTimeText;

    [Header("Resultados del nivel 3")]
    [SerializeField] private TMP_Text level3HitsText;
    [SerializeField] private TMP_Text level3MissesText;
    [SerializeField] private TMP_Text level3ReactionTimeText;

    [Header("Formato")]
    [SerializeField, Min(0)] private int reactionTimeDecimals = 2;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError(
                "[PASS RESULTS UI] Asigna PassReactionGameManager.",
                this
            );
        }
    }

    private void OnEnable()
    {
        if (gameManager == null)
            return;

        gameManager.OnLevelFinished += HandleLevelFinished;
        gameManager.OnExerciseFinished += HandleExerciseFinished;
    }

    private void Start()
    {
        ClearResults();
    }

    private void OnDisable()
    {
        if (gameManager == null)
            return;

        gameManager.OnLevelFinished -= HandleLevelFinished;
        gameManager.OnExerciseFinished -= HandleExerciseFinished;
    }

    private void HandleLevelFinished(int level)
    {
        UpdateLevelResults(level);
    }

    private void HandleExerciseFinished()
    {
        UpdateAllResults();
    }

    public void UpdateAllResults()
    {
        if (gameManager == null)
            return;

        UpdateLevelResults(1);
        UpdateLevelResults(2);
        UpdateLevelResults(3);
    }

    public void UpdateLevelResults(int level)
    {
        if (gameManager == null)
            return;

        PassReactionStatsManager.LevelStats stats = GetLevelStats(level);

        if (stats == null)
        {
            Debug.LogWarning(
                $"[PASS RESULTS UI] No hay estadísticas para el nivel {level}.",
                this
            );
            return;
        }

        switch (level)
        {
            case 1:
                SetResultTexts(
                    level1HitsText,
                    level1MissesText,
                    level1ReactionTimeText,
                    stats
                );
                break;

            case 2:
                SetResultTexts(
                    level2HitsText,
                    level2MissesText,
                    level2ReactionTimeText,
                    stats
                );
                break;

            case 3:
                SetResultTexts(
                    level3HitsText,
                    level3MissesText,
                    level3ReactionTimeText,
                    stats
                );
                break;

            default:
                Debug.LogWarning(
                    $"[PASS RESULTS UI] Nivel inválido: {level}.",
                    this
                );
                break;
        }
    }

    private PassReactionStatsManager.LevelStats GetLevelStats(int level)
    {
        switch (level)
        {
            case 1:
                return gameManager.Level1Stats;

            case 2:
                return gameManager.Level2Stats;

            case 3:
                return gameManager.Level3Stats;

            default:
                return null;
        }
    }

    private void SetResultTexts(
        TMP_Text hitsText,
        TMP_Text missesText,
        TMP_Text reactionTimeText,
        PassReactionStatsManager.LevelStats stats)
    {
        if (hitsText != null)
        {
            hitsText.text =
                $"Aciertos: {stats.caught + stats.correctPasses}\n" +
                $"Atrapadas correctas: {stats.caught}\n" +
                $"Pases correctos: {stats.correctPasses}";
        }

        if (missesText != null)
        {
            missesText.text =
                $"Errores / omisiones: {stats.wrongCaught + stats.missed}\n" +
                $"Atrapadas incorrectas: {stats.wrongCaught}\n" +
                $"Balones omitidos: {stats.missed}\n" +
                $"Precisión: {stats.accuracy.ToString("F1")}%";
        }

        if (reactionTimeText != null)
        {
            string average =
                stats.averageReaction.ToString($"F{reactionTimeDecimals}");

            reactionTimeText.text = $"Tiempo promedio: {average} s";
        }
    }

    public void ClearResults()
    {
        ClearLevelTexts(
            level1HitsText,
            level1MissesText,
            level1ReactionTimeText
        );

        ClearLevelTexts(
            level2HitsText,
            level2MissesText,
            level2ReactionTimeText
        );

        ClearLevelTexts(
            level3HitsText,
            level3MissesText,
            level3ReactionTimeText
        );
    }

    private void ClearLevelTexts(
        TMP_Text hitsText,
        TMP_Text missesText,
        TMP_Text reactionTimeText)
    {
        if (hitsText != null)
            hitsText.text = "Aciertos: --";

        if (missesText != null)
            missesText.text = "Errores / omisiones: --";

        if (reactionTimeText != null)
            reactionTimeText.text = "Tiempo promedio: -- s";
    }
}
