using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [System.Serializable]
    public class WeightedPrefab
    {
        public GameObject prefab;
        [Min(0f)] public float weight = 1f;
    }

    [System.Serializable]
    public class SpawnCategory
    {
        public string id = "Default";

        [Header("Prefabs (Weighted)")]
        public List<WeightedPrefab> prefabs = new();

        [Header("Spawn Settings")]
        public float minInterval = 1f;
        public float maxInterval = 3f;
        public int maxAlive = 0;    // 0 = unlimited

        [Header("Position Override")]
        public bool overrideX = false;
        public bool overrideY = false;
        public float positionX = 0f;
        public float positionY = 0f;

        [System.NonSerialized] public int currentAlive;
    }
    [Header("Spawner Settings")]
    public float startDelay = 0f;  // delay in seconds before spawning starts
    public float xOffset = 2f;     // how far outside the camera to spawn

    public float verticalPadding = 1f; // avoid spawning too close to edges

    [Header("Categories")]
    public List<SpawnCategory> categories = new();

    private Camera cam;
    private bool spawning = false;

    private void Start()
    {
        cam = Camera.main;
        UpdateSpawnerPosition();
        StartCoroutine(DelayedStart());
    }

    private IEnumerator DelayedStart()
    {
        yield return new WaitForSeconds(startDelay);
        StartSpawning();
    }

    private void LateUpdate()
    {
        // Keep the spawner always aligned with the camera
        UpdateSpawnerPosition();
    }

    private void UpdateSpawnerPosition()
    {
        if (cam == null)
            cam = Camera.main;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        Vector3 pos = transform.position;
        pos.x = cam.transform.position.x + camWidth + xOffset;   // right side
        pos.y = cam.transform.position.y;                        // vertical center
        transform.position = pos;
    }

    public void StartSpawning()
    {
        if (spawning) return;
        spawning = true;

        foreach (var category in categories)
            StartCoroutine(SpawnRoutine(category));
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private IEnumerator SpawnRoutine(SpawnCategory category)
    {
        while (spawning)
        {
            bool canSpawn =
                category.maxAlive == 0 ||
                category.currentAlive < category.maxAlive;

            if (canSpawn)
            {
                Spawn(category);
            }

            float wait = Random.Range(category.minInterval, category.maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    private void Spawn(SpawnCategory category)
    {
        if (category.prefabs.Count == 0)
        {
            Debug.LogWarning($"Spawner category {category.id} has no prefabs!");
            return;
        }

        GameObject prefab = ChooseWeightedPrefab(category);
        if (prefab == null)
        {
            // Total weight is 0 (or all prefabs are null/weight<=0) -> do not spawn
            return;
        }

        float spawnX = category.overrideX ? category.positionX : transform.position.x;
        float spawnY = category.overrideY ? category.positionY : GetRandomYInsideCamera();

        Vector3 pos = new Vector3(spawnX, spawnY, 0);
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

        // Track alive for maxAlive setting
        category.currentAlive++;

        // Auto reduce alive when destroyed
        var tracker = instance.AddComponent<SpawnTracker>();
        tracker.Initialize(this, category);
    }

    private GameObject ChooseWeightedPrefab(SpawnCategory category)
    {
        float totalWeight = 0f;
        for (int i = 0; i < category.prefabs.Count; i++)
        {
            var entry = category.prefabs[i];
            if (entry == null || entry.prefab == null) continue;
            if (entry.weight <= 0f) continue;
            totalWeight += entry.weight;
        }

        if (totalWeight <= 0f)
        {
            Debug.LogWarning($"Spawner category {category.id} has total weight 0; skipping spawn.");
            return null;
        }

        float roll = Random.value * totalWeight;
        float cumulative = 0f;
        for (int i = 0; i < category.prefabs.Count; i++)
        {
            var entry = category.prefabs[i];
            if (entry == null || entry.prefab == null) continue;
            if (entry.weight <= 0f) continue;

            cumulative += entry.weight;
            if (roll <= cumulative)
                return entry.prefab;
        }

        // Fallback due to float error; return last eligible
        for (int i = category.prefabs.Count - 1; i >= 0; i--)
        {
            var entry = category.prefabs[i];
            if (entry == null || entry.prefab == null) continue;
            if (entry.weight <= 0f) continue;
            return entry.prefab;
        }

        return null;
    }

    private float GetRandomYInsideCamera()
    {
        float camHeight = cam.orthographicSize;
        float minY = cam.transform.position.y - camHeight + verticalPadding;
        float maxY = cam.transform.position.y + camHeight - verticalPadding;

        return Random.Range(minY, maxY);
    }

    public void NotifyDespawn(SpawnCategory category)
    {
        category.currentAlive = Mathf.Max(0, category.currentAlive - 1);
    }
}
