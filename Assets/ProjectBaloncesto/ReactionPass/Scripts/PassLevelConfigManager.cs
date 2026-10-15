
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

        [Header("Velocidades de lanzamiento")]
        [Min(0f)] public float slowSpeed = 8f;
        [Min(0f)] public float mediumSpeed = 11f;
        [Min(0f)] public float fastSpeed = 14f;

        [Header("Probabilidad relativa de cada velocidad")]
        [Min(0f)] public float slowWeight = 1f;
        [Min(0f)] public float mediumWeight = 0f;
        [Min(0f)] public float fastWeight = 0f;
    }

    [Header("Configuración del nivel 1")]
    [SerializeField]
    private LevelConfig level1 = new LevelConfig
    {
        requiredShots = 10,
        shotDelay = 1f,
        slowSpeed = 8f,
        mediumSpeed = 11f,
        fastSpeed = 14f,
        slowWeight = 1f,
        mediumWeight = 0f,
        fastWeight = 0f
    };

    [Header("Configuración del nivel 2")]
    [SerializeField]
    private LevelConfig level2 = new LevelConfig
    {
        requiredShots = 12,
        shotDelay = 0.7f,
        slowSpeed = 8f,
        mediumSpeed = 11f,
        fastSpeed = 14f,
        slowWeight = 0.3f,
        mediumWeight = 0.7f,
        fastWeight = 0f
    };

    [Header("Configuración del nivel 3")]
    [SerializeField]
    private LevelConfig level3 = new LevelConfig
    {
        requiredShots = 15,
        shotDelay = 0.5f,
        slowSpeed = 8f,
        mediumSpeed = 11f,
        fastSpeed = 14f,
        slowWeight = 0.10f,
        mediumWeight = 0.35f,
        fastWeight = 0.55f
    };

    // =====================================================
    // CONSULTAS
    // =====================================================

    public int GetRequiredShots(int level)
    {
        LevelConfig config = GetLevelConfigForLevel(level);

        return config != null
            ? Mathf.Max(1, config.requiredShots)
            : 0;
    }

    public float GetShotDelay(int level)
    {
        LevelConfig config = GetLevelConfigForLevel(level);

        return config != null
            ? Mathf.Max(0f, config.shotDelay)
            : 0f;
    }

    public LevelConfig GetLevelConfigForLevel(int level)
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

    // =====================================================
    // VALIDACIÓN
    // =====================================================

    public bool ValidateLevelConfig(int level)
    {
        LevelConfig config = GetLevelConfigForLevel(level);

        if (config == null)
            return false;

        if (config.requiredShots < 1)
        {
            Debug.LogError(
                $"[PASSES] El nivel {level} debe tener al menos un disparo.",
                this
            );

            return false;
        }

        if (config.slowSpeed < 0f ||
            config.mediumSpeed < 0f ||
            config.fastSpeed < 0f)
        {
            Debug.LogError(
                $"[PASSES] Hay velocidades negativas en el nivel {level}.",
                this
            );

            return false;
        }

        float totalWeight =
            Mathf.Max(0f, config.slowWeight) +
            Mathf.Max(0f, config.mediumWeight) +
            Mathf.Max(0f, config.fastWeight);

        if (totalWeight <= 0f)
        {
            Debug.LogError(
                $"[PASSES] Los pesos de velocidad del nivel {level} " +
                "no pueden ser todos cero.",
                this
            );

            return false;
        }

        return true;
    }
}
