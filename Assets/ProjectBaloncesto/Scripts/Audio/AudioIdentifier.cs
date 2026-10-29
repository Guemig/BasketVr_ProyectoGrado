
using UnityEngine;

public enum AudioCategory
{
    UI,
    SFX,
    Background
}

public enum UIEvent
{
    None,
    Button,
    Slider,
    Confirm,
    Error,
    Warning,
    Transition,
    PanelOpen,
    PanelClose
}

public enum SFXEvent
{
    None,
    Interaction,
    Stimulus,
    Correct,
    Incorrect,
    LevelStart,
    LevelComplete,
    ExerciseComplete
}

public enum BackgroundContext
{
    None,
    MainMenu,
    Registration,
    Tutorial,
    Level1,
    Level2,
    Level3,
    Results
}


public class AudioIdentifier : MonoBehaviour
{
    [Header("Identificación")]
    [SerializeField] private AudioCategory category;
    [SerializeField] private UIEvent uiEvent = UIEvent.None;
    [SerializeField] private SFXEvent sfxEvent = SFXEvent.None;
    [SerializeField]
    private BackgroundContext backgroundContext =
        BackgroundContext.None;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    private float configuredVolume = 1f;

    public AudioCategory Category => category;
    public UIEvent UIEventId => uiEvent;
    public SFXEvent SFXEventId => sfxEvent;
    public BackgroundContext BackgroundId => backgroundContext;
    public AudioSource Source => audioSource;
    public float ConfiguredVolume => configuredVolume;

    public bool IsValid
    {
        get
        {
            if (audioSource == null || audioSource.clip == null)
                return false;

            switch (category)
            {
                case AudioCategory.UI:
                    return uiEvent != UIEvent.None;

                case AudioCategory.SFX:
                    return sfxEvent != SFXEvent.None;

                case AudioCategory.Background:
                    return backgroundContext != BackgroundContext.None;

                default:
                    return false;
            }
        }
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            configuredVolume = audioSource.volume;
    }

    private void OnEnable()
    {
        AudioManager.RegisterIdentifier(this);
    }

    private void OnDisable()
    {
        AudioManager.UnregisterIdentifier(this);
    }

    private void OnDestroy()
    {
        AudioManager.UnregisterIdentifier(this);
    }

    // Reproduce un efecto usando exclusivamente este AudioSource.
    public void PlayOneShot()
    {
        if (!IsValid || audioSource == null)
            return;

        audioSource.PlayOneShot(audioSource.clip);
    }

    // Inicia la reproducción continua de este identificador.
    public void PlayBackground()
    {
        if (!IsValid || audioSource == null)
            return;

        audioSource.loop = true;
        audioSource.spatialBlend = 0f;
        audioSource.Play();
    }

    public void StopPlayback()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    // Permite al manager realizar fundidos sin perder
    // el volumen configurado originalmente en el Inspector.
    public void SetPlaybackVolume(float volume)
    {
        if (audioSource != null)
            audioSource.volume = Mathf.Clamp01(volume);
    }

    public void RestoreConfiguredVolume()
    {
        SetPlaybackVolume(configuredVolume);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
#endif
}
