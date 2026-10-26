
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField, Min(0f)]
    private float backgroundFadeDuration = 0.5f;

    private readonly Dictionary<UIEvent, List<AudioIdentifier>> uiRegistry =
        new Dictionary<UIEvent, List<AudioIdentifier>>();

    private readonly Dictionary<SFXEvent, List<AudioIdentifier>> sfxRegistry =
        new Dictionary<SFXEvent, List<AudioIdentifier>>();

    private readonly Dictionary<BackgroundContext, AudioIdentifier>
        backgroundRegistry =
            new Dictionary<BackgroundContext, AudioIdentifier>();

    private static readonly HashSet<AudioIdentifier> pendingIdentifiers =
        new HashSet<AudioIdentifier>();

    private AudioIdentifier currentBackground;
    private Coroutine backgroundFadeRoutine;

    // --------------------------------------------------
    // INICIALIZACIÓN
    // --------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (AudioIdentifier identifier in pendingIdentifiers)
            Register(identifier);

        pendingIdentifiers.Clear();

        RefreshSceneIdentifiers();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (backgroundFadeRoutine != null)
            StopCoroutine(backgroundFadeRoutine);

        Instance = null;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshSceneIdentifiers();
    }

    // --------------------------------------------------
    // REGISTRO AUTOMÁTICO
    // --------------------------------------------------

    public static void RegisterIdentifier(AudioIdentifier identifier)
    {
        if (identifier == null)
            return;

        if (Instance != null)
            Instance.Register(identifier);
        else
            pendingIdentifiers.Add(identifier);
    }

    public static void UnregisterIdentifier(AudioIdentifier identifier)
    {
        if (identifier == null)
            return;

        pendingIdentifiers.Remove(identifier);

        if (Instance != null)
            Instance.Unregister(identifier);
    }

    private void Register(AudioIdentifier identifier)
    {
        if (identifier == null || !identifier.isActiveAndEnabled)
            return;

        if (IsRegistered(identifier))
            return;

        Unregister(identifier);

        if (!identifier.IsValid)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"[AudioManager] Identificador inválido: {identifier.name}. " +
                "Revisa la categoría, el evento y el AudioClip.",
                identifier
            );
#endif
            return;
        }

        switch (identifier.Category)
        {
            case AudioCategory.UI:
                AddToRegistry(
                    uiRegistry, identifier.UIEventId, identifier);
                break;

            case AudioCategory.SFX:
                AddToRegistry(
                    sfxRegistry, identifier.SFXEventId, identifier);
                break;

            case AudioCategory.Background:
                RegisterBackground(identifier);
                break;
        }
    }

    private void Unregister(AudioIdentifier identifier)
    {
        RemoveFromRegistry(uiRegistry, identifier);
        RemoveFromRegistry(sfxRegistry, identifier);

        if (identifier.Category != AudioCategory.Background)
            return;

        BackgroundContext context = identifier.BackgroundId;

        if (backgroundRegistry.TryGetValue(
                context, out AudioIdentifier registered) &&
            registered == identifier)
        {
            backgroundRegistry.Remove(context);
        }

        if (currentBackground == identifier)
        {
            if (backgroundFadeRoutine != null)
            {
                StopCoroutine(backgroundFadeRoutine);
                backgroundFadeRoutine = null;
            }

            identifier.StopPlayback();
            identifier.RestoreConfiguredVolume();
            currentBackground = null;
        }
    }

    private bool IsRegistered(AudioIdentifier identifier)
    {
        if (identifier == null)
            return false;

        switch (identifier.Category)
        {
            case AudioCategory.UI:
                return uiRegistry.TryGetValue(
                           identifier.UIEventId,
                           out List<AudioIdentifier> uiList) &&
                       uiList.Contains(identifier);

            case AudioCategory.SFX:
                return sfxRegistry.TryGetValue(
                           identifier.SFXEventId,
                           out List<AudioIdentifier> sfxList) &&
                       sfxList.Contains(identifier);

            case AudioCategory.Background:
                return backgroundRegistry.TryGetValue(
                           identifier.BackgroundId,
                           out AudioIdentifier registered) &&
                       registered == identifier;

            default:
                return false;
        }
    }

    private void RefreshSceneIdentifiers()
    {
        AudioIdentifier[] identifiers =
            FindObjectsByType<AudioIdentifier>(
                FindObjectsSortMode.None);

        foreach (AudioIdentifier identifier in identifiers)
            Register(identifier);
    }

    private void RegisterBackground(AudioIdentifier identifier)
    {
        BackgroundContext context = identifier.BackgroundId;

        if (backgroundRegistry.TryGetValue(
                context, out AudioIdentifier existing) &&
            existing != identifier)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"[AudioManager] Ya existe un fondo registrado para " +
                $"{context}. Se conservará el primero.",
                identifier
            );
