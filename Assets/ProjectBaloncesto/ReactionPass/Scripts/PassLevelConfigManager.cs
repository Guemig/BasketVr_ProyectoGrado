
using UnityEngine;

public class PassLevelConfigManager : MonoBehaviour
{
    [System.Serializable]
    public class LevelConfig
    {
        [Header("Cantidad de disparos")]
        [Min(1)] public int requiredShots = 10;

        [Header("Tiempo entre disparos")]
        [Min(0f)] public float shotDelay = 1f;
    }

    [Header("Configuración de niveles")]
    [SerializeField]
    private LevelConfig level1 = new LevelConfig
    {
        requiredShots = 10,
        shotDelay = 1f
    };

    [SerializeField]
    private LevelConfig level2 = new LevelConfig
    {
        requiredShots = 12,
        shotDelay = 0.7f
    };

    [SerializeField]
    private LevelConfig level3 = new LevelConfig
    {
        requiredShots = 15,
        shotDelay = 0.5f
    };

    public int GetRequiredShots(int level)
    {
        LevelConfig config = GetLevelConfig(level);

        return config != null
            ? Mathf.Max(1, config.requiredShots)
            : 0;
    }

    public float GetShotDelay(int level)
    {
        LevelConfig config = GetLevelConfig(level);

        return config != null
            ? Mathf.Max(0f, config.shotDelay)
            : 0f;
    }

    private LevelConfig GetLevelConfig(int level)
    {
        switch (level)
        {
            case 1:
                return level1;

            case 2:
                return level2;

            case 3:
                return level3;

            default:
                Debug.LogError(
                    $"[PASSES] No existe configuración para el nivel {level}.",
                    this
                );

                return null;
        }
    }
}
