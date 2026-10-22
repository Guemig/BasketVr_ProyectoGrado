
using System;
using System.Collections;
using UnityEngine;

public class PassTutorialReaction : MonoBehaviour
{
    public enum ExpectedResult
    {
        CorrectCatch,
        MissCatch,
        CorrectPass,
        WrongCatch
    }

    [Header("Referencias")]
    [SerializeField] private PassMachineManager machineManager;

    private BallLauncher currentLauncher;
    private DoubleHandPassBall currentBall;

    private Action completionCallback;
    private DoubleHandPassBall.BallAction currentAction;
    private ExpectedResult expectedResult;

    private int requiredCorrect;
    private int requiredErrors;

    private int successfulBalls;
    private int errorsCount;

    private bool tutorialActive;
    private bool waitingForResult;

    private Coroutine nextBallCoroutine;

    // =====================================================
    // INICIAR SECUENCIA DEL PASO
    // =====================================================

    public void StartDemonstration(
        DoubleHandPassBall.BallAction action,
        ExpectedResult expected,
        int requiredCorrectBalls,
        int requiredErrorsCount,
        Action onCompleted)
    {
        if (tutorialActive)
        {
            Debug.LogWarning(
                "[PASS TUTORIAL] Ya hay una secuencia activa.",
                this
            );
            return;
        }

        if (machineManager == null)
        {
            Debug.LogError(
                "[PASS TUTORIAL] Falta PassMachineManager.",
                this
            );
            return;
        }

        if (!machineManager.ConfigureLevel(1))
        {
            Debug.LogError(
                "[PASS TUTORIAL] No se pudo configurar el nivel 1.",
                this
            );
            return;
        }

        currentLauncher = machineManager.GetNextLauncher();

        if (currentLauncher == null)
        {
            Debug.LogError(
                "[PASS TUTORIAL] No se encontró una máquina disponible.",
                this
            );
            return;
        }

        currentAction = action;
        expectedResult = expected;

        requiredCorrect = Mathf.Max(0, requiredCorrectBalls);
        requiredErrors = Mathf.Max(0, requiredErrorsCount);

        successfulBalls = 0;
        errorsCount = 0;

        completionCallback = onCompleted;
        tutorialActive = true;
        waitingForResult = false;

        currentLauncher.SetTutorialMode(true);

        LaunchNextBall();
    }

    // =====================================================
    // LANZAMIENTO
    // =====================================================

    private void LaunchNextBall()
    {
        if (!tutorialActive || currentLauncher == null)
            return;

        if (waitingForResult)
            return;

        currentLauncher.OnTutorialBallSpawned -= OnTutorialBallSpawned;
        currentLauncher.OnTutorialBallSpawned += OnTutorialBallSpawned;

        currentLauncher.LaunchBall(currentAction);

        Debug.Log(
            $"[PASS TUTORIAL] Solicitando balón. " +
            $"Acción: {currentAction}. " +
            $"Aciertos: {successfulBalls}/{requiredCorrect}. " +
            $"Errores: {errorsCount}/{requiredErrors}.",
            this
        );
    }

    private void OnTutorialBallSpawned(ILaunchableBall ball)
    {
        if (!tutorialActive)
            return;

        currentLauncher.OnTutorialBallSpawned -= OnTutorialBallSpawned;

        if (ball is not DoubleHandPassBall passBall)
        {
            Debug.LogError(
                "[PASS TUTORIAL] El objeto lanzado no es DoubleHandPassBall.",
                this
            );

            StopTutorialReaction();
            return;
        }

        currentBall = passBall;
        waitingForResult = true;

        currentBall.OnCorrectCatch += OnCorrectCatch;
        currentBall.OnMiss += OnMiss;
        currentBall.OnCorrectPass += OnCorrectPass;
        currentBall.OnWrongCatch += OnWrongCatch;
    }

    // =====================================================
    // RESULTADOS DEL BALÓN
    // =====================================================

    private void OnCorrectCatch(float reactionTime)
    {
        RegisterResult(ExpectedResult.CorrectCatch);
    }

