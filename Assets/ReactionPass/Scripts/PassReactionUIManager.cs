using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PassReactionUIManager : MonoBehaviour
{
    [Header("Game Manager")]
    [SerializeField]
    private PassReactionGameManager gameManager;

    [Header("Canvas")]
    [SerializeField]
    private GameObject statusCanvas;

    [SerializeField]
    private GameObject resultsCanvas;

    [Header("Canvas Estado")]
    [SerializeField]
    private TMP_Text statusText;

    [SerializeField]
    private GameObject startButton;

    [SerializeField]
    private GameObject nextButton;

    [SerializeField]
    private GameObject exitButton;

    [Header("Canvas Resultados")]
    [SerializeField]
    private TMP_Text level1Text;

    [SerializeField]
    private TMP_Text level2Text;

    [SerializeField]
    private TMP_Text level3Text;

    [Header("Escena de salida")]
    [SerializeField]
    private string exitSceneName =
        "MenuPrincipal";


    private void OnEnable()
    {
        if (gameManager == null)
            return;

        gameManager.OnInitialState +=
            ShowInitialState;

        gameManager.OnLevelStarted +=
            ShowLevelStarted;

        gameManager.OnLevelFinished +=
            ShowLevelFinished;

        gameManager.OnExerciseFinished +=
            ShowFinalResults;
    }


    private void OnDisable()
    {
        if (gameManager == null)
            return;

        gameManager.OnInitialState -=
            ShowInitialState;

        gameManager.OnLevelStarted -=
            ShowLevelStarted;

        gameManager.OnLevelFinished -=
            ShowLevelFinished;

        gameManager.OnExerciseFinished -=
            ShowFinalResults;
    }


    // =========================
    // ESTADO INICIAL
    // =========================

    private void ShowInitialState()
    {
        if (statusCanvas != null)
        {
            statusCanvas.SetActive(true);
        }

        if (resultsCanvas != null)
        {
            resultsCanvas.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text =
                "REACCIÓN A PASES\n\n" +
                "Presiona INICIAR para comenzar";
        }

        if (startButton != null)
        {
            startButton.SetActive(true);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.SetActive(true);
        }
    }


    // =========================
    // NIVEL COMENZADO
    // =========================

    private void ShowLevelStarted(
        int level)
    {
        if (statusCanvas != null)
        {
            statusCanvas.SetActive(true);
        }

        if (resultsCanvas != null)
        {
            resultsCanvas.SetActive(false);
        }

        if (statusText != null)
        {
            statusText.text =
                "NIVEL " + level;
        }

        if (startButton != null)
        {
            startButton.SetActive(false);
        }

        if (nextButton != null)
        {
            nextButton.SetActive(false);
        }

        if (exitButton != null)
        {
            exitButton.SetActive(false);
        }
    }


    // =========================
    // NIVEL FINALIZADO
    // =========================

    private void ShowLevelFinished(
        int level)
    {
        // En nivel 3 NO mostramos
        // el botón siguiente porque
        // aparecerá el Canvas final.

        if (level >= 3)
            return;

        if (statusText != null)
        {
            statusText.text =
                "NIVEL " +
                level +
                " FINALIZADO\n\n" +
                "Presiona SIGUIENTE para continuar";
        }

        if (nextButton != null)
        {
            nextButton.SetActive(true);
        }

        if (exitButton != null)
        {
            exitButton.SetActive(true);
        }
    }


    // =========================
    // RESULTADOS FINALES
    // =========================

    private void ShowFinalResults()
    {
        if (statusCanvas != null)
        {
            statusCanvas.SetActive(false);
        }

        if (resultsCanvas != null)
        {
            resultsCanvas.SetActive(true);
        }

        SetLevelText(
            level1Text,
            gameManager.Level1Stats
        );

        SetLevelText(
            level2Text,
            gameManager.Level2Stats
        );

        SetLevelText(
            level3Text,
            gameManager.Level3Stats
        );
    }


    // =========================
    // TEXTO DE CADA NIVEL
    // =========================

    private void SetLevelText(
        TMP_Text text,
        PassReactionGameManager.LevelStats stats)
    {
        if (text == null)
            return;

        text.text =
            "NIVEL " +
            stats.level +
            "\n\n" +

            "Atrapadas: " +
            stats.caught +
            "\n" +

            "Falladas: " +
            stats.missed +
            "\n" +

            "Precisión: " +
            stats.accuracy.ToString("F1") +
            "%";
    }


    // =========================
    // BOTONES
    // =========================

    public void StartExercise()
    {
        if (gameManager != null)
        {
            gameManager.StartGame();
        }
    }


    public void NextLevel()
    {
        if (gameManager != null)
        {
            gameManager.NextLevel();
        }
    }


    public void RestartExercise()
    {
        if (gameManager != null)
        {
            gameManager.RestartExercise();
        }
    }


    public void ExitExercise()
    {
        SceneManager.LoadScene(
            exitSceneName
        );
    }
}