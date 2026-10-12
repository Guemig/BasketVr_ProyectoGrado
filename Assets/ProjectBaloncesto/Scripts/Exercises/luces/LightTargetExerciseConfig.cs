
using UnityEngine;

public class LightTargetExerciseConfig : MonoBehaviour
{
    // =========================================================
    // GRUPOS DE NIVELES
    // =========================================================

    [Header("GRUPOS DE NIVELES")]
    [SerializeField] private GameObject level1Group;
    [SerializeField] private GameObject level2Group;
    [SerializeField] private GameObject level3Group;

    // =========================================================
    // CANTIDAD DE REACCIONES
    // =========================================================

    [Header("CANTIDAD DE REACCIONES POR NIVEL")]
    [SerializeField]
    private int[] reactionsRequiredByLevel =
    {
        10, 15, 20
    };

    // =========================================================
    // TIEMPO TOTAL
    // =========================================================

    [Header("TIEMPO TOTAL DE REACCIÓN POR NIVEL")]
    [SerializeField]
    private float[] totalTimeByLevel =
    {
        1.5f, 1.2f, 1.0f
    };

    // =========================================================
    // PORCENTAJE DE AVISO
    // =========================================================

    [Header("PORCENTAJE DEL TIEMPO PARA EL AVISO")]
    [Range(0f, 100f)]
    [SerializeField]
    private float warningPercentage = 20f;

    // =========================================================
    // ESPERA ENTRE TARGETS
    // =========================================================

    [Header("ESPERA ENTRE TARGETS POR NIVEL")]
    [SerializeField]
    private float[] delayBetweenTargetsByLevel =
    {
        0.5f, 0.4f, 0.3f
    };

    // =========================================================
    // PROPIEDADES PÚBLICAS
    // =========================================================

    public GameObject Level1Group => level1Group;
    public GameObject Level2Group => level2Group;
    public GameObject Level3Group => level3Group;

    // =========================================================
    // CANTIDAD DE REACCIONES
    // =========================================================

    public int GetRequiredReactions(int currentLevel)
    {
        int index = currentLevel - 1;

        if (reactionsRequiredByLevel == null ||
            index < 0 ||
            index >= reactionsRequiredByLevel.Length)
        {
            return 10;
        }

        return Mathf.Max(1, reactionsRequiredByLevel[index]);
    }

    // =========================================================
    // TIEMPO DE AVISO
    // =========================================================

    public float GetWarningTime(int currentLevel)
    {
        float totalTime = GetTimeForLevel(
            totalTimeByLevel,
            currentLevel,
            1.5f
        );

        float percentage = Mathf.Clamp(
            warningPercentage, 0f, 100f
        );

        return totalTime * percentage / 100f;
    }

    // =========================================================
    // TIEMPO ACTIVO DEL TARGET
    // =========================================================

    public float GetMaxReactionTime(int currentLevel)
    {
        float totalTime = GetTimeForLevel(
            totalTimeByLevel,
            currentLevel,
            1.5f
        );

        float percentage = Mathf.Clamp(
            warningPercentage, 0f, 100f
        );

        return totalTime * (1f - percentage / 100f);
    }

    // =========================================================
    // ESPERA ENTRE TARGETS
    // =========================================================

    public float GetDelayBetweenTargets(int currentLevel)
    {
        return GetTimeForLevel(
            delayBetweenTargetsByLevel,
            currentLevel,
            0.5f
        );
    }

    // =========================================================
    // OBTENER VALOR POR NIVEL
    // =========================================================

    private float GetTimeForLevel(
        float[] values,
        int currentLevel,
        float defaultValue)
    {
        int index = currentLevel - 1;

        if (values == null ||
            index < 0 ||
            index >= values.Length)
        {
            return defaultValue;
        }

        return Mathf.Max(0f, values[index]);
    }
}
