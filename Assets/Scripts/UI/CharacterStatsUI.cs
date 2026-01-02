using TMPro;
using UnityEngine;

/// <summary>
/// UI component for displaying player character stats in the progression/shop scene.
/// Retrieves base stats from the PlayerController prefab and adds saved upgrade values
/// from PlayerPrefs to calculate and display the final stats.
/// </summary>
public class CharacterStatsUI : MonoBehaviour
{
    #region Inspector Fields

    [Header("Player Prefab Reference")]
    [SerializeField]
    [Tooltip("Reference to the PlayerController prefab to get base stats from")]
    private PlayerController playerPrefab;

    [Header("Stat Text Fields")]
    [SerializeField]
    [Tooltip("Text display for max health")]
    private TextMeshProUGUI healthText;

    [SerializeField]
    [Tooltip("Text display for max air")]
    private TextMeshProUGUI airText;

    [SerializeField]
    [Tooltip("Text display for damage range")]
    private TextMeshProUGUI damageText;

    [SerializeField]
    [Tooltip("Text display for attack speed (fire rate)")]
    private TextMeshProUGUI attackSpeedText;

    [SerializeField]
    [Tooltip("Text display for movement speed")]
    private TextMeshProUGUI moveSpeedText;

    #endregion

    #region Unity Lifecycle

    private void Start()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("CharacterStatsUI: PlayerController prefab reference is not assigned!");
            return;
        }

        // Initial display update - load from prefab + saved data
        UpdateStatsDisplay();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Manually refreshes all stat displays from prefab + saved data
    /// </summary>
    public void UpdateStatsDisplay()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("CharacterStatsUI: Cannot update stats - PlayerController prefab is null!");
            return;
        }

        UpdateHealthDisplay();
        UpdateAirDisplay();
        UpdateDamageDisplay();
        UpdateAttackSpeedDisplay();
        UpdateMoveSpeedDisplay();
    }

    #endregion

    #region Private Display Methods

    /// <summary>
    /// Updates the health text display from prefab base value + saved upgrade data
    /// </summary>
    private void UpdateHealthDisplay()
    {
        if (healthText == null) return;

        int baseMaxHealth = playerPrefab.GetMaxHealth();
        int healthUpgradeLevel = PlayerPrefs.GetInt("UpgradeLevel_HealthUpgrade", 0);
        int maxHealth = baseMaxHealth + healthUpgradeLevel;
        healthText.text = $"{maxHealth}";
    }

    /// <summary>
    /// Updates the air text display from prefab base value + saved upgrade data
    /// </summary>
    private void UpdateAirDisplay()
    {
        if (airText == null) return;

        int baseMaxAir = playerPrefab.GetMaxAir();
        int airUpgradeLevel = PlayerPrefs.GetInt("UpgradeLevel_AirUpgrade", 0);
        int maxAir = baseMaxAir + airUpgradeLevel;
        airText.text = $"{maxAir}";
    }

    /// <summary>
    /// Updates the damage text display from prefab base value + saved upgrade data
    /// </summary>
    private void UpdateDamageDisplay()
    {
        if (damageText == null) return;

        int baseMinDamage = playerPrefab.GetMinDamage();
        int baseMaxDamage = playerPrefab.GetMaxDamage();
        int damageUpgradeLevel = PlayerPrefs.GetInt("UpgradeLevel_DamageUpgrade", 0);

        int minDamage = baseMinDamage + damageUpgradeLevel;
        int maxDamage = baseMaxDamage + damageUpgradeLevel;

        if (minDamage == maxDamage)
        {
            damageText.text = $"{minDamage}";
        }
        else
        {
            damageText.text = $"{minDamage} - {maxDamage}";
        }
    }

    /// <summary>
    /// Updates the attack speed text display from prefab base value
    /// </summary>
    private void UpdateAttackSpeedDisplay()
    {
        if (attackSpeedText == null) return;

        float baseFireRate = playerPrefab.GetAttackSpeed();
        // Fire rate doesn't have upgrades in the current system
        // Display as attacks per second
        float attacksPerSecond = 1f / baseFireRate;
        attackSpeedText.text = $"{attacksPerSecond:F2} / sec";
    }

    /// <summary>
    /// Updates the movement speed text display from prefab base value + saved upgrade data
    /// </summary>
    private void UpdateMoveSpeedDisplay()
    {
        if (moveSpeedText == null) return;

        float baseSpeed = playerPrefab.GetMoveSpeed();
        int speedUpgradeLevel = PlayerPrefs.GetInt("UpgradeLevel_SpeedUpgrade", 0);
        float moveSpeed = baseSpeed + (speedUpgradeLevel * 0.1f);
        moveSpeedText.text = $"{moveSpeed:F1}";
    }

    #endregion
}
