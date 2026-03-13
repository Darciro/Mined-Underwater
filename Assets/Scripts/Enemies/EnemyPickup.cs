using System;
using UnityEngine;

[Serializable]
public struct PickupEntry
{
    public GameObject prefab;
    [Range(0f, 100f)] public float percentage;
}

public class EnemyPickup : MonoBehaviour
{
    [Header("Pickup Drops")]
    [SerializeField] private PickupEntry[] possiblePickups;

    private void OnValidate()
    {
        if (possiblePickups == null || possiblePickups.Length == 0) return;

        float total = 0f;
        foreach (PickupEntry entry in possiblePickups)
            total += entry.percentage;

        if (!Mathf.Approximately(total, 100f) && !Mathf.Approximately(total, 0f))
            Debug.LogWarning($"[EnemyPickup] Pickup percentages on '{name}' sum to {total:F1}%, not 100%.", this);
    }

    /// <summary>
    /// Rolls a random pickup based on the configured percentages and spawns it at this object's position.
    /// Returns the spawned GameObject, or null if no pickup was selected.
    /// </summary>
    public GameObject SpawnPickup()
    {
        if (possiblePickups == null || possiblePickups.Length == 0) return null;

        float roll = UnityEngine.Random.Range(0f, 100f);
        float cumulative = 0f;

        foreach (PickupEntry entry in possiblePickups)
        {
            if (entry.prefab == null) continue;

            cumulative += entry.percentage;
            if (roll < cumulative)
                return Instantiate(entry.prefab, transform.position, Quaternion.identity);
        }

        return null;
    }
}
