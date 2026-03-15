using UnityEngine;

[CreateAssetMenu(fileName = "New Restore Air Effect", menuName = "Inventory/Effects/Restore Air")]
public class RestoreAirEffect : ItemEffect
{
    [Min(1)]
    public int airAmount = 10;

    public override void Use(PlayerController player)
    {
        player.RestoreAir(airAmount);
    }
}
