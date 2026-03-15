using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays detailed information about an inventory item.
/// Attach to the popup GameObject in your scene and wire up the UI references.
/// </summary>
public class ItemInfo : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI itemTypeText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI actionTypeText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Hide);
    }

    public void Show(Item item, int count)
    {
        if (item == null) return;

        if (itemNameText != null) itemNameText.text = item.name;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;
        if (itemTypeText != null) itemTypeText.text = item.itemType.ToString();
        if (itemPriceText != null) itemPriceText.text = item.price;
        if (actionTypeText != null) actionTypeText.text = item.actionType.ToString();
        if (rangeText != null) rangeText.text = $"{item.range.x} x {item.range.y}";
        if (iconImage != null) iconImage.sprite = item.icon;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
