using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    /// <summary>
    /// Invoked when the player successfully doubles their rewards after watching an ad.
    /// </summary>
    public event Action OnRewardsDoubled;

    #endregion

    #region State Management

    [SerializeField]
    [Tooltip("Current state of the game")]
    private GameStateEnum currentState = GameStateEnum.MainMenu;

    [Header("Level Complete")]
    [SerializeField] private GameObject letterbox;

    [Header("Spawner Managers")]
    [SerializeField] private SpawnerManager[] spawnerManagers;

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

    /// <summary>
    /// Calculates the product of the nth and (n+1)th Fibonacci numbers (1-indexed).
    /// e.g. n=4 -> fib(4) * fib(5) = 5 * 8 = 40
    /// </summary>
    /// <param name="n">The position in the Fibonacci sequence</param>
    /// <returns>The product of Fibonacci numbers at positions n and n+1</returns>
    private int CalculateFibonacci(int n)
    {
        if (n <= 0) return 1; // For level 0 or negative, return 1
        if (n == 1) return 1;
        if (n == 2) return 2;

        int a = 1, b = 2;
        for (int i = 3; i <= n; i++)
        {
            int temp = a + b;
            a = b;
            b = temp;
        }
        return b;
    }

    #endregion

    #region Stage Unlocking & Completion

    private const string HIGHEST_UNLOCKED_STAGE_KEY = "HighestUnlockedStage";
    private const string STAGE_COMPLETED_KEY_PREFIX = "StageCompleted_";
    private const string STAGE_EGGS_KEY_PREFIX = "StageEggs_";
    private const string STAGE_COINS_KEY_PREFIX = "StageCoins_";
    private const string STAGE_STARS_KEY_PREFIX = "StageStars_";

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
    private bool rewardsAlreadyDoubled = false;

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
    [SerializeField] private int debugSetLevelTarget;
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

        // Apply spawner configuration for the saved level on first scene load
        ApplySpawnerForLevel(currentLevel);

        SceneManager.sceneLoaded += OnSceneLoaded;

#if UNITY_EDITOR
        UpdateDebugValues();
