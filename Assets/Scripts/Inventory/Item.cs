using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class Item : ScriptableObject
{
    public TileBase tile;
    public Sprite icon;
    public ItemType itemType;
    public ActionType actionType;
    public Vector2Int range = new Vector2Int(5, 4);
    public bool stackable = true;
}

public enum ItemType
{
    None,
    Pickaxe,
    Shovel,
    Axe
}

public enum ActionType
{
    None,
    Mine,
    Dig,
    Chop
}