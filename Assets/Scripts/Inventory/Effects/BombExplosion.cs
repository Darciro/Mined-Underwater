using UnityEngine;

[CreateAssetMenu(fileName = "New Bomb Explosion", menuName = "Inventory/Effects/Bomb Explosion")]
public class BombExplosion : ItemEffect
{
    [Min(1)]
    public int damageAmount = 50;
    public GameObject bombPrefab;

    private float spawnOffset = 1.5f;

    public override void Use(PlayerController player)
    {
        // Instantiate the bomb explosion effect slightly in front of the player
        if (bombPrefab != null)
        {
            Vector3 spawnPosition = player.transform.position + Vector3.right * spawnOffset;
            GameObject bomb = Instantiate(bombPrefab, spawnPosition, Quaternion.identity);
            /* Bomb bombScript = bomb.GetComponent<Bomb>();
            if (bombScript != null)
            {
                bombScript.Initialize(damageAmount);
            } */
        } else
        {
            Debug.LogWarning("Bomb prefab is not assigned in the BombExplosion effect.");
        }
    }
}