#endif
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Main" && scene.name != "_Playground") return;

        // Re-discover spawner managers from the freshly loaded scene (stale references
        // become null when Main unloads; this keeps the array current each reload)
        var found = FindObjectsByType<SpawnerManager>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        System.Array.Sort(found, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
        spawnerManagers = found;
        ApplySpawnerForLevel(currentLevel - 1);
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
    /// Calculates the egg requirement for a specific level using the Fibonacci sequence
    /// </summary>
    /// <param name="level">The level to calculate for</param>
    /// <returns>Number of eggs required (Fibonacci number for the level)</returns>
    public int CalculateEggRequirement(int level)
    {
        // Use Fibonacci sequence for egg requirements
        // Level 0 -> 1, Level 1 -> 1, Level 2 -> 2, Level 3 -> 3, Level 4 -> 5, etc.
        return CalculateFibonacci(level);
    }

    /// <summary>
    /// Advances to the next level
    /// </summary>
    public void NextLevel()
    {
        // Mark current stage as completed and save its stats
        SaveStageCompletion(currentLevel, levelEggs, levelCoins, LastStarRating);

        // Unlock next stage if not already unlocked
        int nextStage = currentLevel + 1;
        if (nextStage > highestUnlockedStage)
        {
            UnlockStage(nextStage);
        }

        currentLevel++;
        SavePersistentData();
        ResetLevelStats();
        RefreshDebug();
    }

    /// <summary>
    /// Resets the current level to 0
    /// </summary>
    public void ResetLevel()
    {
        currentLevel = 0;
        SavePersistentData();
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
        RefreshDebug();
    }

    /// <summary>
    /// Spends coins from the player's total
    /// </summary>
    /// <param name="amount">Amount of coins to spend</param>
    /// <returns>True if the purchase was successful, false if not enough coins</returns>
    public bool SpendCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot spend negative coins!");
            return false;
        }

        if (totalCoins < amount)
        {
            Debug.LogWarning($"Not enough coins! Have {totalCoins}, need {amount}");
            return false;
        }

        totalCoins -= amount;
        SavePersistentData();
        RefreshDebug();
        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot add zero or negative coins!");
            return;
        }

        totalCoins += amount;
        SavePersistentData();
        RefreshDebug();
    }

    /// <summary>
    /// Spends eggs from the player's total
    /// </summary>
    /// <param name="amount">Amount of eggs to spend</param>
    /// <returns>True if the purchase was successful, false if not enough eggs</returns>
    public bool SpendEggs(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot spend negative eggs!");
            return false;
        }

        if (totalEggs < amount)
        {
            Debug.LogWarning($"Not enough eggs! Have {totalEggs}, need {amount}");
            return false;
        }

        totalEggs -= amount;
        SavePersistentData();
        RefreshDebug();
        return true;
    }

    /// <summary>
    /// Resets all level-specific statistics (eggs, coins)
    /// </summary>
    public void ResetLevelStats()
    {
        levelEggs = 0;
        levelCoins = 0;
        rewardsAlreadyDoubled = false;
        RefreshDebug();
    }

    /// <summary>
    /// Doubles the coins and eggs earned this level and adds them to the lifetime totals.
    /// Can only be claimed once per level.
    /// </summary>
    /// <returns>True if rewards were doubled, false if already claimed.</returns>
    public bool ClaimDoubleRewards()
    {
        if (rewardsAlreadyDoubled)
        {
            Debug.LogWarning("Double rewards already claimed for this level.");
            return false;
        }

        rewardsAlreadyDoubled = true;

        // Add the level amounts again to both level and lifetime totals
        totalEggs += levelEggs;
        levelEggs *= 2;

        totalCoins += levelCoins;
        levelCoins *= 2;

        SavePersistentData();
        RefreshDebug();

        OnRewardsDoubled?.Invoke();
        return true;
    }

#if UNITY_EDITOR
    /// <summary>
    /// [EDITOR ONLY] Adds 10 eggs for testing
    /// </summary>
    [ContextMenu("Debug/Add 10 Eggs")]
    private void Debug_Add10Eggs()
    {
        for (int i = 0; i < 10; i++)
        {
            AddEgg();
        }
    }

    /// <summary>
    /// [EDITOR ONLY] Adds 100 coins for testing
    /// </summary>
    [ContextMenu("Debug/Add 100 Coins")]
    private void Debug_Add100Coins()
    {
        for (int i = 0; i < 100; i++)
        {
            AddCoin();
        }
    }

    /// <summary>
    /// [EDITOR ONLY] Adds 1000 coins for testing shop
    /// </summary>
    [ContextMenu("Debug/Add 1000 Coins")]
    private void Debug_Add1000Coins()
    {
        for (int i = 0; i < 1000; i++)
        {
            levelCoins++;
            totalCoins++;
        }
        SavePersistentData();
        RefreshDebug();
    }

    /// <summary>
    /// [EDITOR ONLY] Adds custom amount of eggs
    /// </summary>
    public void Debug_AddEggs(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            AddEgg();
        }
    }

    /// <summary>
    /// [EDITOR ONLY] Adds custom amount of coins
    /// </summary>
    public void Debug_AddCoins(int amount)
    {
        levelCoins += amount;
        totalCoins += amount;
        SavePersistentData();
        RefreshDebug();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying) return;

        if (SceneManager.GetActiveScene().name == "_Playground")
        {
            currentLevel = debugSetLevelTarget;
            highestUnlockedStage = Mathf.Max(highestUnlockedStage, currentLevel);
            ResetLevelStats();
            currentLevel = debugSetLevelTarget;
            ApplySpawnerForLevel(currentLevel);
            SavePersistentData();
            RefreshDebug();
        }
        ;

    }

    /* private void OnValidate()
    {
        if (!Application.isPlaying) return;
        
        currentLevel = debugSetLevelTarget;
        highestUnlockedStage = Mathf.Max(highestUnlockedStage, currentLevel);
        ResetLevelStats();
        ApplySpawnerForLevel(currentLevel);
        SavePersistentData();
        RefreshDebug();
    } */
