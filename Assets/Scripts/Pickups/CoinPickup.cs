using UnityEngine;

/// <summary>
/// Pickup that adds a coin to the player's collection when collected.
/// Follows the same pattern as EggPickup.
/// </summary>
public class CoinPickup : PickupBase
{
    [Header("Coin Amount")]
    public int minCoins = 1;
    public int maxCoins = 1;

    protected override void OnPickup(PlayerController player)
    {
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            int amount = Random.Range(minCoins, maxCoins + 1);
            scoreManager.AddCoins(amount);
        }

        ObjectivesManager.Instance?.ReportCoinCollected();

        player.ShowCoinCollectionPopup();
    }
}
