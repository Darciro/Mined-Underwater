using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class InventorySlotData
{
    public int slotIndex;
    public string itemName;
    public int count;
}

[Serializable]
public class InventorySaveData
{
    public List<InventorySlotData> slots = new List<InventorySlotData>();
}

/// <summary>
/// Singleton MonoBehaviour that owns the runtime inventory state.
/// Persists across scene loads via DontDestroyOnLoad; saves to PlayerPrefs on every change.
/// </summary>
public class InventoryManager : MonoBehaviour
{
    private const string SaveKey = "InventoryData";

    public InventorySlot[] inventorySlots;
    public GameObject inventoryItemPrefab;
    public int maxStackSize = 4;
    public int selectedSlot = -1;

    [Header("Item Info")]
    [SerializeField] private ItemInfo ItemInfo;

    [Header("Known Items (for save/load)")]
    [SerializeField] private Item[] allItems;

    private void Start()
    {
        RefreshItemInfoReference();
        LoadInventory();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        InventorySlot.OnSlotClicked += HandleSlotClicked;
        InventoryItem.OnItemMoved += SaveInventory;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        InventorySlot.OnSlotClicked -= HandleSlotClicked;
        InventoryItem.OnItemMoved -= SaveInventory;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshItemInfoReference();
    }

    private void RefreshItemInfoReference()
    {
        ItemInfo = null;

        GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        for (int i = 0; i < rootObjects.Length; i++)
        {
            ItemInfo foundInfo = rootObjects[i].GetComponentInChildren<ItemInfo>(true);
            if (foundInfo != null)
            {
                ItemInfo = foundInfo;
                break;
            }
        }
    }

    private void HandleSlotClicked(InventoryItem inventoryItem)
    {
        Debug.Log($"Clicked on slot with item: {inventoryItem.item.name}");

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
                SaveInventory();
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
                SaveInventory();
                return true;
            }
        }
        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItem;
        if(slot.alternativePosition != null) {
            // slot.alternativePosition.gameObject.GetComponent<Image>().sprite = null;
            newItem = Instantiate(inventoryItemPrefab, slot.alternativePosition.transform); 
        } else {
            newItem = Instantiate(inventoryItemPrefab, slot.transform);
        }
        // GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public void SaveInventory()
    {
        var saveData = new InventorySaveData();
        for (int i = 0; i < inventorySlots.Length; i++)
        {
            InventoryItem inventoryItem = inventorySlots[i].GetComponentInChildren<InventoryItem>();
            if (inventoryItem != null)
            {
                saveData.slots.Add(new InventorySlotData
                {
                    slotIndex = i,
                    itemName = inventoryItem.item.name,
                    count = inventoryItem.count
                });
            }
        }
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saveData));
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return;

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(
            PlayerPrefs.GetString(SaveKey)
        );

        foreach (InventorySlotData slotData in saveData.slots)
        {
            Item item = Array.Find(allItems, i => i.name == slotData.itemName);
            if (item == null || slotData.slotIndex >= inventorySlots.Length) continue;

            InventorySlot slot = inventorySlots[slotData.slotIndex];
            SpawnNewItem(item, slot);

            InventoryItem spawnedItem = slot.GetComponentInChildren<InventoryItem>();
            if (spawnedItem != null)
            {
                spawnedItem.count = slotData.count;
                spawnedItem.UpdateCount();
            }
        }
    }
}
