using UnityEngine;

public class HeartPickup : PickupBase
{
    [Header("Heal")]
    [SerializeField] private int healAmount = 3;

    protected override void OnPickup(PlayerController player)
    {
        int healed = Mathf.Min(
            healAmount,
            player.GetMaxHealth() - player.GetHealth()
        );

        if (healed <= 0) return;

        player.Heal(healAmount);
    }
}
