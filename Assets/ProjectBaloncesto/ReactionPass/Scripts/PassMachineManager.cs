
using UnityEngine;

public class PassMachineManager : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Configuración de velocidades y disparos")]
    [SerializeField]
    private PassLevelConfigManager levelConfigManager;

    [Header("Lanzadores: índice 0, 1 y 2")]
    [SerializeField]
    private BallLauncher[] machineLaunchers = new BallLauncher[3];

    // =====================================================
    // POSICIONES POR NIVEL
    // =====================================================

    [Header("Nivel 1 - Posición de la máquina 1")]
    [SerializeField]
    private Transform level1Machine1Position;

    [Header("Nivel 2 - Posiciones de las máquinas 1 y 2")]
    [SerializeField]
    private Transform level2Machine1Position;

    [SerializeField]
    private Transform level2Machine2Position;

    [Header("Nivel 3 - Posiciones de las máquinas 1, 2 y 3")]
    [SerializeField]
    private Transform level3Machine1Position;

    [SerializeField]
    private Transform level3Machine2Position;

    [SerializeField]
    private Transform level3Machine3Position;

    // =====================================================
    // ESTADO
    // =====================================================

    private int currentLevel = 1;
    private int lastMachineIndex = -1;
    private bool levelConfigured;

    public int CurrentLevel => currentLevel;

    public int ActiveMachineCount =>
        levelConfigured ? GetMachineCountForLevel(currentLevel) : 0;

    // =====================================================
    // CONFIGURACIÓN DEL NIVEL
    // =====================================================

    public bool ConfigureLevel(int level)
    {
        if (level < 1 || level > 3)
        {
            Debug.LogError(
                $"[PASS MACHINES] Nivel inválido: {level}.",
                this
            );

            return false;
        }

        if (levelConfigManager == null)
        {
            Debug.LogError(
                "[PASS MACHINES] Asigna PassLevelConfigManager.",
                this
            );

            return false;
        }

        PassLevelConfigManager.LevelConfig config =
            levelConfigManager.GetLevelConfigForLevel(level);

        if (config == null)
            return false;

        if (!ValidateLaunchers())
            return false;

        if (!ValidatePositions(level))
            return false;

        // Detener las máquinas antes de cambiar sus posiciones.
        StopAllMachines();

        currentLevel = level;
        lastMachineIndex = -1;
        levelConfigured = false;

        int machineCount = GetMachineCountForLevel(level);

        for (int i = 0; i < machineLaunchers.Length; i++)
        {
            BallLauncher launcher = machineLaunchers[i];

            if (launcher == null)
                continue;

            bool shouldBeActive = i < machineCount;

            if (!shouldBeActive)
            {
                launcher.gameObject.SetActive(false);
                continue;
            }

            // Activar primero para asegurar que el objeto esté disponible.
            launcher.gameObject.SetActive(true);

            Transform destination = GetMachinePosition(level, i);

            if (destination == null)
            {
                Debug.LogError(
                    $"[PASS MACHINES] Falta la posición de la máquina " +
                    $"{i + 1} para el nivel {level}.",
                    this
                );

                StopAllMachines();
                return false;
            }

            // La raíz de la máquina recibe la posición y orientación
            // del punto asignado en el Inspector.
            launcher.transform.SetPositionAndRotation(
                destination.position,
                destination.rotation
            );

            launcher.SetMachineBaseRotation(destination.rotation);

            // Aplicar los datos de velocidad y disparos del nivel.
            launcher.SetLevel(level, config);
        }

        levelConfigured = true;

        Debug.Log(
            $"[PASS MACHINES] Nivel {level} configurado. " +
            $"Máquinas activas: {machineCount}.",
            this
        );

        return true;
    }

    // =====================================================
    // OBTENER POSICIÓN DE LA MÁQUINA
    // =====================================================

    private Transform GetMachinePosition(int level, int machineIndex)
    {
        switch (level)
        {
            case 1:
                return machineIndex == 0
                    ? level1Machine1Position
                    : null;

            case 2:
                switch (machineIndex)
                {
                    case 0: return level2Machine1Position;
                    case 1: return level2Machine2Position;
                    default: return null;
                }

            case 3:
                switch (machineIndex)
                {
                    case 0: return level3Machine1Position;
                    case 1: return level3Machine2Position;
                    case 2: return level3Machine3Position;
                    default: return null;
                }

            default:
                return null;
        }
    }

    private int GetMachineCountForLevel(int level)
    {
        switch (level)
        {
            case 1: return 1;
            case 2: return 2;
            case 3: return 3;
            default: return 0;
        }
    }

    // =====================================================
    // SELECCIONAR MÁQUINA ALEATORIA
    // =====================================================

    public BallLauncher GetNextLauncher()
    {
        if (!levelConfigured)
        {
            Debug.LogError(
                "[PASS MACHINES] Configura el nivel antes de lanzar.",
                this
            );

            return null;
        }

        int machineCount = GetMachineCountForLevel(currentLevel);

        if (machineCount <= 0)
            return null;

        int[] candidates = new int[machineCount];
        int candidateCount = 0;

        for (int i = 0; i < machineCount; i++)
        {
            BallLauncher launcher = machineLaunchers[i];

            if (launcher == null || !launcher.gameObject.activeInHierarchy)
                continue;

            // Evitar repetir la máquina anterior si hay otra disponible.
            if (machineCount > 1 && i == lastMachineIndex)
                continue;

            candidates[candidateCount] = i;
            candidateCount++;
        }

        // Si solo quedaba la última máquina como opción, permitirla.
        if (candidateCount == 0)
        {
            for (int i = 0; i < machineCount; i++)
            {
                BallLauncher launcher = machineLaunchers[i];

                if (launcher == null ||
                    !launcher.gameObject.activeInHierarchy)
                {
                    continue;
                }

                candidates[candidateCount] = i;
                candidateCount++;
            }
        }

        if (candidateCount == 0)
        {
            Debug.LogError(
                "[PASS MACHINES] No hay máquinas activas disponibles.",
                this
            );

            return null;
        }

        int selectedIndex =
            candidates[Random.Range(0, candidateCount)];

        lastMachineIndex = selectedIndex;

        return machineLaunchers[selectedIndex];
    }

    // =====================================================
    // EFECTO VISUAL DE MUNICIÓN
    // =====================================================

    public void InitializeVisualFeeds(int totalShots)
    {
        int machineCount = GetMachineCountForLevel(currentLevel);

        for (int i = 0; i < machineLaunchers.Length; i++)
        {
            BallLauncher launcher = machineLaunchers[i];

            if (launcher == null)
                continue;

            if (i < machineCount)
                launcher.InitializeVisualFeed(totalShots);
            else
                launcher.ResetVisualFeed();
        }
    }

    public void ResetVisualFeeds()
    {
        foreach (BallLauncher launcher in machineLaunchers)
        {
            if (launcher != null)
                launcher.ResetVisualFeed();
        }
    }

    // =====================================================
    // DETENER MÁQUINAS
    // =====================================================

    public void StopAllMachines()
    {
        if (machineLaunchers == null)
            return;

        foreach (BallLauncher launcher in machineLaunchers)
        {
            if (launcher != null)
                launcher.StopLauncher();
        }
    }

    // =====================================================
    // VALIDACIONES
    // =====================================================

    private bool ValidateLaunchers()
    {
        if (machineLaunchers == null || machineLaunchers.Length < 3)
        {
            Debug.LogError(
                "[PASS MACHINES] Asigna tres lanzadores al array.",
                this
            );

            return false;
        }

        for (int i = 0; i < 3; i++)
        {
            if (machineLaunchers[i] == null)
            {
                Debug.LogError(
                    $"[PASS MACHINES] Falta el lanzador {i + 1}.",
                    this
                );

                return false;
            }
        }

        return true;
    }

    private bool ValidatePositions(int level)
    {
        int machineCount = GetMachineCountForLevel(level);

        for (int i = 0; i < machineCount; i++)
        {
            if (GetMachinePosition(level, i) == null)
            {
                Debug.LogError(
                    $"[PASS MACHINES] Falta asignar la posición " +
                    $"de la máquina {i + 1} del nivel {level}.",
                    this
                );

                return false;
            }
        }

        return true;
    }
}
