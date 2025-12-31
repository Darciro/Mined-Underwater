using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private SceneTransition sceneTransition;

    public void LoadGame()
    {
        // Set game state to Playing when starting game
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeState(GameStateEnum.Playing);
        }

        sceneTransition.StartSceneTransition("Main");
    }

    public void RestartGame()
    {
        SceneManager.LoadScene("Main");
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
