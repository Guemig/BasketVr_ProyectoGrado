
using TMPro;
using UnityEngine;

public class PassLevelResultsUI : MonoBehaviour
{
    [Header("Exercise Manager")]
    [SerializeField] private PassReactionGameManager gameManager;

    [Header("Level 1 - Results")]
    [SerializeField] private TMP_Text level1CorrectHitsText;
    [SerializeField] private TMP_Text level1AccuracyText;
    [SerializeField] private TMP_Text level1AverageReactionTimeText;

    [Header("Level 2 - Results")]
    [SerializeField] private TMP_Text level2CorrectHitsText;
    [SerializeField] private TMP_Text level2AccuracyText;
    [SerializeField] private TMP_Text level2AverageReactionTimeText;

    [Header("Level 3 - Results")]
    [SerializeField] private TMP_Text level3CorrectHitsText;
    [SerializeField] private TMP_Text level3AccuracyText;
    [SerializeField] private TMP_Text level3AverageReactionTimeText;

    [Header("Formatting")]
    [SerializeField, Min(0)] private int reactionTimeDecimals = 2;

    private void Awake()
    {
        if (gameManager == null)
        {
            Debug.LogError(
                "[PASS RESULTS UI] Assign PassReactionGameManager.",
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
                $"[PASS RESULTS UI] No stats available for level {level}.",
                this
            );
            return;
        }

        switch (level)
        {
            case 1:
                SetResultTexts(
                    level1CorrectHitsText,
                    level1AccuracyText,
                    level1AverageReactionTimeText,
                    stats
                );
                break;

            case 2:
                SetResultTexts(
                    level2CorrectHitsText,
                    level2AccuracyText,
                    level2AverageReactionTimeText,
                    stats
                );
                break;

            case 3:
                SetResultTexts(
                    level3CorrectHitsText,
                    level3AccuracyText,
                    level3AverageReactionTimeText,
                    stats
                );
                break;

            default:
                Debug.LogWarning(
                    $"[PASS RESULTS UI] Invalid level: {level}.",
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
        TMP_Text correctHitsText,
        TMP_Text accuracyText,
        TMP_Text averageReactionTimeText,
        PassReactionStatsManager.LevelStats stats)
    {
        int correctHits = stats.CorrectDecisions;
        int totalShots = stats.Total;

        float accuracy = totalShots > 0
            ? (float)correctHits / totalShots * 100f
            : 0f;

        if (correctHitsText != null)
            correctHitsText.text = $"{correctHits}/{totalShots}";

        if (accuracyText != null)
            accuracyText.text = $"{accuracy:F1}%";

        if (averageReactionTimeText != null)
        {
            averageReactionTimeText.text =
                stats.reactionTimes != null &&
                stats.reactionTimes.Count > 0
                    ? $"{stats.averageReaction.ToString($"F{reactionTimeDecimals}")} s"
                    : "-- s";
        }
    }

    public void ClearResults()
    {
        ClearLevelTexts(
            level1CorrectHitsText,
            level1AccuracyText,
            level1AverageReactionTimeText
        );

        ClearLevelTexts(
            level2CorrectHitsText,
            level2AccuracyText,
            level2AverageReactionTimeText
        );

        ClearLevelTexts(
            level3CorrectHitsText,
            level3AccuracyText,
            level3AverageReactionTimeText
        );
    }

    private void ClearLevelTexts(
        TMP_Text correctHitsText,
        TMP_Text accuracyText,
        TMP_Text averageReactionTimeText)
    {
        if (correctHitsText != null)
            correctHitsText.text = "--/--";

        if (accuracyText != null)
            accuracyText.text = "--%";

        if (averageReactionTimeText != null)
            averageReactionTimeText.text = "-- s";
    }
}
