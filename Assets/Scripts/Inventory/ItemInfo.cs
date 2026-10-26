using System.Collections;
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
    [SerializeField] private TextMeshProUGUI itemSellPriceText;
    [SerializeField] private Image iconCoin;
    [SerializeField] private Image eggCoin;
    [SerializeField] private Image iconImage;
    [SerializeField] private Button closeButton;

    [Header("Purchase Feedback")]
    [Tooltip("Price color used when the player cannot buy the item")]
    [SerializeField] private Color cannotBuyPriceColor = Color.red;
    [Tooltip("Panel that shakes on a failed purchase. If empty, uses the child named 'Popup', then this object.")]
    [SerializeField] private RectTransform shakeTarget;
    [SerializeField, Min(0f)] private float shakeDuration = 0.35f;
    [Tooltip("Max horizontal offset in UI units")]
    [SerializeField, Min(0f)] private float shakeMagnitude = 15f;
    [Tooltip("Oscillations per second")]
    [SerializeField, Min(0f)] private float shakeFrequency = 12f;

    public Item CurrentItem { get; private set; }

    private Color defaultPriceColor = Color.white;
    private Coroutine shakeRoutine;
    private Vector2 shakeOrigin;

    private void Awake()
    {
        if (itemPriceText != null)
            defaultPriceColor = itemPriceText.color;

        if (shakeTarget == null)
        {
            Transform popup = transform.Find("Popup");
            shakeTarget = popup != null ? popup as RectTransform : transform as RectTransform;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Hide);

        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        // Coroutines stop when the popup is hidden: put the panel back where it was
        if (shakeRoutine != null)
        {
            shakeRoutine = null;
            shakeTarget.anchoredPosition = shakeOrigin;
        }
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
        if (itemPriceText != null)
        {
            itemPriceText.text = item.price;
            itemPriceText.color = defaultPriceColor; // Callers that sell items mark it with SetCanBuy
        }
        if (itemSellPriceText != null) itemSellPriceText.text = item.sellPrice;
        if (iconImage != null) iconImage.sprite = item.icon;

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    /// <summary>Shows the price in the normal color, or in red when the item cannot be bought.</summary>
    public void SetCanBuy(bool canBuy)
    {
        if (itemPriceText != null)
            itemPriceText.color = canBuy ? defaultPriceColor : cannotBuyPriceColor;
    }

    /// <summary>Shakes the popup horizontally to signal a refused purchase.</summary>
    public void Shake()
    {
        if (shakeTarget == null || !isActiveAndEnabled) return;

        // Restarting mid-shake keeps the original resting position
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);
        else
            shakeOrigin = shakeTarget.anchoredPosition;

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float damping = 1f - elapsed / shakeDuration;
            float offset = Mathf.Sin(elapsed * shakeFrequency * 2f * Mathf.PI) * shakeMagnitude * damping;
            shakeTarget.anchoredPosition = shakeOrigin + new Vector2(offset, 0f);

            elapsed += Time.unscaledDeltaTime; // works even if the game is paused
            yield return null;
        }

        shakeTarget.anchoredPosition = shakeOrigin;
        shakeRoutine = null;
    }
}
