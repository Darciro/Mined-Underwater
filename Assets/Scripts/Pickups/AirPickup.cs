using UnityEngine;

public class AirPickup : PickupBase
{
    [Header("Recovered Air Amount")]
    [SerializeField] private int recoverAmount = 5;

    protected override void OnPickup(PlayerController player)
    {
        int restored = Mathf.Min(
            recoverAmount,
            player.GetMaxAir() - player.GetAir()
        );

        if (restored <= 0) return;

        player.RestoreAir(recoverAmount);
    }
}
