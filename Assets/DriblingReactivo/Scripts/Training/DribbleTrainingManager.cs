using UnityEngine;

public class DribbleTrainingManager : MonoBehaviour
{
    [Header("Niveles")]
    [SerializeField] private DribbleLevel1 level1;
    [SerializeField] private DribbleLevel2 level2;
    [SerializeField] private DribbleLevel3 level3;

    [Header("UI")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private DribbleIndicatorUI indicatorUI;

    [Header("Sistemas")]
    [SerializeField] private DribbleTrainingStats stats;
    [SerializeField] private DribbleBallRespawn ballRespawn;

    private int currentLevel;

    public int CurrentLevel =>
        currentLevel;

    private void OnEnable()
    {
        if (level1 != null)
            level1.OnLevelCompleted +=
                Level1Completed;

        if (level2 != null)
            level2.OnLevelCompleted +=
                Level2Completed;

        if (level3 != null)
            level3.OnLevelCompleted +=
                Level3Completed;
    }

    private void OnDisable()
    {
        if (level1 != null)
            level1.OnLevelCompleted -=
                Level1Completed;

        if (level2 != null)
            level2.OnLevelCompleted -=
                Level2Completed;

        if (level3 != null)
            level3.OnLevelCompleted -=
                Level3Completed;
    }

    public void StartLevel1()
    {
        StopAllLevels();

        currentLevel = 1;

        if (startCanvas != null)
            startCanvas.SetActive(false);

        if (stats != null)
            stats.ResetAll();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (indicatorUI != null)
            indicatorUI.ShowLevel1Mode();

        if (level1 != null)
            level1.StartLevel();
    }

    public void StartLevel2()
    {
        StopAllLevels();

        currentLevel = 2;

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (indicatorUI != null)
            indicatorUI.ShowLevel2Mode();

        if (level2 != null)
            level2.StartLevel();
    }

    public void StartLevel3()
    {
        StopAllLevels();

        currentLevel = 3;

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (indicatorUI != null)
            indicatorUI.ShowLevel3Mode();

        if (level3 != null)
            level3.StartLevel();
    }

    private void Level1Completed()
    {
        Debug.Log(
            "Nivel 1 completado"
        );
    }

    private void Level2Completed()
    {
        Debug.Log(
            "Nivel 2 completado"
        );
    }

    private void Level3Completed()
    {
        Debug.Log(
            "Nivel 3 completado"
        );

        currentLevel = 0;

        if (indicatorUI != null)
            indicatorUI.HideIndicator();
    }

    private void StopAllLevels()
    {
        if (level1 != null)
            level1.StopLevel();

        if (level2 != null)
            level2.StopLevel();

        if (level3 != null)
            level3.StopLevel();
    }
}