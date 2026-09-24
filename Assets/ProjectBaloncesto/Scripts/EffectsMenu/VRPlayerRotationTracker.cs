using System.Collections.Generic;
using UnityEngine;

public class VRPlayerRotationTracker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerCamera;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationThreshold = 90f;

    private readonly List<IVRPlayerRotationObserver> observers = new();

    private Vector3 lastForward;

    public Transform PlayerCamera => playerCamera;

    private void Start()
    {
        if (playerCamera == null)
        {
            Debug.LogError(
                "VRPlayerRotationTracker: No se asignó la cámara."
            );

            return;
        }

        lastForward = GetFlatForward(playerCamera.forward);
    }

    private void Update()
    {
        if (playerCamera == null)
            return;

        Vector3 currentForward =
            GetFlatForward(playerCamera.forward);

        float angle =
            Vector3.Angle(
                lastForward,
                currentForward
            );

        if (angle >= rotationThreshold)
        {
            NotifyObservers();

            lastForward = currentForward;
        }
    }

    public void RegisterObserver(
        IVRPlayerRotationObserver observer)
    {
        if (observer == null)
            return;

        if (observers.Contains(observer))
            return;

        observers.Add(observer);
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
            observer.OnPlayerRotation();
        }
    }

    private Vector3 GetFlatForward(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return lastForward;

        return direction.normalized;
    }
}