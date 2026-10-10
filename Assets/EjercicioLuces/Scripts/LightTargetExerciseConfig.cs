
using UnityEngine;

public class LightTargetExerciseConfig : MonoBehaviour
{
    [Header("GRUPOS DE NIVELES")]
    [SerializeField] private GameObject level1Group;
    [SerializeField] private GameObject level2Group;
    [SerializeField] private GameObject level3Group;

    [Header("CANTIDAD DE REACCIONES POR NIVEL")]
    [SerializeField]
    private int[] reactionsRequiredByLevel =
    {
        10, 15, 20
    };

    [Header("TIEMPO DE AVISO POR NIVEL")]
    [SerializeField]
    private float[] warningTimeByLevel =
    {
        0.3f, 0.3f, 0.3f
    };

    [Header("ESPERA ENTRE TARGETS POR NIVEL")]
    [SerializeField]
    private float[] delayBetweenTargetsByLevel =
    {
        0.5f, 0.4f, 0.3f
    };

    [Header("TIEMPO MÁXIMO DE REACCIÓN POR NIVEL")]
    [SerializeField]
    private float[] maxReactionTimeByLevel =
    {
        1.5f, 1.2f, 1.0f
    };

    public GameObject Level1Group => level1Group;
    public GameObject Level2Group => level2Group;
    public GameObject Level3Group => level3Group;

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

    public float GetWarningTime(int currentLevel)
    {
        return GetTimeForLevel(
            warningTimeByLevel, currentLevel, 0.3f);
    }

    public float GetDelayBetweenTargets(int currentLevel)
    {
        return GetTimeForLevel(
            delayBetweenTargetsByLevel, currentLevel, 0.5f);
    }

    public float GetMaxReactionTime(int currentLevel)
    {
        return GetTimeForLevel(
            maxReactionTimeByLevel, currentLevel, 1.5f);
    }

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

        return values[index];
    }
}
