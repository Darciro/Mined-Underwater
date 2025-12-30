using Febucci.TextAnimatorForUnity;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using TMPro;
using UnityEngine;

/// <summary>
/// Base class for animated text popups using Text Animator.
/// Handles setup, animations, and auto-destroy functionality.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
[RequireComponent(typeof(RectTransform))]
public abstract class TextAnimatorPopupBase : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] protected float floatSpeed = 40f;
    [SerializeField] protected Vector2 floatDirection = Vector2.up;

    [Header("Lifetime")]
    [SerializeField] protected float lifetime = 1.5f;

    [Header("Text Animator")]
    [SerializeField] protected TextAnimator_TMP textAnimator;

    protected TextMeshProUGUI textComponent;
    protected RectTransform rectTransform;
    private float elapsedTime;

    protected virtual void Awake()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        // Try to get or add TextAnimator_TMP component
        if (textAnimator == null)
        {
            textAnimator = GetComponent<TextAnimator_TMP>();
            if (textAnimator == null)
            {
                textAnimator = gameObject.AddComponent<TextAnimator_TMP>();
            }
        }
    }

    protected virtual void Update()
    {
        elapsedTime += Time.deltaTime;

        // Move popup
        rectTransform.anchoredPosition += floatDirection.normalized * (floatSpeed * Time.deltaTime);

        // Auto-destroy after lifetime
        if (elapsedTime >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Sets up the popup with text content and optional animation tags.
    /// </summary>
    /// <param name="text">The text to display</param>
    /// <param name="animationTags">Text Animator tags to wrap the text (e.g., "bounce", "wave")</param>
    protected void SetupText(string text, string animationTags = "")
    {
        if (string.IsNullOrEmpty(animationTags))
        {
            textComponent.text = text;
        }
        else
        {
            textComponent.text = $"<{animationTags}>{text}</{animationTags}>";
        }

        elapsedTime = 0f;
    }

    /// <summary>
    /// Sets up the popup with a numeric value.
    /// </summary>
    /// <param name="value">The numeric value to display</param>
    /// <param name="prefix">Optional prefix (e.g., "+")</param>
    /// <param name="animationTags">Text Animator tags to wrap the text</param>
    protected void SetupNumeric(int value, string prefix = "", string animationTags = "")
    {
        string displayText = prefix + value.ToString();
        SetupText(displayText, animationTags);
    }
}
