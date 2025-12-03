using UnityEngine;

public class SpawnTracker : MonoBehaviour
{
    private SpawnerManager manager;
    private SpawnerManager.SpawnChannel channel;

    public void Initialize(SpawnerManager manager, SpawnerManager.SpawnChannel channel)
    {
        this.manager = manager;
        this.channel = channel;
    }

    private void OnDestroy()
    {
        if (manager != null && channel != null)
            manager.NotifyDespawn(channel);
    }
}
