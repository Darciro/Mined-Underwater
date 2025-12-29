using UnityEngine;

public class EggPickup : PickupBase
{
    protected override void OnPickup(PlayerController player)
    {
        ScoreManager scoreManager = FindFirstObjectByType<ScoreManager>();
        if (scoreManager != null)
        {
            scoreManager.AddEgg();
        }

        player.ShowEggCollectionPopup();
    }
}
