using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI References")]
    [SerializeField] private Image image;
    [SerializeField] private TextMeshProUGUI countText;

    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;
    [HideInInspector] public Transform parentAfterDrag;

    private void Start()
    {
        InitialiseItem(item);
    }

    public void InitialiseItem(Item newItem)
    {
        item = newItem;
        image.sprite = item.icon;
        UpdateCount();
    }

    public void UpdateCount()
    {
        countText.text = count.ToString();
        bool showCount = item.stackable && count > 1;
        countText.gameObject.SetActive(showCount);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        image.raycastTarget = false; // Disable raycast target to allow drop detection  
        parentAfterDrag = transform.parent; // Store the original parent to return to if needed
        transform.SetParent(transform.root); // Move to root to avoid being masked by other UI elements
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position; // Follow the mouse position
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true; // Re-enable raycast target
        transform.SetParent(parentAfterDrag); // Return to original parent if not dropped on a valid slot
    }
}
