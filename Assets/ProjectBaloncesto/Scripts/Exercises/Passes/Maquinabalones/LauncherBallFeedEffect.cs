
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LauncherBallFeedEffect : MonoBehaviour
{
    // =========================================================
    // PREFAB Y OBJECT POOL
    // =========================================================

    [Header("Prefab visual")]
    [SerializeField] private GameObject ballPrefab;
    [SerializeField, Min(1)] private int initialPoolSize = 8;
    [SerializeField] private Transform poolContainer;

    private ObjectPool ballPool;

    // =========================================================
    // POSICIONES
    // =========================================================

    [Header("Posiciones")]
    [Tooltip("Ordenadas desde la posición superior hasta la inferior.")]
    [SerializeField] private LauncherBallSlot[] slots;

    // =========================================================
    // ANIMACIÓN
    // =========================================================

    [Header("Animación de movimiento")]
    [SerializeField, Min(0.01f)]
    private float movementDuration = 0.3f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Animación de llenado")]
    [Tooltip("Tiempo de espera entre la aparición de cada pelota.")]
    [SerializeField, Min(0f)]
    private float fillDelay = 0.1f;

    [Tooltip("Distancia desde la que baja cada pelota al llenar un slot.")]
    [SerializeField, Min(0f)]
    private float fillStartDistance = 0.25f;

    private bool isAnimating;
    private bool isProcessingPending;

    // =========================================================
    // ESTADO VISUAL
    // =========================================================

    // Disparos que todavía no han comenzado.
    // La cantidad siempre la recibe del manager.
    private int remainingLaunches;

    // Lanzamientos recibidos mientras otra animación estaba activa.
    private int pendingFeedAnimations;

    // =========================================================
    // INICIALIZACIÓN
    // =========================================================

    private void Awake()
    {
        if (ballPrefab == null)
        {
            Debug.LogError(
                "[LauncherBallFeedEffect] Falta el prefab visual.",
                this
            );
            return;
        }

        if (slots == null || slots.Length == 0)
        {
            slots = GetComponentsInChildren<LauncherBallSlot>(true);
        }

        if (poolContainer == null)
        {
            GameObject containerObject =
                new GameObject("LauncherBallFeedPool");

            containerObject.transform.SetParent(transform, false);
            poolContainer = containerObject.transform;
        }

        ballPool = new ObjectPool(
            ballPrefab,
            initialPoolSize,
            poolContainer
        );
    }

    // =========================================================
    // PREPARAR EL NIVEL
    // =========================================================

    public void InitializeForLevel(int totalShots)
    {
        if (ballPool == null)
            return;

        ResetFeed();

        if (slots == null || slots.Length == 0)
        {
            Debug.LogWarning(
                "[LauncherBallFeedEffect] No hay slots configurados.",
                this
            );
            return;
        }

        // La cantidad de disparos viene exclusivamente del manager.
        remainingLaunches = Mathf.Max(0, totalShots);

        if (remainingLaunches == 0)
            return;

        StartCoroutine(FillInitialSlots());
    }

    // =========================================================
    // LLENADO INICIAL ANIMADO
    // =========================================================


    private IEnumerator FillInitialSlots()
    {
        isAnimating = true;

        // La cantidad inicial de pelotas depende de los disparos
        // configurados para el nivel y de los slots disponibles.
        int visibleCount = Mathf.Min(
            remainingLaunches,
            slots.Length
        );

        // Llenar los slots en orden: primero el superior,
        // después el segundo, el tercero y así sucesivamente.
        for (int i = 0; i < visibleCount; i++)
        {
            LauncherBallSlot slot = slots[i];

            if (slot == null || slot.IsOccupied)
                continue;

            // Obtener una pelota nueva del Object Pool.
            GameObject ball = GetBall();

            if (ball == null)
                continue;

            Transform target = slot.transform;

            // La pelota comienza por encima de su slot.
            Vector3 startPosition =
                target.position + target.up * fillStartDistance;

            Quaternion startRotation = target.rotation;

            ball.transform.SetPositionAndRotation(
                startPosition,
                startRotation
            );

            // Reservar el slot para esta pelota.
            // La pelota anterior permanece en su propio slot.
            slot.SetBall(ball);

            // Animar la pelota hasta su posición definitiva.
            yield return StartCoroutine(
                MoveBall(
                    ball,
                    target,
                    startPosition,
                    startRotation
                )
            );

            // Esperar antes de hacer aparecer la siguiente pelota.
            if (fillDelay > 0f)
                yield return new WaitForSeconds(fillDelay);
        }

        isAnimating = false;

        // Procesar los lanzamientos que hayan ocurrido
        // durante el llenado inicial.
        ProcessPendingFeedAnimations();
    }

    // =========================================================
    // EVENTO DE LANZAMIENTO
    // =========================================================

    public void OnBallLaunched()
    {
        if (ballPool == null || remainingLaunches <= 0)
            return;

        // Se descuenta cada lanzamiento real, incluso si la
        // animación de llenado o descenso sigue en ejecución.
        remainingLaunches--;

        pendingFeedAnimations++;

        ProcessPendingFeedAnimations();
    }

    // =========================================================
    // PROCESAR ANIMACIONES PENDIENTES
    // =========================================================

    private void ProcessPendingFeedAnimations()
    {
        if (isAnimating || isProcessingPending)
            return;

        if (pendingFeedAnimations <= 0)
            return;

        StartCoroutine(ProcessPendingFeedAnimationsRoutine());
    }

    private IEnumerator ProcessPendingFeedAnimationsRoutine()
    {
        isProcessingPending = true;

        while (pendingFeedAnimations > 0)
        {
            pendingFeedAnimations--;

            yield return StartCoroutine(AnimateFeed());
        }

        isProcessingPending = false;

        // Comprobar si llegó otro evento al terminar la rutina.
        if (pendingFeedAnimations > 0)
            ProcessPendingFeedAnimations();
    }

    // =========================================================
    // DESCENSO Y REPOSICIÓN DE BALONES
    // =========================================================

    private IEnumerator AnimateFeed()
    {
        isAnimating = true;

        if (slots == null || slots.Length == 0)
        {
            isAnimating = false;
            yield break;
        }

        // Retirar la pelota de la posición inferior.
        LauncherBallSlot lastSlot = slots[slots.Length - 1];

        if (lastSlot != null && lastSlot.IsOccupied)
        {
            GameObject removedBall = lastSlot.RemoveBall();

            if (removedBall != null)
                ReturnBall(removedBall);
        }

        // Desplazar de abajo hacia arriba para evitar
        // sobrescribir los slots que todavía contienen pelotas.
        List<Coroutine> animations = new List<Coroutine>();

        for (int i = slots.Length - 2; i >= 0; i--)
        {
            LauncherBallSlot source = slots[i];
            LauncherBallSlot destination = slots[i + 1];

            if (source == null || destination == null)
                continue;

            if (!source.IsOccupied || destination.IsOccupied)
                continue;

            GameObject ball = source.RemoveBall();

            if (ball == null)
                continue;

            // Guardar la posición original ANTES de asignar
            // la pelota al siguiente slot.
            Vector3 startPosition = ball.transform.position;
            Quaternion startRotation = ball.transform.rotation;

            destination.SetBall(ball);

            animations.Add(
                StartCoroutine(
                    MoveBall(
                        ball,
                        destination.transform,
                        startPosition,
                        startRotation
                    )
                )
            );
        }

        // Esperar a que todas las pelotas terminen de bajar.
        foreach (Coroutine animation in animations)
        {
            yield return animation;
        }

        // Solo rellenar el slot superior si quedan suficientes
        // disparos pendientes para mantener la fila completa.
        if (remainingLaunches >= slots.Length)
        {
            LauncherBallSlot firstSlot = slots[0];

            if (firstSlot != null && !firstSlot.IsOccupied)
            {
                GameObject newBall = GetBall();

                if (newBall != null)
                {
                    Transform target = firstSlot.transform;

                    Vector3 startPosition =
                        target.position +
                        target.up * fillStartDistance;

                    Quaternion startRotation = target.rotation;

                    newBall.transform.SetPositionAndRotation(
                        startPosition,
                        startRotation
                    );

                    firstSlot.SetBall(newBall);

                    yield return StartCoroutine(
                        MoveBall(
                            newBall,
                            target,
                            startPosition,
                            startRotation
                        )
                    );
                }
            }
        }

        isAnimating = false;
    }

    // =========================================================
    // MOVIMIENTO SUAVE
    // =========================================================

    private IEnumerator MoveBall(
        GameObject ball,
        Transform destination,
        Vector3 startPosition,
        Quaternion startRotation)
    {
        if (ball == null || destination == null)
            yield break;

        Vector3 endPosition = destination.position;
        Quaternion endRotation = destination.rotation;

        float elapsed = 0f;

        // Restablecer el origen incluso si SetBall modificó
        // la posición o el padre del objeto.
        ball.transform.SetPositionAndRotation(
            startPosition,
            startRotation
        );

        while (elapsed < movementDuration)
        {
            if (ball == null || destination == null)
                yield break;

            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(
                elapsed / movementDuration
            );

            float curveT = movementCurve.Evaluate(t);

            ball.transform.position = Vector3.Lerp(
                startPosition,
                endPosition,
                curveT
            );

            ball.transform.rotation = Quaternion.Slerp(
                startRotation,
                endRotation,
                curveT
            );

            yield return null;
        }

        if (ball != null && destination != null)
        {
            ball.transform.SetPositionAndRotation(
                destination.position,
                destination.rotation
            );
        }
    }

    // =========================================================
    // OBJECT POOL
    // =========================================================

    private GameObject GetBall()
    {
        if (ballPool == null)
            return null;

        return ballPool.Get();
    }

    private void ReturnBall(GameObject ball)
    {
        if (ball == null || ballPool == null)
            return;

        ballPool.Return(ball);
    }

    // =========================================================
    // LIMPIEZA
    // =========================================================

    public void ResetFeed()
    {
        StopAllCoroutines();

        isAnimating = false;
        isProcessingPending = false;
        pendingFeedAnimations = 0;
        remainingLaunches = 0;

        ResetSlots();
    }

    private void ResetSlots()
    {
        if (slots == null)
            return;

        foreach (LauncherBallSlot slot in slots)
        {
            if (slot == null || !slot.IsOccupied)
                continue;

            GameObject ball = slot.RemoveBall();

            if (ball != null)
                ReturnBall(ball);
        }
    }

    // =========================================================
    // CONSULTAS
    // =========================================================

    public int GetRemainingLaunches()
    {
        return remainingLaunches;
    }
}
