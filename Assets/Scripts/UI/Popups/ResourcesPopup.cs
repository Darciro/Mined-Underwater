using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Popup that displays detailed information about a shop item or upgrade.
/// Shows type, name, description, icon, and allows purchasing.
/// </summary>
public class ResourcesPopup : MonoBehaviour
{
    #region Inspector Fields

    [Header("UI References")]
    [SerializeField]
    [Tooltip("Text to display item type (Upgrade/Item)")]
    private TextMeshProUGUI typeText;

    [SerializeField]
    [Tooltip("Text to display item name")]
    private TextMeshProUGUI nameText;

    [SerializeField]
    [Tooltip("Text to display item description")]
    private TextMeshProUGUI descriptionText;

    [SerializeField]
    [Tooltip("Image to display item icon")]
    private Image iconImage;

    [SerializeField]
    [Tooltip("Text to display item cost")]
    private TextMeshProUGUI costText;

    [SerializeField]
    [Tooltip("Button to purchase the item")]
    private Button buyButton;

    [SerializeField]
    [Tooltip("Button to close the popup")]
    private Button closeButton;

    [Header("Animation")]
    [SerializeField]
    [Tooltip("Optional feedback to play when popup shows")]
    private MMF_Player showFeedback;

    [SerializeField]
    [Tooltip("Optional feedback to play on successful purchase")]
    private MMF_Player purchaseSuccessFeedback;

    [SerializeField]
    [Tooltip("Optional feedback to play when purchase fails")]
    private MMF_Player purchaseFailedFeedback;

    #endregion

    #region Private Fields

    private ShopItemSO currentItem;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        // Setup button listeners
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseClicked);
        }

        // Start hidden
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up button listeners
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Shows the popup with the specified shop item
    /// </summary>
    /// <param name="item">The shop item to display</param>
    public void Show(ShopItemSO item)
    {
        if (item == null)
        {
            Debug.LogError("Cannot show ResourcesPopup with null item!");
            return;
        }

        currentItem = item;

        // Update UI elements
        UpdateUI();

        // Enable the popup
        gameObject.SetActive(true);

        // Play show animation
        if (showFeedback != null)
        {
            showFeedback.PlayFeedbacks();
        }

        Debug.Log($"ResourcesPopup shown for: {item.itemName}");
    }

    /// <summary>
    /// Closes the popup
    /// </summary>
    public void Close()
    {
        gameObject.SetActive(false);
        currentItem = null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Updates all UI elements with current item data
    /// </summary>
    private void UpdateUI()
    {
        if (currentItem == null) return;

        // Update type text
        if (typeText != null)
        {
            typeText.text = currentItem.itemType == ShopItemType.Upgrade ? "UPGRADE" : "ITEM";
        }

        // Update name text
        if (nameText != null)
        {
            nameText.text = currentItem.itemName;
        }

        // Update description text
        if (descriptionText != null)
        {
            descriptionText.text = currentItem.description;
        }

        // Update icon image
        if (iconImage != null && currentItem.icon != null)
        {
            iconImage.sprite = currentItem.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        // Update cost text
        if (costText != null)
        {
            costText.text = $"{currentItem.cost} Coins";
        }

        // Update buy button interactability based on available coins
        UpdateBuyButtonState();
    }

    /// <summary>
    /// Updates the buy button's interactable state based on if player can afford the item
    /// </summary>
    private void UpdateBuyButtonState()
    {
        if (buyButton == null || currentItem == null) return;

        if (GameManager.Instance != null)
        {
            bool canAfford = GameManager.Instance.TotalCoins >= currentItem.cost;
            buyButton.interactable = canAfford;
        }
    }

    /// <summary>
    /// Called when the buy button is clicked
    /// </summary>
    private void OnBuyClicked()
    {
        if (currentItem == null)
        {
            Debug.LogWarning("Cannot purchase - no item selected!");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        // Check if player can afford the item
        if (GameManager.Instance.TotalCoins < currentItem.cost)
        {
            Debug.Log($"Cannot afford {currentItem.itemName}. Need {currentItem.cost}, have {GameManager.Instance.TotalCoins}");

            if (purchaseFailedFeedback != null)
            {
                purchaseFailedFeedback.PlayFeedbacks();
            }

            return;
        }

        // Deduct coins from GameManager
        bool success = GameManager.Instance.SpendCoins(currentItem.cost);

        if (!success)
        {
            Debug.LogWarning("Failed to spend coins!");

            if (purchaseFailedFeedback != null)
            {
                purchaseFailedFeedback.PlayFeedbacks();
            }

            return;
        }

        // Apply the purchase to the player
        ApplyPurchase();

        // Play success feedback
        if (purchaseSuccessFeedback != null)
        {
            purchaseSuccessFeedback.PlayFeedbacks();
        }

        Debug.Log($"Successfully purchased {currentItem.itemName} for {currentItem.cost} coins!");

        // Close the popup after successful purchase
        Close();
    }

    /// <summary>
    /// Applies the purchased item/upgrade to the player
    /// </summary>
    private void ApplyPurchase()
    {
        if (currentItem == null) return;

        // Apply based on item type
        if (currentItem.itemType == ShopItemType.Upgrade)
        {
            ApplyUpgrade();
        }
        else if (currentItem.itemType == ShopItemType.Item)
        {
            AddItemToInventory();
        }

        // If player is in scene, reload their upgrades/inventory immediately
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            if (currentItem.itemType == ShopItemType.Upgrade)
            {
                player.LoadUpgrades();
                Debug.Log("Upgrades reloaded and applied to player immediately");
            }
            else
            {
                player.LoadInventory();
                Debug.Log("Inventory reloaded immediately");
            }
        }
        else
        {
            Debug.Log("Purchase saved. Will be applied when player spawns.");
        }
    }

    /// <summary>
    /// Applies a permanent upgrade to player stats
    /// </summary>
    private void ApplyUpgrade()
    {
        string upgradeKey = GetUpgradePrefsKey(currentItem.effectType);
        int currentLevel = PlayerPrefs.GetInt(upgradeKey, 0);
        currentLevel++;
        PlayerPrefs.SetInt(upgradeKey, currentLevel);
        PlayerPrefs.Save();

        Debug.Log($"Upgraded {currentItem.effectType} to level {currentLevel}");
    }

    /// <summary>
    /// Adds a consumable item to player's inventory
    /// </summary>
    private void AddItemToInventory()
    {
        string itemKey = GetItemPrefsKey(currentItem.effectType);
        int currentCount = PlayerPrefs.GetInt(itemKey, 0);
        currentCount += currentItem.usesCount;
        PlayerPrefs.SetInt(itemKey, currentCount);
        PlayerPrefs.Save();

        Debug.Log($"Added {currentItem.usesCount}x {currentItem.effectType} to inventory (total: {currentCount})");
    }

    /// <summary>
    /// Gets the PlayerPrefs key for an upgrade type
    /// </summary>
    private string GetUpgradePrefsKey(ShopEffectType effectType)
    {
        return $"UpgradeLevel_{effectType}";
    }

    /// <summary>
    /// Gets the PlayerPrefs key for an item type
    /// </summary>
    private string GetItemPrefsKey(ShopEffectType effectType)
    {
        return $"ItemCount_{effectType}";
    }

    /// <summary>
    /// Called when the close button is clicked
    /// </summary>
    private void OnCloseClicked()
    {
        Close();
    }

    #endregion
}
