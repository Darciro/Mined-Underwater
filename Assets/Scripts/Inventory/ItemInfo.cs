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
    [SerializeField] private Image iconCoin;
    [SerializeField] private Image eggCoin;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button closeButton;

    public Item CurrentItem { get; private set; }

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

    public void Show(Item item, int count, bool costInCoins = true)
    {
        if (item == null) return;

        CurrentItem = item;

        if (costInCoins)
        {
            if (iconCoin != null) iconCoin.gameObject.SetActive(true);
            if (eggCoin != null) eggCoin.gameObject.SetActive(false);
        }
        else
        {
            if (iconCoin != null) iconCoin.gameObject.SetActive(false);
            if (eggCoin != null) eggCoin.gameObject.SetActive(true);
        }

        if (itemNameText != null) itemNameText.text = item.name;
        if (itemDescriptionText != null) itemDescriptionText.text = item.description;
        if (itemTypeText != null) itemTypeText.text = item.itemType.ToString();
        if (itemPriceText != null) itemPriceText.text = item.price;
        if (iconImage != null) iconImage.sprite = item.icon;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
