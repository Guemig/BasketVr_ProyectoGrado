
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LightTargetDirectionImage
{
    [Header("Dirección")]
    public LightTargetReaction.Direction direction;

    [Header("Imagen del Canvas")]
    public Image image;

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.yellow;
}

public class LightTargetDirectionIndicator : MonoBehaviour
{
    [Header("NIVEL 1")]
    [SerializeField] private LightTargetDirectionImage[] level1Directions;

    [Header("NIVEL 2")]
    [SerializeField] private LightTargetDirectionImage[] level2Directions;

    [Header("NIVEL 3")]
    [SerializeField] private LightTargetDirectionImage[] level3Directions;

    [Header("TUTORIAL (opcional)")]
    [SerializeField] private LightTargetDirectionImage[] tutorialDirections;

    private int currentLevel;
    private LightTargetDirectionImage[] currentDirections;

    public void SetLevel(int level)
    {
        HideAllDirections();

        currentLevel = level;
        currentDirections = GetDirectionsForLevel(level);

        ShowCurrentDirections();
    }

    // Se conserva para los lugares que ya llaman este método.
    public void SetTutorialMode()
    {
        SetTutorialMode(1);
    }

    // Selecciona las imágenes del nivel del target tutorial.
    public void SetTutorialMode(int level)
    {
        HideAllDirections();

        currentLevel = level;

        if (tutorialDirections != null &&
            tutorialDirections.Length > 0)
        {
            currentDirections = tutorialDirections;
        }
        else
        {
            currentDirections = GetDirectionsForLevel(level);

            // Respaldo para targets que no pertenecen
            // a ninguno de los grupos de entrenamiento.
            if (currentDirections == null)
                currentDirections = level1Directions;
        }

        ShowCurrentDirections();
    }

    private void ShowCurrentDirections()
    {
        if (currentDirections == null)
        {
            Debug.LogWarning(
                "LightTargetDirectionIndicator: no hay direcciones configuradas."
            );
            return;
        }

        foreach (LightTargetDirectionImage item in currentDirections)
        {
            if (item == null || item.image == null)
                continue;

            item.image.gameObject.SetActive(true);
            item.image.color = item.normalColor;
        }
    }

    public void ShowDirection(LightTargetReaction.Direction direction)
    {
        if (currentDirections == null)
        {
            Debug.LogWarning(
                "LightTargetDirectionIndicator: no se ha seleccionado " +
                "un conjunto de direcciones."
            );
            return;
        }

        ClearDirection();

        foreach (LightTargetDirectionImage item in currentDirections)
        {
            if (item == null || item.image == null)
                continue;

            if (item.direction != direction)
                continue;

            item.image.color = item.activeColor;
            return;
        }

        Debug.LogWarning(
            "No existe una imagen configurada para la dirección " +
            direction + " en el nivel " + currentLevel + "."
        );
    }

    public void ClearDirection()
    {
        if (currentDirections == null)
            return;

        foreach (LightTargetDirectionImage item in currentDirections)
        {
            if (item == null || item.image == null)
                continue;

            item.image.color = item.normalColor;
        }
    }

    public void HideAllDirections()
    {
        SetDirectionsActive(level1Directions, false);
        SetDirectionsActive(level2Directions, false);
        SetDirectionsActive(level3Directions, false);
        SetDirectionsActive(tutorialDirections, false);

        currentDirections = null;
        currentLevel = 0;
    }

    private void SetDirectionsActive(
        LightTargetDirectionImage[] directions,
        bool active)
    {
        if (directions == null)
            return;

        foreach (LightTargetDirectionImage item in directions)
        {
            if (item == null || item.image == null)
                continue;

            item.image.color = item.normalColor;
            item.image.gameObject.SetActive(active);
        }
    }

    private LightTargetDirectionImage[] GetDirectionsForLevel(int level)
    {
        switch (level)
        {
            case 1: return level1Directions;
            case 2: return level2Directions;
            case 3: return level3Directions;
            default: return null;
        }
    }
}
