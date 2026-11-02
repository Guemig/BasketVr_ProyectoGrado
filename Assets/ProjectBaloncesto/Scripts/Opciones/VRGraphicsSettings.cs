
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VRGraphicsSettings : MonoBehaviour
{
    [Header("Global Volume")]
    [SerializeField] private Volume globalVolume;

    [Header("Sliders")]
    [SerializeField] private Slider lightingSlider;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private Slider visualIntensitySlider;

    private ColorAdjustments colorAdjustments;
    private Bloom bloom;

    private const string LightingKey = "VR_Lighting";
    private const string BrightnessKey = "VR_Brightness";
    private const string VisualIntensityKey = "VR_VisualIntensity";

    private void Awake()
    {
        if (globalVolume == null)
        {
            Debug.LogError("VRGraphicsSettings: Global Volume no asignado.");
            enabled = false;
            return;
        }

        VolumeProfile profile = globalVolume.profile;

        if (!profile.TryGet(out colorAdjustments))
        {
            Debug.LogError("VRGraphicsSettings: Falta Color Adjustments.");
            enabled = false;
            return;
        }

        if (!profile.TryGet(out bloom))
        {
            Debug.LogError("VRGraphicsSettings: Falta Bloom.");
            enabled = false;
            return;
        }

        if (lightingSlider == null ||
            brightnessSlider == null ||
            visualIntensitySlider == null)
        {
            Debug.LogError("VRGraphicsSettings: Faltan sliders.");
            enabled = false;
            return;
        }

        ConfigureSlider(lightingSlider);
        ConfigureSlider(brightnessSlider);
        ConfigureSlider(visualIntensitySlider);

        lightingSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat(LightingKey, 0.6f));

        brightnessSlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat(BrightnessKey, 0.6f));

        visualIntensitySlider.SetValueWithoutNotify(
            PlayerPrefs.GetFloat(VisualIntensityKey, 0.75f));

        ApplyLighting(lightingSlider.value);
        ApplyBrightness(brightnessSlider.value);
        ApplyVisualIntensity(visualIntensitySlider.value);

        lightingSlider.onValueChanged.AddListener(OnLightingChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
        visualIntensitySlider.onValueChanged.AddListener(OnVisualIntensityChanged);
    }

    private void ConfigureSlider(Slider slider)
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
    }

    private void OnLightingChanged(float value)
    {
        ApplyLighting(value);
        PlayerPrefs.SetFloat(LightingKey, value);
        PlayerPrefs.Save();
    }

    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        PlayerPrefs.SetFloat(BrightnessKey, value);
        PlayerPrefs.Save();
    }

    private void OnVisualIntensityChanged(float value)
    {
        ApplyVisualIntensity(value);
        PlayerPrefs.SetFloat(VisualIntensityKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyLighting(float value)
    {
        bloom.intensity.Override(
            Mathf.Lerp(0f, 0.20f, value));

        bloom.scatter.Override(
            Mathf.Lerp(0.3f, 0.7f, value));
    }

    private void ApplyBrightness(float value)
    {
        colorAdjustments.postExposure.Override(
            Mathf.Lerp(-0.5f, 0.5f, value));
    }

    private void ApplyVisualIntensity(float value)
    {
        colorAdjustments.contrast.Override(
            Mathf.Lerp(0f, 30f, value));

        colorAdjustments.saturation.Override(
            Mathf.Lerp(0f, 20f, value));
    }

    private void OnDestroy()
    {
        if (lightingSlider != null)
            lightingSlider.onValueChanged.RemoveListener(OnLightingChanged);

        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);

        if (visualIntensitySlider != null)
            visualIntensitySlider.onValueChanged.RemoveListener(OnVisualIntensityChanged);
    }
}
