using System.Collections.Generic;
using UnityEngine;

public class ShopTabsController : MonoBehaviour
{
    [Header("Container dos itens")]
    [SerializeField] private Transform contentParent;

    private List<ShopItemUI> shopItems = new List<ShopItemUI>();

    private void Awake()
    {
        CacheItems();
    }

    private void Start()
    {
        ShowCategory(ShopItemCategory.All);
    }

    private void CacheItems()
    {
        shopItems.Clear();

        foreach (Transform child in contentParent)
        {
            ShopItemUI item = child.GetComponent<ShopItemUI>();
            if (item != null)
            {
                shopItems.Add(item);
            }
        }
    }

    public void ShowCategory(ShopItemCategory category)
    {
        foreach (ShopItemUI item in shopItems)
        {
            bool shouldShow = category == ShopItemCategory.All || item.category == category;
            item.gameObject.SetActive(shouldShow);
        }
    }

    public void ShowAll()
    {
        ShowCategory(ShopItemCategory.All);
    }

    public void ShowConsumables()
    {
        ShowCategory(ShopItemCategory.Consumable);
    }

    public void ShowEquipment()
    {
        ShowCategory(ShopItemCategory.Equipment);
    }

    public void ShowUpgrades()
    {
        ShowCategory(ShopItemCategory.Upgrade);
    }
}