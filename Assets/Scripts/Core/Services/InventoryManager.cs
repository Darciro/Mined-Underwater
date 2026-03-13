using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton MonoBehaviour that owns the runtime inventory state.
/// Initialises from an InventorySO asset on Awake; all changes are in-memory only.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    public int maxStackSize = 4;
    public int selectedSlot = -1;

    [Header("Item Info")]
    [SerializeField] private ItemInfo ItemInfo;

    private void OnEnable()
    {
        InventorySlot.OnSlotClicked += HandleSlotClicked;
    }

    private void OnDisable()
    {
        InventorySlot.OnSlotClicked -= HandleSlotClicked;
    }

    private void HandleSlotClicked(InventoryItem inventoryItem)
    {
        if (ItemInfo != null)
            ItemInfo.Show(inventoryItem.item, inventoryItem.count);
    }

    public void ChangeSelectedSlot(int newIndex)
    {
        if (selectedSlot >= 0)
        {
            inventorySlots[selectedSlot].Deselect();
        }

        inventorySlots[newIndex].Select();
        selectedSlot = newIndex;
    }

    public bool AddItem(Item item)
    {
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem existingItem = slot.transform.GetComponentInChildren<InventoryItem>();
            if (
                existingItem != null
                && existingItem.item == item
                && existingItem.count < maxStackSize
                && item.stackable
            )
            {
                existingItem.count++;
                existingItem.UpdateCount();
                return true;
            }
        }

        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventorySlot slot = inventorySlots[i];
            InventoryItem existingItem = slot.transform.GetComponentInChildren<InventoryItem>();
            if (existingItem == null)
            {
                SpawnNewItem(item, slot);
                return true;
            }
        }
        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }
}
