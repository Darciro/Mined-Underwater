using System;
using UnityEngine;

/// <summary>
/// Singleton manager responsible for handling game states and persistent user points.
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

    #region User Points (Persistent Currency)

    private const string POINTS_SAVE_KEY = "UserPoints";
    private int userPoints = 0;

    /// <summary>
    /// Gets the current user points
    /// </summary>
    public int UserPoints => userPoints;

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

        // Load persistent points from PlayerPrefs
        LoadPoints();
    }

    private void OnApplicationQuit()
    {
        // Save points when application closes
        SavePoints();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        // Save points when application is paused (mobile)
        if (pauseStatus)
        {
            SavePoints();
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
            case GameStateEnum.MainMenu:
                // Time scale for main menu can be customized
                Time.timeScale = 1f;
                break;
        }
    }

    #endregion

    #region Points Management

    /// <summary>
    /// Adds points to the user's persistent currency
    /// </summary>
    /// <param name="amount">Amount of points to add</param>
    public void AddPoints(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempted to add negative points: {amount}. Use SpendPoints instead.");
            return;
        }

        userPoints += amount;
        SavePoints();
        Debug.Log($"Added {amount} points. Total: {userPoints}");
    }

    /// <summary>
    /// Attempts to spend points from the user's currency
    /// </summary>
    /// <param name="amount">Amount of points to spend</param>
    /// <returns>True if the transaction was successful, false if insufficient funds</returns>
    public bool SpendPoints(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Attempted to spend negative points: {amount}");
            return false;
        }

        if (userPoints < amount)
        {
            Debug.LogWarning($"Insufficient points. Required: {amount}, Available: {userPoints}");
            return false;
        }

        userPoints -= amount;
        SavePoints();
        Debug.Log($"Spent {amount} points. Remaining: {userPoints}");
        return true;
    }

    /// <summary>
    /// Gets the current user points
    /// </summary>
    /// <returns>Current points total</returns>
    public int GetPoints()
    {
        return userPoints;
    }

    /// <summary>
    /// Saves user points to PlayerPrefs
    /// </summary>
    private void SavePoints()
    {
        PlayerPrefs.SetInt(POINTS_SAVE_KEY, userPoints);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Loads user points from PlayerPrefs
    /// </summary>
    private void LoadPoints()
    {
        userPoints = PlayerPrefs.GetInt(POINTS_SAVE_KEY, 0);
        Debug.Log($"Loaded user points: {userPoints}");
    }

    /// <summary>
    /// Resets user points to zero (for testing or reset functionality)
    /// </summary>
    public void ResetPoints()
    {
        userPoints = 0;
        SavePoints();
        Debug.Log("User points reset to 0");
    }

    #endregion
}
