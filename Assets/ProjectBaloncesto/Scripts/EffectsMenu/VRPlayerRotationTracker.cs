using System.Collections.Generic;
using UnityEngine;

public class VRPlayerRotationTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;

    [Header("Rotation")]
    [SerializeField] private float rotationThreshold = 90f;

    [Header("Height")]
    [SerializeField] private float heightThreshold = 0.01f;

    private readonly List<IVRPlayerRotationObserver> observers =
        new List<IVRPlayerRotationObserver>();

    private Vector3 lastForward;
    private float lastHeight;

    public Transform PlayerCamera => playerCamera;

    private void Start()
    {
        if (playerCamera == null)
            return;

        lastForward =
            GetFlatForward(playerCamera.forward);

        lastHeight =
            playerCamera.position.y;
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        CheckPlayerMovement();
    }

    private void CheckPlayerMovement()
    {
        Vector3 currentForward =
            GetFlatForward(playerCamera.forward);

        float angle =
            Vector3.Angle(
                lastForward,
                currentForward
            );

        float currentHeight =
            playerCamera.position.y;

        float heightDifference =
            Mathf.Abs(
                currentHeight -
                lastHeight
            );

        bool rotationChanged =
            angle >= rotationThreshold;

        bool heightChanged =
            heightDifference >= heightThreshold;

        if (rotationChanged || heightChanged)
        {
            NotifyObservers();

            lastForward =
                currentForward;

            lastHeight =
                currentHeight;
        }
    }

    public void RegisterObserver(
        IVRPlayerRotationObserver observer)
    {
        if (observer == null)
            return;

        if (!observers.Contains(observer))
        {
            observers.Add(observer);
        }
    }

    public void UnregisterObserver(
        IVRPlayerRotationObserver observer)
    {
        if (observer == null)
            return;

        observers.Remove(observer);
    }

    private void NotifyObservers()
    {
        foreach (IVRPlayerRotationObserver observer in observers)
        {
            observer.OnPlayerTransformChanged();
        }
    }

    private Vector3 GetFlatForward(
        Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return lastForward;

        return direction.normalized;
    }
}