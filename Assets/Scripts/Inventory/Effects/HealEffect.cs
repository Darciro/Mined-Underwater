using UnityEngine;

[CreateAssetMenu(fileName = "New Heal Effect", menuName = "Mined Underwater/Inventory/Effects/Heal")]
public class HealEffect : ItemEffect
{
    [Min(1)]
    public int healAmount = 10;

    public override void Use(PlayerController player)
    {
        player.Heal(healAmount);
    }
}
