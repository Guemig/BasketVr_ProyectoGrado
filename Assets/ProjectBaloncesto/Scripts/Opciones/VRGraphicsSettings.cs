using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class VRGraphicsSettings : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider lightingSlider;
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private Slider shadowsSlider;

    [Header("Scene Light")]
    [SerializeField] private Light sceneLight;

    private void Awake()
    {
        if (lightingSlider == null ||
            temperatureSlider == null ||
            shadowsSlider == null ||
            sceneLight == null)
        {
            Debug.LogError("VRGraphicsSettings: Faltan referencias.");
            enabled = false;
            return;
        }

        ConfigureSlider(lightingSlider);
        ConfigureSlider(temperatureSlider);
        ConfigureSlider(shadowsSlider);

        lightingSlider.SetValueWithoutNotify(1f);
        temperatureSlider.SetValueWithoutNotify(1f);
        shadowsSlider.SetValueWithoutNotify(1f);

        RenderSettings.ambientMode = AmbientMode.Skybox;

        sceneLight.useColorTemperature = true;
        sceneLight.color = Color.white;

        ApplyLighting(1f);
        ApplyTemperature(1f);
        ApplyShadows(1f);
    }

    private void OnEnable()
    {
        if (lightingSlider != null)
            lightingSlider.onValueChanged.AddListener(ApplyLighting);

        if (temperatureSlider != null)
            temperatureSlider.onValueChanged.AddListener(ApplyTemperature);

        if (shadowsSlider != null)
            shadowsSlider.onValueChanged.AddListener(ApplyShadows);
    }

    private void OnDisable()
    {
        if (lightingSlider != null)
            lightingSlider.onValueChanged.RemoveListener(ApplyLighting);

        if (temperatureSlider != null)
            temperatureSlider.onValueChanged.RemoveListener(ApplyTemperature);

        if (shadowsSlider != null)
            shadowsSlider.onValueChanged.RemoveListener(ApplyShadows);
    }

    private void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void ApplyLighting(float value)
    {
        RenderSettings.ambientIntensity =
            Mathf.Lerp(0f, 2f, Mathf.Clamp01(value));
    }

    private void ApplyTemperature(float value)
    {
        if (sceneLight == null)
            return;

        float temperature = Mathf.Lerp(
            1700f,
            3945f,
            Mathf.Clamp01(value));

        sceneLight.useColorTemperature = true;
        sceneLight.color = Color.white;
        sceneLight.colorTemperature = temperature;
    }

    private void ApplyShadows(float value)
    {
        if (sceneLight == null)
            return;

        sceneLight.shadowStrength =
            Mathf.Lerp(0.1f, 0.7f, Mathf.Clamp01(value));
    }
}