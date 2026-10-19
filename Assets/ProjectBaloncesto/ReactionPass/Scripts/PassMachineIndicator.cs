
using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PassMachineImage
{
    [Header("Índice de la máquina (0, 1 o 2)")]
    [Range(0, 2)]
    public int machineIndex;

    [Header("Imagen del Canvas")]
    public Image image;

    [Header("Colores")]
    public Color normalColor = Color.white;
    public Color activeColor = Color.yellow;
}

public class PassMachineIndicator : MonoBehaviour
{
    [Header("NIVEL 1")]
    [SerializeField] private PassMachineImage[] level1Machines;

    [Header("NIVEL 2")]
    [SerializeField] private PassMachineImage[] level2Machines;

    [Header("NIVEL 3")]
    [SerializeField] private PassMachineImage[] level3Machines;

    private int currentLevel;
    private PassMachineImage[] currentMachines;

    private PassMachineManager machineManager;

    private readonly BallLauncher[] observedLaunchers =
        new BallLauncher[3];

    // =====================================================
    // CONFIGURAR NIVEL
    // =====================================================

    public void SetLevel(int level)
    {
        HideAllMachines();

        currentLevel = level;
        currentMachines = GetMachinesForLevel(level);

        ShowCurrentMachines();
    }

    private void ShowCurrentMachines()
    {
        if (currentMachines == null)
        {
            Debug.LogWarning(
                "[PASS INDICATOR] No hay imágenes configuradas " +
                "para el nivel " + currentLevel + ".",
                this
            );

            return;
        }

        foreach (PassMachineImage item in currentMachines)
        {
            if (item == null || item.image == null)
                continue;

            item.image.gameObject.SetActive(true);
            item.image.color = item.normalColor;
        }
    }

    // =====================================================
    // VINCULAR MÁQUINAS
    // =====================================================

    public void BindMachines(PassMachineManager manager)
    {
        UnbindMachines();

        machineManager = manager;

        if (machineManager == null)
        {
            Debug.LogWarning(
                "[PASS INDICATOR] PassMachineManager no asignado.",
                this
            );

            return;
        }

        for (int i = 0; i < observedLaunchers.Length; i++)
        {
            BallLauncher launcher = machineManager.GetLauncher(i);

            observedLaunchers[i] = launcher;

            if (launcher != null)
            {
                launcher.OnLaunchStateChanged +=
                    HandleLaunchStateChanged;
            }
        }
    }

    private void UnbindMachines()
    {
        for (int i = 0; i < observedLaunchers.Length; i++)
        {
            BallLauncher launcher = observedLaunchers[i];

            if (launcher != null)
            {
                launcher.OnLaunchStateChanged -=
                    HandleLaunchStateChanged;
            }

            observedLaunchers[i] = null;
        }
    }

    private void HandleLaunchStateChanged(
        BallLauncher launcher,
        BallLauncher.LaunchState state)
    {
        if (machineManager == null || launcher == null)
            return;

        int machineIndex =
            machineManager.GetMachineIndex(launcher);

        if (machineIndex < 0)
            return;

        switch (state)
        {
            case BallLauncher.LaunchState.Warning:
                ShowMachine(machineIndex);
                break;

            case BallLauncher.LaunchState.Firing:
            case BallLauncher.LaunchState.Idle:
                ClearSelection();
                break;
        }
    }

    private void OnDestroy()
    {
        UnbindMachines();
    }

    // =====================================================
    // INDICAR MÁQUINA SELECCIONADA
    // =====================================================

    public void ShowMachine(int machineIndex)
    {
        if (currentMachines == null)
        {
            Debug.LogWarning(
                "[PASS INDICATOR] Primero debes configurar el nivel.",
                this
            );

            return;
        }

        ClearSelection();

        foreach (PassMachineImage item in currentMachines)
        {
            if (item == null || item.image == null)
                continue;

            if (item.machineIndex != machineIndex)
                continue;

            item.image.color = item.activeColor;
            return;
        }

        Debug.LogWarning(
            "[PASS INDICATOR] No existe una imagen para la máquina " +
            (machineIndex + 1) + " del nivel " + currentLevel + ".",
            this
        );
    }

    // =====================================================
    // RESTABLECER SELECCIÓN
    // =====================================================

    public void ClearSelection()
    {
        if (currentMachines == null)
            return;

        foreach (PassMachineImage item in currentMachines)
        {
            if (item == null || item.image == null)
                continue;

            item.image.color = item.normalColor;
        }
    }

    // =====================================================
    // OCULTAR TODAS LAS MÁQUINAS
    // =====================================================

    public void HideAllMachines()
    {
        SetMachinesActive(level1Machines, false);
        SetMachinesActive(level2Machines, false);
        SetMachinesActive(level3Machines, false);

        currentMachines = null;
        currentLevel = 0;
    }

    private void SetMachinesActive(
        PassMachineImage[] machines,
        bool active)
    {
        if (machines == null)
            return;

        foreach (PassMachineImage item in machines)
        {
            if (item == null || item.image == null)
                continue;

            item.image.color = item.normalColor;
            item.image.gameObject.SetActive(active);
        }
    }

    // =====================================================
    // OBTENER IMÁGENES DEL NIVEL
    // =====================================================

    private PassMachineImage[] GetMachinesForLevel(int level)
    {
        switch (level)
        {
            case 1: return level1Machines;
            case 2: return level2Machines;
            case 3: return level3Machines;
            default: return null;
        }
    }
}
