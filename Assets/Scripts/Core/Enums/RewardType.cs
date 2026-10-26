/// <summary>
/// Defines what a stage reward grants to the player.
/// </summary>
public enum RewardType
{
    /// <summary>An inventory item (uses the reward's Item reference).</summary>
    Item,

    /// <summary>A number of coins added to the player's total.</summary>
    Coins,

    /// <summary>A number of eggs added to the player's total.</summary>
    Eggs
}
