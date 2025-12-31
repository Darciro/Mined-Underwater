using UnityEngine;

/// <summary>
/// Pickup that adds a coin to the player's collection when collected.
/// Follows the same pattern as EggPickup.
/// </summary>
public class CoinPickup : PickupBase
{
    protected override void OnPickup(PlayerController player)
    {
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddCoin();
        }

        player.ShowCoinCollectionPopup();
    }
}
