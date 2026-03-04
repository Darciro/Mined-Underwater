using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a stage in the selector.
/// Displays main/special/dark branches and handles click events.
/// </summary>
public class StageButtonGroup : MonoBehaviour
{
    private enum StageState { Active, Completed, Locked }

    [Header("Paths")]
    [SerializeField] private bool hasPathLeft;
    [SerializeField] private bool hasPathRight = true;
    [SerializeField] private GameObject pathLeft;
    [SerializeField] private GameObject pathRight;
    [SerializeField] private GameObject pathUp;
    [SerializeField] private GameObject pathDown;

    [Header("Branch Slots")]
    [SerializeField] private GameObject specialStage;
    [SerializeField] private GameObject darkStage;

    [Header("Main Button")]
    [SerializeField] private GameObject mainStageButton;
    [SerializeField] private GameObject stageLevelText;
    [SerializeField] private GameObject activeState;
    [SerializeField] private GameObject completedState;
    [SerializeField] private GameObject lockedState;

    private int mainStageIndex;
    private bool isLocked;
    private Action<int> onStageSelected;
    private Button cachedButton;

    /// <summary>
    /// Configures the button group with stage data and selection callback.
    /// </summary>
    public void Initialize(int mainIndex, int specialIndex, int darkIndex, Action<int> onSelected)
    {
        mainStageIndex = mainIndex;
        onStageSelected = onSelected;

        CacheMainButton();
        SetLabelText(mainIndex);
        QueryStageState(mainIndex);
        ConfigureBranches(specialIndex, darkIndex);
        ConfigurePaths(specialIndex >= 0, darkIndex >= 0);
        ApplyVisualState();
        BindClickHandler();
    }

    /// <summary>
    /// Re-queries GameManager and updates visuals (called when stages unlock).
    /// </summary>
    public void RefreshVisualState()
    {
        if (GameManager.Instance == null) return;

        QueryStageState(mainStageIndex);
        ApplyVisualState();
        UpdateButtonInteractability();
    }

    private void CacheMainButton()
    {
        if (mainStageButton != null)
            cachedButton = mainStageButton.GetComponent<Button>();
    }

    private void SetLabelText(int stageIndex)
    {
        if (stageLevelText == null) return;

        var label = stageLevelText.GetComponent<TextMeshProUGUI>();
        if (label != null)
            label.text = stageIndex.ToString();
    }

    private void QueryStageState(int stageIndex)
    {
        if (GameManager.Instance == null) return;

        isLocked = !GameManager.Instance.IsStageUnlocked(stageIndex);
    }

    private StageState GetCurrentState()
    {
        if (isLocked) return StageState.Locked;

        if (GameManager.Instance != null && GameManager.Instance.IsStageCompleted(mainStageIndex))
            return StageState.Completed;

        return StageState.Active;
    }

    private void ConfigureBranches(int specialIndex, int darkIndex)
    {
        bool hasSpecial = specialIndex >= 0;
        bool hasDark = darkIndex >= 0;

        SetActiveIfNotNull(specialStage, hasSpecial);
        SetActiveIfNotNull(darkStage, hasDark);
    }

    private void ConfigurePaths(bool hasSpecial, bool hasDark)
    {
        SetActiveIfNotNull(pathLeft, hasPathLeft);
        SetActiveIfNotNull(pathRight, hasPathRight);
        SetActiveIfNotNull(pathUp, hasSpecial);
        SetActiveIfNotNull(pathDown, hasDark);
    }

    private void ApplyVisualState()
    {
        StageState state = GetCurrentState();

        SetActiveIfNotNull(activeState, state == StageState.Active);
        SetActiveIfNotNull(completedState, state == StageState.Completed);
        SetActiveIfNotNull(lockedState, state == StageState.Locked);

        UpdateButtonInteractability();
    }

    private void BindClickHandler()
    {
        if (cachedButton == null) return;

        cachedButton.onClick.RemoveAllListeners();
        cachedButton.onClick.AddListener(HandleClick);
        cachedButton.interactable = !isLocked;
    }

    private void HandleClick()
    {
        if (!isLocked)
            onStageSelected?.Invoke(mainStageIndex);
    }

    private void UpdateButtonInteractability()
    {
        if (cachedButton != null)
            cachedButton.interactable = !isLocked;
    }

    private static void SetActiveIfNotNull(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }
}
