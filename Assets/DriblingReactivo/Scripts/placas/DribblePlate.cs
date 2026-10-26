using System;
using UnityEngine;

public class DribblePlate : MonoBehaviour
{
    public enum PlateType
    {
        Off,
        Green,
        Orange
    }

    [SerializeField] private Renderer plateRenderer;

    [Header("Materiales")]
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material greenMaterial;
    [SerializeField] private Material orangeMaterial;

    private PlateType plateType = PlateType.Off;
    private bool canDetectBall;
    private bool isVisible;

    public PlateType Type => plateType;
    public bool IsVisible => isVisible;

    public event Action<DribblePlate> OnBallTouched;
    public event Action<DribblePlate> OnStateChanged;

    public void ShowOff()
    {
        gameObject.SetActive(true);

        isVisible = true;
        plateType = PlateType.Off;
        canDetectBall = false;

        UpdateMaterial();

        OnStateChanged?.Invoke(this);
    }

    public void ShowGreen()
    {
        gameObject.SetActive(true);

        isVisible = true;
        plateType = PlateType.Green;
        canDetectBall = true;

        UpdateMaterial();

        OnStateChanged?.Invoke(this);
    }

    public void ShowOrange()
    {
        gameObject.SetActive(true);

        isVisible = true;
        plateType = PlateType.Orange;
        canDetectBall = true;

        UpdateMaterial();

        OnStateChanged?.Invoke(this);
    }

    public void Hide()
    {
        canDetectBall = false;
        isVisible = false;
        plateType = PlateType.Off;

        OnStateChanged?.Invoke(this);

        gameObject.SetActive(false);
    }

    private void UpdateMaterial()
    {
        if (plateRenderer == null)
            return;

        switch (plateType)
        {
            case PlateType.Off:
                plateRenderer.material = offMaterial;
                break;

            case PlateType.Green:
                plateRenderer.material = greenMaterial;
                break;

            case PlateType.Orange:
                plateRenderer.material = orangeMaterial;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!canDetectBall)
            return;

        if (!other.CompareTag("DribbleBall"))
            return;

        OnBallTouched?.Invoke(this);
    }
}