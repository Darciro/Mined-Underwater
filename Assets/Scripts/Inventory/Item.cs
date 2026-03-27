using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Mined Underwater/Inventory/Item")]
public class Item : ScriptableObject
{
    public string name;
    public string description;
    public string price;
    public string sellPrice;
    public Sprite icon;
    public ItemType itemType;
    public bool stackable = true;

    [Header("The item effect")]
    public ItemEffect effect;
}

public enum ItemType
{
    Weapon,
    Armor,
    Consumable,
    Equipment,
    Upgrade,
    Currency
}