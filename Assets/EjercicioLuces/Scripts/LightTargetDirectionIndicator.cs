
using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Representa una imagen asociada a una dirección del escenario.
/// </summary>
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

/// <summary>
/// Muestra las direcciones disponibles para cada nivel
/// y resalta la dirección del target durante su aviso.
/// </summary>
public class LightTargetDirectionIndicator : MonoBehaviour
{
    [Header("NIVEL 1")]
    [SerializeField] private LightTargetDirectionImage[] level1Directions;

    [Header("NIVEL 2")]
    [SerializeField] private LightTargetDirectionImage[] level2Directions;

    [Header("NIVEL 3")]
    [SerializeField] private LightTargetDirectionImage[] level3Directions;

    private int currentLevel;
    private LightTargetDirectionImage[] currentDirections;

    // =========================================================
    // INICIAR NIVEL
    // =========================================================

    public void SetLevel(int level)
    {
        HideAllDirections();

        currentLevel = level;
        currentDirections = GetDirectionsForLevel(level);

        if (currentDirections == null)
        {
            Debug.LogWarning(
                "LightTargetDirectionIndicator: nivel inválido: " + level
            );

            return;
        }

        foreach (LightTargetDirectionImage direction in currentDirections)
        {
            if (direction == null || direction.image == null)
                continue;

            direction.image.gameObject.SetActive(true);
            direction.image.color = direction.normalColor;
        }
    }

    // =========================================================
    // MOSTRAR DIRECCIÓN DEL TARGET
    // =========================================================

    public void ShowDirection(LightTargetReaction.Direction direction)
    {
        if (currentDirections == null)
            return;

        // Primero, todas las flechas vuelven a su color normal.
        ClearDirection();

        // Después, se resalta la dirección solicitada.
        foreach (LightTargetDirectionImage directionImage in currentDirections)
        {
            if (directionImage == null || directionImage.image == null)
                continue;

            if (directionImage.direction == direction)
            {
                directionImage.image.color = directionImage.activeColor;
                return;
            }
        }

        Debug.LogWarning(
            "LightTargetDirectionIndicator: no existe una imagen " +
            "configurada para la dirección " + direction +
            " en el nivel " + currentLevel + "."
        );
    }

    // =========================================================
    // LIMPIAR RESALTADO
    // =========================================================

    public void ClearDirection()
    {
        if (currentDirections == null)
            return;

        foreach (LightTargetDirectionImage direction in currentDirections)
        {
            if (direction == null || direction.image == null)
                continue;

            direction.image.color = direction.normalColor;
        }
    }

    // =========================================================
    // OCULTAR TODAS LAS DIRECCIONES
    // =========================================================

    public void HideAllDirections()
    {
        SetDirectionsActive(level1Directions, false);
        SetDirectionsActive(level2Directions, false);
        SetDirectionsActive(level3Directions, false);

        currentDirections = null;
        currentLevel = 0;
    }

    private void SetDirectionsActive(
        LightTargetDirectionImage[] directions,
        bool active)
    {
        if (directions == null)
            return;

        foreach (LightTargetDirectionImage direction in directions)
        {
            if (direction == null || direction.image == null)
                continue;

            direction.image.color = direction.normalColor;
            direction.image.gameObject.SetActive(active);
        }
    }

    // =========================================================
    // OBTENER DIRECCIONES DEL NIVEL
    // =========================================================

    private LightTargetDirectionImage[] GetDirectionsForLevel(int level)
    {
        switch (level)
        {
            case 1:
                return level1Directions;

            case 2:
                return level2Directions;

            case 3:
                return level3Directions;

            default:
                return null;
        }
    }
}
