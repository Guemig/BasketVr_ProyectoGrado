
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class AudioVolumeSlider : MonoBehaviour
{
    [SerializeField] private AudioSettingsManager.AudioChannel channel;

    [Header("Slider Sound")]
    [SerializeField] private float soundInterval = 0.08f;

    private Slider slider;
    private float lastSoundTime = -999f;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void Start()
    {
        if (AudioSettingsManager.Instance == null)
        {
            Debug.LogError(
                "No existe un AudioSettingsManager en la escena."
            );
            return;
        }

        LoadVolume();

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDestroy()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnValueChanged);
        }
    }

    private void LoadVolume()
    {
        slider.value =
            AudioSettingsManager.Instance.GetVolume(channel);
    }

    private void OnValueChanged(float value)
    {
        AudioSettingsManager.Instance.SetVolume(channel, value);

        PlaySliderSound();
    }

    private void PlaySliderSound()
    {
        if (Time.unscaledTime - lastSoundTime < soundInterval)
            return;

        lastSoundTime = Time.unscaledTime;

        AudioManager.Instance?.PlayUI(UIEvent.Slider);
    }
}
