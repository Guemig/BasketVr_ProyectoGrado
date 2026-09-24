using System;
using UnityEngine;

public class HandIKTarget : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform initialTarget;

    [SerializeField] private Transform reactionMiddleTarget;
    [SerializeField] private Transform reactionHighTarget;
    [SerializeField] private Transform reactionLowTarget;

    [Header("Movement")]
    [SerializeField] private float movementDuration = 0.5f;

    [SerializeField]
    private AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Reaction Activation")]
    [SerializeField, Range(0f, 1f)]
    private float reactionActivationProgress = 0.8f;

    [Header("Finger Bones")]
    [SerializeField] private Transform indexFinger;
    [SerializeField] private Transform middleFinger;
    [SerializeField] private Transform ringFinger;
    [SerializeField] private Transform littleFinger;

    private Vector3 startPosition;
    private Vector3 targetPosition;

    private Quaternion startRotation;
    private Quaternion targetRotation;

    private float movementTime;

    private bool isMoving;

    private Action onReactionActivated;


    private void Awake()
    {
        StraightenFinger(indexFinger);
        StraightenFinger(middleFinger);
        StraightenFinger(ringFinger);
        StraightenFinger(littleFinger);
    }


    private void Start()
    {
        if (initialTarget == null)
            return;

        transform.position = initialTarget.position;
        transform.rotation = initialTarget.rotation;
    }


    private void Update()
    {
        if (!isMoving)
            return;

        movementTime += Time.deltaTime;

        float normalizedTime = movementTime / movementDuration;
        normalizedTime = Mathf.Clamp01(normalizedTime);

        float curveValue = movementCurve.Evaluate(normalizedTime);

        transform.position = Vector3.Lerp(
            startPosition,
            targetPosition,
            curveValue
        );

        transform.rotation = Quaternion.Slerp(
            startRotation,
            targetRotation,
            curveValue
        );


        // Activa la reacción cuando alcanza
        // el porcentaje configurado del movimiento.
        if (onReactionActivated != null &&
            normalizedTime >= reactionActivationProgress)
        {
            onReactionActivated.Invoke();
            onReactionActivated = null;
        }


        if (normalizedTime >= 1f)
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;

            isMoving = false;
        }
    }


    private void StraightenFinger(Transform finger)
    {
        if (finger == null)
            return;

        finger.localRotation = Quaternion.identity;
    }


    // =====================================================
    // MOVEMENT
    // =====================================================

    public void MoveToMiddle(Action onActivated = null)
    {
        MoveToTarget(
            reactionMiddleTarget,
            onActivated
        );
    }


    public void MoveToHigh(Action onActivated = null)
    {
        MoveToTarget(
            reactionHighTarget,
            onActivated
        );
    }


    public void MoveToLow(Action onActivated = null)
    {
        MoveToTarget(
            reactionLowTarget,
            onActivated
        );
    }


    public void ReturnToInitialPosition()
    {
        // La vuelta a la posición inicial no necesita
        // activar ninguna reacción.
        onReactionActivated = null;

        MoveToTarget(
            initialTarget,
            null
        );
    }


    private void MoveToTarget(
        Transform target,
        Action onActivated
    )
    {
        if (target == null)
            return;

        startPosition = transform.position;
        startRotation = transform.rotation;

        targetPosition = target.position;
        targetRotation = target.rotation;

        movementTime = 0f;

        onReactionActivated = onActivated;

        isMoving = true;
    }
}