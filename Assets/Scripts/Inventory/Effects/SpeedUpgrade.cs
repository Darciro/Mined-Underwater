using UnityEngine;

[CreateAssetMenu(fileName = "Speed Upgrade", menuName = "Mined Underwater/Inventory/Upgrades/Speed Upgrade")]
public class SpeedUpgrade : ItemEffect
{
    [Min(0f)]
    [Tooltip("Speed increase amount.")]
    public float speedIncrease = 1f;

    public override void Use(PlayerController player)
    {
        int current = PlayerPrefs.GetInt("UpgradeLevel_SpeedUpgrade", 0);
        PlayerPrefs.SetInt("UpgradeLevel_SpeedUpgrade", current + 1);
        PlayerPrefs.Save();
    }
}
