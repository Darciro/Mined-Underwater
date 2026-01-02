using System;
using UnityEngine;

/// <summary>
/// Manages a group of stage buttons with optional special/dark branches.
/// Simple container for button slots and connection lines.
/// </summary>
public class StageButtonGroup : MonoBehaviour
{
    #region Inspector Fields

    [Header("Configuration")]
    [SerializeField] private bool hasPathLeft = false;

    [SerializeField] private bool hasPathRight = true;

    [SerializeField] private bool hasSpecialStage = false;

    [SerializeField] private bool hasDarkStage = false;

    [Header("Path Objects")]
    [SerializeField] private GameObject pathLeft;

    [SerializeField] private GameObject pathRight;

    [SerializeField] private GameObject pathUp;

    [SerializeField] private GameObject pathDown;

    [Header("Button Slots")]
    [SerializeField] private GameObject specialStage;

    [SerializeField] private GameObject darkStage;

    [SerializeField] private GameObject mainStageButton;

    [Header("Main stage")]
    [SerializeField] private bool stageCompleted = false;
    [SerializeField] private bool stageLocked = false;
    [SerializeField] private GameObject stageLevelText;
    [SerializeField] private GameObject activeState;
    [SerializeField] private GameObject completedState;
    [SerializeField] private GameObject lockedState;

    #endregion

    #region Private Fields

    private int mainStageIndex;
    private int specialStageIndex;
    private int darkStageIndex;
    private Action<int> onStageSelectedCallback;

    #endregion

    #region Initialization

    /// <summary>
    /// Initializes the button group with stage indices
    /// </summary>
    public void Initialize(int mainIndex, int specialIndex, int darkIndex, Action<int> onSelected)
    {
        // Store indices and callback
        mainStageIndex = mainIndex;
        specialStageIndex = specialIndex;
        darkStageIndex = darkIndex;
        onStageSelectedCallback = onSelected;

        // Update stage level text (0-based: 0 is tutorial, then 1, 2, 3...)
        if (stageLevelText != null)
        {
            var textComponent = stageLevelText.GetComponent<TMPro.TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = mainIndex.ToString();
            }
        }

        // Query GameManager for stage state
        if (GameManager.Instance != null)
        {
            stageLocked = !GameManager.Instance.IsStageUnlocked(mainIndex);
            stageCompleted = GameManager.Instance.IsStageCompleted(mainIndex);
        }

        // Determine if special/dark stages exist
        hasSpecialStage = specialIndex >= 0;
        hasDarkStage = darkIndex >= 0;

        // Setup paths
        if (pathLeft != null)
            pathLeft.SetActive(hasPathLeft);

        if (pathRight != null)
            pathRight.SetActive(hasPathRight);

        if (pathUp != null)
            pathUp.SetActive(hasSpecialStage);

        if (pathDown != null)
            pathDown.SetActive(hasDarkStage);

        // Setup special/dark stages
        if (specialStage != null)
            specialStage.SetActive(hasSpecialStage);

        if (darkStage != null)
            darkStage.SetActive(hasDarkStage);

        // Set main stage state based on locked/completed status
        UpdateMainStageState();

        // Setup button click handler
        SetupButtonClickHandler();
    }

    /// <summary>
    /// Updates the main stage visual state
    /// </summary>
    private void UpdateMainStageState()
    {
        if (stageLocked)
        {
            if (activeState != null) activeState.SetActive(false);
            if (completedState != null) completedState.SetActive(false);
            if (lockedState != null) lockedState.SetActive(true);
        }
        else if (stageCompleted)
        {
            if (activeState != null) activeState.SetActive(false);
            if (completedState != null) completedState.SetActive(true);
            if (lockedState != null) lockedState.SetActive(false);
        }
        else
        {
            if (activeState != null) activeState.SetActive(true);
            if (completedState != null) completedState.SetActive(false);
            if (lockedState != null) lockedState.SetActive(false);
        }
    }

    /// <summary>
    /// Sets up button click handler for main stage button
    /// </summary>
    private void SetupButtonClickHandler()
    {
        if (mainStageButton != null)
        {
            var button = mainStageButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnMainStageClicked());
                button.interactable = !stageLocked;
            }
        }
    }

    /// <summary>
    /// Called when main stage button is clicked
    /// </summary>
    private void OnMainStageClicked()
    {
        if (!stageLocked)
        {
            onStageSelectedCallback?.Invoke(mainStageIndex);
        }
    }

    /// <summary>
    /// Refreshes the visual state of this button group
    /// </summary>
    public void RefreshVisualState()
    {
        if (GameManager.Instance != null)
        {
            stageLocked = !GameManager.Instance.IsStageUnlocked(mainStageIndex);
            stageCompleted = GameManager.Instance.IsStageCompleted(mainStageIndex);
            UpdateMainStageState();

            // Update button interactability
            if (mainStageButton != null)
            {
                var button = mainStageButton.GetComponent<UnityEngine.UI.Button>();
                if (button != null)
                {
                    button.interactable = !stageLocked;
                }
            }
        }
    }

    #endregion
}
