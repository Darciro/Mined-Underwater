using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup that appears before starting a stage, allowing the player to prepare.
/// Displays stage information and provides options to start the level or go back.
/// </summary>
public class StagePopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField]
    [Tooltip("Button to start the selected level")]
    private Button startLevelButton;

    [Header("Optional UI Elements")]
    [SerializeField]
    [Tooltip("Optional text component to display the stage number")]
    private TMPro.TextMeshProUGUI stageText;

    [SerializeField]
    [Tooltip("Optional text component to display egg requirement")]
    private TMPro.TextMeshProUGUI eggRequirementText;

    [Header("Animation")]
    [SerializeField]
    [Tooltip("Optional feedback to play when popup shows (Feel/MMFeedbacks)")]
    private MMF_Player showFeedback;

    [Header("Objectives Preview")]
    [Tooltip("Container where objective rows are spawned (e.g. a VerticalLayoutGroup)")]
    [SerializeField] private Transform objectivesContainer;

    [Tooltip("Prefab with an ObjectiveItemUI component used to display each objective")]
    [SerializeField] private ObjectiveItemUI objectiveItemPrefab;

    private int selectedStageIndex = -1;

    #region Unity Lifecycle

    private void Awake()
    {
        gameObject.SetActive(false); // Ensure popup starts hidden

        // Setup button listeners
        if (startLevelButton != null)
        {
            startLevelButton.onClick.AddListener(OnStartLevelClicked);
        }

        /* if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        } */
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (startLevelButton != null)
        {
            startLevelButton.onClick.RemoveListener(OnStartLevelClicked);
        }

        /*  if (backButton != null)
         {
             backButton.onClick.RemoveListener(OnBackClicked);
         } */
    }

    #endregion

    public void Show(int stageIndex)
    {
        selectedStageIndex = stageIndex;

        // Update UI elements
        UpdateStageInfo(stageIndex);
        PopulateObjectives(stageIndex);

        // Enable the popup
        gameObject.SetActive(true);

        // Play show animation/feedback
        if (showFeedback != null)
        {
            showFeedback.PlayFeedbacks();
        }
    }

    public void Close()
    {
        // Play show animation/feedback
        if (showFeedback != null)
        {
            showFeedback.PlayFeedbacks();
        }

        gameObject.SetActive(false);
        selectedStageIndex = -1;

    }

    private void UpdateStageInfo(int stageIndex)
    {
        // Update stage number text
        if (stageText != null)
        {
            stageText.text = stageIndex == 0 ? "TUTORIAL" : $"STAGE {stageIndex}";
        }

        // Update egg requirement text
        if (eggRequirementText != null && GameManager.Instance != null)
        {
            int eggRequirement = GameManager.Instance.CalculateEggRequirement(stageIndex);
            eggRequirementText.text = $"x {eggRequirement}";
        }
    }

    private void PopulateObjectives(int stageIndex)
    {
        if (objectivesContainer == null || objectiveItemPrefab == null) return;

        // Clear only previously spawned ObjectiveItemUI rows
        for (int i = objectivesContainer.childCount - 1; i >= 0; i--)
        {
            var child = objectivesContainer.GetChild(i);
            if (child.GetComponent<ObjectiveItemUI>() != null)
                Destroy(child.gameObject);
        }

        if (ObjectivesManager.Instance == null || ObjectivesManager.Instance.Database == null) return;

        var objectives = ObjectivesManager.Instance.Database.GetObjectivesForStage(stageIndex);
        foreach (var data in objectives)
        {
            if (data == null) continue;
            var item = Instantiate(objectiveItemPrefab, objectivesContainer);
            item.InitializeStatic(data);
        }
    }

    private void OnStartLevelClicked()
    {
        if (selectedStageIndex < 0)
        {
            Debug.LogWarning("Cannot start level - no stage selected!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // Set the current stage in GameManager
        GameManager.Instance.SetCurrentStage(selectedStageIndex);

        // Start the level through LevelManager
        bool isTutorial = selectedStageIndex == 0;
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartGame(isTutorial);
        }
        else
        {
            Debug.LogError("LevelManager instance not found! Cannot start stage.");
        }

        // Close the popup
        Close();
    }
}