using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopTabButton : MonoBehaviour
{
    [Header("Config")]
    public ShopItemCategory category;
    public ShopTabsController controller;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image underline;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;

    private bool isActive;

    public void OnClick()
    {
        controller.SelectTab(this);
    }

    public void SetActive(bool active)
    {
        isActive = active;

        // Cor do texto
        label.color = active ? activeColor : inactiveColor;

        // Underline ligado/desligado
        underline.gameObject.SetActive(active);
    }

    public ShopItemCategory GetCategory()
    {
        return category;
    }
}