using UnityEngine;

[CreateAssetMenu(fileName = "New Shield Effect", menuName = "Mined Underwater/Inventory/Effects/Shield Effect")]
public class ShieldEffect : ItemEffect
{
    [Min(1)]
    public int shieldAmount = 15;
    public GameObject shieldPrefab;

    public override void Use(PlayerController player)
    {
        GameObject instance = null;
        if (shieldPrefab != null)
            instance = Instantiate(shieldPrefab, player.transform);

        player.AddShield(shieldAmount, instance);
    }
}
