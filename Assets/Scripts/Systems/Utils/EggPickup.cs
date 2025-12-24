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

        player.SendMessage(
            "ShowEggCollectionPopup",
            player.transform.position + Vector3.up * 0.5f,
            SendMessageOptions.DontRequireReceiver
        );
    }
}
