using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public string name;
    public string description;
    public string price;
    public Sprite icon;
    public ItemType itemType;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);
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

public enum ActionType
{
    None,
    Mine,
    Dig,
    Chop
}