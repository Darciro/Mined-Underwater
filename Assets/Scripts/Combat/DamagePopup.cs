using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 40f;
    [SerializeField] private float fadeDuration = 1f;

    private TextMeshProUGUI text;
    private RectTransform rectTransform;
    private Color startColor;
    private float lifetime;

    public void Setup(int damage)
    {
        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();

        text.text = damage.ToString();
        startColor = text.color;
        lifetime = 0f;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;

        // Move up
        rectTransform.anchoredPosition += new Vector2(0, floatSpeed) * Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, lifetime / fadeDuration);
        text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

        if (lifetime >= fadeDuration)
        {
            Destroy(gameObject);
        }
    }
}
