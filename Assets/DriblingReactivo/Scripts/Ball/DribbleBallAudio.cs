using UnityEngine;

public class DribbleBallAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    [Header("Pitch")]
    [SerializeField] private float minimumPitch = 0.9f;
    [SerializeField] private float maximumPitch = 1.1f;

    [Header("Volumen")]
    [SerializeField] private float volume = 1f;

    public void PlayBounce()
    {
        if (audioSource == null)
            return;

        if (bounceClip == null)
            return;

        audioSource.pitch =
            Random.Range(
                minimumPitch,
                maximumPitch
            );

        audioSource.PlayOneShot(
            bounceClip,
            volume
        );
    }
}