using UnityEngine;

public class ReactionResultsUI : MonoBehaviour
{
    [System.Serializable]
    private class LevelResultUI
    {
        public ReactionLevelController.ReactionLevel level;

        public ReactionResultEntry[] entries;
    }


    [Header("References")]
    [SerializeField]
    private ReactionResultsController resultsController;


    [Header("Level Results")]
    [SerializeField]
    private LevelResultUI[] levelResults;


    // =====================================================
    // SHOW LEVEL
    // =====================================================

    public void ShowLevel(
        ReactionLevelController.ReactionLevel level)
    {
        LevelResultUI levelUI =
            GetLevelUI(level);


        if (levelUI == null)
            return;


        if (resultsController == null)
        {
            Debug.LogWarning(
                "[ReactionResultsUI] " +
                "ReactionResultsController is missing."
            );

            return;
        }


        foreach (
            ReactionResultEntry entry
            in levelUI.entries)
        {
            if (entry == null)
                continue;


            UpdateEntry(
                entry,
                level
            );
        }
    }

    // =====================================================
    // SHOW ALL LEVELS
    // =====================================================

    public void ShowAllLevels()
    {
        if (levelResults == null)
            return;

        if (resultsController == null)
        {
            Debug.LogWarning(
                "[ReactionResultsUI] " +
                "ReactionResultsController is missing."
            );

            return;
        }

        foreach (
            LevelResultUI levelUI
            in levelResults)
        {
            if (levelUI == null)
                continue;

            if (levelUI.entries == null)
                continue;

            foreach (
                ReactionResultEntry entry
                in levelUI.entries)
            {
                if (entry == null)
                    continue;

                UpdateEntry(entry, levelUI.level);
            }
        }
    }


    // =====================================================
    // UPDATE ENTRY
    // =====================================================

    private void UpdateEntry(
        ReactionResultEntry entry,
        ReactionLevelController.ReactionLevel level)
    {
        if (entry.value == null)
            return;


        switch (entry.key)
        {
            case "average_reaction_time":

                entry.value.text =
                    $"{resultsController.GetAverageReactionTime(level):0.00} s";

                break;


            case "correct_answers":

                int correct =
                    resultsController.GetCorrectReactions(
                        level
                    );


                int total =
                    resultsController.GetTotalReactions(
                        level
                    );


                entry.value.text =
                    $"{correct} / {total}";

                break;


            case "accuracy":

                entry.value.text =
                    $"{resultsController.GetAccuracy(level):0.0}%";

                break;


            default:

                Debug.LogWarning(
                    $"[ReactionResultsUI] " +
                    $"Unknown result key: {entry.key}"
                );

                break;
        }
    }


    // =====================================================
    // LEVEL
    // =====================================================

    private LevelResultUI GetLevelUI(
        ReactionLevelController.ReactionLevel level)
    {
        if (levelResults == null)
            return null;


        foreach (
            LevelResultUI levelUI
            in levelResults)
        {
            if (levelUI == null)
                continue;


            if (levelUI.level == level)
                return levelUI;
        }


        return null;
    }
}