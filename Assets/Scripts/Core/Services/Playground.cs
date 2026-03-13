using System;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour
{
    public InventoryManager inventoryManager;
    public Item[] itemAvailable;

    public void PickupItem(int id)
    {
        bool itemAdded = inventoryManager.AddItem(itemAvailable[id]);
        if (itemAdded)
        {
            Debug.Log("Item added to inventory.");
        }
        else
        {
            Debug.Log("Inventory full. Could not add item.");
        }
    }
}
