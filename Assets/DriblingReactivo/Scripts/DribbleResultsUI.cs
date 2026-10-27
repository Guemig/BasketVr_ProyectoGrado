using TMPro;
using UnityEngine;

public class DribbleResultsUI : MonoBehaviour
{
    [Header("Estadisticas")]
    [SerializeField] private DribbleTrainingStats stats;

    [Header("Nivel 1")]
    [SerializeField] private TMP_Text level1HitsValue;
    [SerializeField] private TMP_Text level1AccuracyValue;
    [SerializeField] private TMP_Text level1ReactionValue;

    [Header("Nivel 2")]
    [SerializeField] private TMP_Text level2HitsValue;
    [SerializeField] private TMP_Text level2AccuracyValue;
    [SerializeField] private TMP_Text level2ReactionValue;

    [Header("Nivel 3")]
    [SerializeField] private TMP_Text level3HitsValue;
    [SerializeField] private TMP_Text level3AccuracyValue;
    [SerializeField] private TMP_Text level3ReactionValue;

    public void RefreshResults()
    {
        if (stats == null)
            return;

        UpdateLevel(
            stats.level1,
            level1HitsValue,
            level1AccuracyValue,
            level1ReactionValue
        );

        UpdateLevel(
            stats.level2,
            level2HitsValue,
            level2AccuracyValue,
            level2ReactionValue
        );

        UpdateLevel(
            stats.level3,
            level3HitsValue,
            level3AccuracyValue,
            level3ReactionValue
        );
    }

    private void UpdateLevel(
        DribbleTrainingStats.LevelStats levelStats,
        TMP_Text hitsValue,
        TMP_Text accuracyValue,
        TMP_Text reactionValue)
    {
        if (levelStats == null)
            return;

        if (hitsValue != null)
        {
            hitsValue.text =
                levelStats.hits.ToString();
        }

        if (accuracyValue != null)
        {
            accuracyValue.text =
                levelStats.Accuracy.ToString("F1") +
                "%";
        }

        if (reactionValue != null)
        {
            if (levelStats.reactionTimes.Count > 0)
            {
                reactionValue.text =
                    levelStats
                        .AverageReactionTime
                        .ToString("F2") +
                    " s";
            }
            else
            {
                reactionValue.text =
                    "--";
            }
        }
    }
}