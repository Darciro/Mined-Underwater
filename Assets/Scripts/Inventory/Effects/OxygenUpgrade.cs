using UnityEngine;

[CreateAssetMenu(fileName = "Oxygen Upgrade", menuName = "Mined Underwater/Inventory/Upgrades/Oxygen Upgrade")]
public class OxygenUpgrade : ItemEffect
{
    [Min(0f)]
    [Tooltip("Oxygen increase amount.")]
    public int oxygenIncrease = 5;

    public override void Use(PlayerController player)
    {
        int current = PlayerPrefs.GetInt("UpgradeLevel_AirUpgrade", 0);
        PlayerPrefs.SetInt("UpgradeLevel_AirUpgrade", current + oxygenIncrease);
        PlayerPrefs.Save();
        player.IncreaseMaxOxygen(oxygenIncrease);
    }
}
