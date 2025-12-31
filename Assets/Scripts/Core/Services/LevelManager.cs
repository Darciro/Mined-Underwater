using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    #region Singleton

    public static LevelManager Instance { get; private set; }

    #endregion

    [SerializeField] private SceneTransition sceneTransition;

    private void Awake()
    {
        // Singleton pattern - allow multiple instances per scene
        // Since LevelManager is scene-specific, we update the instance each time
        Instance = this;
    }

    private void OnEnable()
    {
        // Subscribe to level complete event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete += OnLevelComplete;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from level complete event
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete -= OnLevelComplete;
        }
    }

    /// <summary>
    /// Called when the player completes a level
    /// </summary>
    private void OnLevelComplete(int completedLevel)
    {
        Debug.Log($"Level {completedLevel} complete! Loading Win scene...");
        LoadWinScene();
    }

    /// <summary>
    /// Starts the game using the current stage set in GameManager
    /// </summary>
    public void StartGame()
    {
        // Set game state to Playing when starting game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.Playing);
            GameManager.Instance.ResetLevelStats();
        }

        // Use scene transition if available, otherwise load directly
        if (sceneTransition != null)
        {
            sceneTransition.StartSceneTransition("Main");
        }
        else
        {
            SceneManager.LoadScene("Main");
        }
    }

    /// <summary>
    /// Legacy method - redirects to StartGame()
    /// </summary>
    public void LoadGame()
    {
        StartGame();
    }

    public void RestartGame()
    {
        // Reset level stats when restarting
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResetLevelStats();
        }

        SceneManager.LoadScene("Main");
    }

    public void LoadWinScene()
    {
        StartCoroutine(WaitAndLoad("Win", 1f));
    }

    public void LoadNextLevel()
    {
        // Advance to next level and restart game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.NextLevel();
            GameManager.Instance.ChangeState(GameStateEnum.Playing);
        }

        // Use scene transition if available, otherwise load directly
        if (sceneTransition != null)
        {
            sceneTransition.StartSceneTransition("StageSelect");
        }
        else
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    public void LoadGameOver()
    {
        // Trigger GameOver state in GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.GameOver);
        }

        StartCoroutine(WaitAndLoad("GameOver", 2f));
    }

    /// <summary>
    /// Loads the stage selection scene
    /// </summary>
    public void LoadStageSelect()
    {
        // Set game state to MainMenu (stage select is part of menu flow)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.MainMenu);
        }

        // Use scene transition if available, otherwise load directly
        if (sceneTransition != null)
        {
            sceneTransition.StartSceneTransition("StageSelect");
        }
        else
        {
            SceneManager.LoadScene("StageSelect");
        }
    }

    public void LoadMainMenu()
    {
        // Set game state to MainMenu
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.MainMenu);
        }

        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    IEnumerator WaitAndLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
