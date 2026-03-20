using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

    private Coroutine _pendingUseSave;
    private PlayerController _player;

    private void Start()
    {
        RefreshItemInfoReference();
        RefreshPlayerReference();
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
        RefreshPlayerReference();
    }

    private void RefreshPlayerReference()
    {
        _player = FindFirstObjectByType<PlayerController>();
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

        if (SceneManager.GetActiveScene().name == "Main" || SceneManager.GetActiveScene().name == "_Playground")
            UseSelectedItem(inventoryItem);
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

    public bool CanAddItem(Item item)
    {
        InventorySaveData saveData = PlayerPrefs.HasKey(SaveKey)
            ? JsonUtility.FromJson<InventorySaveData>(PlayerPrefs.GetString(SaveKey)) ?? new InventorySaveData()
            : new InventorySaveData();

        if (item.stackable)
        {
            foreach (InventorySlotData slotData in saveData.slots)
            {
                if (slotData.itemName == item.name && slotData.count < maxStackSize)
                    return true;
            }
        }

        int totalSlots = inventorySlots.Length;
        for (int i = 0; i < totalSlots; i++)
        {
            bool occupied = false;
            foreach (InventorySlotData slotData in saveData.slots)
            {
                if (slotData.slotIndex == i) { occupied = true; break; }
            }
            if (!occupied) return true;
        }

        return false;
    }

    public bool AddItemOnStoreData(Item item)
    {
        InventorySaveData saveData;

        if (PlayerPrefs.HasKey(SaveKey))
        {
            string rawJson = PlayerPrefs.GetString(SaveKey);
            saveData = JsonUtility.FromJson<InventorySaveData>(rawJson) ?? new InventorySaveData();
        }
        else
        {
            saveData = new InventorySaveData();
        }

        if (item.stackable)
        {
            foreach (InventorySlotData slotData in saveData.slots)
            {
                if (slotData.itemName == item.name && slotData.count < maxStackSize)
                {
                    slotData.count++;
                    string updatedJson = JsonUtility.ToJson(saveData);
                    PlayerPrefs.SetString(SaveKey, updatedJson);
                    PlayerPrefs.Save();
                    return true;
                }
            }
        }

        int totalSlots = inventorySlots.Length;
        for (int i = 0; i < totalSlots; i++)
        {
            bool occupied = false;
            foreach (InventorySlotData slotData in saveData.slots)
            {
                if (slotData.slotIndex == i)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
            {
                saveData.slots.Add(new InventorySlotData
                {
                    slotIndex = i,
                    itemName = item.name,
                    count = 1
                });

                string updatedJson = JsonUtility.ToJson(saveData);
                PlayerPrefs.SetString(SaveKey, updatedJson);
                PlayerPrefs.Save();
                return true;
            }
        }

        return false;
    }

    public void SpawnNewItem(Item item, InventorySlot slot)
    {
        GameObject newItem;
        if (slot.alternativePosition != null)
        {
            // slot.alternativePosition.gameObject.GetComponent<Image>().sprite = null;
            newItem = Instantiate(inventoryItemPrefab, slot.alternativePosition.transform);
        }
        else
        {
            newItem = Instantiate(inventoryItemPrefab, slot.transform);
        }
        // GameObject newItem = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
        inventoryItem.InitialiseItem(item);
    }

    public Item GetSelectedItem(bool use)
    {
        InventorySlot slot = inventorySlots[selectedSlot];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();
        if (itemInSlot != null)
        {
            Item item = itemInSlot.item;
            if (use)
            {
                if (item.stackable)
                {
                    itemInSlot.count--;
                    if (itemInSlot.count <= 0)
                    {
                        Destroy(itemInSlot.gameObject);
                    }
                    else
                    {
                        itemInSlot.UpdateCount();
                    }
                }
                else
                {
                    Destroy(itemInSlot.gameObject);
                }

                ScheduleSaveAfterUse();
            }
            return itemInSlot.item;
        }
        return null;
    }

    public void UseSelectedItem(InventoryItem itemInSlot)
    {
        if (itemInSlot == null) return;

        Item item = itemInSlot.item;

        if (item.effect != null && _player != null)
            item.effect.Use(_player);

        if (item.stackable)
        {
            itemInSlot.count--;
            if (itemInSlot.count <= 0)
                Destroy(itemInSlot.gameObject);
            else
                itemInSlot.UpdateCount();
        }
        else
        {
            Destroy(itemInSlot.gameObject);
        }

        ScheduleSaveAfterUse();
    }

    private void ScheduleSaveAfterUse()
    {
        if (_pendingUseSave != null)
        {
            StopCoroutine(_pendingUseSave);
        }

        _pendingUseSave = StartCoroutine(SaveInventoryAfterUse());
    }

    private IEnumerator SaveInventoryAfterUse()
    {
        yield return null;
        _pendingUseSave = null;
        SaveInventory();
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
        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SaveKey, json);
        Debug.Log($"Saving inventory json: {json}");
        PlayerPrefs.Save();
    }

    public void LoadInventory()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.Log("No inventory save found.");
            return;
        }

        string rawJson = PlayerPrefs.GetString(SaveKey);

        InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(rawJson);

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
