using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component that displays a single stage reward (icon, label and drop chance).
/// Rewards can be inventory items, coins or eggs.
///
/// Attach to the RewardItem prefab and wire up the references in the Inspector.
/// Spawned by the StagePopup for each reward defined in the ObjectivesDatabase.
/// </summary>
public class RewardItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;

    [Tooltip("Optional text showing the drop chance (e.g. 30%)")]
    [SerializeField] private TextMeshProUGUI dropChanceText;

    [Header("Currency Display")]
    [Tooltip("Icon shown for Coins rewards")]
    [SerializeField] private Sprite coinSprite;

    [Tooltip("Name shown for Coins rewards")]
    [SerializeField] private string coinLabel = "Coins";

    [Tooltip("Icon shown for Eggs rewards")]
    [SerializeField] private Sprite eggSprite;

    [Tooltip("Name shown for Eggs rewards")]
    [SerializeField] private string eggLabel = "Eggs";

    /// <summary>The reward this row is displaying.</summary>
    public ObjectivesDatabase.StageReward Reward { get; private set; }

    /// <summary>
    /// Fills the row with the icon and label matching the reward type.
    /// Hide the drop chance when showing rewards already earned (e.g. Win scene).
    /// </summary>
    public void Initialize(ObjectivesDatabase.StageReward reward, bool showDropChance = true)
    {
        Reward = reward;

        Sprite icon;
        string label;
        switch (reward.type)
        {
            case RewardType.Coins:
                icon = coinSprite;
                label = $"{coinLabel} x {reward.amount}";
                break;
            case RewardType.Eggs:
                icon = eggSprite;
                label = $"{eggLabel} x {reward.amount}";
                break;
            default:
                icon = reward.item.icon;
                label = reward.amount > 1 ? $"{reward.item.name} x {reward.amount}" : reward.item.name;
                break;
        }

        if (itemIcon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.enabled = icon != null;
        }

        if (icon == null)
            Debug.LogWarning($"[RewardItemUI] No icon for {reward.type} reward. Assign it on the RewardItem prefab.", this);

        if (itemNameText != null)
            itemNameText.text = label;

        if (dropChanceText != null)
        {
            dropChanceText.gameObject.SetActive(showDropChance);
            dropChanceText.text = $"{reward.dropChance:0.#}%";
        }
    }
}
