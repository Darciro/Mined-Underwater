using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup that appears before starting a stage, allowing the player to prepare.
/// Displays stage information and provides options to start the level or go back.
/// </summary>
public class PreparePopup : MonoBehaviour
{
    #region Inspector Fields

    [Header("UI References")]
    [SerializeField]
    [Tooltip("Button to start the selected level")]
    private Button startLevelButton;

    [SerializeField]
    [Tooltip("Button to close the popup and go back")]
    private Button backButton;

    [Header("Optional UI Elements")]
    [SerializeField]
    [Tooltip("Optional text component to display the stage number")]
    private TMPro.TextMeshProUGUI stageNumberText;

    [SerializeField]
    [Tooltip("Optional text component to display egg requirement")]
    private TMPro.TextMeshProUGUI eggRequirementText;

    [Header("Animation")]
    [SerializeField]
    [Tooltip("Optional feedback to play when popup shows (Feel/MMFeedbacks)")]
    private MMF_Player showFeedback;

    [Header("Shop System")]
    [SerializeField]
    [Tooltip("Reference to the ResourcesPopup for displaying shop items")]
    private ResourcesPopup resourcesPopup;

    [Header("Shop Items - Upgrades")]
    [SerializeField]
    [Tooltip("Health upgrade item")]
    private ShopItemSO healthUpgradeItem;

    [SerializeField]
    [Tooltip("Air upgrade item")]
    private ShopItemSO airUpgradeItem;

    [SerializeField]
    [Tooltip("Speed upgrade item")]
    private ShopItemSO speedUpgradeItem;

    [SerializeField]
    [Tooltip("Damage upgrade item")]
    private ShopItemSO damageUpgradeItem;

    [Header("Shop Items - Consumables")]
    [SerializeField]
    [Tooltip("Health potion item")]
    private ShopItemSO potionItem;

    [SerializeField]
    [Tooltip("Shield item")]
    private ShopItemSO shieldItem;

    [SerializeField]
    [Tooltip("Bomb item")]
    private ShopItemSO bombItem;

    [SerializeField]
    [Tooltip("Magnet item")]
    private ShopItemSO magnetItem;

    #endregion

    #region Private Fields

    private int selectedStageIndex = -1;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Setup button listeners
        if (startLevelButton != null)
        {
            startLevelButton.onClick.AddListener(OnStartLevelClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackClicked);
        }
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (startLevelButton != null)
        {
            startLevelButton.onClick.RemoveListener(OnStartLevelClicked);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveListener(OnBackClicked);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Shows the prepare popup for a specific stage
    /// </summary>
    /// <param name="stageIndex">The stage index to prepare for (0-based)</param>
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

        Debug.Log($"PreparePopup shown for stage {stageIndex}");
    }

    /// <summary>
    /// Closes the prepare popup
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        selectedStageIndex = -1;

        Debug.Log("PreparePopup closed");
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates the stage information displayed in the popup
    /// </summary>
    /// <param name="stageIndex">The stage index</param>
    private void UpdateStageInfo(int stageIndex)
    {
        // Update stage number text
        if (stageNumberText != null)
        {
            stageNumberText.text = stageIndex == 0 ? "Tutorial" : $"Stage {stageIndex}";
        }

        // Update egg requirement text
        if (eggRequirementText != null && GameManager.Instance != null)
        {
            int eggRequirement = GameManager.Instance.CalculateEggRequirement(stageIndex);
            eggRequirementText.text = $"Collect {eggRequirement} eggs to complete";
        }
    }

    /// <summary>
    /// Called when the start level button is clicked
    /// </summary>
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
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("LevelManager instance not found! Cannot start stage.");
        }

        // Close the popup
        Close();
    }

    /// <summary>
    /// Called when the back button is clicked
    /// </summary>
    private void OnBackClicked()
    {
        Close();
    }

    #endregion

    #region Shop System - Public Methods for UI Buttons

    /// <summary>
    /// Shows the ResourcesPopup for Health Upgrade
    /// </summary>
    public void OnHealthUpgradeClicked()
    {
        ShowShopItem(healthUpgradeItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Air Upgrade
    /// </summary>
    public void OnAirUpgradeClicked()
    {
        ShowShopItem(airUpgradeItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Speed Upgrade
    /// </summary>
    public void OnSpeedUpgradeClicked()
    {
        ShowShopItem(speedUpgradeItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Damage Upgrade
    /// </summary>
    public void OnDamageUpgradeClicked()
    {
        ShowShopItem(damageUpgradeItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Potion
    /// </summary>
    public void OnPotionClicked()
    {
        ShowShopItem(potionItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Shield
    /// </summary>
    public void OnShieldClicked()
    {
        ShowShopItem(shieldItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Bomb
    /// </summary>
    public void OnBombClicked()
    {
        ShowShopItem(bombItem);
    }

    /// <summary>
    /// Shows the ResourcesPopup for Magnet
    /// </summary>
    public void OnMagnetClicked()
    {
        ShowShopItem(magnetItem);
    }

    /// <summary>
    /// Helper method to show the ResourcesPopup with a shop item
    /// </summary>
    /// <param name="item">The shop item to display</param>
    private void ShowShopItem(ShopItemSO item)
    {
        if (resourcesPopup == null)
        {
            Debug.LogError("ResourcesPopup reference not assigned in PreparePopup!");
            return;
        }

        if (item == null)
        {
            Debug.LogError("Shop item is null!");
            return;
        }

        resourcesPopup.Show(item);
    }

    #endregion
}
