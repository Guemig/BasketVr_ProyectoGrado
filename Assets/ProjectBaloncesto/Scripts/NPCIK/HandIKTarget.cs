using System;
using UnityEngine;

public class HandIKTarget : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform initialTarget;

    [SerializeField] private Transform reactionMiddleTarget;
    [SerializeField] private Transform reactionHighTarget;
    [SerializeField] private Transform reactionLowTarget;


    [Header("Movement By Level")]
    [SerializeField]
    private float[] movementDurations =
    {
        0.5f,
        0.4f,
        0.3f
    };


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
    private float currentMovementDuration;

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

        transform.position =
            initialTarget.position;

        transform.rotation =
            initialTarget.rotation;
    }


    private void Update()
    {
        // Pausable: no avanzar movimiento ni activar reacción mientras el ejercicio está pausado.
        if (ReactionExerciseController.IsPaused)
            return;

        if (!isMoving)
            return;

        movementTime += Time.deltaTime;

        float normalizedTime =
            movementTime /
            currentMovementDuration;

        normalizedTime =
            Mathf.Clamp01(normalizedTime);

        float curveValue =
            movementCurve.Evaluate(
                normalizedTime
            );

        transform.position =
            Vector3.Lerp(
                startPosition,
                targetPosition,
                curveValue
            );

        transform.rotation =
            Quaternion.Slerp(
                startRotation,
                targetRotation,
                curveValue
            );


        if (onReactionActivated != null &&
            normalizedTime >= reactionActivationProgress)
        {
            onReactionActivated.Invoke();
            onReactionActivated = null;
        }


        if (normalizedTime >= 1f)
        {
            transform.position =
                targetPosition;

            transform.rotation =
                targetRotation;

            isMoving = false;
        }
    }


    private void StraightenFinger(
        Transform finger)
    {
        if (finger == null)
            return;

        finger.localRotation =
            Quaternion.identity;
    }


    // =====================================================
    // MOVEMENT
    // =====================================================

    public void MoveToMiddle(
        ReactionLevelController.ReactionLevel level,
        Action onActivated = null)
    {
        MoveToTarget(
            reactionMiddleTarget,
            level,
            onActivated
        );
    }


    public void MoveToHigh(
        ReactionLevelController.ReactionLevel level,
        Action onActivated = null)
    {
        MoveToTarget(
            reactionHighTarget,
            level,
            onActivated
        );
    }


    public void MoveToLow(
        ReactionLevelController.ReactionLevel level,
        Action onActivated = null)
    {
        MoveToTarget(
            reactionLowTarget,
            level,
            onActivated
        );
    }


    public void ReturnToInitialPosition()
    {
        onReactionActivated = null;

        MoveToTarget(
            initialTarget,
            ReactionLevelController.ReactionLevel.Level1,
            null
        );
    }


    private void MoveToTarget(
        Transform target,
        ReactionLevelController.ReactionLevel level,
        Action onActivated)
    {
        if (target == null)
            return;


        startPosition =
            transform.position;

        startRotation =
            transform.rotation;


        targetPosition =
            target.position;

        targetRotation =
            target.rotation;


        movementTime = 0f;


        currentMovementDuration =
            GetMovementDuration(level);


        onReactionActivated =
            onActivated;


        isMoving = true;
    }


    private float GetMovementDuration(
        ReactionLevelController.ReactionLevel level)
    {
        int index =
            (int)level;


        if (movementDurations == null ||
            movementDurations.Length == 0)
        {
            return 0.5f;
        }


        if (index < 0 ||
            index >= movementDurations.Length)
        {
            return movementDurations[
                movementDurations.Length - 1
            ];
        }


        return Mathf.Max(
            0.01f,
            movementDurations[index]
        );
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


        if (movementDurations == null ||
            movementDurations.Length != levelCount)
        {
            float[] newDurations =
                new float[levelCount];


            for (int i = 0;
                 i < levelCount;
                 i++)
            {
                if (movementDurations != null &&
                    i < movementDurations.Length)
                {
                    newDurations[i] =
                        movementDurations[i];
                }
                else
                {
                    newDurations[i] =
                        0.5f;
                }
            }


            movementDurations =
                newDurations;
        }
    }

#endif
}