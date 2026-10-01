using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class ReactionTransition : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;

    [SerializeField] private AudioSource transitionAudioSource;

    [SerializeField] private TextMeshProUGUI levelText;


    [Header("Position")]
    [SerializeField] private float radius = 2f;

    [SerializeField] private float heightOffset = 0f;


    [Header("Angles")]
    [SerializeField] private float startAngle = 30f;

    [SerializeField] private float middleAngle = -30f;

    [SerializeField] private float finalAngle = 180f;


    [Header("Movement")]
    [SerializeField] private float slowMovementDuration = 4f;

    [SerializeField] private float fastMovementDuration = 0.5f;


    [Header("Curves")]
    [SerializeField]
    private AnimationCurve slowMovementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [SerializeField]
    private AnimationCurve fastMovementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);


    private Coroutine transitionCoroutine;

    private bool isTransitioning;


    // =====================================================
    // PUBLIC API
    // =====================================================

    public void SetLevelLabel(string label)
    {
        if (levelText == null)
            return;

        levelText.text = label;
    }


    // =====================================================
    // UPDATE
    // =====================================================

    private void LateUpdate()
    {
        if (isTransitioning)
            return;


        // Mientras está esperando,
        // permanece siempre detrás del jugador.
        SetPositionAtAngle(finalAngle);
    }


    // =====================================================
    // START TRANSITION
    // =====================================================

    public void PlayTransition(Action onComplete)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }


        transitionCoroutine =
            StartCoroutine(
                TransitionCoroutine(
                    onComplete
                )
            );
    }


    // =====================================================
    // TRANSITION
    // =====================================================

    private IEnumerator TransitionCoroutine(
        Action onComplete)
    {
        Debug.Log(
            "[TRANSITION] Starting level transition."
        );


        if (playerCamera == null)
        {
            Debug.LogWarning(
                "[TRANSITION] Player Camera is missing."
            );

            onComplete?.Invoke();

            yield break;
        }


        isTransitioning = true;


        if (transitionAudioSource != null)
        {
            transitionAudioSource.Play();
        }


        // -------------------------------------------------
        // START
        // +30°
        // -------------------------------------------------

        SetPositionAtAngle(
            startAngle
        );


        // -------------------------------------------------
        // SLOW MOVEMENT
        // +30° → -30°
        // -------------------------------------------------

        yield return StartCoroutine(
            MoveBetweenAngles(
                startAngle,
                middleAngle,
                slowMovementDuration,
                slowMovementCurve
            )
        );


        // -------------------------------------------------
        // FAST MOVEMENT
        // -30° → 180°
        // -------------------------------------------------

        yield return StartCoroutine(
            MoveBetweenAngles(
                middleAngle,
                finalAngle,
                fastMovementDuration,
                fastMovementCurve
            )
        );


        // -------------------------------------------------
        // FINAL POSITION
        // 180°
        // -------------------------------------------------

        SetPositionAtAngle(
            finalAngle
        );


        Debug.Log(
            "[TRANSITION] Transition completed."
        );


        isTransitioning = false;

        transitionCoroutine = null;


        onComplete?.Invoke();
    }


    // =====================================================
    // MOVE BETWEEN ANGLES
    // =====================================================

    private IEnumerator MoveBetweenAngles(
        float startAngle,
        float targetAngle,
        float duration,
        AnimationCurve curve)
    {
        float elapsed = 0f;


        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;


            float t =
                Mathf.Clamp01(
                    elapsed / duration
                );


            t = curve.Evaluate(t);


            float currentAngle =
                Mathf.Lerp(
                    startAngle,
                    targetAngle,
                    t
                );


            SetPositionAtAngle(
                currentAngle
            );


            yield return null;
        }


        SetPositionAtAngle(
            targetAngle
        );
    }


    // =====================================================
    // POSITION
    // =====================================================

    private void SetPositionAtAngle(
        float angle)
    {
        if (playerCamera == null)
            return;


        Vector3 forward =
            playerCamera.forward;

        forward.y = 0f;


        if (forward.sqrMagnitude < 0.001f)
            return;


        forward.Normalize();


        Vector3 direction =
            Quaternion.Euler(
                0f,
                angle,
                0f
            ) * forward;


        Vector3 targetPosition =
            playerCamera.position +
            direction * radius;


        targetPosition.y += heightOffset;


        transform.position =
            targetPosition;


        LookAtPlayer(
            playerCamera.position
        );
    }


    // =====================================================
    // LOOK AT PLAYER
    // =====================================================

    private void LookAtPlayer(
        Vector3 playerPosition)
    {
        Vector3 direction =
            transform.position -
            playerPosition;


        direction.y = 0f;


        if (direction.sqrMagnitude < 0.001f)
            return;


        transform.rotation =
            Quaternion.LookRotation(
                direction
            );
    }
}