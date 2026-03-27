using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    public static event Action<InventoryItem> OnSlotClicked;

    public GameObject alternativePosition;
    [SerializeField] private Image image;
    [SerializeField] private Color selectedColor, defaultColor;

    private void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Deselect()
    {
        image.color = defaultColor;
    }

    public InventoryItem GetItem()
    {
        if (alternativePosition != null)
        {
            InventoryItem item = alternativePosition.GetComponentInChildren<InventoryItem>();
            if (item != null) return item;
        }
        return GetComponentInChildren<InventoryItem>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (GetItem() == null)
        {
            InventoryItem item = eventData.pointerDrag.GetComponent<InventoryItem>();
            item.parentAfterDrag = alternativePosition != null ? alternativePosition.transform : transform;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryItem item = GetItem();
        if (item != null)
        {
            OnSlotClicked?.Invoke(item);
        }
    }

}
