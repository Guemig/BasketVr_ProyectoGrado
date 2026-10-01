using System.Collections;
using UnityEngine;

public class VRMenuFollowPlayer :
    MonoBehaviour,
    IVRPlayerRotationObserver
{
    [Header("References")]
    [SerializeField] private VRPlayerRotationTracker playerTracker;

    [Header("Position")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float heightOffset = 0f;

    [Header("Follow Height")]
    [SerializeField] private bool followPlayerHeight = true;

    [Header("Menu Angle")]
    [SerializeField] private float maxAngle = 90f;

    [Header("Movement")]
    [SerializeField] private float movementDuration = 0.7f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Coroutine movementCoroutine;

    private void OnEnable()
    {
        if (playerTracker == null)
            return;

        playerTracker.RegisterObserver(this);

        CheckMenuPosition();
    }

    private void OnDisable()
    {
        if (playerTracker == null)
            return;

        playerTracker.UnregisterObserver(this);

        SetInitialPosition();
    }

    private void SetInitialPosition()
    {
        Transform playerCamera =
            playerTracker.PlayerCamera;

        if (playerCamera == null)
            return;

        Vector3 forward =
            playerCamera.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
            return;

        forward.Normalize();

        Vector3 targetPosition =
            playerCamera.position +
            forward * radius;

        targetPosition.y =
            playerCamera.position.y +
            heightOffset;

        transform.position =
            targetPosition;

        LookAtPlayer(
            playerCamera.position
        );
    }

    public void OnPlayerTransformChanged()
    {
        CheckMenuPosition();
    }

    private void CheckMenuPosition()
    {
        Transform playerCamera =
            playerTracker.PlayerCamera;

        if (playerCamera == null)
            return;

        // -----------------------------------------
        // SEGUIR ALTURA DEL JUGADOR
        // -----------------------------------------

        if (followPlayerHeight)
        {
            Vector3 currentPosition =
                transform.position;

            currentPosition.y =
                playerCamera.position.y +
                heightOffset;

            transform.position =
                currentPosition;
        }

        // -----------------------------------------
        // LÓGICA DE ROTACIÓN
        // -----------------------------------------

        Vector3 playerForward =
            playerCamera.forward;

        playerForward.y = 0f;

        if (playerForward.sqrMagnitude < 0.001f)
            return;

        playerForward.Normalize();

        Vector3 toMenu =
            transform.position -
            playerCamera.position;

        toMenu.y = 0f;

        if (toMenu.sqrMagnitude < 0.001f)
        {
            RepositionInFront();
            return;
        }

        toMenu.Normalize();

        float dot =
            Vector3.Dot(
                playerForward,
                toMenu
            );

        float angle =
            Mathf.Acos(
                Mathf.Clamp(dot, -1f, 1f)
            ) * Mathf.Rad2Deg;

        if (angle > maxAngle)
        {
            RepositionInFront();
        }
    }

    private void RepositionInFront()
    {
        if (playerTracker == null)
            return;

        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }

        movementCoroutine =
            StartCoroutine(
                MoveInFront()
            );
    }

    private IEnumerator MoveInFront()
    {
        Transform playerCamera =
            playerTracker.PlayerCamera;

        Vector3 playerPosition =
            playerCamera.position;

        Vector3 forward =
            playerCamera.forward;

        forward.y = 0f;

        if (forward.sqrMagnitude < 0.001f)
        {
            movementCoroutine = null;
            yield break;
        }

        forward.Normalize();

        Vector3 targetPosition =
            playerPosition +
            forward * radius;

        targetPosition.y =
            playerCamera.position.y +
            heightOffset;

        Vector3 startDirection =
            GetFlatDirection(
                transform.position -
                playerPosition
            );

        Vector3 targetDirection =
            GetFlatDirection(
                targetPosition -
                playerPosition
            );

        if (startDirection == Vector3.zero)
        {
            startDirection =
                targetDirection;
        }

        float totalAngle =
            Vector3.SignedAngle(
                startDirection,
                targetDirection,
                Vector3.up
            );

        float elapsed = 0f;

        while (elapsed < movementDuration)
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    movementDuration
                );

            t =
                movementCurve.Evaluate(t);

            float currentAngle =
                totalAngle * t;

            Vector3 currentDirection =
                Quaternion.Euler(
                    0f,
                    currentAngle,
                    0f
                ) * startDirection;

            Vector3 currentPosition =
                playerPosition +
                currentDirection * radius;

            // -----------------------------------------
            // ALTURA
            // -----------------------------------------

            if (followPlayerHeight)
            {
                currentPosition.y =
                    playerCamera.position.y +
                    heightOffset;
            }
            else
            {
                currentPosition.y =
                    targetPosition.y;
            }

            transform.position =
                currentPosition;

            LookAtPlayer(
                playerCamera.position
            );

            yield return null;
        }

        // -----------------------------------------
        // POSICIÓN FINAL
        // -----------------------------------------

        targetPosition.y =
            followPlayerHeight
                ? playerCamera.position.y +
                  heightOffset
                : targetPosition.y;

        transform.position =
            targetPosition;

        LookAtPlayer(
            playerCamera.position
        );

        movementCoroutine = null;
    }

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

    private Vector3 GetFlatDirection(
        Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return Vector3.zero;

        return direction.normalized;
    }
}