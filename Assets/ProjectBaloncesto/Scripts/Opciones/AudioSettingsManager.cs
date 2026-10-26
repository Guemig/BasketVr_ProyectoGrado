using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsManager : MonoBehaviour
{
    public static AudioSettingsManager Instance { get; private set; }

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

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

    private void SetMixerVolume(AudioChannel channel)
    {
        string parameter = GetMixerParameter(channel);

        float value = PlayerPrefs.GetFloat(parameter, 1f);

        float dB = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;

        audioMixer.SetFloat(parameter, dB);
    }
}