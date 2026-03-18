using UnityEngine;

[CreateAssetMenu(fileName = "New Defense Boost Effect", menuName = "Mined Underwater/Inventory/Effects/Defense Boost")]
public class BoostDefenseEffect : ItemEffect
{
    [Min(0f)]
    [Tooltip("Damage reduction percentage to add temporarily.")]
    public float bonusPercentage = 25f;

    [Min(0f)]
    [Tooltip("How long the boost lasts in seconds.")]
    public float duration = 10f;

    public override void Use(PlayerController player)
    {
        player.TemporaryDefenseBoost(bonusPercentage, duration);
    }
}
