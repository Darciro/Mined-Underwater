using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages the stage selection screen: generates stage buttons,
/// handles selection, and shows the preparation popup.
/// </summary>
public class StageSelectorManager : MonoBehaviour
{
    [Header("Stage Buttons")]
    [SerializeField] private Transform stageButtonsContainer;
    [SerializeField] private GameObject stageButtonGroupPrefab;

    [Header("Prepare Popup")]
    [SerializeField] private PreparePopup preparePopup;

    [Header("Stage Configuration")]
    [SerializeField] private int totalStages = 15;
    [SerializeField] private bool autoGenerateButtons = true;

    [Header("Egg Requirement UI")]
    [Tooltip("TextMeshPro to display the egg requirement of the selected stage")]
    [SerializeField] private TextMeshProUGUI eggRequirementText;

    // Special stages start after main stages; dark stages offset further to avoid collision
    private const int DarkStageIndexOffset = 100;

    /// <summary>Egg requirement for the currently selected stage.</summary>
    private int selectedStageEggRequirement;

    private readonly List<StageButtonGroup> buttonGroups = new();

    public int TotalStages => totalStages;

    /// <summary>Egg requirement for the last stage the player tapped.</summary>
    public int SelectedStageEggRequirement => selectedStageEggRequirement;

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[StageSelectorManager] GameManager not found.");
            return;
        }

        if (autoGenerateButtons)
            GenerateStageButtons();

        RefreshAllButtons();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelComplete += OnLevelCompleted;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelComplete -= OnLevelCompleted;
    }

    /// <summary>
    /// Destroys existing buttons and regenerates all stage button groups.
    /// </summary>
    public void GenerateStageButtons()
    {
        if (!ValidatePrefabAndContainer()) return;

        ClearStageButtons();

        int specialStageCounter = totalStages;
        int darkStageCounter = totalStages + DarkStageIndexOffset;

        for (int i = 0; i < totalStages; i++)
        {
            int specialIndex = ShouldHaveSpecialBranch(i) ? specialStageCounter++ : -1;
            int darkIndex = ShouldHaveDarkBranch(i) ? darkStageCounter++ : -1;

            CreateStageButtonGroup(i, specialIndex, darkIndex);
        }
    }

    public void RefreshAllButtons()
    {
        foreach (var group in buttonGroups)
            group?.RefreshVisualState();
    }

    public void RefreshButton(int stageIndex)
    {
        if (stageIndex >= 0 && stageIndex < buttonGroups.Count)
            buttonGroups[stageIndex]?.RefreshVisualState();
    }

    public void SetTotalStages(int count)
    {
        totalStages = Mathf.Max(1, count);
        GenerateStageButtons();
    }

    // --- Branch rules ---
    // Level 0 is the tutorial and never has branches.
    // Special branches appear every 2nd level (indices 1, 3, 5…).
    // Dark branches appear every 4th level (indices 3, 7, 11…).

    private static bool ShouldHaveSpecialBranch(int stageIndex) =>
        stageIndex > 0 && (stageIndex + 1) % 2 == 0;

    private static bool ShouldHaveDarkBranch(int stageIndex) =>
        stageIndex > 0 && (stageIndex + 1) % 4 == 0;

    private void CreateStageButtonGroup(int mainIndex, int specialIndex, int darkIndex)
    {
        var groupObject = Instantiate(stageButtonGroupPrefab, stageButtonsContainer);
        groupObject.name = $"StageButtonGroup_{mainIndex}";

        var buttonGroup = groupObject.GetComponent<StageButtonGroup>();
        if (buttonGroup == null)
            buttonGroup = groupObject.AddComponent<StageButtonGroup>();

        buttonGroup.Initialize(mainIndex, specialIndex, darkIndex, OnStageSelected);
        buttonGroups.Add(buttonGroup);
    }

    private void ClearStageButtons()
    {
        buttonGroups.Clear();

        if (stageButtonsContainer == null) return;

        foreach (Transform child in stageButtonsContainer)
            Destroy(child.gameObject);
    }

    private void OnStageSelected(int stageIndex)
    {
        if (!GameManager.Instance.IsStageUnlocked(stageIndex))
        {
            Debug.LogWarning($"[StageSelectorManager] Attempted to select locked stage {stageIndex}.");
            return;
        }

        // Cache the egg requirement for the selected stage
        selectedStageEggRequirement = GameManager.Instance != null
            ? GameManager.Instance.CalculateEggRequirement(stageIndex)
            : 0;

        if (eggRequirementText != null)
            eggRequirementText.text = selectedStageEggRequirement.ToString();

        if (preparePopup != null)
        {
            preparePopup.Show(stageIndex);
            return;
        }

        // Fallback when popup is missing
        Debug.LogError("[StageSelectorManager] PreparePopup not assigned — starting stage directly.");
        GameManager.Instance.SetCurrentStage(stageIndex);
        LevelManager.Instance?.StartGame(stageIndex == 0);
    }

    private void OnLevelCompleted(int completedLevel) => RefreshAllButtons();

    private bool ValidatePrefabAndContainer()
    {
        if (stageButtonGroupPrefab == null)
        {
            Debug.LogError("[StageSelectorManager] Stage button prefab is not assigned.");
            return false;
        }
        if (stageButtonsContainer == null)
        {
            Debug.LogError("[StageSelectorManager] Stage buttons container is not assigned.");
            return false;
        }
        return true;
    }

#if UNITY_EDITOR
    [ContextMenu("Regenerate Stage Buttons")]
    private void EditorRegenerateButtons()
    {
        GenerateStageButtons();
        RefreshAllButtons();
    }

    [ContextMenu("Refresh All Buttons")]
    private void EditorRefreshButtons() => RefreshAllButtons();
#endif
}