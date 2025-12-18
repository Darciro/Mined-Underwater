using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnChannel
    {
        public string id = "Default";
        public List<GameObject> prefabs = new();
        public float minInterval = 1f;
        public float maxInterval = 3f;
        public int maxAlive = 0;    // 0 = unlimited

        [Header("Position Override")]
        public float positionX = 0f;
        public float positionY = 0f;

        [HideInInspector] public int currentAlive;
    }
    [Header("Spawner Settings")]
    public float xOffset = 2f;     // how far outside the camera to spawn

    public float verticalPadding = 1f; // avoid spawning too close to edges

    [Header("Channels")]
    public List<SpawnChannel> channels = new();

    private Camera cam;
    private bool spawning = false;

    private void Start()
    {
        cam = Camera.main;
        UpdateSpawnerPosition();
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

        foreach (var c in channels)
            StartCoroutine(SpawnRoutine(c));
    }

    public void StopSpawning()
    {
        spawning = false;
    }

    private IEnumerator SpawnRoutine(SpawnChannel channel)
    {
        while (spawning)
        {
            bool canSpawn =
                channel.maxAlive == 0 ||
                channel.currentAlive < channel.maxAlive;

            if (canSpawn)
            {
                Spawn(channel);
            }

            float wait = Random.Range(channel.minInterval, channel.maxInterval);
            yield return new WaitForSeconds(wait);
        }
    }

    private void Spawn(SpawnChannel channel)
    {
        if (channel.prefabs.Count == 0)
        {
            Debug.LogWarning($"Spawner channel {channel.id} has no prefabs!");
            return;
        }

        GameObject prefab = channel.prefabs[Random.Range(0, channel.prefabs.Count)];

        float spawnX = channel.positionX != 0 ? channel.positionX : transform.position.x;
        float spawnY = channel.positionY != 0 ? channel.positionY : GetRandomYInsideCamera();

        Vector3 pos = new Vector3(spawnX, spawnY, 0);
        GameObject instance = Instantiate(prefab, pos, Quaternion.identity);

        // Track alive for maxAlive setting
        channel.currentAlive++;

        // Auto reduce alive when destroyed
        var tracker = instance.AddComponent<SpawnTracker>();
        tracker.Initialize(this, channel);
    }

    private float GetRandomYInsideCamera()
    {
        float camHeight = cam.orthographicSize;
        float minY = cam.transform.position.y - camHeight + verticalPadding;
        float maxY = cam.transform.position.y + camHeight - verticalPadding;

        return Random.Range(minY, maxY);
    }

    public void NotifyDespawn(SpawnChannel channel)
    {
        channel.currentAlive = Mathf.Max(0, channel.currentAlive - 1);
    }
}
