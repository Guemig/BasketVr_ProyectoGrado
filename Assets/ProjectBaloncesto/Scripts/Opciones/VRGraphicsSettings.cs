using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class VRGraphicsSettings : MonoBehaviour
{
    public static VRGraphicsSettings Instance { get; private set; }

    [Header("Sliders")]
    [SerializeField] private Slider lightingSlider;
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private Slider shadowsSlider;

    [Header("Slider Sound")]
    [SerializeField] private float soundInterval = 0.08f;

    private Light sceneLight;

    private float lightingValue = 1f;
    private float temperatureValue = 1f;
    private float shadowsValue = 1f;

    private float lastLightingSoundTime = -999f;
    private float lastTemperatureSoundTime = -999f;
    private float lastShadowsSoundTime = -999f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Valores máximos al iniciar la aplicación
        lightingValue = 1f;
        temperatureValue = 1f;
        shadowsValue = 1f;

        ConfigureSliders();

        SceneManager.sceneLoaded += OnSceneLoaded;

        FindSceneLight();
        ApplyAllSettings();
    }

    private void ConfigureSliders()
    {
        SetupSlider(lightingSlider, lightingValue, SetLighting);
        SetupSlider(temperatureSlider, temperatureValue, SetTemperature);
        SetupSlider(shadowsSlider, shadowsValue, SetShadows);
    }

    private void SetupSlider(
        Slider slider,
        float value,
        UnityEngine.Events.UnityAction<float> callback)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;

        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.RemoveListener(callback);
        slider.onValueChanged.AddListener(callback);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindSceneLight();
        ApplyAllSettings();

        // Rebuscar sliders si pertenecen a la nueva escena
        if (lightingSlider == null ||
            temperatureSlider == null ||
            shadowsSlider == null)
        {
            Debug.Log(
                "VRGraphicsSettings: Reconecta los sliders de esta escena si hay un menú nuevo.");
        }
    }

    private void FindSceneLight()
    {
        sceneLight = null;

        // Buscar la luz direccional principal
        Light[] lights = FindObjectsByType<Light>(
            FindObjectsSortMode.None);

        foreach (Light light in lights)
        {
            if (light.type == LightType.Directional)
            {
                sceneLight = light;
                break;
            }
        }

        if (sceneLight == null)
            Debug.LogWarning(
                "VRGraphicsSettings: No se encontró Directional Light.");
    }

    // =====================================================
    // GRAPHICS SETTINGS
    // =====================================================

    public void SetLighting(float value)
    {
        lightingValue = Mathf.Clamp01(value);

        ApplyLighting();

        PlaySliderSound(ref lastLightingSoundTime);
    }

    public void SetTemperature(float value)
    {
        temperatureValue = Mathf.Clamp01(value);

        ApplyTemperature();

        PlaySliderSound(ref lastTemperatureSoundTime);
    }

    public void SetShadows(float value)
    {
        shadowsValue = Mathf.Clamp01(value);

        ApplyShadows();

        PlaySliderSound(ref lastShadowsSoundTime);
    }

    // =====================================================
    // SLIDER SOUND
    // =====================================================

    private void PlaySliderSound(ref float lastSoundTime)
    {
        if (Time.unscaledTime - lastSoundTime < soundInterval)
            return;

        lastSoundTime = Time.unscaledTime;

        AudioManager.Instance?.PlayUI(
            UIEvent.Slider
        );
    }

    // =====================================================
    // APPLY SETTINGS
    // =====================================================

    private void ApplyAllSettings()
    {
        ApplyLighting();
        ApplyTemperature();
        ApplyShadows();
    }

    private void ApplyLighting()
    {
        RenderSettings.ambientMode = AmbientMode.Skybox;

        RenderSettings.ambientIntensity =
            Mathf.Lerp(0f, 2f, lightingValue);
    }

    private void ApplyTemperature()
    {
        if (sceneLight == null)
            return;

        sceneLight.useColorTemperature = true;
        sceneLight.color = Color.white;

        sceneLight.colorTemperature =
            Mathf.Lerp(1700f, 3945f, temperatureValue);
    }

    private void ApplyShadows()
    {
        if (sceneLight == null)
            return;

        sceneLight.shadowStrength =
            Mathf.Lerp(0.1f, 0.7f, shadowsValue);
    }

    // =====================================================
    // CLEANUP
    // =====================================================

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (lightingSlider != null)
            lightingSlider.onValueChanged.RemoveListener(SetLighting);

        if (temperatureSlider != null)
            temperatureSlider.onValueChanged.RemoveListener(SetTemperature);

        if (shadowsSlider != null)
            shadowsSlider.onValueChanged.RemoveListener(SetShadows);

        Instance = null;
    }
}