using System;
using System.Collections;
using UnityEngine;

public class ReactionExerciseController : MonoBehaviour
{
    public enum Hand
    {
        Left,
        Right
    }


    [Header("References")]
    [SerializeField] private ReactionSequence sequence;

    [SerializeField] private ReactionLevelController levelController;

    [SerializeField] private ReactionTransition transition;

    [SerializeField] private ReactionInteraction interaction;

    [SerializeField] private ReactionHandInteractable leftHandInteractable;

    [SerializeField] private ReactionHandInteractable rightHandInteractable;

    [SerializeField] private CharacterIKController characterIK;

    [SerializeField] private ReactionMenuController menuController;

    [SerializeField] private ReactionResultsUI resultsUI;

    [SerializeField] private ReactionResultsController resultsController;

    [SerializeField] private ReactionTutorialController tutorialController;


    [Header("Reaction Timing By Level")]
    [SerializeField]
    private float[] timeBetweenReactions =
    {
        1f,
        0.8f,
        0.6f
    };


    private Hand currentHand;

    private bool reactionInProgress;


    // =====================================================
    // PAUSE
    // =====================================================

    private static bool isPaused;

    private static float totalPausedTime;

    private static float pauseStartRealtime;

    public static bool IsPaused =>
        isPaused;

    public static float TotalPausedTime =>
        totalPausedTime;


    // =====================================================
    // UNITY
    // =====================================================

    private void Awake()
    {
        SetupInteractables();
    }


    private void Start()
    {
        if (ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Tutorial)
        {
            if (tutorialController != null)
            {
                if (menuController != null)
                {
                    menuController.SetStartButtonActive(
                        false
                    );
                }


                tutorialController.StartTutorial();
            }
        }
    }


    // =====================================================
    // EXERCISE
    // =====================================================

    public void StartExercise()
    {
        StopAllCoroutines();

        DisableAllInteractables();


        if (transition != null)
        {
            if (levelController != null)
            {
                transition.SetLevelLabel(
                    levelController.CurrentLevel.ToString()
                );
            }


            transition.PlayTransition(
                StartCurrentLevel
            );


            return;
        }


        StartCurrentLevel();
    }


    private void StartCurrentLevel()
    {
        Debug.Log(
            $"[EXERCISE] Starting " +
            $"{levelController.CurrentLevel}"
        );


        sequence.CreateSequence();

        ExecuteNextReaction();
    }


    // =====================================================
    // PAUSE API
    // =====================================================

    public void PauseExercise()
    {
        if (isPaused)
            return;


        isPaused = true;

        pauseStartRealtime =
            Time.realtimeSinceStartup;


        Debug.Log(
            "[EXERCISE] Paused."
        );
    }


    public void ResumeExercise()
    {
        if (!isPaused)
            return;


        float pausedDuration =
            Time.realtimeSinceStartup -
            pauseStartRealtime;


        totalPausedTime +=
            pausedDuration;


        pauseStartRealtime = 0f;

        isPaused = false;


        Debug.Log(
            "[EXERCISE] Resumed."
        );
    }


    // =====================================================
    // NEXT REACTION
    // =====================================================

    private void ExecuteNextReaction()
    {
        if (!sequence.HasNext())
        {
            CompleteCurrentLevel();

            return;
        }


        currentHand =
            sequence.GetNext();


        reactionInProgress = true;


        Debug.Log(
            $"[REACTION] STARTING {currentHand}"
        );


        interaction.StartReaction(
            currentHand
        );


        levelController.ExecuteReaction(
            currentHand,
            ActivateCurrentReaction
        );
    }


    // =====================================================
    // REACTION ACTIVATION
    // =====================================================

    private void ActivateCurrentReaction()
    {
        if (!reactionInProgress)
            return;


        if (IsPaused)
            return;


        Debug.Log(
            $"[REACTION] ACTIVE {currentHand}"
        );


        interaction.ActivateReaction();


        StartCurrentHandReaction();


        StartCoroutine(
            WaitAndExecute(
                GetTimeBetweenReactions(),
                ReturnCurrentHand
            )
        );
    }


    private void StartCurrentHandReaction()
    {
        switch (currentHand)
        {
            case Hand.Left:

                if (leftHandInteractable != null)
                {
                    leftHandInteractable.StartReaction();
                }

                break;


            case Hand.Right:

                if (rightHandInteractable != null)
                {
                    rightHandInteractable.StartReaction();
                }

                break;
        }
    }


    private void EndCurrentHandReaction()
    {
        switch (currentHand)
        {
            case Hand.Left:

                if (leftHandInteractable != null)
                {
                    leftHandInteractable.EndReaction();
                }

                break;


            case Hand.Right:

                if (rightHandInteractable != null)
                {
                    rightHandInteractable.EndReaction();
                }

                break;
        }
    }


    // =====================================================
    // RETURN
    // =====================================================

    private void ReturnCurrentHand()
    {
        if (!reactionInProgress)
            return;


        if (IsPaused)
        {
            return;
        }


        interaction.ResolveTimeout();

        EndCurrentHandReaction();

        interaction.EndReaction();

        reactionInProgress = false;


        switch (currentHand)
        {
            case Hand.Left:

                characterIK.ReturnLeftHand();

                break;


            case Hand.Right:

                characterIK.ReturnRightHand();

                break;
        }


        StartCoroutine(
            WaitAndExecute(
                GetTimeBetweenReactions(),
                ExecuteNextReaction
            )
        );
    }


    // =====================================================
    // LEVEL COMPLETE
    // =====================================================

    private void CompleteCurrentLevel()
    {
        reactionInProgress = false;

        DisableAllInteractables();


        Debug.Log(
            $"[LEVEL] Completed: " +
            $"{levelController.CurrentLevel}"
        );


        if (levelController.HasNextLevel())
        {
            StartLevelTransition();

            return;
        }


        CompleteExercise();
    }


