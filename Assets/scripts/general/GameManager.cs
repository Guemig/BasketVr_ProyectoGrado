using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public static bool isPaused = false;

    private void Awake()
    {
        // ----------------- Initialize singleton instance ------------------
        if (Instance == null)
        {
            Instance = this;
        }
         DontDestroyOnLoad(gameObject);
    }

    // ----------------- Enable or disable cursor visibility and lock state ------------------
    public static void CursorVisible(bool state)
    {
        Cursor.visible = state;

        if (state)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void testeoGame()
    {
        Debug.Log("GameManager is working!");
    }
    // ----------------- Change current scene and reset game state ------------------
    public void ChangeScene(string sceneName)
    {
        Time.timeScale = 1f;
        isPaused = false;
        SceneManager.LoadScene(sceneName);
    }

    // ----------------- Quit the application ------------------
    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    // ----------------- Pause or resume the game ------------------
    public void GamePause(bool pause)
    {
        isPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    // ----------------- Restart the current scene ------------------
    public void Restart()
    {
        Time.timeScale = 1f;
        isPaused = false;

        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }
}
