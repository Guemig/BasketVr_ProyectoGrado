using System;
using UnityEngine;

public class CharacterIKController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;

    [SerializeField] private HandIKTarget rightHandTarget;
    [SerializeField] private HandIKTarget leftHandTarget;

    [Header("IK Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float rightHandWeight = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float leftHandWeight = 1f;

    [Header("Rotation")]
    [SerializeField] private bool useHandRotation = true;


    private void Reset()
    {
        animator = GetComponent<Animator>();
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        UpdateRightHandIK();
        UpdateLeftHandIK();
    }


    private void UpdateRightHandIK()
    {
        if (rightHandTarget == null)
            return;

        animator.SetIKPositionWeight(
            AvatarIKGoal.RightHand,
            rightHandWeight
        );

        animator.SetIKPosition(
            AvatarIKGoal.RightHand,
            rightHandTarget.transform.position
        );

        if (useHandRotation)
        {
            animator.SetIKRotationWeight(
                AvatarIKGoal.RightHand,
                rightHandWeight
            );

            animator.SetIKRotation(
                AvatarIKGoal.RightHand,
                rightHandTarget.transform.rotation
            );
        }
    }


    private void UpdateLeftHandIK()
    {
        if (leftHandTarget == null)
            return;

        animator.SetIKPositionWeight(
            AvatarIKGoal.LeftHand,
            leftHandWeight
        );

        animator.SetIKPosition(
            AvatarIKGoal.LeftHand,
            leftHandTarget.transform.position
        );

        if (useHandRotation)
        {
            animator.SetIKRotationWeight(
                AvatarIKGoal.LeftHand,
                leftHandWeight
            );

            animator.SetIKRotation(
                AvatarIKGoal.LeftHand,
                leftHandTarget.transform.rotation
            );
        }
    }


    // =====================================================
    // RIGHT HAND
    // =====================================================

    public void MoveRightHandMiddle(Action onActivated = null)
    {
        if (rightHandTarget == null)
            return;

        rightHandTarget.MoveToMiddle(onActivated);
    }


    public void MoveRightHandHigh(Action onActivated = null)
    {
        if (rightHandTarget == null)
            return;

        rightHandTarget.MoveToHigh(onActivated);
    }


    public void MoveRightHandLow(Action onActivated = null)
    {
        if (rightHandTarget == null)
            return;

        rightHandTarget.MoveToLow(onActivated);
    }


    public void ReturnRightHand()
    {
        if (rightHandTarget == null)
            return;

        rightHandTarget.ReturnToInitialPosition();
    }


    // =====================================================
    // LEFT HAND
    // =====================================================

    public void MoveLeftHandMiddle(Action onActivated = null)
    {
        if (leftHandTarget == null)
            return;

        leftHandTarget.MoveToMiddle(onActivated);
    }


    public void MoveLeftHandHigh(Action onActivated = null)
    {
        if (leftHandTarget == null)
            return;

        leftHandTarget.MoveToHigh(onActivated);
    }


    public void MoveLeftHandLow(Action onActivated = null)
    {
        if (leftHandTarget == null)
            return;

        leftHandTarget.MoveToLow(onActivated);
    }


    public void ReturnLeftHand()
    {
        if (leftHandTarget == null)
            return;

        leftHandTarget.ReturnToInitialPosition();
    }
}