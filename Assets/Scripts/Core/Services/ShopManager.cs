using NUnit.Framework;
using UnityEngine;

[DisallowMultipleComponent]
public class ShopManager : MonoBehaviour
{
    [SerializeField] private ItemInfo itemInfo;

    public InventoryManager inventoryManager;
    public PlayerController playerController;
    public GameObject shopSucessPanel;

    private void Awake()
    {
        if (inventoryManager == null)
            inventoryManager = FindFirstObjectByType<InventoryManager>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
    }

    public void ShowItem(Item item)
    {
        if (itemInfo != null)
        {
            if (item.itemType == ItemType.Upgrade)
                itemInfo.Show(item, 1, false);
            else
                itemInfo.Show(item, 1);
        }
    }

    public void BuyItem()
    {
        if (itemInfo == null || itemInfo.CurrentItem == null)
            return;

        Item item = itemInfo.CurrentItem;

        if (!CanBuyItem(item, out int cost))
            return;

        if (item.itemType == ItemType.Upgrade)
        {
            item.effect.Use(playerController);
            GameManager.Instance.SpendEggs(cost);
        }
        else
        {
            inventoryManager.AddItemOnStoreData(item);
            GameManager.Instance.SpendCoins(cost);
        }

        if (shopSucessPanel != null) {}
            shopSucessPanel.SetActive(true);

        Debug.Log($"Bought '{item.name}' for {(item.itemType == ItemType.Upgrade ? cost + " eggs" : cost + " coins")}.");
    }

    private bool CanBuyItem(Item item, out int cost, bool upgrade = false)
    {
        cost = 0;

        if (!int.TryParse(item.price, out cost))
        {
            Debug.LogWarning($"Item '{item.name}' has an invalid price: '{item.price}'.");
            return false;
        }

        if (GameManager.Instance == null || inventoryManager == null)
        {
            Debug.LogError("GameManager or InventoryManager instance not found!");
            return false;
        }

        if (upgrade)
        {
            if (GameManager.Instance.TotalEggs < cost)
            {
                Debug.Log($"Not enough eggs to buy '{item.name}'. Have {GameManager.Instance.TotalEggs}, need {cost}.");
                return false;
            }
        }
        else
        {
            if (GameManager.Instance.TotalCoins < cost)
            {
                Debug.Log($"Not enough coins to buy '{item.name}'. Have {GameManager.Instance.TotalCoins}, need {cost}.");
                return false;
            }

            if (!inventoryManager.CanAddItem(item))
            {
                Debug.Log($"Inventory is full. Cannot buy '{item.name}'.");
                return false;
            }
        }

        return true;
    }
}
