using UnityEngine;

/// <summary>
/// Defines the type of shop item (permanent upgrade vs consumable item)
/// </summary>
public enum ShopItemType
{
    Upgrade,    // Permanent stat increases
    Item        // Consumable items used during gameplay
}

/// <summary>
/// Defines what effect the shop item provides
/// </summary>
public enum ShopEffectType
{
    // Upgrades (permanent)
    HealthUpgrade,
    AirUpgrade,
    SpeedUpgrade,
    DamageUpgrade,

    // Items (consumables)
    Potion,
    Shield,
    Bomb,
    Magnet
}

/// <summary>
/// ScriptableObject that defines a shop item or upgrade
/// </summary>
[CreateAssetMenu(fileName = "ShopItem", menuName = "Shop/New Shop Item", order = 1)]
public class ShopItemSO : ScriptableObject
{
    [Header("Display Information")]
    [Tooltip("Name of the item/upgrade displayed in UI")]
    public string itemName;

    [Tooltip("Description explaining what this item/upgrade does")]
    [TextArea(3, 5)]
    public string description;

    [Tooltip("Icon sprite displayed in UI")]
    public Sprite icon;

    [Header("Shop Configuration")]
    [Tooltip("Cost in coins to purchase this item")]
    public int cost;

    [Tooltip("Type of item: Upgrade (permanent) or Item (consumable)")]
    public ShopItemType itemType;

    [Tooltip("What effect this item provides")]
    public ShopEffectType effectType;

    [Header("Effect Values")]
    [Tooltip("Numeric value for the effect (e.g., +1 health, +0.1 speed)")]
    public float effectValue;

    [Tooltip("For consumable items, how many uses it provides")]
    public int usesCount = 1;
}
