using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ShopItem : MonoBehaviour
{
    [SerializeField] private Item item;

    public void ShowItem()
    {
        if (item != null)
        {
            // Show the item details in the shop UI
            ShopManager shopManager = FindObjectOfType<ShopManager>();
            if (shopManager != null)
            {
                shopManager.ShowItem(item);
            }
        }
    }
    
}
