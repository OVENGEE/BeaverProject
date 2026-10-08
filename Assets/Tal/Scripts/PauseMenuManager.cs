using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject pauseMenuCanvas;

    public static bool IsPaused { get; private set; } = false;

    private void Start()
    {
        // Ensure the pause canvas starts hidden and game time is running
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;
    }

    private void Update()
    {
        // Toggle pause whenever Escape is pressed
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(true);
        }

        Time.timeScale = 0f; // Freeze game physics and turn timers
        IsPaused = true;
    }

    public void Resume()
    {
        if (pauseMenuCanvas != null)
        {
            pauseMenuCanvas.SetActive(false);
        }

        Time.timeScale = 1f; // Unfreeze game
        IsPaused = false;
    }

    public void Restart()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze before reloading!
        IsPaused = false;

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadGameplay();
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void MainMenu()
    {
        Time.timeScale = 1f; // ALWAYS unfreeze before scene swap!
        IsPaused = false;

        if (GameSceneManager.Instance != null)
        {
            GameSceneManager.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
}