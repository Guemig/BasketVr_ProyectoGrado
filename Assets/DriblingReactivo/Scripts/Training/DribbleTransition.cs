using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class DribbleTransition : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerCamera;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Audio")]
    [SerializeField] private AudioSource transitionAudioSource;
    [SerializeField] private AudioClip transitionSound;

    [Header("Posicion")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Angulos")]
    [SerializeField] private float startAngle = 30f;
    [SerializeField] private float middleAngle = -30f;
    [SerializeField] private float finalAngle = 180f;

    [Header("Movimiento")]
    [SerializeField] private float slowMovementDuration = 4f;
    [SerializeField] private float fastMovementDuration = 0.5f;

    [Header("Curvas")]
    [SerializeField]
    private AnimationCurve slowMovementCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    [SerializeField]
    private AnimationCurve fastMovementCurve =
        AnimationCurve.EaseInOut(
            0f,
            0f,
            1f,
            1f
        );

    private Coroutine transitionCoroutine;
    private bool isTransitioning;

    public bool IsTransitioning =>
        isTransitioning;

    public void SetLevelLabel(
        string label)
    {
        if (levelText == null)
            return;

        levelText.text =
            label;
    }

    private void LateUpdate()
    {
        if (isTransitioning)
            return;

        SetPositionAtAngle(
            finalAngle
        );
    }

    public void PlayTransition(
        Action onComplete)
    {
        if (transitionCoroutine != null)
        {
            StopCoroutine(
                transitionCoroutine
            );
        }

        transitionCoroutine =
            StartCoroutine(
                TransitionCoroutine(
                    onComplete
                )
            );
    }

    private IEnumerator TransitionCoroutine(
        Action onComplete)
    {
        if (playerCamera == null)
        {
            onComplete?.Invoke();
            yield break;
        }

        isTransitioning =
            true;

        SetPositionAtAngle(
            startAngle
        );

        PlayTransitionSound();

        yield return StartCoroutine(
            MoveBetweenAngles(
                startAngle,
                middleAngle,
                slowMovementDuration,
                slowMovementCurve
            )
        );

        yield return StartCoroutine(
            MoveBetweenAngles(
                middleAngle,
                finalAngle,
                fastMovementDuration,
                fastMovementCurve
            )
        );

        SetPositionAtAngle(
            finalAngle
        );

        isTransitioning =
            false;

        transitionCoroutine =
            null;

        onComplete?.Invoke();
    }

    private IEnumerator MoveBetweenAngles(
        float start,
        float target,
        float duration,
        AnimationCurve curve)
    {
        float elapsed =
            0f;

        while (elapsed < duration)
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    duration
                );

            t =
                curve.Evaluate(
                    t
                );

            float currentAngle =
                Mathf.Lerp(
                    start,
                    target,
                    t
                );

            SetPositionAtAngle(
                currentAngle
            );

            yield return null;
        }

        SetPositionAtAngle(
            target
        );
    }

    private void SetPositionAtAngle(
        float angle)
    {
        if (playerCamera == null)
            return;

        Vector3 forward =
            playerCamera.forward;

        forward.y =
            0f;

        if (forward.sqrMagnitude <
            0.001f)
        {
            return;
        }

        forward.Normalize();

        Vector3 direction =
            Quaternion.Euler(
                0f,
                angle,
                0f
            ) *
            forward;

        Vector3 targetPosition =
            playerCamera.position +
            direction *
            radius;

        targetPosition.y +=
            heightOffset;

        transform.position =
            targetPosition;

        LookAtPlayer(
            playerCamera.position
        );
    }

    private void LookAtPlayer(
        Vector3 playerPosition)
    {
        Vector3 direction =
            transform.position -
            playerPosition;

        direction.y =
            0f;

        if (direction.sqrMagnitude <
            0.001f)
        {
            return;
        }

        transform.rotation =
            Quaternion.LookRotation(
                direction
            );
    }

    private void PlayTransitionSound()
    {
        if (transitionAudioSource == null)
            return;

        if (transitionSound == null)
            return;

        transitionAudioSource.Stop();

        transitionAudioSource.PlayOneShot(
            transitionSound
        );
    }
}