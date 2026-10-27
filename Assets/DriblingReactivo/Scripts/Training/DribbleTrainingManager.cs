using UnityEngine;

public class DribbleTrainingManager : MonoBehaviour
{
    [Header("Niveles")]
    [SerializeField] private DribbleLevel1 level1;
    [SerializeField] private DribbleLevel2 level2;
    [SerializeField] private DribbleLevel3 level3;

    [Header("Transicion")]
    [SerializeField] private DribbleTransition levelTransition;

    [Header("Audio Inicio Nivel")]
    [SerializeField] private AudioSource levelStartAudioSource;
    [SerializeField] private AudioClip levelStartSound;

    [Header("UI")]
    [SerializeField] private GameObject startCanvas;
    [SerializeField] private GameObject finalCanvas;
    [SerializeField] private DribbleIndicatorUI indicatorUI;
    [SerializeField] private DribbleResultsUI resultsUI;

    [Header("Sistemas")]
    [SerializeField] private DribbleTrainingStats stats;
    [SerializeField] private DribbleBallRespawn ballRespawn;

    private int currentLevel;
    private bool trainingActive;

    public int CurrentLevel =>
        currentLevel;

    public bool TrainingActive =>
        trainingActive;

    private void Awake()
    {
        currentLevel = 0;
        trainingActive = false;

        if (finalCanvas != null)
            finalCanvas.SetActive(false);
    }

    private void OnEnable()
    {
        if (level1 != null)
        {
            level1.OnLevelCompleted +=
                Level1Completed;
        }

        if (level2 != null)
        {
            level2.OnLevelCompleted +=
                Level2Completed;
        }

        if (level3 != null)
        {
            level3.OnLevelCompleted +=
                Level3Completed;
        }
    }

    private void OnDisable()
    {
        if (level1 != null)
        {
            level1.OnLevelCompleted -=
                Level1Completed;
        }

        if (level2 != null)
        {
            level2.OnLevelCompleted -=
                Level2Completed;
        }

        if (level3 != null)
        {
            level3.OnLevelCompleted -=
                Level3Completed;
        }
    }

    public void StartTraining()
    {
        StopAllLevels();

        currentLevel = 0;
        trainingActive = true;

        if (stats != null)
            stats.ResetAll();

        if (startCanvas != null)
            startCanvas.SetActive(false);

        if (finalCanvas != null)
            finalCanvas.SetActive(false);

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        PrepareLevel1();
    }

    private void PrepareLevel1()
    {
        currentLevel = 1;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel(
                "NIVEL 1"
            );

            levelTransition.PlayTransition(
                StartLevel1
            );
        }
        else
        {
            StartLevel1();
        }
    }

    private void StartLevel1()
    {
        if (!trainingActive)
            return;

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        PlayLevelStartSound();

        if (indicatorUI != null)
            indicatorUI.ShowLevel1Mode();

        if (level1 != null)
            level1.StartLevel();
    }

    private void Level1Completed()
    {
        if (!trainingActive)
            return;

        PrepareLevel2();
    }

    private void PrepareLevel2()
    {
        currentLevel = 2;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel(
                "NIVEL 2"
            );

            levelTransition.PlayTransition(
                StartLevel2
            );
        }
        else
        {
            StartLevel2();
        }
    }

    private void StartLevel2()
    {
        if (!trainingActive)
            return;

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        PlayLevelStartSound();

        if (indicatorUI != null)
            indicatorUI.ShowLevel2Mode();

        if (level2 != null)
            level2.StartLevel();
    }

    private void Level2Completed()
    {
        if (!trainingActive)
            return;

        PrepareLevel3();
    }

    private void PrepareLevel3()
    {
        currentLevel = 3;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel(
                "NIVEL 3"
            );

            levelTransition.PlayTransition(
                StartLevel3
            );
        }
        else
        {
            StartLevel3();
        }
    }

    private void StartLevel3()
    {
        if (!trainingActive)
            return;

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        PlayLevelStartSound();

        if (indicatorUI != null)
            indicatorUI.ShowLevel3Mode();

        if (level3 != null)
            level3.StartLevel();
    }

    private void Level3Completed()
    {
        if (!trainingActive)
            return;

        FinishTraining();
    }

    private void PrepareTransition()
    {
        StopAllLevels();

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (levelStartAudioSource != null)
            levelStartAudioSource.Stop();
    }

    private void PlayLevelStartSound()
    {
        if (levelStartAudioSource == null)
            return;

        if (levelStartSound == null)
            return;

        levelStartAudioSource.Stop();

        levelStartAudioSource.PlayOneShot(
            levelStartSound
        );
    }

    private void FinishTraining()
    {
        trainingActive = false;
        currentLevel = 0;

        StopAllLevels();

        if (levelStartAudioSource != null)
            levelStartAudioSource.Stop();

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        if (resultsUI != null)
            resultsUI.RefreshResults();

        if (finalCanvas != null)
            finalCanvas.SetActive(true);
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