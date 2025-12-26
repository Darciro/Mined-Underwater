using UnityEngine;
using UnityEngine.UI;

public class UIToggler : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform handle;

    [Header("Sprites")]
    [SerializeField] private Sprite enabledSprite;
    [SerializeField] private Sprite disabledSprite;

    [Header("State")]
    [SerializeField] private bool isEnabled = true;

    private Image toggleImage;

    private void Awake()
    {
        toggleImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateVisual();
    }

    // Called by the Button OnClick event
    public void Toggle()
    {
        isEnabled = !isEnabled;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        // Update sprite
        if (toggleImage != null)
        {
            toggleImage.sprite = isEnabled ? enabledSprite : disabledSprite;
        }

        // Update handle anchor position
        if (handle != null)
        {
            if (isEnabled)
            {
                // Middle right
                handle.anchorMin = new Vector2(1f, 0.5f);
                handle.anchorMax = new Vector2(1f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
            }
            else
            {
                // Middle left
                handle.anchorMin = new Vector2(0f, 0.5f);
                handle.anchorMax = new Vector2(0f, 0.5f);
                handle.anchoredPosition = Vector2.zero;
            }
        }
    }

    public bool IsEnabled() => isEnabled;
}
