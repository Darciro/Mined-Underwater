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

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem item = eventData.pointerDrag.GetComponent<InventoryItem>();
            item.parentAfterDrag = transform;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryItem item = GetComponentInChildren<InventoryItem>();
        if (item != null)
        {
            OnSlotClicked?.Invoke(item);
        }
    }

}
