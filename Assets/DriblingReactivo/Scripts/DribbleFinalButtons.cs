using UnityEngine;
using UnityEngine.SceneManagement;

public class DribbleFinalButtons : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string lockerSceneName = "Casilleros";

    public void GoToLockers()
    {
        SceneManager.LoadScene(
            lockerSceneName
        );
    }

    public void RestartScene()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            currentScene.name
        );
    }
}