using UnityEngine;

[CreateAssetMenu(fileName = "Health Upgrade", menuName = "Mined Underwater/Inventory/Upgrades/Health Upgrade")]
public class HealthUpgrade : ItemEffect
{
    [Min(0f)]
    [Tooltip("Health increase amount.")]
    public int healthIncrease = 5;

    public override void Use(PlayerController player)
    {
        int current = PlayerPrefs.GetInt("UpgradeLevel_HealthUpgrade", 0);
        PlayerPrefs.SetInt("UpgradeLevel_HealthUpgrade", current + healthIncrease);
        PlayerPrefs.Save();
        player.IncreaseMaxHealth(healthIncrease);
    }
}