#endif

    #endregion

    #region Win Condition

    /// <summary>
    /// Star rating awarded at the end of the last completed level.
    /// 1 star = egg requirement met.
    /// +1 star per completed objective, capped at 3.
    /// </summary>
    public int LastStarRating { get; private set; } = 0;

    /// <summary>
    /// Checks if the player has met the win condition for the current level
    /// </summary>
    private void CheckWinCondition()
    {
        int requirement = GetEggRequirement();

        if (levelEggs >= requirement)
        {
            OnLevelWon();
        }
    }

    /// <summary>
    /// Called when the level is won. Calculates the star rating, changes state, and invokes the event.
    /// </summary>
    private void OnLevelWon()
    {
        // 1 star for meeting the egg requirement, +1 per completed objective, max 3
        int completedObjectives = 0;
        if (ObjectivesManager.Instance != null)
        {
            foreach (var progress in ObjectivesManager.Instance.ActiveObjectives)
            {
                if (progress.IsCompleted) completedObjectives++;
            }
        }
        LastStarRating = Mathf.Clamp(1 + completedObjectives, 1, 3);

        // Tutorial always awards 3 stars
        if (currentLevel == 0)
            LastStarRating = 3;

        // Activate letterbox and play its feedbacks
        if (letterbox != null)
        {
            letterbox.SetActive(true);
            letterbox.GetComponent<MoreMountains.Feedbacks.MMF_Player>()?.PlayFeedbacks();
        }

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
    /// Gets the best star rating for a specific stage
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <returns>Best star rating (1-3) for the stage, or 0 if not completed</returns>
    public int GetStageStars(int stageIndex)
    {
        string key = STAGE_STARS_KEY_PREFIX + stageIndex;
        return PlayerPrefs.GetInt(key, 0);
    }

    /// <summary>
    /// Saves a star rating for a stage if it is better than the current saved value.
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <param name="stars">Star rating to record (1-3)</param>
    public void RecordStageStars(int stageIndex, int stars)
    {
        string key = STAGE_STARS_KEY_PREFIX + stageIndex;
        int current = PlayerPrefs.GetInt(key, 0);
        if (stars > current)
        {
            PlayerPrefs.SetInt(key, stars);
            PlayerPrefs.Save();
        }
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
            RefreshDebug();
        }
    }

    /// <summary>
    /// Saves completion data for a specific stage (only if better than previous)
    /// </summary>
    /// <param name="stageIndex">The stage index (0-based)</param>
    /// <param name="eggs">Eggs collected in this completion</param>
    /// <param name="coins">Coins collected in this completion</param>
    /// <param name="stars">Star rating earned in this completion (1-3)</param>
    private void SaveStageCompletion(int stageIndex, int eggs, int coins, int stars)
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

        // Save stars if better than previous best
        string starsKey = STAGE_STARS_KEY_PREFIX + stageIndex;
        int previousBestStars = PlayerPrefs.GetInt(starsKey, 0);
        if (stars > previousBestStars)
        {
            PlayerPrefs.SetInt(starsKey, stars);
        }

        PlayerPrefs.Save();
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
            RefreshDebug();
            ApplySpawnerForLevel(stageIndex);
        }
        else
        {
            Debug.LogWarning($"Cannot set current stage to {stageIndex} - stage is locked!");
        }
    }

    /// <summary>
    /// Disables all spawner managers and enables only the one matching the given level index.
    /// </summary>
    /// <param name="level">The level index whose spawner should be activated</param>
    private void ApplySpawnerForLevel(int level)
    {
        if (spawnerManagers == null || spawnerManagers.Length == 0)
            return;

        for (int i = 0; i < spawnerManagers.Length; i++)
        {
            if (spawnerManagers[i] == null) continue;
            spawnerManagers[i].gameObject.SetActive(i == level);
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
