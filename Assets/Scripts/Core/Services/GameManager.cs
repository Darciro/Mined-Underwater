using System;
using UnityEngine;

/// <summary>
/// Singleton manager responsible for handling game states and tracking collectibles (eggs and coins).
/// Allows other objects to subscribe to OnGameStateChanged events.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton

    public static GameManager Instance { get; private set; }

    #endregion

    #region Events

    /// <summary>
    /// Invoked when the game state changes. Subscribers receive the new state.
    /// </summary>
    public event Action<GameStateEnum> OnGameStateChanged;

    /// <summary>
    /// Invoked when the player completes a level. Subscribers receive level stats.
    /// </summary>
    public event Action<int> OnLevelComplete;

    #endregion

    #region State Management

    [SerializeField]
    [Tooltip("Current state of the game")]
    private GameStateEnum currentState = GameStateEnum.MainMenu;

    /// <summary>
    /// Gets the current game state
    /// </summary>
    public GameStateEnum CurrentState => currentState;

    #endregion

    #region Level Progression

    private const string LEVEL_SAVE_KEY = "CurrentLevel";
    private int currentLevel = 0;

    /// <summary>
    /// Gets the current game level
    /// </summary>
    public int CurrentLevel => currentLevel;

    // Egg requirement formula parameters
    private int baseEggRequirement = 5;
    private int eggsPerLevel = 3;

    #endregion

    #region Stage Unlocking & Completion

    private const string HIGHEST_UNLOCKED_STAGE_KEY = "HighestUnlockedStage";
    private const string STAGE_COMPLETED_KEY_PREFIX = "StageCompleted_";
    private const string STAGE_EGGS_KEY_PREFIX = "StageEggs_";
    private const string STAGE_COINS_KEY_PREFIX = "StageCoins_";

    private int highestUnlockedStage = 0;

    /// <summary>
    /// Gets the highest unlocked stage (0-based index)
    /// </summary>
    public int HighestUnlockedStage => highestUnlockedStage;

    #endregion

    #region Collection Tracking (Session)

    // Current level statistics
    private int levelEggs = 0;
    private int levelCoins = 0;

    /// <summary>
    /// Gets eggs collected in current level
    /// </summary>
    public int LevelEggs => levelEggs;

    /// <summary>
    /// Gets coins collected in current level
    /// </summary>
    public int LevelCoins => levelCoins;

    #endregion

    #region Collection Tracking (Lifetime Totals)

    private const string TOTAL_EGGS_SAVE_KEY = "TotalEggs";
    private const string TOTAL_COINS_SAVE_KEY = "TotalCoins";

    private int totalEggs = 0;
    private int totalCoins = 0;

    /// <summary>
    /// Gets total eggs collected across all levels
    /// </summary>
    public int TotalEggs => totalEggs;

    /// <summary>
    /// Gets total coins collected across all levels
    /// </summary>
    public int TotalCoins => totalCoins;

    #endregion

    #region Editor Debug (Read-Only)

#if UNITY_EDITOR
    [Header("DEBUG - Game Progress (Read Only)")]
    [SerializeField] private int debugCurrentLevel;
    [SerializeField] private int debugEggRequirement;
    [SerializeField] private int debugHighestUnlockedStage;
    [Space(10)]
    [Header("Current Level Stats")]
    [SerializeField] private int debugLevelEggs;
    [SerializeField] private int debugLevelCoins;
    [Space(10)]
    [Header("Lifetime Stats")]
    [SerializeField] private int debugTotalEggs;
    [SerializeField] private int debugTotalCoins;
#endif

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Load persistent data from PlayerPrefs
        LoadPersistentData();

#if UNITY_EDITOR
        UpdateDebugValues();