    // =====================================================
    // LEVEL TRANSITION
    // =====================================================

    private void StartLevelTransition()
    {
        Debug.Log(
            "[EXERCISE] Preparing next level."
        );


        levelController.TryAdvanceLevel();


        if (transition != null)
        {
            if (levelController != null)
            {
                transition.SetLevelLabel(
                    levelController.CurrentLevel.ToString()
                );
            }


            transition.PlayTransition(
                StartCurrentLevel
            );
        }
        else
        {
            StartCurrentLevel();
        }
    }


    // =====================================================
    // EXERCISE COMPLETE
    // =====================================================

    private async void CompleteExercise()
    {
        reactionInProgress = false;


        Debug.Log(
            "[EXERCISE] Exercise completed."
        );


        // =================================================
        // SAVE TO FIRESTORE
        // =================================================

        if (resultsController != null)
        {
            if (FirestoreService.Instance == null)
            {
                Debug.LogError(
                    "[FIRESTORE] FirestoreService.Instance is null."
                );
            }
            else if (!FirestoreService.Instance.HasCurrentStudent)
            {
                Debug.LogError(
                    "[FIRESTORE] No hay un estudiante activo."
                );
            }
            else
            {
                try
                {
                    FirestoreService.MovementSession session =
                        BuildMovementSession();


                    string studentCode =
                        FirestoreService.Instance
                            .CurrentStudentCode;


                    string attemptId =
                        await FirestoreService.Instance
                            .SaveCompletedAttempt(
                                studentCode,
                                session
                            );


                    Debug.Log(
                        "[FIRESTORE] Attempt saved successfully. " +
                        $"Student: {studentCode} | " +
                        $"Attempt: {attemptId}"
                    );
                }
                catch (Exception e)
                {
                    Debug.LogError(
                        "[FIRESTORE] Error saving attempt: " +
                        e
                    );
                }
            }
        }


        // =================================================
        // END UI
        // =================================================

        if (menuController != null)
        {
            menuController.ShowEndMenu();
        }


        if (resultsUI != null)
        {
            resultsUI.ShowAllLevels();
        }
    }


    // =====================================================
    // BUILD MOVEMENT SESSION
    // =====================================================

    private FirestoreService.MovementSession
        BuildMovementSession()
    {
        FirestoreService.MovementSession session =
            FirestoreService.Instance
                .CreateMovementSession();


        int levelCount =
            Enum.GetValues(
                typeof(
                    ReactionLevelController.ReactionLevel
                )
            ).Length;


        for (int i = 0;
             i < levelCount;
             i++)
        {
            ReactionLevelController.ReactionLevel level =
                (ReactionLevelController.ReactionLevel)i;


            FirestoreService.MovementLevel levelData =
                resultsController
                    .GetFirestoreLevelData(
                        level
                    );


            FirestoreService.Instance
                .AddLevelToSession(
                    session,
                    levelData
                );
        }


        return session;
    }


    // =====================================================
    // INTERACTABLES
    // =====================================================

    private void SetupInteractables()
    {
        if (leftHandInteractable != null)
        {
            leftHandInteractable
                .SetReactionInteraction(
                    interaction
                );
        }


        if (rightHandInteractable != null)
        {
            rightHandInteractable
                .SetReactionInteraction(
                    interaction
                );
        }


        if (interaction != null)
        {
            interaction.ReactionResolved +=
                OnReactionResolved;
        }
    }


    private void OnReactionResolved(
        bool correct,
        float reactionTime)
    {
        if (ReactionModeController.CurrentMode ==
            ReactionModeController.ReactionMode.Tutorial)
        {
            return;
        }


        if (resultsController == null ||
            levelController == null)
        {
            return;
        }


        resultsController.RegisterReaction(
            levelController.CurrentLevel,
            correct,
            reactionTime,
            currentHand
        );
    }


    private void DisableAllInteractables()
    {
        if (leftHandInteractable != null)
        {
            leftHandInteractable.EndReaction();
        }


        if (rightHandInteractable != null)
        {
            rightHandInteractable.EndReaction();
        }
    }


    // =====================================================
    // TIMING
    // =====================================================

    private float GetTimeBetweenReactions()
    {
        int index =
            (int)levelController.CurrentLevel;


        if (timeBetweenReactions == null ||
            timeBetweenReactions.Length == 0)
        {
            return 1f;
        }


        if (index < 0 ||
            index >= timeBetweenReactions.Length)
        {
            return timeBetweenReactions[
                timeBetweenReactions.Length - 1
            ];
        }


        return Mathf.Max(
            0f,
            timeBetweenReactions[index]
        );
    }


    private IEnumerator WaitAndExecute(
        float duration,
        Action onComplete)
    {
        float elapsed = 0f;


        while (elapsed < duration)
        {
            if (!IsPaused)
            {
                elapsed += Time.deltaTime;
            }


            yield return null;
        }


        onComplete?.Invoke();
    }


#if UNITY_EDITOR

    private void OnValidate()
    {
        int levelCount =
            Enum.GetValues(
                typeof(
                    ReactionLevelController.ReactionLevel
                )
            ).Length;


        if (timeBetweenReactions == null ||
            timeBetweenReactions.Length != levelCount)
        {
            float[] newTimes =
                new float[levelCount];


            for (
                int i = 0;
                i < levelCount;
                i++)
            {
                if (
                    timeBetweenReactions != null &&
                    i < timeBetweenReactions.Length)
                {
                    newTimes[i] =
                        timeBetweenReactions[i];
                }
                else
                {
                    newTimes[i] = 1f;
                }
            }


            timeBetweenReactions =
                newTimes;
        }
    }

#endif
}