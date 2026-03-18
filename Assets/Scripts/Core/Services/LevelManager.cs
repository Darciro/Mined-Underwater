using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private const string TutorialCompletedKey = "StageCompleted_0";
    private const string HighestUnlockedStageKey = "HighestUnlockedStage";

    #region Singleton

    public static LevelManager Instance { get; private set; }

    #endregion

    [SerializeField] private SceneTransition sceneTransition;

    private GameManager gameManager => GameManager.Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (gameManager != null)
            gameManager.OnLevelComplete += OnLevelComplete;
    }

    private void OnDisable()
    {
        if (gameManager != null)
            gameManager.OnLevelComplete -= OnLevelComplete;
    }

    private void OnLevelComplete(int completedLevel)
    {
        LoadWinScene();
    }

    public void StartGame(bool isTutorial = false)
    {
        if (gameManager != null)
        {
            gameManager.ChangeState(isTutorial ? GameStateEnum.Tutorial : GameStateEnum.Playing);
            gameManager.ResetLevelStats();
        }

        // Initialize objectives for the stage that is about to begin
        ObjectivesManager.Instance?.InitializeForStage(gameManager?.CurrentLevel ?? 0);

        LoadScene(isTutorial ? "Tutorial" : "Main");
    }

    public void LoadGame()
    {
        AdsManager.Instance?.HideBanner();
        StartGame();
    }

    public void RestartGame()
    {
        gameManager?.ResetLevelStats();
        SceneManager.LoadScene("Main");
    }

    public void LoadWinScene()
    {
        StartCoroutine(WaitAndLoad("Win", 1f));
    }

    public void LoadNextLevel()
    {
        if (gameManager != null)
        {
            gameManager.NextLevel();
            gameManager.ChangeState(GameStateEnum.Playing);
        }

        LoadScene("StageSelect");
    }

    public void LoadGameOver()
    {
        gameManager?.ChangeState(GameStateEnum.GameOver);
        StartCoroutine(WaitAndLoad("GameOver", 2f));
    }

    public void RestartTutorial()
    {
        if (gameManager != null)
        {
            gameManager.ResetLevelStats();
            gameManager.ChangeState(GameStateEnum.Tutorial);
        }

        StartCoroutine(WaitAndLoad("Tutorial", 2f));
    }

    public void LoadStageSelect(bool hasSkipedTutorial = false)
    {
        if (hasSkipedTutorial)
        {
            // Skipping tutorial should count as tutorial completion and unlock stage 1.
            PlayerPrefs.SetInt(TutorialCompletedKey, 1);

            if (gameManager != null)
            {
                gameManager.UnlockStage(1);
                gameManager.RecordStageStars(0, 3);
            }
            else
            {
                int highestUnlockedStage = PlayerPrefs.GetInt(HighestUnlockedStageKey, 0);
                if (highestUnlockedStage < 1)
                    PlayerPrefs.SetInt(HighestUnlockedStageKey, 1);

                // Save 3 stars for tutorial directly if GameManager is unavailable
                int savedTutorialStars = PlayerPrefs.GetInt("StageStars_0", 0);
                if (3 > savedTutorialStars)
                    PlayerPrefs.SetInt("StageStars_0", 3);
            }

            PlayerPrefs.Save();
        }

        bool hasCompletedTutorial = gameManager != null
            ? gameManager.IsStageCompleted(0)
            : PlayerPrefs.GetInt(TutorialCompletedKey, 0) == 1;

        if (!hasCompletedTutorial && !hasSkipedTutorial)
        {
            gameManager?.ChangeState(GameStateEnum.Tutorial);
            LoadScene("Tutorial");
            return;
        }

        gameManager?.ChangeState(GameStateEnum.Playing);
        LoadScene("StageSelect");
    }

    public void LoadMainMenu()
    {
        gameManager?.ChangeState(GameStateEnum.MainMenu);
        SceneManager.LoadScene("Menu");
    }

    public void LoadInventory()
    {
        gameManager?.ChangeState(GameStateEnum.Playing);
        SceneManager.LoadScene("Inventory");
    }

    public void LoadShop()
    {
        gameManager?.ChangeState(GameStateEnum.Playing);
        SceneManager.LoadScene("Shop");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator WaitAndLoad(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadCharacterScene()
    {
        gameManager?.ChangeState(GameStateEnum.Playing);
        SceneManager.LoadScene("Character");
    }

    private void LoadScene(string sceneName)
    {
        if (sceneTransition != null)
            sceneTransition.StartSceneTransition(sceneName);
        else
            SceneManager.LoadScene(sceneName);
    }
}