#endif
    }

    private void OnApplicationQuit()
    {
        // Save all persistent data when application closes
        SavePersistentData();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Save all persistent data when application is paused (mobile)
        if (pauseStatus)
        {
            SavePersistentData();
        }
    }

    #endregion

    #region State Management Methods

    /// <summary>
    /// Changes the game state and notifies all subscribers
    /// </summary>
    /// <param name="newState">The new game state</param>
    public void ChangeState(GameStateEnum newState)
    {
        if (currentState == newState)
            return;

        GameStateEnum previousState = currentState;
        currentState = newState;

        // Handle time scale based on state
        HandleTimeScale(newState);

        // Notify subscribers
        OnGameStateChanged?.Invoke(newState);

        Debug.Log($"Game State Changed: {previousState} -> {newState}");
    }

    /// <summary>
    /// Handles time scale changes based on game state
    /// </summary>
    private void HandleTimeScale(GameStateEnum state)
    {
        switch (state)
        {
            case GameStateEnum.Playing:
                Time.timeScale = 1f;
                break;
            case GameStateEnum.Paused:
                // case GameStateEnum.GameOver:
                Time.timeScale = 0f;
                break;
            case GameStateEnum.LevelComplete:
                // Keep time running for celebration/transition
                Time.timeScale = 1f;
                break;
            case GameStateEnum.MainMenu:
                // Time scale for main menu can be customized
                Time.timeScale = 1f;
                break;
        }
    }

    #endregion

    #region Level Progression Methods

    /// <summary>
    /// Calculates the number of eggs required to complete the current level
    /// </summary>
    /// <returns>Number of eggs required</returns>
    public int GetEggRequirement()
    {
        return CalculateEggRequirement(currentLevel);
    }

    /// <summary>
    /// Calculates the egg requirement for a specific level using the current formula
    /// </summary>
    /// <param name="level">The level to calculate for</param>
    /// <returns>Number of eggs required</returns>
    public int CalculateEggRequirement(int level)
    {
        // Linear formula: base + (level * increment)
        return baseEggRequirement + (level * eggsPerLevel);
    }

    /// <summary>
    /// Sets custom parameters for the egg requirement formula
    /// </summary>
    /// <param name="baseRequirement">Base number of eggs required at level 0</param>
    /// <param name="incrementPerLevel">Additional eggs required per level</param>
    public void SetEggRequirementFormula(int baseRequirement, int incrementPerLevel)
    {
        baseEggRequirement = baseRequirement;
        eggsPerLevel = incrementPerLevel;
        Debug.Log($"Egg requirement formula updated: {baseRequirement} + (level * {incrementPerLevel})");
        RefreshDebug();
    }

    /// <summary>
    /// Advances to the next level
    /// </summary>
    public void NextLevel()
    {
        // Mark current stage as completed and save its stats
        SaveStageCompletion(currentLevel, levelEggs, levelCoins);

        // Unlock next stage if not already unlocked
        int nextStage = currentLevel + 1;
        if (nextStage > highestUnlockedStage)
        {
            UnlockStage(nextStage);
        }

        currentLevel++;
        SavePersistentData();
        ResetLevelStats();
        Debug.Log($"Advanced to level {currentLevel}. Egg requirement: {GetEggRequirement()}");
        RefreshDebug();
    }

    /// <summary>
    /// Resets the current level to 0
    /// </summary>
    public void ResetLevel()
    {
        currentLevel = 0;
        SavePersistentData();
        Debug.Log("Level reset to 0");
        RefreshDebug();
    }

    #endregion

    #region Collection Methods

    /// <summary>
    /// Adds an egg to the current level count and checks win condition
    /// </summary>
    public void AddEgg()
    {
        levelEggs++;
        totalEggs++;
        SavePersistentData();
        Debug.Log($"Egg collected! Level: {levelEggs}/{GetEggRequirement()}, Total: {totalEggs}");

        RefreshDebug();

        // Check if level is complete
        CheckWinCondition();
    }

    /// <summary>
    /// Adds a coin to the current level count
    /// </summary>
    public void AddCoin()
    {
        levelCoins++;
        totalCoins++;
        SavePersistentData();
        Debug.Log($"Coin collected! Level: {levelCoins}, Total: {totalCoins}");
        RefreshDebug();
    }

    /// <summary>
    /// Resets all level-specific statistics (eggs, coins)
    /// </summary>
    public void ResetLevelStats()
    {
        levelEggs = 0;
        levelCoins = 0;
        Debug.Log("Level statistics reset");
        RefreshDebug();
    }

    #endregion

    #region Win Condition

    /// <summary>
    /// Checks if the player has met the win condition for the current level
    /// </summary>
    private void CheckWinCondition()
    {
        int requirement = GetEggRequirement();

        if (levelEggs >= requirement)
        {
            Debug.Log($"Level {currentLevel} complete! Collected {levelEggs}/{requirement} eggs");
            OnLevelWon();
        }
    }

    /// <summary>
    /// Called when the level is won. Changes state and invokes event.
    /// </summary>
    private void OnLevelWon()
    {
        // Change to level complete state
        ChangeState(GameStateEnum.LevelComplete);

        // Invoke level complete event
        OnLevelComplete?.Invoke(currentLevel);
    }

    #endregion

    #region Stage Management

    /// <summary>
    /// Checks if a specific stage is unlocked
    /// </summary>
    /// <param name="stageIndex">The stage index to check (0-based)</param>
    /// <returns>True if the stage is unlocked</returns>
    public bool IsStageUnlocked(int stageIndex)
    {
        // Stage 0 is always unlocked
        if (stageIndex == 0)
            return true;

        return stageIndex <= highestUnlockedStage;
    }

    /// <summary>
    /// Checks if a specific stage has been completed
    /// </summary>
    /// <param name="stageIndex">The stage index to check (0-based)</param>
    /// <returns>True if the stage has been completed</returns>
    public bool IsStageCompleted(int stageIndex)
    {
        string key = STAGE_COMPLETED_KEY_PREFIX + stageIndex;
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    /// <summary>
    /// Gets the best egg count for a specific stage
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <returns>Best egg count for the stage, or 0 if not completed</returns>
    public int GetStageEggs(int stageIndex)
    {
        string key = STAGE_EGGS_KEY_PREFIX + stageIndex;
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// Gets the best coin count for a specific stage
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <returns>Best coin count for the stage, or 0 if not completed</returns>
    public int GetStageCoins(int stageIndex)
    {
        string key = STAGE_COINS_KEY_PREFIX + stageIndex;
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// Unlocks a specific stage
    /// </summary>
    /// <param name="stageIndex">The stage index to unlock (0-based)</param>
    public void UnlockStage(int stageIndex)
    {
        if (stageIndex > highestUnlockedStage)
        {
            highestUnlockedStage = stageIndex;
            SavePersistentData();
            Debug.Log($"Stage {stageIndex} unlocked!");
            RefreshDebug();
        }
    }

    /// <summary>
    /// Saves completion data for a specific stage (only if better than previous)
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <param name="eggs">Eggs collected in this completion</param>
    /// <param name="coins">Coins collected in this completion</param>
    private void SaveStageCompletion(int stageIndex, int eggs, int coins)
    {
        // Mark as completed
        string completedKey = STAGE_COMPLETED_KEY_PREFIX + stageIndex;
        PlayerPrefs.SetInt(completedKey, 1);

        // Save eggs if better than previous best
        string eggsKey = STAGE_EGGS_KEY_PREFIX + stageIndex;
        int previousBestEggs = PlayerPrefs.GetInt(eggsKey, 0);
        if (eggs > previousBestEggs)
        {
            PlayerPrefs.SetInt(eggsKey, eggs);
        }

        // Save coins if better than previous best
        string coinsKey = STAGE_COINS_KEY_PREFIX + stageIndex;
        int previousBestCoins = PlayerPrefs.GetInt(coinsKey, 0);
        if (coins > previousBestCoins)
        {
            PlayerPrefs.SetInt(coinsKey, coins);
        }

        PlayerPrefs.Save();
        Debug.Log($"Stage {stageIndex} completion saved - Eggs: {eggs}, Coins: {coins}");
    }

    /// <summary>
    /// Sets the current level to a specific stage (for stage selection)
    /// </summary>
    /// <param name="stageIndex">The stage index to set as current (0-based)</param>
    public void SetCurrentStage(int stageIndex)
    {
        if (IsStageUnlocked(stageIndex))
        {
            currentLevel = stageIndex;
            ResetLevelStats();
            Debug.Log($"Current stage set to {stageIndex}");
            RefreshDebug();
        }
        else
        {
            Debug.LogWarning($"Cannot set current stage to {stageIndex} - stage is locked!");
        }
    }

    #endregion

    #region Data Persistence

    /// <summary>
    /// Saves all persistent data to PlayerPrefs
    /// </summary>
    private void SavePersistentData()
    {
        PlayerPrefs.SetInt(LEVEL_SAVE_KEY, currentLevel);
        PlayerPrefs.SetInt(TOTAL_EGGS_SAVE_KEY, totalEggs);
        PlayerPrefs.SetInt(TOTAL_COINS_SAVE_KEY, totalCoins);
        PlayerPrefs.SetInt(HIGHEST_UNLOCKED_STAGE_KEY, highestUnlockedStage);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads all persistent data from PlayerPrefs
    /// </summary>
    private void LoadPersistentData()
    {
        currentLevel = PlayerPrefs.GetInt(LEVEL_SAVE_KEY, 0);
        totalEggs = PlayerPrefs.GetInt(TOTAL_EGGS_SAVE_KEY, 0);
        totalCoins = PlayerPrefs.GetInt(TOTAL_COINS_SAVE_KEY, 0);
        highestUnlockedStage = PlayerPrefs.GetInt(HIGHEST_UNLOCKED_STAGE_KEY, 0);

        Debug.Log($"Loaded persistent data - Level: {currentLevel}, Total Eggs: {totalEggs}, Total Coins: {totalCoins}, Highest Unlocked: {highestUnlockedStage}");
        RefreshDebug();
    }

    /// <summary>
    /// Resets all persistent data (for testing or full game reset)
    /// </summary>
    [ContextMenu("Reset All Data")]
    public void ResetAllData()
    {
        currentLevel = 0;
        totalEggs = 0;
        totalCoins = 0;
        highestUnlockedStage = 0; // Stage 0 is always unlocked by default
        ResetLevelStats();

        // Clear all stage completion data
        PlayerPrefs.DeleteAll();

        SavePersistentData();
        Debug.Log("All game data reset");
        RefreshDebug();
    }

    #endregion

    #region Editor Debug Helpers

#if UNITY_EDITOR
    private void RefreshDebug()
    {
        UpdateDebugValues();
    }

    private void UpdateDebugValues()
    {
        debugCurrentLevel = currentLevel;
        debugEggRequirement = GetEggRequirement();
        debugHighestUnlockedStage = highestUnlockedStage;
        debugLevelEggs = levelEggs;
        debugLevelCoins = levelCoins;
        debugTotalEggs = totalEggs;
        debugTotalCoins = totalCoins;
    }
#else
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void RefreshDebug() { }
#endif

    #endregion
}
