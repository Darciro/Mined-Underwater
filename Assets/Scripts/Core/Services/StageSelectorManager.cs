using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Scene-specific manager for the stage selection UI.
/// Manages stage buttons and handles stage selection flow.
/// </summary>
public class StageSelectorManager : MonoBehaviour
{
    #region Inspector Fields

    [Header("Stage Buttons")]
    [SerializeField]
    [Tooltip("Parent container that holds all stage buttons")]
    private Transform stageButtonsContainer;

    [SerializeField]
    [Tooltip("Prefab for stage buttons (must have StageButton component)")]
    private GameObject stageButtonGroupPrefab;

    [Header("Prepare Popup")]
    [SerializeField]
    [Tooltip("Popup that shows before starting a stage")]
    private PreparePopup preparePopup;

    [Header("Stage Configuration")]
    [SerializeField]
    [Tooltip("Total number of stages to display")]
    private int totalStages = 15;

    [SerializeField]
    [Tooltip("Auto-generate buttons on start")]
    private bool autoGenerateButtons = true;

    #endregion

    #region Private Fields

    private List<StageButtonGroup> stageButtonGroups = new List<StageButtonGroup>();

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        // Validate GameManager existence
        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found! StageSelectorManager requires GameManager.");
            return;
        }

        // Auto-generate buttons if enabled
        if (autoGenerateButtons)
        {
            GenerateStageButtons();
        }

        // Refresh all button states
        RefreshAllButtons();
    }

    private void OnEnable()
    {
        // Subscribe to level complete event to refresh buttons
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete += OnLevelCompleted;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from events
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelComplete -= OnLevelCompleted;
        }
    }

    #endregion

    #region Button Generation

    /// <summary>
    /// Generates stage buttons dynamically
    /// </summary>
    public void GenerateStageButtons()
    {
        if (stageButtonGroupPrefab == null)
        {
            Debug.LogError("Stage button prefab is not assigned!");
            return;
        }

        if (stageButtonsContainer == null)
        {
            Debug.LogError("Stage buttons container is not assigned!");
            return;
        }

        // Clear existing buttons
        ClearStageButtons();

        // Generate button groups for each stage (starting from level 0 as tutorial)
        int specialStageCounter = totalStages;
        int darkStageCounter = totalStages + 100;

        for (int i = 0; i < totalStages; i++)
        {
            int specialIndex = -1;
            int darkIndex = -1;

            // Level 0 is the tutorial and has no special/dark branches
            // Special stages appear at odd indices: 1, 3, 5, 7...
            if (i > 0 && (i + 1) % 2 == 0)
            {
                specialIndex = specialStageCounter++;
            }

            // Dark stages appear at indices 3, 7, 11, 15... (every 4th level after tutorial)
            if (i > 0 && (i + 1) % 4 == 0)
            {
                darkIndex = darkStageCounter++;
            }

            CreateStageButtonGroup(i, specialIndex, darkIndex);
        }

    }

    /// <summary>
    /// Creates a single stage button group
    /// </summary>
    /// <param name="mainIndex">The main stage index</param>
    /// <param name="specialIndex">The special branch index (-1 for none)</param>
    /// <param name="darkIndex">The dark branch index (-1 for none)</param>
    private void CreateStageButtonGroup(int mainIndex, int specialIndex, int darkIndex)
    {
        // Instantiate group
        GameObject groupObject = Instantiate(stageButtonGroupPrefab, stageButtonsContainer);
        groupObject.name = $"StageButtonGroup_{mainIndex}";

        // Get or add StageButtonGroup component
        StageButtonGroup buttonGroup = groupObject.GetComponent<StageButtonGroup>();
        if (buttonGroup == null)
        {
            buttonGroup = groupObject.AddComponent<StageButtonGroup>();
        }

        // Initialize group
        buttonGroup.Initialize(mainIndex, specialIndex, darkIndex, OnStageSelected);

        // Add to list
        stageButtonGroups.Add(buttonGroup);
    }

    /// <summary>
    /// Clears all existing stage button groups
    /// </summary>
    private void ClearStageButtons()
    {
        // Clear list
        stageButtonGroups.Clear();

        // Destroy group game objects
        if (stageButtonsContainer != null)
        {
            foreach (Transform child in stageButtonsContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }

    #endregion

    #region Button State Management

    /// <summary>
    /// Refreshes all stage button states
    /// </summary>
    public void RefreshAllButtons()
    {
        foreach (var group in stageButtonGroups)
        {
            if (group != null)
            {
                group.RefreshVisualState();
            }
        }
    }

    /// <summary>
    /// Refreshes a specific stage button group
    /// </summary>
    /// <param name="stageIndex">The stage index to refresh</param>
    public void RefreshButton(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < stageButtonGroups.Count)
        {
            stageButtonGroups[stageIndex]?.RefreshVisualState();
        }
    }

    #endregion

    #region Stage Selection

    /// <summary>
    /// Called when a stage is selected
    /// </summary>
    /// <param name="stageIndex">The selected stage index</param>
    private void OnStageSelected(int stageIndex)
    {

        // Validate stage is unlocked
        if (!GameManager.Instance.IsStageUnlocked(stageIndex))
        {
            Debug.LogWarning($"Attempted to select locked stage {stageIndex}");
            return;
        }

        // Show prepare popup instead of immediately starting the game
        if (preparePopup != null)
        {
            preparePopup.Show(stageIndex);
        }
        else
        {
            Debug.LogError("PreparePopup is not assigned! Cannot show preparation screen.");

            // Fallback: Start game directly if popup is missing
            GameManager.Instance.SetCurrentStage(stageIndex);
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.StartGame();
            }
        }
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Called when a level is completed
    /// </summary>
    /// <param name="completedLevel">The level that was completed</param>
    private void OnLevelCompleted(int completedLevel)
    {
        // Refresh buttons to show newly unlocked stage
        RefreshAllButtons();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Gets the total number of stages
    /// </summary>
    public int TotalStages => totalStages;

    /// <summary>
    /// Sets the total number of stages and regenerates buttons
    /// </summary>
    /// <param name="count">Number of stages</param>
    public void SetTotalStages(int count)
    {
        totalStages = Mathf.Max(1, count);
        GenerateStageButtons();
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    [ContextMenu("Regenerate Stage Buttons")]
    private void EditorRegenerateButtons()
    {
        GenerateStageButtons();
        RefreshAllButtons();
    }

    [ContextMenu("Refresh All Buttons")]
    private void EditorRefreshButtons()
    {
        RefreshAllButtons();
    }
#endif

    #endregion
}