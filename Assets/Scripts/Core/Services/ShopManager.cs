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
            bool isUpgrade = item.itemType == ItemType.Upgrade;
            itemInfo.Show(item, 1, !isUpgrade);
            itemInfo.SetCanBuy(CanBuyItem(item, out _, isUpgrade, logReason: false));
        }
    }

    public void BuyItem()
    {
        if (itemInfo == null || itemInfo.CurrentItem == null)
            return;

        Item item = itemInfo.CurrentItem;

        bool isUpgrade = item.itemType == ItemType.Upgrade;

        if (!CanBuyItem(item, out int cost, isUpgrade))
        {
            RejectPurchase();
            return;
        }

        if (isUpgrade)
        {
            // Pay first so a failing effect can never grant a free upgrade
            if (!GameManager.Instance.SpendEggs(cost))
            {
                RejectPurchase();
                return;
            }

            if (item.effect != null)
                item.effect.Use(playerController);
        }
        else
        {
            if (!inventoryManager.AddItemOnStoreData(item))
            {
                RejectPurchase();
                return;
            }

            GameManager.Instance.SpendCoins(cost);
        }

        // The balance changed: the same item may no longer be affordable
        itemInfo.SetCanBuy(CanBuyItem(item, out _, isUpgrade, logReason: false));

        if (shopSucessPanel != null)
            shopSucessPanel.SetActive(true);

        Debug.Log($"Bought '{item.name}' for {(item.itemType == ItemType.Upgrade ? cost + " eggs" : cost + " coins")}.");
    }

    private void RejectPurchase()
    {
        itemInfo.SetCanBuy(false);
        itemInfo.Shake();
    }

    /// <param name="logReason">False when only probing (e.g. to color the price), so the Console is not spammed</param>
    private bool CanBuyItem(Item item, out int cost, bool upgrade = false, bool logReason = true)
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
                if (logReason)
                    Debug.Log($"Not enough eggs to buy '{item.name}'. Have {GameManager.Instance.TotalEggs}, need {cost}.");
                return false;
            }
        }
        else
        {
            if (GameManager.Instance.TotalCoins < cost)
            {
                if (logReason)
                    Debug.Log($"Not enough coins to buy '{item.name}'. Have {GameManager.Instance.TotalCoins}, need {cost}.");
                return false;
            }

            if (!inventoryManager.CanAddItem(item))
            {
                if (logReason)
                    Debug.Log($"Inventory is full. Cannot buy '{item.name}'.");
                return false;
            }
        }

        return true;
    }
}
