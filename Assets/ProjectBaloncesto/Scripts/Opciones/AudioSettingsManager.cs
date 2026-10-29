using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Slider Sound")]
    [SerializeField] private float sliderSoundInterval = 0.15f;

    private float nextSliderSoundTime;

    public enum AudioChannel
    {
        Master,
        Ambient,
        SFX,
        UI
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
    }

    private void PlaySliderSound()
    {
        if (Time.unscaledTime < nextSliderSoundTime)
            return;

        AudioManager.Instance?.PlayUI(UIEvent.Slider);

        nextSliderSoundTime =
            Time.unscaledTime + sliderSoundInterval;
    }
    public void SetVolume(AudioChannel channel, float value)
    {
        string parameter = GetMixerParameter(channel);

        float dB;

        if (value <= 0.8f)
        {
            dB = Mathf.Lerp(-80f, 0f, value / 0.8f);
        }
        else
        {
            dB = Mathf.Lerp(0f, 10f, (value - 0.8f) / 0.2f);
        }

        PlaySliderSound();
        audioMixer.SetFloat(parameter, dB);

        PlayerPrefs.SetFloat(parameter, value);
    }

    public float GetVolume(AudioChannel channel)
    {
        string parameter = GetMixerParameter(channel);

        return PlayerPrefs.GetFloat(parameter, 1f);
    }

    private string GetMixerParameter(AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                return "MasterVolume";

            case AudioChannel.Ambient:
                return "AmbientVolume";

            case AudioChannel.SFX:
                return "SFXVolume";

            case AudioChannel.UI:
                return "UISoundVolume";

            default:
                return "MasterVolume";
        }
    }

    private void LoadVolumeSettings()
    {
        SetMixerVolume(AudioChannel.Master);
        SetMixerVolume(AudioChannel.Ambient);
        SetMixerVolume(AudioChannel.SFX);
        SetMixerVolume(AudioChannel.UI);
    }

    public void SetMasterVolume(float value)
    {
        SetVolume(AudioChannel.Master, value);
    }

    public void SetAmbientVolume(float value)
    {
        SetVolume(AudioChannel.Ambient, value);
    }

    public void SetSFXVolume(float value)
    {
        SetVolume(AudioChannel.SFX, value);
    }

    public void SetUIVolume(float value)
    {
        SetVolume(AudioChannel.UI, value);
    }

    private void SetMixerVolume(AudioChannel channel)
    {
        string parameter = GetMixerParameter(channel);

        float value = PlayerPrefs.GetFloat(parameter, 1f);

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;

        audioMixer.SetFloat(parameter, dB);
    }
}