    private void OnMiss()
    {
        RegisterResult(ExpectedResult.MissCatch);
    }

    private void OnCorrectPass()
    {
        RegisterResult(ExpectedResult.CorrectPass);
    }

    private void OnWrongCatch(float reactionTime)
    {
        RegisterResult(ExpectedResult.WrongCatch);
    }


    private void RegisterResult(ExpectedResult actualResult)
    {
        if (!tutorialActive || !waitingForResult)
            return;

        waitingForResult = false;

        UnsubscribeFromCurrentBall();

        bool matchesExpected = actualResult == expectedResult;

        bool isErrorObjective =
            expectedResult == ExpectedResult.MissCatch ||
            expectedResult == ExpectedResult.WrongCatch;

        if (isErrorObjective)
        {
            // En estos pasos se cuenta el error específico
            // que el tutorial quiere enseñar.
            if (matchesExpected)
            {
                errorsCount++;

                Debug.Log(
                    $"[PASS TUTORIAL] Error esperado realizado: {actualResult}. " +
                    $"Progreso: {errorsCount}/{requiredErrors}.",
                    this
                );
            }
            else
            {
                Debug.Log(
                    $"[PASS TUTORIAL] Resultado recibido: {actualResult}. " +
                    $"Se esperaba: {expectedResult}. " +
                    "Este resultado no completa el objetivo.",
                    this
                );
            }

            if (errorsCount >= Mathf.Max(1, requiredErrors))
            {
                CompleteTutorialStep();
                return;
            }
        }
        else
        {
            // En los pasos normales se cuentan las respuestas correctas.
            if (matchesExpected)
            {
                successfulBalls++;

                Debug.Log(
                    $"[PASS TUTORIAL] Respuesta correcta: {actualResult}. " +
                    $"Progreso: {successfulBalls}/{requiredCorrect}.",
                    this
                );
            }
            else
            {
                Debug.Log(
                    $"[PASS TUTORIAL] Resultado incorrecto: {actualResult}. " +
                    $"Se esperaba: {expectedResult}.",
                    this
                );
            }

            if (successfulBalls >= Mathf.Max(1, requiredCorrect))
            {
                CompleteTutorialStep();
                return;
            }
        }

        nextBallCoroutine = StartCoroutine(WaitAndLaunchNextBall());
    }

    // =====================================================
    // SIGUIENTE BALÓN
    // =====================================================

    private IEnumerator WaitAndLaunchNextBall()
    {
        while (tutorialActive &&
               currentLauncher != null &&
               currentLauncher.HasActiveBall())
        {
            yield return null;
        }

        nextBallCoroutine = null;

        if (!tutorialActive || currentLauncher == null)
            yield break;

        yield return null;

        if (tutorialActive)
            LaunchNextBall();
    }

    // =====================================================
    // FINALIZAR PASO
    // =====================================================

    private void CompleteTutorialStep()
    {
        if (!tutorialActive)
            return;

        Action callback = completionCallback;

        StopTutorialReaction();

        callback?.Invoke();
    }

    // =====================================================
    // LIMPIEZA
    // =====================================================

    private void UnsubscribeFromCurrentBall()
    {
        if (currentBall == null)
            return;

        currentBall.OnCorrectCatch -= OnCorrectCatch;
        currentBall.OnMiss -= OnMiss;
        currentBall.OnCorrectPass -= OnCorrectPass;
        currentBall.OnWrongCatch -= OnWrongCatch;

        currentBall = null;
    }

    public void StopTutorialReaction()
    {
        tutorialActive = false;
        waitingForResult = false;

        if (nextBallCoroutine != null)
        {
            StopCoroutine(nextBallCoroutine);
            nextBallCoroutine = null;
        }

        UnsubscribeFromCurrentBall();

        if (currentLauncher != null)
        {
            currentLauncher.OnTutorialBallSpawned -=
                OnTutorialBallSpawned;

            currentLauncher.SetTutorialMode(false);
            currentLauncher.StopLauncher();

            currentLauncher = null;
        }

        completionCallback = null;
    }

    private void OnDisable()
    {
        StopTutorialReaction();
    }
}
