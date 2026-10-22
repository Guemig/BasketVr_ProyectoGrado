
using System;
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
    [SerializeField] private DoubleHandGripController gripController;

    private BallLauncher currentLauncher;
    private DoubleHandPassBall currentBall;

    private Action<bool> resultCallback;
    private ExpectedResult expectedResult;

    private bool waitingForResult;

    public void StartDemonstration(
        DoubleHandPassBall.BallAction action,
        ExpectedResult expected,
        Action<bool> onResult)
    {
        if (waitingForResult)
        {
            Debug.LogWarning(
                "[PASS TUTORIAL] Ya hay una demostración en curso.",
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

            onResult?.Invoke(false);
            return;
        }

        if (!machineManager.ConfigureLevel(1))
        {
            Debug.LogError(
                "[PASS TUTORIAL] No se pudo configurar el nivel.",
                this
            );

            onResult?.Invoke(false);
            return;
        }

        currentLauncher = machineManager.GetNextLauncher();

        if (currentLauncher == null)
        {
            Debug.LogError(
                "[PASS TUTORIAL] No se encontró una máquina disponible.",
                this
            );

            onResult?.Invoke(false);
            return;
        }

        expectedResult = expected;
        resultCallback = onResult;
        waitingForResult = true;

        // Activar el modo tutorial sin iniciar las estadísticas normales.
        currentLauncher.SetTutorialMode(true);

        if (gripController != null)
            gripController.SetTutorialMode(true);

        currentLauncher.OnTutorialBallSpawned += OnTutorialBallSpawned;

        currentLauncher.LaunchBall(action);
    }

    private void OnTutorialBallSpawned(ILaunchableBall ball)
    {
        if (!waitingForResult)
            return;

        // Evitar aceptar un objeto que no sea el balón esperado.
        if (ball is not DoubleHandPassBall passBall)
        {
            Debug.LogError(
                "[PASS TUTORIAL] El objeto lanzado no es DoubleHandPassBall.",
                this
            );

            // No se interpreta como un fallo de captura o pase.
            // Es un error técnico de la demostración.
            Resolve(false);
            return;
        }

        if (currentLauncher != null)
        {
            currentLauncher.OnTutorialBallSpawned -=
                OnTutorialBallSpawned;
        }

        currentBall = passBall;

        currentBall.OnCorrectCatch += OnCorrectCatch;
        currentBall.OnMiss += OnMiss;
        currentBall.OnCorrectPass += OnCorrectPass;
        currentBall.OnWrongCatch += OnWrongCatch;
    }

    private void OnCorrectCatch(float reactionTime)
    {
        Resolve(ExpectedResult.CorrectCatch);
    }

    private void OnMiss()
    {
        Resolve(ExpectedResult.MissCatch);
    }

    private void OnCorrectPass()
    {
        Resolve(ExpectedResult.CorrectPass);
    }

    private void OnWrongCatch(float reactionTime)
    {
        Resolve(ExpectedResult.WrongCatch);
    }

    private void Resolve(ExpectedResult actualResult)
    {
        Resolve(actualResult == expectedResult);
    }

    private void Resolve(bool success)
    {
        if (!waitingForResult)
            return;

        waitingForResult = false;

        UnsubscribeFromBall();

        // Guardar las referencias antes de limpiar el estado.
        BallLauncher launcher = currentLauncher;
        currentLauncher = null;

        if (launcher != null)
            launcher.SetTutorialMode(false);

        Action<bool> callback = resultCallback;
        resultCallback = null;

        callback?.Invoke(success);
    }

    private void UnsubscribeFromBall()
    {
        if (currentBall != null)
        {
            currentBall.OnCorrectCatch -= OnCorrectCatch;
            currentBall.OnMiss -= OnMiss;
            currentBall.OnCorrectPass -= OnCorrectPass;
            currentBall.OnWrongCatch -= OnWrongCatch;

            currentBall = null;
        }

        if (currentLauncher != null)
        {
            currentLauncher.OnTutorialBallSpawned -=
                OnTutorialBallSpawned;
        }
    }

    public void StopTutorialReaction()
    {
        waitingForResult = false;

        UnsubscribeFromBall();

        if (currentLauncher != null)
        {
            currentLauncher.OnTutorialBallSpawned -=
                OnTutorialBallSpawned;

            currentLauncher.SetTutorialMode(false);
            currentLauncher.StopLauncher();

            currentLauncher = null;
        }

        if (gripController != null)
            gripController.SetTutorialMode(false);

        resultCallback = null;
    }

    private void OnDisable()
    {
        StopTutorialReaction();
    }
}
