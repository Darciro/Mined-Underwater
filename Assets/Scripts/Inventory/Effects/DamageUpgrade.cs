using UnityEngine;

[CreateAssetMenu(fileName = "Damage Upgrade", menuName = "Mined Underwater/Inventory/Upgrades/Damage Upgrade")]
public class DamageUpgrade : ItemEffect
{
    [Min(0f)]
    [Tooltip("Damage increase amount.")]
    public float damageIncrease = 1f;

    public override void Use(PlayerController player)
    {
        int current = PlayerPrefs.GetInt("UpgradeLevel_DamageUpgrade", 0);
        PlayerPrefs.SetInt("UpgradeLevel_DamageUpgrade", current + 1);
        PlayerPrefs.Save();
    }
}
