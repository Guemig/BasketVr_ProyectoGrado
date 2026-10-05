using UnityEngine;

public class UISounds : MonoBehaviour
{
    public static UISounds Instance;

    [Header("Audio")]
    public AudioSource audioSource;

    [Header("Clips")]
    public AudioClip sonidoBoton;
    public AudioClip sonidoInput;

    private void Awake()
    {
        Instance = this;
    }

    public void ReproducirBoton()
    {
        if (audioSource != null && sonidoBoton != null)
            audioSource.PlayOneShot(sonidoBoton);
    }

    public void ReproducirInput()
    {
        if (audioSource != null && sonidoInput != null)
            audioSource.PlayOneShot(sonidoInput);
    }
}