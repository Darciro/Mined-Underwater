using UnityEngine;

public class SpawnTracker : MonoBehaviour
{
    private SpawnerManager manager;
    private SpawnerManager.SpawnCategory category;

    public void Initialize(SpawnerManager manager, SpawnerManager.SpawnCategory category)
    {
        this.manager = manager;
        this.category = category;
    }

    private void OnDestroy()
    {
        if (manager != null && category != null)
            manager.NotifyDespawn(category);
    }
}
