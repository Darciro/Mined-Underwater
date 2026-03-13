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