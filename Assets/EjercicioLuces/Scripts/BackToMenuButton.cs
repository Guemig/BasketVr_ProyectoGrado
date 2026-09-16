using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    [Header("Escena")]
    [SerializeField] private string menuSceneName = "Menu";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip buttonSound;

    private bool isLoading = false;


    public void GoToMenu()
    {
       
        if (isLoading)
            return;

        isLoading = true;

        StartCoroutine(GoToMenuCoroutine());
    }


    private IEnumerator GoToMenuCoroutine()
    {
        
        if (audioSource != null && buttonSound != null)
        {
            audioSource.PlayOneShot(buttonSound);

            
            yield return new WaitForSeconds(buttonSound.length);
        }

        // Cambiar de escena
        SceneManager.LoadScene(menuSceneName);
    }
}