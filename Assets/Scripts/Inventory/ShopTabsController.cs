using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopTabsController : MonoBehaviour
{
    [Header("Items")]
    [SerializeField] private Transform contentParent;

    [Header("Tabs")]
    [SerializeField] private List<ShopTabButton> tabs;

    private List<ShopItemUI> shopItems = new List<ShopItemUI>();
    private ShopTabButton currentTab;

    private void Awake()
    {
        CacheItems();
    }

    private void Start()
    {
        // Seleciona a primeira aba automaticamente
        if (tabs.Count > 0)
        {
            SelectTab(tabs[0]);
        }
    }

    private void CacheItems()
    {
        shopItems.Clear();

        foreach (Transform child in contentParent)
        {
            var item = child.GetComponent<ShopItemUI>();
            if (item != null)
                shopItems.Add(item);
        }
    }

    public void SelectTab(ShopTabButton selectedTab)
    {
        currentTab = selectedTab;

        // Atualiza visual das abas
        foreach (var tab in tabs)
        {
            tab.SetActive(tab == selectedTab);
        }

        // Filtra itens
        ShowCategory(selectedTab.GetCategory());
    }

    private void ShowCategory(ShopItemCategory category)
    {
        foreach (var item in shopItems)
        {
            bool show = category == ShopItemCategory.All || item.category == category;
            item.gameObject.SetActive(show);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent as RectTransform);
    }
}