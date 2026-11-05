
using System;
using System.Threading.Tasks;
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

    // Evita guardar dos veces el mismo entrenamiento.
    private bool firebaseSaveStarted;

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
            level1.OnLevelCompleted += Level1Completed;

        if (level2 != null)
            level2.OnLevelCompleted += Level2Completed;

        if (level3 != null)
            level3.OnLevelCompleted += Level3Completed;
    }

    private void OnDisable()
    {
        if (level1 != null)
            level1.OnLevelCompleted -= Level1Completed;

        if (level2 != null)
            level2.OnLevelCompleted -= Level2Completed;

        if (level3 != null)
            level3.OnLevelCompleted -= Level3Completed;
    }

    // =========================================================
    // START TRAINING
    // =========================================================

    public void StartTraining()
    {
        StopAllLevels();

        currentLevel = 0;
        trainingActive = true;

        // Permite guardar un nuevo intento.
        firebaseSaveStarted = false;

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

    // =========================================================
    // LEVEL 1
    // =========================================================

    private void PrepareLevel1()
    {
        currentLevel = 1;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel("NIVEL 1");
            levelTransition.PlayTransition(StartLevel1);
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

        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Level1
        );

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

    // =========================================================
    // LEVEL 2
    // =========================================================

    private void PrepareLevel2()
    {
        currentLevel = 2;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel("NIVEL 2");
            levelTransition.PlayTransition(StartLevel2);
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

        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Level2
        );

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

    // =========================================================
    // LEVEL 3
    // =========================================================

    private void PrepareLevel3()
    {
        currentLevel = 3;

        PrepareTransition();

        if (levelTransition != null)
        {
            levelTransition.SetLevelLabel("NIVEL 3");
            levelTransition.PlayTransition(StartLevel3);
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

        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Level3
        );

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

    // =========================================================
    // TRANSITION
    // =========================================================

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
        levelStartAudioSource.PlayOneShot(levelStartSound);
    }

    // =========================================================
    // FINISH TRAINING
    // =========================================================

    private void FinishTraining()
    {
        trainingActive = false;
        currentLevel = 0;

        StopAllLevels();

        if (levelStartAudioSource != null)
            levelStartAudioSource.Stop();

        // Música de resultados.
        AudioManager.Instance?.PlayBackground(
            BackgroundContext.Results
        );

        if (indicatorUI != null)
            indicatorUI.HideIndicator();

        if (ballRespawn != null)
            ballRespawn.RespawnNow();

        // Actualiza las estadísticas visuales.
        if (resultsUI != null)
            resultsUI.RefreshResults();

        // Primero muestra los resultados.
        if (finalCanvas != null)
            finalCanvas.SetActive(true);

        // Después inicia el guardado en Firebase.
        _ = SaveDribbleResultsToFirebase();
    }

    // =========================================================
    // FIREBASE - SAVE DRIBBLE RESULTS
    // =========================================================

    private async Task SaveDribbleResultsToFirebase()
    {
        // Evita múltiples guardados del mismo intento.
        if (firebaseSaveStarted)
            return;

        firebaseSaveStarted = true;

        try
        {
            // Verificar estadísticas.
            if (stats == null)
            {
                Debug.LogError(
                    "[DRIBBLE FIREBASE] No se encontró DribbleTrainingStats."
                );
                return;
            }

            // Verificar Firestore.
            if (FirestoreService.Instance == null)
            {
                Debug.LogError(
                    "[DRIBBLE FIREBASE] FirestoreService no está disponible."
                );
                return;
            }

            // Verificar estudiante registrado.
            string studentCode =
                FirestoreService.Instance.CurrentStudentCode;

            if (string.IsNullOrEmpty(studentCode))
            {
                Debug.LogError(
                    "[DRIBBLE FIREBASE] No hay un estudiante activo."
                );
                return;
            }

            // Crear sesión.
            FirestoreService.DribbleSession session =
                FirestoreService.Instance.CreateDribbleSession();

            // Registrar nivel 1.
            session = AddDribbleLevel(
                session,
                stats.level1
            );

            // Registrar nivel 2.
            session = AddDribbleLevel(
                session,
                stats.level2
            );

            // Registrar nivel 3.
            session = AddDribbleLevel(
                session,
                stats.level3
            );

            // Guardar los tres niveles en un único intento.
            string attemptId =
                await FirestoreService.Instance.SaveDribbleAttempt(
                    studentCode,
                    session
                );

            Debug.Log(
                "[DRIBBLE FIREBASE] Entrenamiento guardado correctamente."
            );

            Debug.Log(
                "[DRIBBLE FIREBASE] Estudiante: " + studentCode
            );

            Debug.Log(
                "[DRIBBLE FIREBASE] ID del intento: " + attemptId
            );
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "[DRIBBLE FIREBASE] Error al guardar el entrenamiento: " +
                exception.Message
            );
        }
    }

    // =========================================================
    // FIREBASE - CONVERT LEVEL STATS
    // =========================================================

    private FirestoreService.DribbleSession AddDribbleLevel(
        FirestoreService.DribbleSession session,
        DribbleTrainingStats.LevelStats levelStats)
    {
        if (levelStats == null)
        {
            throw new Exception(
                "No se encontraron estadísticas de uno de los niveles."
            );
        }

        FirestoreService.DribbleLevel levelData =
            new FirestoreService.DribbleLevel
            {
                correctAnswers = levelStats.hits,
                totalAttempts = levelStats.TotalAttempts,
                accuracy = levelStats.Accuracy,
                averageReaction = levelStats.AverageReactionTime
            };

        return FirestoreService.Instance.AddDribbleLevelToSession(
            session,
            levelData
        );
    }

    // =========================================================
    // STOP LEVELS
    // =========================================================

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