#endif
            return;
        }

        backgroundRegistry[context] = identifier;
    }

    private void AddToRegistry<T>(
        Dictionary<T, List<AudioIdentifier>> registry,
        T key,
        AudioIdentifier identifier)
    {
        if (!registry.TryGetValue(key, out List<AudioIdentifier> list))
        {
            list = new List<AudioIdentifier>();
            registry.Add(key, list);
        }

        if (!list.Contains(identifier))
            list.Add(identifier);
    }

    private void RemoveFromRegistry<T>(
        Dictionary<T, List<AudioIdentifier>> registry,
        AudioIdentifier identifier)
    {
        List<T> keysToRemove = new List<T>();

        foreach (KeyValuePair<T, List<AudioIdentifier>> entry in registry)
        {
            entry.Value.Remove(identifier);

            if (entry.Value.Count == 0)
                keysToRemove.Add(entry.Key);
        }

        foreach (T key in keysToRemove)
            registry.Remove(key);
    }

    // --------------------------------------------------
    // UI
    // --------------------------------------------------

    public void PlayUI(
        UIEvent audioEvent,
        AudioIdentifier emitter = null)
    {
        if (audioEvent == UIEvent.None)
            return;

        AudioIdentifier identifier = emitter != null
            ? IsMatchingUI(emitter, audioEvent) ? emitter : null
            : GetRandomUI(audioEvent);

        PlayIdentifier(identifier);
    }

    private bool IsMatchingUI(
        AudioIdentifier identifier,
        UIEvent audioEvent)
    {
        return identifier != null &&
               identifier.isActiveAndEnabled &&
               identifier.Category == AudioCategory.UI &&
               identifier.UIEventId == audioEvent &&
               identifier.IsValid;
    }

    private AudioIdentifier GetRandomUI(UIEvent audioEvent)
    {
        if (!uiRegistry.TryGetValue(
                audioEvent, out List<AudioIdentifier> list))
            return null;

        return GetRandomValidIdentifier(list);
    }

    // --------------------------------------------------
    // SFX
    // --------------------------------------------------

    public void PlaySFX(
        SFXEvent audioEvent,
        AudioIdentifier emitter = null)
    {
        if (audioEvent == SFXEvent.None)
            return;

        AudioIdentifier identifier = emitter != null
            ? IsMatchingSFX(emitter, audioEvent) ? emitter : null
            : GetRandomSFX(audioEvent);

        PlayIdentifier(identifier);
    }

    private bool IsMatchingSFX(
        AudioIdentifier identifier,
        SFXEvent audioEvent)
    {
        return identifier != null &&
               identifier.isActiveAndEnabled &&
               identifier.Category == AudioCategory.SFX &&
               identifier.SFXEventId == audioEvent &&
               identifier.IsValid;
    }

    private AudioIdentifier GetRandomSFX(SFXEvent audioEvent)
    {
        if (!sfxRegistry.TryGetValue(
                audioEvent, out List<AudioIdentifier> list))
            return null;

        return GetRandomValidIdentifier(list);
    }

    private AudioIdentifier GetRandomValidIdentifier(
        List<AudioIdentifier> list)
    {
        if (list == null || list.Count == 0)
            return null;

        List<AudioIdentifier> valid = new List<AudioIdentifier>();

        foreach (AudioIdentifier identifier in list)
        {
            if (identifier != null &&
                identifier.isActiveAndEnabled &&
                identifier.IsValid)
            {
                valid.Add(identifier);
            }
        }

        if (valid.Count == 0)
            return null;

        return valid[Random.Range(0, valid.Count)];
    }

    private void PlayIdentifier(AudioIdentifier identifier)
    {
        if (identifier == null)
            return;

        identifier.PlayOneShot();
    }

    // --------------------------------------------------
    // MÚSICA Y AMBIENTE
    // --------------------------------------------------

    public void PlayBackground(BackgroundContext context)
    {
        if (context == BackgroundContext.None)
            return;

        if (!backgroundRegistry.TryGetValue(
                context, out AudioIdentifier identifier) ||
            identifier == null ||
            !identifier.IsValid)
        {
#if UNITY_EDITOR
            Debug.LogWarning(
                $"[AudioManager] No hay un fondo válido registrado " +
                $"para {context}."
            );
#endif
            return;
        }

        if (currentBackground == identifier &&
            identifier.Source.isPlaying)
        {
            return;
        }

        if (backgroundFadeRoutine != null)
            StopCoroutine(backgroundFadeRoutine);

        AudioIdentifier previousBackground = currentBackground;
        currentBackground = identifier;

        backgroundFadeRoutine = StartCoroutine(
            FadeToBackground(previousBackground, identifier));
    }

    public void StopBackground()
    {
        if (backgroundFadeRoutine != null)
            StopCoroutine(backgroundFadeRoutine);

        AudioIdentifier previousBackground = currentBackground;
        currentBackground = null;

        if (previousBackground == null)
            return;

        if (backgroundFadeDuration <= 0f)
        {
            previousBackground.StopPlayback();
            previousBackground.RestoreConfiguredVolume();
            backgroundFadeRoutine = null;
            return;
        }

        backgroundFadeRoutine =
            StartCoroutine(FadeOutBackground(previousBackground));
    }

    private IEnumerator FadeToBackground(
        AudioIdentifier previous,
        AudioIdentifier next)
    {
        if (previous != null && previous != next)
        {
            yield return FadeVolume(previous, 0f);
            previous.StopPlayback();
            previous.RestoreConfiguredVolume();
        }

        if (next == null || !next.IsValid)
        {
            backgroundFadeRoutine = null;
            yield break;
        }

        next.SetPlaybackVolume(0f);
        next.PlayBackground();

        yield return FadeVolume(next, next.ConfiguredVolume);

        backgroundFadeRoutine = null;
    }

    private IEnumerator FadeOutBackground(AudioIdentifier identifier)
    {
        yield return FadeVolume(identifier, 0f);

        if (identifier != null)
        {
            identifier.StopPlayback();
            identifier.RestoreConfiguredVolume();
        }

        backgroundFadeRoutine = null;
    }

    private IEnumerator FadeVolume(
        AudioIdentifier identifier,
        float targetVolume)
    {
        if (identifier == null)
            yield break;

        float duration = backgroundFadeDuration;

        if (duration <= 0f)
        {
            identifier.SetPlaybackVolume(targetVolume);
            yield break;
        }

        float initialVolume = identifier.Source.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (identifier == null)
                yield break;

            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            identifier.SetPlaybackVolume(
                Mathf.Lerp(initialVolume, targetVolume, t));

            yield return null;
        }

        if (identifier != null)
            identifier.SetPlaybackVolume(targetVolume);
    }
}
