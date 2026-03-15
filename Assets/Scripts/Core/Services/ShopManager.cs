using UnityEngine;

[DisallowMultipleComponent]
public class ShopManager : MonoBehaviour
{
    [SerializeField] private ItemInfo itemInfo;
    [SerializeField] private GameObject buyButton;
    
    public InventoryManager inventoryManager;

    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();
    }

    public void ShowItem(Item item)
    {
        if (itemInfo != null)
            itemInfo.Show(item, 1);
    }

    public void BuyItem()
    {
        if (itemInfo == null || itemInfo.CurrentItem == null)
            return;

        Item item = itemInfo.CurrentItem;

        if (!int.TryParse(item.price, out int cost))
        {
            Debug.LogWarning($"Item '{item.name}' has an invalid price: '{item.price}'.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("GameManager instance not found!");
            return;
        }

        if (GameManager.Instance.TotalCoins < cost)
        {
            Debug.Log($"Not enough coins to buy '{item.name}'. Have {GameManager.Instance.TotalCoins}, need {cost}.");
            return;
        }

        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }

        if (!inventoryManager.AddItemOnStoreData(item))
        {
            Debug.Log($"Inventory is full. Cannot buy '{item.name}'.");
            return;
        }

        GameManager.Instance.SpendCoins(cost);
        Debug.Log($"Bought '{item.name}' for {cost} coins. Remaining: {GameManager.Instance.TotalCoins}.");
    }
}
