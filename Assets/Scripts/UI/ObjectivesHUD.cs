using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// In-game HUD panel that lists all active objectives and keeps them updated
/// as the player makes progress.
/// 
/// Setup:
///   1. Add a UI Panel to the gameplay scene's Canvas.
///   2. Give it a child "ItemsContainer" (a VerticalLayoutGroup works well).
///   3. Create an ObjectiveItemUI prefab and assign both fields below.
///   4. The HUD reads from ObjectivesManager.Instance on Start, so make sure
///      ObjectivesManager.InitializeForStage has been called before the scene loads.
/// </summary>
public class ObjectivesHUD : MonoBehaviour
{
    [Tooltip("Parent transform that holds the spawned objective rows")]
    [SerializeField] private Transform itemsContainer;

    [Tooltip("Prefab with an ObjectiveItemUI component on its root")]
    [SerializeField] private ObjectiveItemUI itemPrefab;

    private readonly List<ObjectiveItemUI> spawnedItems = new();

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    private void Start()
    {
        Populate();
    }

    private void OnEnable()
    {
        if (ObjectivesManager.Instance != null)
            ObjectivesManager.Instance.OnAllObjectivesCompleted += HandleAllCompleted;
    }

    private void OnDisable()
    {
        if (ObjectivesManager.Instance != null)
            ObjectivesManager.Instance.OnAllObjectivesCompleted -= HandleAllCompleted;
    }

    // -----------------------------------------------------------------------
    // Private
    // -----------------------------------------------------------------------

    private void Populate()
    {
        ClearItems();

        if (ObjectivesManager.Instance == null || itemPrefab == null || itemsContainer == null)
            return;

        foreach (var progress in ObjectivesManager.Instance.ActiveObjectives)
        {
            var item = Instantiate(itemPrefab, itemsContainer);
            item.Initialize(progress);
            spawnedItems.Add(item);
        }

        // Hide the entire HUD if there are no objectives for this stage
        gameObject.SetActive(spawnedItems.Count > 0);
    }

    private void ClearItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();
    }

    private void HandleAllCompleted()
    {
        Debug.Log("[ObjectivesHUD] All objectives completed!");
        // Optionally play a celebration fx or visual here
    }
}
